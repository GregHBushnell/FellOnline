using UnityEngine;
using FishNet.Object;
using UnityEngine.AI;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using FellOnline.Client;

namespace FellOnline.Shared
{
    public class MobDamageController : MobBehaviour, IDamageable, IHealable
    {
        [Tooltip("The resource attribute the damage will be applied to. health, mana, stamina etc.")]
		public MobAttributeTemplate ResourceAttribute;
        public bool Immortal = false;
		public bool IsAggro = false;

        [SerializeField]
        public readonly  SyncVar<int> MobHealth = new SyncVar<int>(new SyncTypeSettings()
        {
            SendRate = 0.0f,
            Channel = Channel.Unreliable,
            ReadPermission = ReadPermission.Observers,
            WritePermission = WritePermission.ServerOnly,
        });
        public AchievementTemplate DamageAchievementTemplate;
        public AchievementTemplate KillAchievementTemplate;
        public AchievementTemplate HealAchievementTemplate;
        public AchievementTemplate ResurrectAchievementTemplate;

        private MobResourceAttribute resourceInstance; // cache the resource

        #if !UNITY_SERVER
		public bool ShowDamage = true;
		public event Func<string, Vector3, Color, float, float, bool, Cached3DLabel> OnDamageDisplay;

		public bool ShowHeals = true;
		public event Func<string, Vector3, Color, float, float, bool, Cached3DLabel> OnHealedDisplay;

		public override void OnStartClient()
		{
			base.OnStartClient();
		
			if (LabelMaker.Instance != null)
			{
				OnDamageDisplay += LabelMaker.Display;
				OnHealedDisplay += LabelMaker.Display;
			}
		}

		public override void OnStopClient()
		{
			base.OnStopClient();
			if (LabelMaker.Instance != null)
			{
				OnDamageDisplay -= LabelMaker.Display;
				OnHealedDisplay -= LabelMaker.Display;
			}
		}
#endif
        public void Damage(Character attacker, int amount, DamageAttributeTemplate damageAttribute)
        {
            if (Immortal)
			{
				return;
			}

			if (resourceInstance != null && resourceInstance.CurrentValue > 0)
			{
				amount = ApplyModifiers(Mob, amount, damageAttribute);
				if (amount < 1)
				{
					return;
				}
				Mob.mobController.Target = attacker.transform;
				resourceInstance.Consume(amount);

				if (attacker.TryGet(out AchievementController attackerAchievementController))
				{
					attackerAchievementController.Increment(DamageAchievementTemplate, (uint)amount);
				}
				
#if !UNITY_SERVER
				if (ShowDamage)
				{
					Vector3 displayPos = transform.position;
					displayPos.y += GetComponent<NavMeshAgent>().height;
					OnDamageDisplay?.Invoke(amount.ToString(), displayPos, new Color(255.0f, 128.0f, 128.0f), 10.0f, 10.0f, false);
				}
#endif

				// check if we died
				if (resourceInstance != null && resourceInstance.CurrentValue < 1)
				{
					Kill(attacker);
				}
			}
        }
		public void Kill(Character killer)
		{
			if (killer != null &&
				killer.TryGet(out AchievementController killerAchievementController))
			{
				killerAchievementController.Increment(KillAchievementTemplate, 1);
			}
			
		}
        public int ApplyModifiers(Mob target, int amount, DamageAttributeTemplate damageAttribute)
		{
			const int MIN_DAMAGE = 0;
			const int MAX_DAMAGE = 999999;

			if (target == null ||
				!target.TryGet(out MobAttributeController attributeController) ||
				damageAttribute == null)
				return 0;

			if (attributeController.TryGetAttribute(damageAttribute.Resistance.ID, out MobAttribute resistance))
			{
				amount = (amount - resistance.FinalValue).Clamp(MIN_DAMAGE, MAX_DAMAGE);
			}
			return amount;
		}


        public void Heal(Character healer, int amount)
        {
           if (resourceInstance != null && resourceInstance.CurrentValue > 0.0f)
			{
				resourceInstance.Gain(amount);
				
				if (healer != null &&
					healer.TryGet(out AchievementController healerAchievementController))
				{
					healerAchievementController.Increment(HealAchievementTemplate, (uint)amount);
				}

#if !UNITY_SERVER
				if (Mob != null &&
					ShowHeals)
				{
					Vector3 displayPos = Mob.Transform.position;
					displayPos.y += Mob.mobController.agent.height;
					OnHealedDisplay?.Invoke(amount.ToString(), displayPos, new Color(128.0f, 255.0f, 128.0f), 10.0f, 10.0f, false);
				}
#endif
        }
    }

}
}
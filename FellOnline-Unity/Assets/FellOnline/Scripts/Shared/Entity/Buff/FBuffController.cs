using FishNet.Object;
using FishNet.Transporting;
using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	[RequireComponent(typeof(Character))]
	public class FBuffController : NetworkBehaviour
	{
		private Dictionary<int, FBuff> buffs = new Dictionary<int, FBuff>();

		public Character Character;

		public Dictionary<int, FBuff> Buffs { get { return buffs; } }
		private List<int> keysToRemove = new List<int>();
		void Update()
		{
			float dt = Time.deltaTime;
			foreach (var pair in buffs)
			{
				pair.Value.SubtractTime(dt);
				var buff = pair.Value;
				buff.SubtractTime(dt);
				
				if (pair.Value.RemainingTime > 0.0f)
				{
					buff.SubtractTickTime(dt);
					buff.TryTick(Character);
				}
				else
				{
					if (buff.Stacks.Count > 0 && buff.Template.IndependantStackTimer)
					{
						buff.RemoveStack(Character);

					}
					foreach (FBuff stack in buff.Stacks)
					{
						stack.RemoveStack(Character);
					}
					buff.Remove(Character);

					// Add the key to the list for later removal
					keysToRemove.Add(pair.Key);
				}
			}
			// Remove keys outside the loop to avoid modifying the dictionary during iteration
			foreach (var key in keysToRemove)
			{
				buffs.Remove(key);
			}
			keysToRemove.Clear();
		}

		public void Apply(FBuffTemplate template)
		{
			FBuff FBuffInstance;
			if (!buffs.TryGetValue(template.ID, out FBuffInstance))
			{
				FBuffInstance = new FBuff(template.ID);
				FBuffInstance.Apply(Character);
				buffs.Add(template.ID, FBuffInstance);
			}
			else if (template.MaxStacks > 0 && FBuffInstance.Stacks.Count < template.MaxStacks)
			{
				FBuff newStack = new FBuff(template.ID);
				FBuffInstance.AddStack(newStack, Character);
				FBuffInstance.ResetDuration();
			}
			else
			{
				FBuffInstance.ResetDuration();
			}
		}

		public void Apply(FBuff FBuff)
		{
			if (!buffs.ContainsKey(FBuff.Template.ID))
			{
				buffs.Add(FBuff.Template.ID, FBuff);
			}
		}

		public void Remove(int FBuffID)
		{
			if (buffs.TryGetValue(FBuffID, out FBuff FBuffInstance))
			{
				foreach (FBuff stack in FBuffInstance.Stacks)
				{
					stack.RemoveStack(Character);
				}
				FBuffInstance.Remove(Character);
				buffs.Remove(FBuffID);
			}
		}

		public void RemoveAll()
		{
			foreach (KeyValuePair<int, FBuff> pair in new Dictionary<int, FBuff>(buffs))
			{
				if (!pair.Value.Template.IsPermanent)
				{
					foreach (FBuff stack in pair.Value.Stacks)
					{
						stack.RemoveStack(Character);
					}
					pair.Value.Remove(Character);
					buffs.Remove(pair.Key);
				}
			}
		}

#if !UNITY_SERVER
		public override void OnStartClient()
		{
			base.OnStartClient();

			if (!base.IsOwner)
			{
				enabled = false;
				return;
			}

			ClientManager.RegisterBroadcast<BuffAddBroadcast>(OnClientFBuffAddBroadcastReceived);
			ClientManager.RegisterBroadcast<BuffAddMultipleBroadcast>(OnClientFBuffAddMultipleBroadcastReceived);
			ClientManager.RegisterBroadcast<BuffRemoveBroadcast>(OnClientFBuffRemoveBroadcastReceived);
			ClientManager.RegisterBroadcast<BuffRemoveMultipleBroadcast>(OnClientFBuffRemoveMultipleBroadcastReceived);
		}

		public override void OnStopClient()
		{
			base.OnStopClient();

			if (base.IsOwner)
			{
				ClientManager.UnregisterBroadcast<BuffAddBroadcast>(OnClientFBuffAddBroadcastReceived);
				ClientManager.UnregisterBroadcast<BuffAddMultipleBroadcast>(OnClientFBuffAddMultipleBroadcastReceived);
				ClientManager.UnregisterBroadcast<BuffRemoveBroadcast>(OnClientFBuffRemoveBroadcastReceived);
				ClientManager.UnregisterBroadcast<BuffRemoveMultipleBroadcast>(OnClientFBuffRemoveMultipleBroadcastReceived);
			}
		}

		/// <summary>
		/// Server sent a FBuff add broadcast.
		/// </summary>
		private void OnClientFBuffAddBroadcastReceived(BuffAddBroadcast msg, Channel channel)
		{
			FBuffTemplate template = FBuffTemplate.Get<FBuffTemplate>(msg.templateID);
			if (template != null)
			{
				Apply(template);
			}
		}

		/// <summary>
		/// Server sent a multiple FBuff add broadcast.
		/// </summary>
		private void OnClientFBuffAddMultipleBroadcastReceived(BuffAddMultipleBroadcast msg, Channel channel)
		{
			foreach (BuffAddBroadcast subMsg in msg.buffs)
			{
				FBuffTemplate template = FBuffTemplate.Get<FBuffTemplate>(subMsg.templateID);
				if (template != null)
				{
					Apply(template);
				}
			}
		}

		/// <summary>
		/// Server sent a remove FBuff add broadcast.
		/// </summary>
		private void OnClientFBuffRemoveBroadcastReceived(BuffRemoveBroadcast msg, Channel channel)
		{
			FBuffTemplate template = FBuffTemplate.Get<FBuffTemplate>(msg.templateID);
			if (template != null)
			{
				Remove(template.ID);
			}
		}

		/// <summary>
		/// Server sent a remove multiple FBuff add broadcast.
		/// </summary>
		private void OnClientFBuffRemoveMultipleBroadcastReceived(BuffRemoveMultipleBroadcast msg, Channel channel)
		{
			foreach (BuffRemoveBroadcast subMsg in msg.buffs)
			{
				FBuffTemplate template = FBuffTemplate.Get<FBuffTemplate>(subMsg.templateID);
				if (template != null)
				{
					Remove(template.ID);
				}
			}
		}
#endif
	}
}
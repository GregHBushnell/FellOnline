using FishNet.Object;
using UnityEngine;

namespace FellOnline.Shared
{
	[RequireComponent(typeof(FCharacterAttributeController))]
	public class FCharacterRegenerationController : NetworkBehaviour
	{
		public static Character localCharacter;
		public FCharacterAttributeController AttributeController;

		public FCharacterAttributeTemplate HealthTemplate;
		public FCharacterAttributeTemplate ManaTemplate;
		public FCharacterAttributeTemplate HealthRegenerationTemplate;
		public FCharacterAttributeTemplate ManaRegenerationTemplate;

		public float nextRegenTick = 0.0f;
		public float regenerateTickRate = 1.0f;

		void Awake()
		{
			AttributeController = gameObject.GetComponent<FCharacterAttributeController>();
		}

		void Update()
		{
			OnRegenerate();
		}

		private void OnRegenerate()
		{
			if (nextRegenTick < regenerateTickRate)
			{
				nextRegenTick = regenerateTickRate;

				if (AttributeController.TryGetResourceAttribute(HealthTemplate, out FCharacterResourceAttribute health))
				{
					if (AttributeController.TryGetAttribute(HealthRegenerationTemplate, out FCharacterAttribute healthRegeneration))
					{
						health.Gain(healthRegeneration.FinalValue);
					}
				}
				if (AttributeController.TryGetResourceAttribute(ManaTemplate, out FCharacterResourceAttribute mana))
				{
					if (AttributeController.TryGetAttribute(ManaRegenerationTemplate, out FCharacterAttribute manaRegeneration))
					{
						mana.Gain(manaRegeneration.FinalValue);
					}
				}

				//stamina regeneration is handled by the run function in the character controller
			}
			nextRegenTick -= Time.deltaTime;
		}
	}
}
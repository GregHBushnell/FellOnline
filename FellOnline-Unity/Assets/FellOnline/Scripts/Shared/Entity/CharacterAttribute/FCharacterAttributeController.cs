using System.Collections.Generic;
using FishNet.Object;
using FishNet.Transporting;

namespace FellOnline.Shared
{
	public class FCharacterAttributeController : NetworkBehaviour
	{
		public FCharacterAttributeTemplateDatabase CharacterAttributeDatabase;

		public readonly Dictionary<int, FCharacterAttribute> Attributes = new Dictionary<int, FCharacterAttribute>();
		public readonly Dictionary<int, FCharacterResourceAttribute> ResourceAttributes = new Dictionary<int, FCharacterResourceAttribute>();

		protected void Awake()
		{
			if (CharacterAttributeDatabase != null)
			{
				foreach (FCharacterAttributeTemplate attribute in CharacterAttributeDatabase.Attributes.Values)
				{
					if (attribute.IsResourceAttribute)
					{
						FCharacterResourceAttribute resource = new FCharacterResourceAttribute(attribute.ID, attribute.InitialValue, attribute.InitialValue, 0);
						AddAttribute(resource);
						ResourceAttributes.Add(resource.Template.ID, resource);
					}
					else
					{
						AddAttribute(new FCharacterAttribute(attribute.ID, attribute.InitialValue, 0));
					}
				}
			}
		}

		public void SetAttribute(int id, int baseValue, int modifier)
		{
			if (Attributes.TryGetValue(id, out FCharacterAttribute attribute))
			{
				attribute.SetValue(baseValue);
				attribute.SetModifier(modifier);
			}
		}

		public void SetResourceAttribute(int id, int baseValue, int modifier, int currentValue)
		{
			if (ResourceAttributes.TryGetValue(id, out FCharacterResourceAttribute attribute))
			{
				attribute.SetValue(baseValue);
				attribute.SetModifier(modifier);
				attribute.SetCurrentValue(currentValue);
			}
		}

		public bool TryGetAttribute(FCharacterAttributeTemplate template, out FCharacterAttribute attribute)
		{
			return Attributes.TryGetValue(template.ID, out attribute);
		}

		public bool TryGetAttribute(int id, out FCharacterAttribute attribute)
		{
			return Attributes.TryGetValue(id, out attribute);
		}

		public bool TryGetResourceAttribute(FCharacterAttributeTemplate template, out FCharacterResourceAttribute attribute)
		{
			return ResourceAttributes.TryGetValue(template.ID, out attribute);
		}

		public float GetResourceAttributeCurrentPercentage(FCharacterAttributeTemplate template)
		{
			if (ResourceAttributes.TryGetValue(template.ID, out FCharacterResourceAttribute attribute))
			{
				return attribute.FinalValue / attribute.CurrentValue;
			}
			return 0.0f;
		}

		public bool TryGetResourceAttribute(int id, out FCharacterResourceAttribute attribute)
		{
			return ResourceAttributes.TryGetValue(id, out attribute);
		}

		public void AddAttribute(FCharacterAttribute instance)
		{
			if (!Attributes.ContainsKey(instance.Template.ID))
			{
				Attributes.Add(instance.Template.ID, instance);

				foreach (FCharacterAttributeTemplate parent in instance.Template.ParentTypes)
				{
					FCharacterAttribute parentInstance;
					if (Attributes.TryGetValue(parent.ID, out parentInstance))
					{
						parentInstance.AddChild(instance);
					}
				}

				foreach (FCharacterAttributeTemplate child in instance.Template.ChildTypes)
				{
					FCharacterAttribute childInstance;
					if (Attributes.TryGetValue(child.ID, out childInstance))
					{
						instance.AddChild(childInstance);
					}
				}

				foreach (FCharacterAttributeTemplate dependant in instance.Template.DependantTypes)
				{
					FCharacterAttribute dependantInstance;
					if (Attributes.TryGetValue(dependant.ID, out dependantInstance))
					{
						instance.AddDependant(dependantInstance);
					}
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

			ClientManager.RegisterBroadcast<CharacterAttributeUpdateBroadcast>(OnClientCharacterAttributeUpdateBroadcastReceived);
			ClientManager.RegisterBroadcast<CharacterAttributeUpdateMultipleBroadcast>(OnClientCharacterAttributeUpdateMultipleBroadcastReceived);

			ClientManager.RegisterBroadcast<CharacterResourceAttributeUpdateBroadcast>(OnClientCharacterResourceAttributeUpdateBroadcastReceived);
			ClientManager.RegisterBroadcast<CharacterResourceAttributeUpdateMultipleBroadcast>(OnClientCharacterResourceAttributeUpdateMultipleBroadcastReceived);
		}

		public override void OnStopClient()
		{
			base.OnStopClient();

			if (base.IsOwner)
			{
				ClientManager.UnregisterBroadcast<CharacterAttributeUpdateBroadcast>(OnClientCharacterAttributeUpdateBroadcastReceived);
				ClientManager.UnregisterBroadcast<CharacterAttributeUpdateMultipleBroadcast>(OnClientCharacterAttributeUpdateMultipleBroadcastReceived);

				ClientManager.UnregisterBroadcast<CharacterResourceAttributeUpdateBroadcast>(OnClientCharacterResourceAttributeUpdateBroadcastReceived);
				ClientManager.UnregisterBroadcast<CharacterResourceAttributeUpdateMultipleBroadcast>(OnClientCharacterResourceAttributeUpdateMultipleBroadcastReceived);
			}
		}

		/// <summary>
		/// Server sent an attribute update broadcast.
		/// </summary>
		private void OnClientCharacterAttributeUpdateBroadcastReceived(CharacterAttributeUpdateBroadcast msg, Channel channel)
		{
			FCharacterAttributeTemplate template = FCharacterAttributeTemplate.Get<FCharacterAttributeTemplate>(msg.templateID);
			if (template != null &&
				Attributes.TryGetValue(template.ID, out FCharacterAttribute attribute))
			{
				attribute.SetFinal(msg.value);
			}
		}

		/// <summary>
		/// Server sent a multiple attribute update broadcast.
		/// </summary>
		private void OnClientCharacterAttributeUpdateMultipleBroadcastReceived(CharacterAttributeUpdateMultipleBroadcast msg, Channel channel)
		{
			foreach (CharacterAttributeUpdateBroadcast subMsg in msg.attributes)
			{
				FCharacterAttributeTemplate template = FCharacterAttributeTemplate.Get<FCharacterAttributeTemplate>(subMsg.templateID);
				if (template != null &&
					Attributes.TryGetValue(template.ID, out FCharacterAttribute attribute))
				{
					attribute.SetFinal(subMsg.value);
				}
			}
		}

		/// <summary>
		/// Server sent a resource attribute update broadcast.
		/// </summary>
		private void OnClientCharacterResourceAttributeUpdateBroadcastReceived(CharacterResourceAttributeUpdateBroadcast msg, Channel channel)
		{
			FCharacterAttributeTemplate template = FCharacterAttributeTemplate.Get<FCharacterAttributeTemplate>(msg.templateID);
			if (template != null &&
				ResourceAttributes.TryGetValue(template.ID, out FCharacterResourceAttribute attribute))
			{
				attribute.SetCurrentValue(msg.value);
				attribute.SetFinal(msg.max);
			}
		}

		/// <summary>
		/// Server sent a multiple resource attribute update broadcast.
		/// </summary>
		private void OnClientCharacterResourceAttributeUpdateMultipleBroadcastReceived(CharacterResourceAttributeUpdateMultipleBroadcast msg, Channel channel)
		{
			foreach (CharacterResourceAttributeUpdateBroadcast subMsg in msg.attributes)
			{
				FCharacterAttributeTemplate template = FCharacterAttributeTemplate.Get<FCharacterAttributeTemplate>(subMsg.templateID);
				if (template != null &&
					ResourceAttributes.TryGetValue(template.ID, out FCharacterResourceAttribute attribute))
				{
					attribute.SetCurrentValue(subMsg.value);
					attribute.SetFinal(subMsg.max);
				}
			}
		}
#endif
	}
}
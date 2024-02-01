using System.Collections.Generic;
using FishNet.Object;
using FishNet.Transporting;

namespace FellOnline.Shared
{
	public class MobAttributeController : MobBehaviour
	{
		public MobAttributeTemplateDatabase MobAttributeDatabase;

		public readonly Dictionary<int, MobAttribute> Attributes = new Dictionary<int, MobAttribute>();
		public readonly Dictionary<int, MobResourceAttribute> ResourceAttributes = new Dictionary<int, MobResourceAttribute>();

		public override void OnAwake()
		{
			if (MobAttributeDatabase != null)
			{
				foreach (MobAttributeTemplate attribute in MobAttributeDatabase.Attributes.Values)
				{
					if (attribute.IsResourceAttribute)
					{
						MobResourceAttribute resource = new MobResourceAttribute(attribute.ID, attribute.InitialValue, attribute.InitialValue, 0);
						AddAttribute(resource);
						ResourceAttributes.Add(resource.Template.ID, resource);
					}
					else
					{
						AddAttribute(new MobAttribute(attribute.ID, attribute.InitialValue, 0));
					}
				}
			}
		}

		public void SetAttribute(int id, int baseValue, int modifier)
		{
			if (Attributes.TryGetValue(id, out MobAttribute attribute))
			{
				attribute.SetValue(baseValue);
				attribute.SetModifier(modifier);
			}
		}

		public void SetResourceAttribute(int id, int baseValue, int modifier, int currentValue)
		{
			if (ResourceAttributes.TryGetValue(id, out MobResourceAttribute attribute))
			{
				attribute.SetValue(baseValue);
				attribute.SetModifier(modifier);
				attribute.SetCurrentValue(currentValue);
			}
		}

		public bool TryGetAttribute(MobAttributeTemplate template, out MobAttribute attribute)
		{
			return Attributes.TryGetValue(template.ID, out attribute);
		}

		public bool TryGetAttribute(int id, out MobAttribute attribute)
		{
			return Attributes.TryGetValue(id, out attribute);
		}

		public bool TryGetResourceAttribute(MobAttributeTemplate template, out MobResourceAttribute attribute)
		{
			return ResourceAttributes.TryGetValue(template.ID, out attribute);
		}

		public float GetResourceAttributeCurrentPercentage(MobAttributeTemplate template)
		{
			if (ResourceAttributes.TryGetValue(template.ID, out MobResourceAttribute attribute))
			{
				return attribute.FinalValue / attribute.CurrentValue;
			}
			return 0.0f;
		}

		public bool TryGetResourceAttribute(int id, out MobResourceAttribute attribute)
		{
			return ResourceAttributes.TryGetValue(id, out attribute);
		}

		public void AddAttribute(MobAttribute instance)
		{
			if (!Attributes.ContainsKey(instance.Template.ID))
			{
				Attributes.Add(instance.Template.ID, instance);

				foreach (MobAttributeTemplate parent in instance.Template.ParentTypes)
				{
					MobAttribute parentInstance;
					if (Attributes.TryGetValue(parent.ID, out parentInstance))
					{
						parentInstance.AddChild(instance);
					}
				}

				foreach (MobAttributeTemplate child in instance.Template.ChildTypes)
				{
					MobAttribute childInstance;
					if (Attributes.TryGetValue(child.ID, out childInstance))
					{
						instance.AddChild(childInstance);
					}
				}

				foreach (MobAttributeTemplate dependant in instance.Template.DependantTypes)
				{
					MobAttribute dependantInstance;
					if (Attributes.TryGetValue(dependant.ID, out dependantInstance))
					{
						instance.AddDependant(dependantInstance);
					}
				}
			}
		}
	}
}
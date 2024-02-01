﻿using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace FellOnline.Shared{

    public class MobAttribute
	{
		public MobAttributeTemplate Template { get; private set; }

		private int baseValue;
		private int modifier;
		private int finalValue;
		private Dictionary<string, MobAttribute> parents = new Dictionary<string, MobAttribute>();
		private Dictionary<string, MobAttribute> children = new Dictionary<string, MobAttribute>();
		private Dictionary<string, MobAttribute> dependencies = new Dictionary<string, MobAttribute>();

		public Action<MobAttribute> OnAttributeUpdated;

		protected virtual void Internal_OnAttributeChanged(MobAttribute item)
		{
			OnAttributeUpdated?.Invoke(item);
		}

		public int BaseValue { get { return baseValue; } }
		public void SetValue(int newValue)
		{
			SetValue(newValue, false);
		}
		public void SetValue(int newValue, bool skipUpdate)
		{
			if (baseValue != newValue)
			{
				baseValue = newValue;
				if (!skipUpdate)
				{
					UpdateValues(true);
				}
			}
		}
		/// <summary>
		/// Used to add or subtract an amount from the base value of the attribute. Addition: AddValue(123) | Subtraction: AddValue(-123)
		/// </summary>
		/// <param name="amount"></param>
		public void AddValue(int amount)
		{
			AddValue(amount, false);
		}
		public void AddValue(int amount, bool skipUpdate)
		{
			int tmp = baseValue + amount;
			if (baseValue != tmp)
			{
				baseValue = tmp;
				if (!skipUpdate)
				{
					UpdateValues(true);
				}
			}
		}
		public void SetModifier(int newValue)
		{
			if (modifier != newValue)
			{
				modifier = newValue;
				finalValue = CalculateFinalValue();
			}
		}
		public void AddModifier(int amount)
		{
			int tmp = modifier + amount;
			if (modifier != tmp)
			{
				modifier = tmp;
				finalValue = CalculateFinalValue();
			}
		}
		public void SetFinal(int newValue)
		{
			finalValue = newValue;
		}

		public int Modifier { get { return modifier; } }
		public int FinalValue { get { return finalValue; } }
		/// <summary>
		/// Returns the value as a float. FinalValue * 0.1f;
		/// </summary>
		public float FinalValueAsFloat { get { return finalValue * 0.1f; } }
		/// <summary>
		/// Returns the value as a percentage instead. FinalValue * 0.01f
		/// </summary>
		public float FinalValueAsPct { get { return finalValue * 0.01f; } }

		public Dictionary<string, MobAttribute> Parents { get { return parents; } }
		public Dictionary<string, MobAttribute> Children { get { return children; } }
		public Dictionary<string, MobAttribute> Dependencies { get { return dependencies; } }

		public override string ToString()
		{
			return Template.Name + ": " + FinalValue;
		}

		public MobAttribute(int templateID, int initialValue, int initialModifier)
		{
			Template = MobAttributeTemplate.Get<MobAttributeTemplate>(templateID);
			baseValue = initialValue;
			modifier = initialModifier;
			finalValue = CalculateFinalValue();
		}

		public void AddParent(MobAttribute parent)
		{
			if (!parents.ContainsKey(parent.Template.Name))
			{
				parents.Add(parent.Template.Name, parent);
			}
		}

		public void RemoveParent(MobAttribute parent)
		{
			parents.Remove(parent.Template.Name);
		}

		public void AddChild(MobAttribute child)
		{
			if (!children.ContainsKey(child.Template.Name))
			{
				children.Add(child.Template.Name, child);
				child.AddParent(this);
				UpdateValues();
			}
		}

		public void RemoveChild(MobAttribute child)
		{
			children.Remove(child.Template.Name);
			child.RemoveParent(this);
			UpdateValues();
		}

		public void AddDependant(MobAttribute dependency)
		{
			Type dependencyType = dependency.GetType();
			if (!dependencies.ContainsKey(dependencyType.Name))
			{
				dependencies.Add(dependencyType.Name, dependency);
			}
		}

		public void RemoveDependant(MobAttribute dependency)
		{
			dependencies.Remove(dependency.GetType().Name);
		}

		public MobAttribute GetDependant(string name)
		{
			dependencies.TryGetValue(name, out MobAttribute result);
			return result;
		}

		public int GetDependantBaseValue(string name)
		{
			return (!dependencies.TryGetValue(name, out MobAttribute attribute)) ? 0 : attribute.BaseValue;
		}

		public int GetDependantMinValue(string name)
		{
			return (!dependencies.TryGetValue(name, out MobAttribute attribute)) ? 0 : attribute.Template.MinValue;
		}

		public int GetDependantMaxValue(string name)
		{
			return (!dependencies.TryGetValue(name, out MobAttribute attribute)) ? 0 : attribute.Template.MaxValue;
		}

		public int GetDependantModifier(string name)
		{
			return (!dependencies.TryGetValue(name, out MobAttribute attribute)) ? 0 : attribute.Modifier;
		}

		public int GetDependantFinalValue(string name)
		{
			return (!dependencies.TryGetValue(name, out MobAttribute attribute)) ? 0 : attribute.FinalValue;
		}

		public void UpdateValues()
		{
			UpdateValues(false);
		}
		public void UpdateValues(bool forceUpdate)
		{
			int oldFinalValue = finalValue;

			ApplyChildren();

			if (forceUpdate || finalValue != oldFinalValue)
			{
				foreach (MobAttribute parent in parents.Values)
				{
					parent.UpdateValues();
				}
			}
		}

		private void ApplyChildren()
		{
			modifier = 0;
			if (Template.Formulas != null)
			{
				foreach (KeyValuePair<MobAttributeTemplate, MobAttributeFormulaTemplate> pair in Template.Formulas)
				{
					if (children.TryGetValue(pair.Key.Name, out MobAttribute child))
					{
						modifier += pair.Value.CalculateBonus(this, child);
					}
				}
			}
			finalValue = CalculateFinalValue();
			OnAttributeUpdated?.Invoke(this);
		}

		private int CalculateFinalValue()
		{
			if (Template.ClampFinalValue)
			{
				return (baseValue + modifier).Clamp(Template.MinValue, Template.MaxValue);
			}
			return baseValue + modifier;
		}
	}
}
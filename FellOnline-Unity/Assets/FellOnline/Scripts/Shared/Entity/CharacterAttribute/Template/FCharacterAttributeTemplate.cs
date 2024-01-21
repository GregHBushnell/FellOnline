using System;
using Cysharp.Text;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Character Attribute", menuName = "FellOnline/Character/Attribute/Character Attribute", order = 1)]
	public class FCharacterAttributeTemplate : FCachedScriptableObject<FCharacterAttributeTemplate>, FICachedObject
	{
		[Serializable]
		public class FCharacterAttributeFormulaDictionary : SerializableDictionary<FCharacterAttributeTemplate, FCharacterAttributeFormulaTemplate> { }

		[Serializable]
		public class FCharacterAttributeSet : SerializableHashSet<FCharacterAttributeTemplate> { }

		public string Description;
		public int InitialValue;
		public int MinValue;
		public int MaxValue;
		public bool IsPercentage;
		public bool IsResourceAttribute;
		public bool ClampFinalValue;
		public FCharacterAttributeSet ParentTypes = new FCharacterAttributeSet();
		public FCharacterAttributeSet ChildTypes = new FCharacterAttributeSet();
		public FCharacterAttributeSet DependantTypes = new FCharacterAttributeSet();
		public FCharacterAttributeFormulaDictionary Formulas = new FCharacterAttributeFormulaDictionary();

		public string Name { get { return this.name; } }

		public string Tooltip()
		{
			using (var sb = ZString.CreateStringBuilder())
			{
				if (!string.IsNullOrWhiteSpace(Name))
				{
					sb.Append("<size=120%><color=#f5ad6e>");
					sb.Append(Name);
					sb.Append("</color></size>");
				}
				if (!string.IsNullOrWhiteSpace(Description))
				{
					sb.AppendLine();
					sb.Append("<color=#a66ef5>Description: ");
					sb.Append(Description);
					sb.Append("</color>");
				}
				if (InitialValue > 0)
				{
					sb.AppendLine();
					sb.Append("<color=#a66ef5>Initial Value: ");
					sb.Append(InitialValue);
					sb.Append("</color>");
				}
				if (MinValue > 0)
				{
					sb.AppendLine();
					sb.Append("<color=#a66ef5>Min Value: ");
					sb.Append(MinValue);
					sb.Append("</color>");
				}
				if (MaxValue > 0)
				{
					sb.AppendLine();
					sb.Append("<color=#a66ef5>Max Value: ");
					sb.Append(MaxValue);
					sb.Append("</color>");
				}
				return sb.ToString();
			}
		}
	}
}
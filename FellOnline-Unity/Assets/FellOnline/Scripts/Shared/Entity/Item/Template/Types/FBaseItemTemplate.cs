using Cysharp.Text;
using UnityEngine;
using System.Collections.Generic;

namespace FellOnline.Shared
{
	public abstract class FBaseItemTemplate : FCachedScriptableObject<FBaseItemTemplate>, FITooltip, FICachedObject
	{
		public bool IsIdentifiable;
		public bool Generate;
		public uint MaxStackSize = 1;
		public long Price;
		//use this for item generation
		public int[] IconPools;
		public Sprite icon;

		public string Name { get { return this.name; } }
		public bool IsStackable { get { return MaxStackSize > 1; } }
		public Sprite Icon { get { return this.icon; } }

		public virtual string Tooltip()
		{
			using (var sb = ZString.CreateStringBuilder())
			{
				sb.Append(FRichText.Format(Name, false, "f5ad6e", "120%"));
				sb.Append("\r\n______________________________\r\n");
				if (Price > 0)
				{
					sb.Append(FRichText.Format("Price", Price, true, "a66ef5FF"));
				}
				return sb.ToString();
			}
		}
		
		public virtual string Tooltip(List<FITooltip> combineList)
		{
			return Tooltip();
		}

		public virtual string GetFormattedDescription()
		{
			return "";
		}
	}
}
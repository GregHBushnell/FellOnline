using UnityEngine;
using System.Collections.Generic;

namespace FellOnline.Shared
{
	public interface FITooltip
	{
		Sprite Icon { get; }
		string Name { get; }
		string GetFormattedDescription();
		string Tooltip();
		string Tooltip(List<FITooltip> combineList);
	}
}
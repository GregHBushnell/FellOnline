using System.Collections.Generic;
using FishNet.Broadcast;
using UnityEngine.Playables;

namespace FellOnline.Shared
{
	public struct WeaponUpdateBroadcast : IBroadcast
	{
         public int templateID;
         public uint visualID;
         public WeaponEquipStyle weaponEquipStyle;
         public ItemSlot equipmentSlot;

	}
}
using System;
using System.Collections.Generic;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
﻿using FishNet.Documenting;
using FishNet.Serializing;
using System.Runtime.CompilerServices;
using FishNet.Managing;
namespace FellOnline.Shared
{
	[Serializable]
	public struct EquipmentAppearanceDetails{
		public int TemplateID;
         public uint VisualID;

         public WeaponEquipStyle WeaponEquipStyle;
		 public ItemSlot EquipmentSlot;

		public EquipmentAppearanceDetails(int templateID, uint visualID ,WeaponEquipStyle weaponEquipStyle,ItemSlot equipmentSlot)
        {
           TemplateID = templateID;
		   VisualID = visualID;
		   WeaponEquipStyle = weaponEquipStyle;
		   EquipmentSlot = equipmentSlot;

        }
	}

}

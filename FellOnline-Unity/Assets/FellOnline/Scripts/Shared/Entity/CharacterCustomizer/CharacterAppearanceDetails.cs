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
	public struct CharacterAppearanceDetails{
		public int SkinColor;
		public int HairID;
		public int HairColor;

		public CharacterAppearanceDetails(int skinColor, int hairID ,int hairColor )
        {
           SkinColor = skinColor;
		   HairID = hairID;
		   HairColor = hairColor;
        }
	}
	public class _SyncCharacterAppearanceDetailsContainer : SyncBase, ICustomSync
	{
		
    	public object GetSerializedType() => typeof(CharacterAppearanceDetails);
	}

}

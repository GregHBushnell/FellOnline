 using System;
 using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
  namespace FellOnline.Shared{

         [Serializable]
         public class mobSpawnList{
            public NetworkObject MobPrefab;
            public int defaultPrefabsID;
            public List<_MobSpawnData> _MobSpawnDataList = new List<_MobSpawnData>();
         }
    [Serializable]
	public struct _MobSpawnData{
		 
        public Vector3 SpawnPosition;
        public Quaternion SpawnRotation;
   
		public _MobSpawnData(Vector3 spawnPosition, Quaternion spawnRotation)
        {
              SpawnPosition = spawnPosition;
                SpawnRotation = spawnRotation;
        }
     }
  }
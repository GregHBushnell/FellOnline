using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace FellOnline.Shared
{
    public class SceneMobController : NetworkBehaviour, IPooledResettable
    {
        [SerializeField]
        public List<mobSpawnList> MobSpawnDataList = new List<mobSpawnList>();

        public void OnPooledReset()
        {
            // throw new NotImplementedException();
        }
        public override void OnStartServer()
        {
            base.OnStartServer();
#if UNITY_SERVER
        if (base.IsServerStarted)
        {
            if (MobSpawnDataList.Count > 0)
            {
            foreach (mobSpawnList mobSpawnDataList in MobSpawnDataList)
            {
                
                    for (int i = 0; i < NetworkManager.SpawnablePrefabs.GetObjectCount(); i++){
                        if(NetworkManager.SpawnablePrefabs.GetObject(true,i).name == mobSpawnDataList.MobPrefab.name){
                            mobSpawnDataList.defaultPrefabsID = i;
                            break;
                        }
                    }
                        if(mobSpawnDataList.defaultPrefabsID>0){
                        NetworkObject prefab = NetworkManager.SpawnablePrefabs.GetObject(true, mobSpawnDataList.defaultPrefabsID);
                        NetworkManager.CacheObjects(prefab, mobSpawnDataList._MobSpawnDataList.Count, true);

                    foreach(_MobSpawnData mobSpawnData in mobSpawnDataList._MobSpawnDataList)
                    {       
                    
                            NetworkObject mob = NetworkManager.ObjectPool.RetrieveObject(mobSpawnDataList.defaultPrefabsID, 0,null, mobSpawnData.SpawnPosition, mobSpawnData.SpawnRotation);
        
                                Spawn(mob, null,gameObject.scene);
                                mob.GetComponent<MobController>().SpawnPosition = mobSpawnData.SpawnPosition;
                            
                            
                            
                        
                    }
                        }
            }
            }
         }
#endif
        }
    }
}
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System.Collections.Generic;
using System.Linq;
using System;

#if !UNITY_SERVER
using FellOnline.Client;
#endif
namespace FellOnline.Shared
{

public class EquipmentAppearanceController : CharacterBehaviour
{


[SerializeField]
public readonly  SyncVar<EquipmentAppearanceDetails> equipmentAppearanceDetails = new SyncVar<EquipmentAppearanceDetails>(new SyncTypeSettings()
{
    SendRate = 0.0f,
    Channel = Channel.Unreliable,
    ReadPermission = ReadPermission.Observers,
    WritePermission = WritePermission.ServerOnly,
});



public Transform RightHandParent;
public Transform LeftHandParent;




    void OnEquipmentAppearanceDetailsChanged(EquipmentAppearanceDetails prev, EquipmentAppearanceDetails next, bool asServer)
    {
        if (ReferenceEquals(prev, null))
        {
            VisualUnequipAll();
            if (!ReferenceEquals(next, null))
            {
                VisualEquipUnequiped(next,true);
            }
        }else{
        if(next.VisualID ==0){
             VisualEquipUnequiped(prev,false);
        }else{
             if(prev.VisualID != next.VisualID){
                VisualEquipUnequiped(prev,false);
                VisualEquipUnequiped(next,true);
             }
        }
    }
       
    }
    void VisualEquipUnequiped(EquipmentAppearanceDetails Equippable,bool equip){
       // if(Equippable.Weapon){
                if (Equippable.EquipmentSlot == ItemSlot.Primary)
                {
                    if (RightHandParent != null)
                    {
                        GameObject weaponGO = RightHandParent.Find(Equippable.VisualID.ToString())?.gameObject;

                        if (weaponGO != null && weaponGO.activeSelf != equip)
                        {
                            weaponGO.SetActive(equip);
                        }
                    }
                }
                else if (Equippable.EquipmentSlot == ItemSlot.Secondary)
                {
                    if (LeftHandParent != null)
                    {
                        GameObject weaponGO = LeftHandParent.Find(Equippable.VisualID.ToString())?.gameObject;

                        if (weaponGO != null && weaponGO.activeSelf != equip)
                        {
                            weaponGO.SetActive(equip);
                        }
                    }
                }
                      
               // }else if(!Equippable.Weapon){
                        //armour stuff
                //}
    }
    void VisualUnequipAll(){
        foreach(Transform child in RightHandParent){
            if(child.gameObject.activeSelf == true)
            child.gameObject.SetActive(false);
        }
        foreach(Transform child in LeftHandParent){
            if(child.gameObject.activeSelf == true)
            child.gameObject.SetActive(false);
        }
    }

    #if !UNITY_SERVER
public override void OnAwake()
		{
              if(RightHandParent == null){
                 RightHandParent = transform.Find("RightHandParent");
                }
                if(LeftHandParent == null){
                    LeftHandParent = transform.Find("LeftHandParent");
                }
			  equipmentAppearanceDetails.OnChange += OnEquipmentAppearanceDetailsChanged;
		}

		public override void OnDestroying()
		{
            equipmentAppearanceDetails.OnChange -= OnEquipmentAppearanceDetailsChanged;
		}
#endif
}

  
}
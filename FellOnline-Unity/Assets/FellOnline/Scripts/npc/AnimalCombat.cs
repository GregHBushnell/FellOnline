using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using FishNet.Object;
// using FishNet.Object.Synchronizing;
// using FishNet.Connection;
public class AnimalCombat : MonoBehaviour
{
 
    // [SyncVar]public int ALevel = 1;
    // [SyncVar]public int AMaxHealth = 25;
    // [SyncVar]public int AHealth = 25;
    
    // [SyncVar]public int dmg = 3;
    // [SyncVar]public int ACritChance = 5;
    // [SyncVar] public int ADefence = 1;

    // [SyncVar]public int AAgility = 1;

    // [SyncVar]public bool AInCombat = false;
    // [SyncVar] public GameObject ATarget;

    // public override void OnStartNetwork(){
    //     base.OnStartNetwork();
    //    AHealth = AMaxHealth;
    // }


    //[ServerRpc(RequireOwnership = false)]   
    // public void ATakeDamage(int dmg,GameObject target)
    // {
    //     int damageTaken = dmg - ADefence;

    //     // Calculate dodge chance based on agility
    //     float dodgeChance = AAgility * 0.01f; // Convert agility to percentage

    //     // Check if the attack is dodged
    //     if (Random.value <= dodgeChance)
    //     {
    //         Debug.Log("Attack dodged!");
    //         return; // Exit the method without taking damage
    //     }
        
    //     if (damageTaken > 0)
    //     {
    //         ATarget = target;
    //         AHealth -= damageTaken;
    //         AInCombat = true;
    //         if(AHealth <= 0){
    //         ADie();
    //         }
    //     }
        
    // }
   // [ServerRpc(RequireOwnership = false)]  
    public void ADie(){
        
    }
}

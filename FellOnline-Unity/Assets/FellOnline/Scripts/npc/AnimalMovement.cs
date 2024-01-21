// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// //using FishNet.Object;
// using System;
// public class AnimalMovement : MonoBehaviour
// {
//       [SerializeField]
//     private Animator anim;
//     public float moveDelay = 2f; // Delay between each move
//     [SerializeField] private float StopDistance = 2f; // Stop 1 tile away from the target position]
//     public float tileSize = 1f; // Size of each tile
//          [SerializeField]
//     private float TilemoveSpeed = 5f;
//        [SerializeField]  private float rotationSpeed = 16f;
    
//    [SerializeField]  private bool inPos;
//     [SerializeField] private Vector3 targetPosition;
//     private Vector3[] movementDirections = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
//     [SerializeField] 
//  private float moveTimer = 0f;
//  AnimalCombat _animalCombat;
// //   public override void OnStartNetwork(){
// //     base.OnStartNetwork();
// //     anim = GetComponentInChildren<Animator>();
// //     _animalCombat = GetComponent<AnimalCombat>();
// // }

//     private void Update()
//     {
//         if(!_animalCombat.AInCombat){
//         if (!inPos)
//         {
//              Move();
           
//         }else{
//          moveTimer -= Time.deltaTime;
//             if (moveTimer <= 0f)
//             {
//                 SetTargetPosition();
               
//                 moveTimer = moveDelay;
//             }
//             }
//         }else{
//             targetPosition = _animalCombat.ATarget.transform.position;
//             if(Vector3.Distance(transform.position,targetPosition) >= StopDistance){
//                 inPos = false;
//             }
//             if(!inPos){AttackMovement();}
            
            
//         }
//     }
//    // [ServerRpc(RequireOwnership = false)]   
//     private void SetTargetPosition()
//     {
//         Vector3 randomDirection;
//         if (UnityEngine.Random.value < 0.4f)
//         {
//             randomDirection = movementDirections[(Array.IndexOf(movementDirections, targetPosition - transform.position) + movementDirections.Length - 1) % movementDirections.Length];
//         }
//         else
//         {
//             randomDirection = movementDirections[UnityEngine.Random.Range(0, movementDirections.Length)];
//         }
//         // Check if the target position is blocked
//          if (Physics.Raycast(transform.position + Vector3.up * 0.5f, randomDirection, out RaycastHit hit, tileSize))
//         {
//             // If the target position is blocked, try to move in a different direction
//             randomDirection = movementDirections[UnityEngine.Random.Range(0, movementDirections.Length)];
//         }
//         Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
//                 transform.rotation = targetRotation;

//         // Calculate the target position
//         targetPosition = transform.position + randomDirection * tileSize;
//         targetPosition = new Vector3(Mathf.Round(targetPosition.x), Mathf.Round(targetPosition.y), Mathf.Round(targetPosition.z));
//         inPos = false;
//     }
// //[ServerRpc(RequireOwnership = false)]  
// private void Move()
// {
//     anim.SetBool("Walking", true);
//     Vector3 moveDirection = targetPosition - transform.position;

//     transform.position = Vector3.MoveTowards(transform.position, transform.position + moveDirection, TilemoveSpeed * Time.deltaTime);
//     //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), rotationSpeed * Time.deltaTime);

        

//     if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
//     {
//         inPos = true;
//         anim.SetBool("Walking", false);
//         transform.position = targetPosition; // Snap to the target position
//        // targetPosition = Vector3.zero;
//     }
// }
// //[ServerRpc(RequireOwnership = false)]  
// private void AttackMovement(){
//     anim.SetBool("Walking", true);
//     Vector3 moveDirection = (targetPosition - transform.position).normalized * StopDistance;

//     transform.position = Vector3.MoveTowards(transform.position, transform.position + moveDirection, TilemoveSpeed * Time.deltaTime);
//    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), rotationSpeed * Time.deltaTime);
//     if (Vector3.Distance(transform.position, targetPosition) <= StopDistance)
//     {
//         inPos = true;
//         anim.SetBool("Walking", false);
//     }
// }

// }
   


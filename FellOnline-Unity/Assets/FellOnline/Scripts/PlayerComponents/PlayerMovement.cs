// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// //using UnityEngine.InputSystem;
// using FishNet.Object;
// namespace FellOnline.Shared
// {
    

// public sealed class PlayerMovement : NetworkBehaviour
// {
//     [SerializeField]
//     private Animator anim;

// private Character fellCharacter;

//     [SerializeField]
//     private float moveSpeed = 4f;
//      [SerializeField]
//     private float rotationSpeed = 16f;
//      [SerializeField]
//     private float TilemoveSpeed = 5f;
//     private float tileSize = 1f;

//     private Vector2 movementInput;
//      private Vector2 newMovementInput = new Vector2(0, 0);
//     private Vector2 lastMovementInput;
//     private Vector3 targetPosition;
//     private Vector3 clickTargetPosition;
//     public bool moving;
//     public bool inPos;
//     private bool useX = false;
//     private bool movingDiagonally = false;
 
//  private float diagonallyTimer = 0f;
//         [SerializeField] private float diagonallySwitchTime = 1f;

// public override void OnStartNetwork(){
//     base.OnStartNetwork();
//     fellCharacter = GetComponent<Character>();
//     anim = GetComponentInChildren<Animator>();
// }
//     //  public void OnMove(InputAction.CallbackContext context)
//     // {
//     //     moving = true;
//     //     inPos = false;
//     //     // clickTargetPosition = Vector3.zero;
//     //     movementInput = context.ReadValue<Vector2>();
//     //     movementInput = new Vector2(Mathf.Round(movementInput.x), Mathf.Round(movementInput.y));
//     //     if (movementInput != new Vector2(0, 0))
//     //     {
//     //         lastMovementInput = movementInput;
//     //     }
//     //     if (movementInput.x != 0 && movementInput.y != 0)
//     //     {
//     //         movingDiagonally = true;
//     //     }else{
//     //         movingDiagonally = false;
//     //     }
//     // }
//     // public void OnClickToMove(InputAction.CallbackContext context)
//     // {
//     //     HandleClickToMove();
//     //      moving = true;
//     // }

//     private void Update()
//     {
//         if(!IsOwner) { return; }
//         if (movementInput == new Vector2(0, 0))
//         {
//             moving = false;
//             SnapToGrid();
//         }
//         else
//         {
//             if(movingDiagonally){
//                 diagonallyTimer -= Time.deltaTime;
//             if (diagonallyTimer <= 0f)
//             {
//                 diagonallyTimer = diagonallySwitchTime;
//                 useX = !useX;
//             }
//             }
//             Move();
            
//         }
//         if(clickTargetPosition != Vector3.zero)
//         {
//             clickToMove();
//         }

//         if (inPos)
//         {
//             anim.SetBool("Walking", false);
//         }
//         else
//         {
//             anim.SetBool("Walking", true);
//         }
//     }

//     private void Move()
//     {
//         moving = true;
        
//         clickTargetPosition = Vector3.zero;
        
        
            
        
//         if(movingDiagonally){
//             // Use x or y based on the current alternate state
//             if (useX)
//             {
//                 newMovementInput.y = 0f;
//                 newMovementInput.x = movementInput.x;
//             }
//             else
//             {
//                 newMovementInput.x = 0f;
//                 newMovementInput.y = movementInput.y;
//             }
//         }else{
//             newMovementInput = movementInput;
//         }



//         //wasd movement
//         Vector3 movement = new Vector3(newMovementInput.x, 0f, newMovementInput.y);
//         Vector3 newPosition = transform.position + (movement * tileSize);
        
//         // Rotate the character towards the move direction
//         if (movement != Vector3.zero)
//         {
//             Quaternion targetRotation = Quaternion.LookRotation(movement);
//             transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
//         }

//         // Check if the path to the new position is blocked
//         RaycastHit hit;
//         if (Physics.Raycast(transform.position + Vector3.up * 0.5f, movement, out hit, tileSize))
//         {
//             // Path is blocked, stop moving
//             return;
//         }
    
//         transform.position = Vector3.MoveTowards(transform.position, newPosition, TilemoveSpeed * Time.deltaTime);

       
       
//     }

//     void clickToMove()
//     {
//         //click to move
//         if (clickTargetPosition != Vector3.zero)
//         {
//             inPos = false;

//             // Calculate the movement direction
//             Vector3 movementDirection = clickTargetPosition - transform.position;
//             movementDirection.y = 0f;

//              // Rotate the character towards the move direction instantly
//             Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

//             // Check if the path to clickTargetPosition is blocked
//             RaycastHit hit;
//             if (Physics.Raycast(transform.position + Vector3.up * 0.5f, movementDirection, out hit, Vector3.Distance(transform.position, clickTargetPosition)))
//             {
//                 // Path is blocked, set clickTargetPosition to zero
//                 clickTargetPosition = Vector3.zero;
//                 return;
//             }

//             // Move in one direction first
//             Vector3 firstMovement = new Vector3(movementDirection.x, 0f, 0f);
//             Vector3 firstPosition = transform.position + (firstMovement * tileSize);
//             transform.position = Vector3.MoveTowards(transform.position, firstPosition, moveSpeed * Time.deltaTime);

//               // Rotate the character towards the move direction instantly
//             targetRotation = Quaternion.LookRotation(firstMovement);
//             transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
//             // Once reached, move in the other direction
//             if (transform.position == firstPosition)
//             {
//                 Vector3 secondMovement = new Vector3(0f, 0f, movementDirection.z);
//                 Vector3 secondPosition = firstPosition + (secondMovement * tileSize);
//                 transform.position = Vector3.MoveTowards(transform.position, secondPosition, moveSpeed * Time.deltaTime);

//                 targetRotation = Quaternion.LookRotation(secondMovement);
//              transform.rotation = targetRotation;
//             }

//             // Check if reached the final target position
//             if (transform.position == clickTargetPosition)
//             {
//                 inPos = true;
//                 moving = false;
//                 clickTargetPosition = Vector3.zero;
//             }
//         }
//     }
//     private void SnapToGrid()
//     {
//         if (!inPos && clickTargetPosition == Vector3.zero)
//         {
//             float x = 0;
//             float z = 0;
//             if (lastMovementInput.x < 0 || lastMovementInput.y < 0)
//             {
//                 x = Mathf.FloorToInt(transform.position.x / tileSize) * tileSize;
//                 z = Mathf.FloorToInt(transform.position.z / tileSize) * tileSize;
//             }
//             else
//             {
//                 x = Mathf.CeilToInt(transform.position.x / tileSize) * tileSize;
//                 z = Mathf.CeilToInt(transform.position.z / tileSize) * tileSize;
//             }

//             targetPosition = new Vector3(x, transform.position.y, z);
//             transform.position = Vector3.MoveTowards(transform.position, targetPosition, TilemoveSpeed * Time.deltaTime);
//             if (transform.position == targetPosition)
//             {
//                 inPos = true;
//                 moving = false;
//                 targetPosition = Vector3.zero;
                
//             }
//         }
//     }

//     private void HandleClickToMove()
//     {
//         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//         RaycastHit hit;
//         if (Physics.Raycast(ray, out hit))
//         {
//             inPos = false;

//             Vector3 destination = hit.point;
//             Vector3 tilePosition = new Vector3(Mathf.Round(destination.x / tileSize) * tileSize, 0, Mathf.Round(destination.z / tileSize) * tileSize);
//             clickTargetPosition = tilePosition;
//         }
//     }
// }
    
// }

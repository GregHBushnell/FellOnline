// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using FishNet.Object;
// public class CameraFollow : NetworkBehaviour
// {
//     [SerializeField]private Transform player;
//     [SerializeField] private Transform playerCam;        
//      [SerializeField]private float smoothSpeed = 0.125f;
//      [SerializeField] private Vector3 offset;


// public override void OnStartNetwork(){
//     base.OnStartNetwork();
// }
// public override void OnStartClient(){
//     base.OnStartClient();
//     playerCam.GetComponent<Camera>().enabled = IsOwner;
//     playerCam.GetComponent<AudioListener>().enabled = IsOwner;
//      playerCam.transform.parent = null;
// }
//         private void LateUpdate()
//         {
//             if(!IsOwner) { return; }
//             Vector3 desiredPosition = player.position + offset;
//             Vector3 smoothedPosition = Vector3.Lerp(playerCam.transform.position, desiredPosition, smoothSpeed);
//             //smoothedPosition.y = playerCam.transform.position.y; // Keep the camera's y position unchanged
//             playerCam.transform.position = smoothedPosition;
//         }
    

// }

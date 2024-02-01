
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

namespace FellOnline.Shared
{
    [RequireComponent(typeof(SceneObjectNamer))]
    public class WorldItem : Interactable
    {
        public WorldItemTemplate Item;
        public Vector3 ItemHiddenPosition = new Vector3(0, -1000, 0);

        // public readonly SyncVar<bool> PickedUp = new SyncVar<bool>(new SyncTypeSetting()
		// {
		// 	SendRate = 0.0f,
		// 	Channel = Channel.Unreliable,
		// 	ReadPermission = ReadPermission.Observers,
		// 	WritePermission = WritePermission.ServerOnly,
		// });


        #region ItemVisuals
        public Vector3 PosOffset = new Vector3(0,0.5f,0);
        public float Period = 2.2f;
        public float Amplitude = 0.08f;
        public float Steps = 100f;
        float RotationRate = 0f;
        float PhaseOffset;
        float Angle;
        Quaternion originalRot;
        Vector3 originalPos;
        public bool Rotates = false;
        #endregion
        
 
        // Start is called before the first frame update
        void Start()
        {
            originalPos = transform.position;
            PhaseOffset = Random.value;
            Angle = Random.value * 360f;
            originalRot = transform.rotation;
            RotationRate = Rotates ? Random.Range(0, 3) * 15f : 0f;
        }

        // Update is called once per frame
        void Update()
        {
           // if(PickedUp.Value == false){
                float phase = PhaseOffset + Time.time / Period;
                phase = 2.0f * Mathf.PI * (int)(phase * Steps) / Steps;
                float offset = Amplitude * Mathf.Cos(phase);
                Vector3 pos = offset * Vector3.up  + PosOffset;
                transform.position = new Vector3(originalPos.x,originalPos.y + pos.y,originalPos.z);
                Angle += RotationRate * Time.deltaTime;
                if (RotationRate != 0f)
                    transform.rotation = Quaternion.AngleAxis(Angle, Vector3.up) * originalRot;
           // }
            
        }
        // private void OnPickedUpChanged(bool prev, bool next, bool asServer)
        // {
        //    if(next == true){
        //        transform.position = ItemHiddenPosition;
        //    }else{
        //        transform.position = originalPos;}
        // }

 //#if !UNITY_SERVER
       // private void Awake()
		//{

           // PickedUp.OnChange += OnPickedUpChanged;
            // if (characterHairDatabase != null)
            // {
            //     string hairString = "Hair";
            //     foreach (CharacterHairTemplate hair in characterHairDatabase.Hairs.Values)
            //     {
            //         foreach (Transform child in hairVisualParent)
			// 		{
			// 			//mabey do add each child hair to a list to speed up for future hair changes
            //              //hairVisuals.Add(hair.ID, hairVisual);
			// 		}
                   
            //     }
            // }
	//	}

		//private void OnDestroy()
		//{
           // PickedUp.OnChange -= OnPickedUpChanged;
		//}

      
//#endif
        public override bool OnInteract(Character character)
		{
			if (Item == null ||
				!base.OnInteract(character))
			{
				return false;
			}
            
            Debug.Log("interact received");
#if UNITY_SERVER
			character.Owner.Broadcast(new WorldItemBroadcast()
			{
				interactableID = ID,
                templateID = Item.ID,
			}, true, Channel.Reliable);
#endif
			return true;


            
		}

    }
}
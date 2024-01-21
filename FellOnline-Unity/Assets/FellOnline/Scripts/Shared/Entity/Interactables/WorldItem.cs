// Copyright Elliot Bentine, 2018-
#if UNITY_SERVER 
using FishNet.Transporting;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
#endif
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

namespace FellOnline.Shared
{
    [RequireComponent(typeof(FSceneObjectNamer))]
    public class WorldItem : FInteractable
    {
        public WorldItemTemplate Item;
        public Vector3 ItemHiddenPosition = new Vector3(0, -1000, 0);
        
        public readonly SyncVar<bool> PickedUp = new SyncVar<bool>(new SyncTypeSetting()
		{
			SendRate = 0.0f,
			Channel = Channel.Reliable,
			ReadPermission = ReadPermission.Observers,
			WritePermission = WritePermission.ServerOnly,
		});

        #region ItemVisuals
        public Vector3 PosOffset = new Vector3(0,0.5f,0);
        public float Period = 2.2f;
        public float Amplitude = 0.08f;
        public float Steps = 100f;
        float RotationRate = 0f;
        float PhaseOffset;
        float Angle;
        Quaternion original;
        public bool Rotates = false;
        #endregion
        
        // #if !UNITY_SERVER
        // void Awake()
        // {
        //     PickedUp.Value = false;
        // }
        // #endif
        // Start is called before the first frame update
        void Start()
        {
            PhaseOffset = Random.value;
            Angle = Random.value * 360f;
            original = transform.rotation;
            RotationRate = Rotates ? Random.Range(0, 3) * 15f : 0f;
        }

        // Update is called once per frame
        void Update()
        {
            if (PickedUp.Value == false)
            {
                float phase = PhaseOffset + Time.time / Period;
                phase = 2.0f * Mathf.PI * (int)(phase * Steps) / Steps;
                float offset = Amplitude * Mathf.Cos(phase);
                transform.position = offset * Vector3.up  + PosOffset;

                Angle += RotationRate * Time.deltaTime;
                if (RotationRate != 0f)
                    transform.rotation = Quaternion.AngleAxis(Angle, Vector3.up) * original;
            }else{
                    
            }
        }

        public override bool OnInteract(Character character)
		{
			if (Item == null ||
				!base.OnInteract(character))
			{
				return false;
			}
            

#if UNITY_SERVER
            PickedUp.Value = true;
            transform.position = ItemHiddenPosition;
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
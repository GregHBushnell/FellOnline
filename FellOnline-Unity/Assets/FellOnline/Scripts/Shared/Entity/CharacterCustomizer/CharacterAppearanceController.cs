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
[RequireComponent(typeof(Character))]
public class CharacterAppearanceController : NetworkBehaviour
{
public Character Character;
public CharacterAppearanceDetails AppearanceDetails;
[SerializeField]
public readonly  SyncVar<CharacterAppearanceDetails> _appearanceDetails = new SyncVar<CharacterAppearanceDetails>(new SyncTypeSetting()
{
    SendRate = 0.0f,
    Channel = Channel.Unreliable,
    ReadPermission = ReadPermission.Observers,
    WritePermission = WritePermission.ServerOnly,
});

public CharacterHairDataBase characterHairDatabase = null;
public Transform hairVisualParent = null;


    void _OnappearanceDetailsChanged(CharacterAppearanceDetails prev, CharacterAppearanceDetails next, bool asServer)
    {
        Debug.Log("OnappearanceDetailsChanged"+ next.HairID.ToString() + " " + next.SkinColor.ToString() + " " + next.HairColor.ToString());
        if (prev.HairID != next.HairID)
        {
            if (characterHairDatabase != null)
            {
                string hairString = "Hair" + next.HairID.ToString();
                Transform hairTransform = hairVisualParent.Find(hairString);
                    if (hairTransform != null)
                    {
                        SkinnedMeshRenderer renderer = hairTransform.GetComponent<SkinnedMeshRenderer>();
                        if (renderer != null)
                        {
                            renderer.enabled = true;
                            Color hcolor = FHex.ToColor(next.HairColor.ToString());
                            hcolor.a = 1;
                             renderer.material.SetColor("_BaseColor",hcolor);
                        }else{
                            Debug.Log("New Hair Renderer  not found");
                        }
                    }else{
                        Debug.Log("New Hair not found");
                    }
            }
        }
        if(prev.SkinColor != next.SkinColor)
        {
             Color scolor = FHex.ToColor(next.SkinColor.ToString());
             scolor.a = 1;
             hairVisualParent.GetComponent<SkinnedMeshRenderer>().material.SetColor("_BaseColor",scolor);
        }
    }

#if !UNITY_SERVER
		public override void OnStartClient()
		{
			base.OnStartClient();

			if (!base.IsOwner)
			{
				enabled = false;
				return;
			}

			ClientManager.RegisterBroadcast<AppearanceUpdateBroadcast>(OnClientAppearanceUpdateBroadcastReceived);
		}

		public override void OnStopClient()
		{
			base.OnStopClient();

			if (base.IsOwner)
			{
				ClientManager.UnregisterBroadcast<AppearanceUpdateBroadcast>(OnClientAppearanceUpdateBroadcastReceived);

			}
		}

		/// <summary>
		/// Server sent a set item broadcast. Item slot is set to the received item details.
		/// </summary>
        private void OnClientAppearanceUpdateBroadcastReceived(AppearanceUpdateBroadcast msg, Channel channel)
        {
            CharacterAppearanceDetails appearance = new CharacterAppearanceDetails{
                SkinColor = msg.SkinColor,
                HairID = msg.HairID,
                HairColor = msg.HairColor,
            };
            AppearanceDetails = appearance;
            _appearanceDetails.Value = new CharacterAppearanceDetails(appearance.SkinColor,appearance.HairID,appearance.HairColor);
        }
#endif

    #if !UNITY_SERVER
private void Awake()
		{
			//HairID.OnChange += OnHairIDChanged;
            _appearanceDetails.OnChange += _OnappearanceDetailsChanged;
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
		}

		private void OnDestroy()
		{
            _appearanceDetails.OnChange -= _OnappearanceDetailsChanged;
		}
#endif
}

  
}
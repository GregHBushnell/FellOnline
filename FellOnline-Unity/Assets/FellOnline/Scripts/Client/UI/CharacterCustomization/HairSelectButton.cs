using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FellOnline.Shared;
using UnityEngine.EventSystems;
namespace FellOnline.Client
{
public class HairSelectButton : Button
{
   public const int NULL_REFERENCE_ID = -1;
   public int ReferenceID = NULL_REFERENCE_ID;

   [SerializeField]
	public Image HairIconImage;
    

        public UICharacterCreate characterCreateReference = null;


        public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);

			if (eventData.button == PointerEventData.InputButton.Left)
			{
                if(characterCreateReference != null)
                {
                    characterCreateReference.OnHairSelectionChanged(ReferenceID);
                }
				//characterCreate
			}
		}
}
}
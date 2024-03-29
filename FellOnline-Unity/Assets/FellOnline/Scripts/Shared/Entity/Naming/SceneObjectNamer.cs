﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FellOnline.Shared
{
	[ExecuteInEditMode]
	public class SceneObjectNamer : MonoBehaviour
	{
#if UNITY_EDITOR
		
		public bool randomiseName = false;
		public NameCache Template;
		public string Name;

		[SerializeField]
		private bool HasName = false;

		protected void Update()
		{
			if (HasName)
			{
				return;
			}
			if(randomiseName){
				if (Template == null ||
					Template.Names == null ||
					Template.Names.Count < 1)
				{
					return;
				}
				System.Random r = new System.Random();
				int j = r.Next(0, Template.Names.Count);
				string characterName = Template.Names[j];
				
				string surname = "";
				Merchant merchant = gameObject.GetComponent<Merchant>();
				if (merchant != null)
				{
					surname = "the Merchant";
				}
				else
				{
					AbilityCrafter abilityCrafter = gameObject.GetComponent<AbilityCrafter>();
					if (abilityCrafter != null)
					{
						surname = "the Ability Crafter";
					}
				}
				this.gameObject.name = (characterName + " " + surname).Trim();
				HasName = true;
			}else if(Name != null && Name != ""){
				string oName = Name;
				this.gameObject.name = (oName).Trim();
			}
			EditorUtility.SetDirty(this);
		}
#endif
	}
}
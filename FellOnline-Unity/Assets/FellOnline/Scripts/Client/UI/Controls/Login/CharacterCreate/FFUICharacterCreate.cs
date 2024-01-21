using FishNet.Transporting;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Object;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class FUICharacterCreate : FUIControl
	{
		public Button createButton;
		public TMP_Text createResultText;
		public TMP_Dropdown startRaceDropdown;
		public TMP_Dropdown startLocationDropdown;
		public RectTransform characterParent;

		public string characterName = "";
		public int raceIndex = -1;
		public List<string> initialRaceNames = new List<string>();
		public List<string> initialSpawnLocationNames = new List<string>();
	
		public FWorldSceneDetailsCache worldSceneDetailsCache = null;
		public int selectedSpawnPosition = -1;

		#region CharacterCustomization
		public UICharacterCreation SkinColor;
		public CharacterHairDataBase characterHairDatabase = null;
		//public List<uint> HairOptions = new List<uint>();
		public HairSelectButton HairOptionButtonPrefab = null;
		public Transform HairVisualParent = null;
		public RectTransform hairButtonParent = null;
		public List<int> HairOptions = new List<int>();
		public int hairIndexUI = 0;

		public int SkinColorInt = 16777215;
		public int HairColorInt = 16777215;
		#endregion

		public override void OnStarting()
		{
			if (startRaceDropdown != null &&
				initialRaceNames != null)
			{
				initialRaceNames.Clear();

				for (int i = 0; i < Client.NetworkManager.SpawnablePrefabs.GetObjectCount(); ++i)
				{
					NetworkObject prefab = Client.NetworkManager.SpawnablePrefabs.GetObject(true, i);
					if (prefab != null)
					{
						Character character = prefab.gameObject.GetComponent<Character>();
						if (character != null)
						{
							initialRaceNames.Add(character.gameObject.name);
						}
					}
				}

				startRaceDropdown.ClearOptions();
				startRaceDropdown.AddOptions(initialRaceNames);
				raceIndex = startRaceDropdown.value;
			}


			if (startLocationDropdown != null &&
				initialSpawnLocationNames != null &&
				worldSceneDetailsCache != null &&
				worldSceneDetailsCache.Scenes != null)
			{
				initialSpawnLocationNames.Clear();
				foreach (FWorldSceneDetails details in worldSceneDetailsCache.Scenes.Values)
				{
					foreach (FCharacterInitialSpawnPosition initialSpawnLocation in details.InitialSpawnPositions.Values)
					{

						initialSpawnLocationNames.Add(initialSpawnLocation.SpawnerName);
					}
				}
				startLocationDropdown.ClearOptions();
				startLocationDropdown.AddOptions(initialSpawnLocationNames);
				selectedSpawnPosition = startLocationDropdown.value;


				if (HairOptionButtonPrefab != null)
				{
					foreach (CharacterHairTemplate hair in characterHairDatabase.Hairs.Values)
					{
						if (hair != null)
						{
							HairSelectButton button = Instantiate(HairOptionButtonPrefab, hairButtonParent);
							button.ReferenceID = hair.HairID;
							button.HairIconImage.sprite = hair.HairImage;
							button.characterCreateReference = this;
							HairOptions.Add(hair.HairID);
						}
					}
				}

				if(HairVisualParent != null){
					foreach (Transform child in HairVisualParent)
					{
						int id = 1;
						if(id> HairOptions.Count){
							break;
						}
						string hairID = "Hair" + HairOptions[id];
						if (child.name == hairID)
						{
							SkinnedMeshRenderer renderer = child.GetComponent<SkinnedMeshRenderer>();
							if (renderer != null)
							{
								renderer.enabled = false;
								id++;
							}
						}
						
					}
					}
			}

			Client.NetworkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
			Client.NetworkManager.ClientManager.RegisterBroadcast<CharacterCreateResultBroadcast>(OnClientCharacterCreateResultBroadcastReceived);
		}


		public override void OnDestroying()
		{
			Client.NetworkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
			Client.NetworkManager.ClientManager.UnregisterBroadcast<CharacterCreateResultBroadcast>(OnClientCharacterCreateResultBroadcastReceived);
		}

		private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
		{
			if (obj.ConnectionState == LocalConnectionState.Stopped)
			{
				Hide();
			}
		}

	private void OnClientCharacterCreateResultBroadcastReceived(CharacterCreateResultBroadcast msg, Channel channel)
		{
			SetCreateButtonLocked(false);
			if (msg.result == CharacterCreateResult.Success)
			{
				Hide();
				FUIManager.Show("UICharacterSelect");
			}
			else if (createResultText != null)
			{
				createResultText.text = msg.result.ToString();
			}
		}

		public void OnCharacterNameChangeEndEdit(TMP_InputField inputField)
		{
			characterName = inputField.text;
		}

		public void OnSpawnLocationDropdownValueChanged(TMP_Dropdown dropdown)
		{
			selectedSpawnPosition = dropdown.value;
		}
		public void OnRaceDropdownValueChanged(TMP_Dropdown dropdown)
		{
			raceIndex = dropdown.value;
		}

		public void OnSkinColorChanged(int colorint)
		{
			SkinColorInt = colorint;
			
		}
		public void OnHairColorChanged(int colorint)
		{
			HairColorInt = colorint;
			
		}


		public void OnHairSelectionChanged(int id)
		{
			//disable current hair
			string hairID = "Hair" + hairIndexUI;
			Transform hairObject = HairVisualParent.Find(hairID);
			if (hairObject != null)
			{
				SkinnedMeshRenderer renderer = hairObject.GetComponent<SkinnedMeshRenderer>();
				if (renderer != null)
				{
					renderer.enabled = false;
				}
			}


			hairIndexUI = id;
			Debug.Log("Hair index is " + hairIndexUI);


			//enable new hair
			hairID = "Hair" + id;
			hairObject = HairVisualParent.Find(hairID);
			if (hairObject != null)
			{
				SkinnedMeshRenderer renderer = hairObject.GetComponent<SkinnedMeshRenderer>();
				if (renderer != null)
				{
					renderer.enabled = true;
				}
			}
		}
		public void OnClick_CreateCharacter()
		{
			Debug.Log("Clicked Create Character");
			Debug.Log("RaceIndex:"+raceIndex);
			Debug.Log("Selected Spawn Pos:"+selectedSpawnPosition);
			Debug.Log("HairIndex:"+hairIndexUI);
				if (worldSceneDetailsCache == null)
				{
					Debug.LogError("WorldSceneDetailsCache is null!");
				}

			if (Client.IsConnectionReady() &&
			Constants.Authentication.IsAllowedCharacterName(characterName) &&
				worldSceneDetailsCache != null &&
				raceIndex > -1 &&
				selectedSpawnPosition > -1 &&
				hairIndexUI > -1
				)
			{
				Debug.Log("Creating Character");
				foreach (FWorldSceneDetails details in worldSceneDetailsCache.Scenes.Values)
				{
					if (details.InitialSpawnPositions.TryGetValue(initialSpawnLocationNames[selectedSpawnPosition], out FCharacterInitialSpawnPosition spawnPosition))
					{
						Debug.Log("Create Character");
						// create character
						Client.NetworkManager.ClientManager.Broadcast(new CharacterCreateBroadcast()
						{
							characterName = characterName,
							raceIndex = raceIndex,
							hairIndex = hairIndexUI,
							initialSpawnPosition = spawnPosition,
							skinColor = SkinColorInt,
							hairColor = HairColorInt,
							

						}, Channel.Reliable);
						SetCreateButtonLocked(true);
						Debug.Log("Test");
						return;
					}
				}
			}
		}

		public void OnClick_QuitToLogin()
		{
			// we should go back to login..
			Client.QuitToLogin();
		}

		public void OnClick_Quit()
		{
			Client.Quit();
		}

		private void SetCreateButtonLocked(bool locked)
		{
			createButton.interactable = !locked;
		}
	}
}
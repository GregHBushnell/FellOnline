using FishNet.Transporting;
using System;
using System.Collections.Generic;
#if !UNITY_EDITOR
using System.IO;
#endif
using FellOnline.Shared;

namespace FellOnline.Client
{
	public static class FClientNamingSystem
	{
		internal static Client Client;

		private static Dictionary<FNamingSystemType, Dictionary<long, string>> idToName = new Dictionary<FNamingSystemType, Dictionary<long, string>>();
		// character names are unique so we can presume this works properly
		private static Dictionary<string, long> nameToID = new Dictionary<string, long>();
		private static Dictionary<FNamingSystemType, Dictionary<long, Action<string>>> pendingNameRequests = new Dictionary<FNamingSystemType, Dictionary<long, Action<string>>>();

		public static void InitializeOnce(Client client)
		{
			if (client == null)
			{
				return;
			}

			Client = client;

			Client.NetworkManager.ClientManager.RegisterBroadcast<FNamingBroadcast>(OnClientNamingBroadcastReceived);

#if !UNITY_EDITOR
			string workingDirectory = Client.GetWorkingDirectory();
			foreach (FNamingSystemType type in FEnumExtensions.ToArray<FNamingSystemType>())
			{
				idToName[type] = FDictionaryExtensions.ReadFromGZipFile(Path.Combine(workingDirectory, type.ToString() + ".bin"));
			}

			Dictionary<long, string> characterNames = idToName[FNamingSystemType.CharacterName];
			if (characterNames != null && characterNames.Count > 0)
			{
				foreach (KeyValuePair<long, string> pair in characterNames)
				{
					nameToID[pair.Value] = pair.Key;
				}
			}
#endif
		}

		public static void Destroy()
		{
			if (Client != null)
			{
				Client.NetworkManager.ClientManager.UnregisterBroadcast<FNamingBroadcast>(OnClientNamingBroadcastReceived);
			}

#if !UNITY_EDITOR
			string workingDirectory = Client.GetWorkingDirectory();
			foreach (KeyValuePair<FNamingSystemType, Dictionary<long, string>> pair in idToName)
			{
				pair.Value.WriteToGZipFile(Path.Combine(workingDirectory, pair.Key.ToString() + ".bin"));
			}
#endif
		}

		/// <summary>
		/// Checks if the name matching the ID and type are known. If they are not the value will be retreived from the server and set at a later time.
		/// Values learned this way are saved to the clients hard drive when the game closes and loaded when the game loads.
		/// </summary>
		public static void SetName(FNamingSystemType type, long id, Action<string> action)
		{
			if (!idToName.TryGetValue(type, out Dictionary<long, string> typeNames))
			{
				idToName.Add(type, typeNames = new Dictionary<long, string>());
			}
			if (typeNames.TryGetValue(id, out string name))
			{
				//UnityEngine.Debug.Log("Found Name for: " + id + ":" + name);
				action?.Invoke(name);
			}
			else if (Client != null)
			{
				if (!pendingNameRequests.TryGetValue(type, out Dictionary<long, Action<string>> pendingActions))
				{
					pendingNameRequests.Add(type, pendingActions = new Dictionary<long, Action<string>>());
				}
				if (!pendingActions.ContainsKey(id))
				{
					pendingActions.Add(id, action);

					//UnityEngine.Debug.Log("Requesting Name for: " + id);

					// send the request to the server to get a name
					Client.NetworkManager.ClientManager.Broadcast(new FNamingBroadcast()
					{
						type = type,
						id = id,
						name = "",
					}, Channel.Reliable);
				}
				else
				{
					//UnityEngine.Debug.Log("Adding pending Name for: " + id);
					pendingActions[id] += action;
				}
			}
		}

		public static bool GetCharacterID(string name, out long id)
		{
			return nameToID.TryGetValue(name, out id);
		}

		private static void OnClientNamingBroadcastReceived(FNamingBroadcast msg, Channel channel)
		{
			if (pendingNameRequests.TryGetValue(msg.type, out Dictionary<long, Action<string>> pendingRequests))
			{
				if (pendingRequests.TryGetValue(msg.id, out Action<string> pendingActions))
				{
					//UnityEngine.Debug.Log("Processing Name for: " + msg.id + ":" + msg.name);

					pendingActions?.Invoke(msg.name);
					pendingRequests[msg.id] = null;
					pendingRequests.Remove(msg.id);
				}
			}
			if (!idToName.TryGetValue(msg.type, out Dictionary<long, string> knownNames))
			{
				idToName.Add(msg.type, knownNames = new Dictionary<long, string>());
			}
			knownNames[msg.id] = msg.name;
			nameToID[msg.name] = msg.id;
		}
	}
}

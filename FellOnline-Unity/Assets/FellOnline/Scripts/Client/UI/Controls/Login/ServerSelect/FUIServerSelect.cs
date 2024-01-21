﻿using FishNet.Transporting;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class FUIServerSelect : FUIControl
	{
		public Button connectToServerButton;
		public Button refreshButton;
		public RectTransform serverParent;
		public FServerDetailsButton serverButtonPrefab;

		private List<FServerDetailsButton> serverList = new List<FServerDetailsButton>();
		private FServerDetailsButton selectedServer;

		public float RefreshRate = 5.0f;
		private float nextRefresh = 0.0f;

		public override void OnStarting()
		{
			nextRefresh = RefreshRate;

			Client.NetworkManager.ClientManager.RegisterBroadcast<ServerListBroadcast>(OnClientServerListBroadcastReceived);
			Client.NetworkManager.ClientManager.RegisterBroadcast<WorldSceneConnectBroadcast>(OnClientWorldSceneConnectBroadcastReceived);

			Client.LoginAuthenticator.OnClientAuthenticationResult += Authenticator_OnClientAuthenticationResult;
		}


		public override void OnDestroying()
		{
			Client.NetworkManager.ClientManager.UnregisterBroadcast<ServerListBroadcast>(OnClientServerListBroadcastReceived);
			Client.NetworkManager.ClientManager.UnregisterBroadcast<WorldSceneConnectBroadcast>(OnClientWorldSceneConnectBroadcastReceived);

			Client.LoginAuthenticator.OnClientAuthenticationResult -= Authenticator_OnClientAuthenticationResult;

			DestroyServerList();
		}

		void Update()
		{
			if (nextRefresh > 0.0f)
			{
				nextRefresh -= Time.deltaTime;
			}
		}

		private void Authenticator_OnClientAuthenticationResult(FClientAuthenticationResult result)
		{
			switch (result)
			{
				case FClientAuthenticationResult.InvalidUsernameOrPassword:
					break;
				case FClientAuthenticationResult.AlreadyOnline:
					break;
				case FClientAuthenticationResult.Banned:
					break;
				case FClientAuthenticationResult.LoginSuccess:
					break;
				case FClientAuthenticationResult.WorldLoginSuccess:
					break;
				case FClientAuthenticationResult.SceneLoginSuccess:
					SetConnectToServerLocked(false);
					Hide();
					break;
				case FClientAuthenticationResult.ServerFull:
					break;
				default:
					break;
			}
		}

		public void DestroyServerList()
		{
			if (serverList != null)
			{
				for (int i = 0; i < serverList.Count; ++i)
				{
					serverList[i].OnServerSelected -= OnServerSelected;
					Destroy(serverList[i].gameObject);
				}
				serverList.Clear();
			}
		}

		private void OnClientServerListBroadcastReceived(ServerListBroadcast msg, Channel channel)
		{
			if (msg.servers != null)
			{
				DestroyServerList();

				serverList = new List<FServerDetailsButton>();
				for (int i = 0; i < msg.servers.Count; ++i)
				{
					FServerDetailsButton newServer = Instantiate(serverButtonPrefab, serverParent);
					newServer.Initialize(msg.servers[i]);
					newServer.OnServerSelected += OnServerSelected;
					serverList.Add(newServer);
				}
			}

			Show();
		}

	private void OnClientWorldSceneConnectBroadcastReceived(WorldSceneConnectBroadcast msg, Channel channel)
		{
			if (Client.IsConnectionReady() &&
				selectedServer != null)
			{
				// connect to the scene server
				Client.ConnectToServer(msg.address, msg.port);

				SetConnectToServerLocked(true);
			}
		}

		private void OnServerSelected(FServerDetailsButton button)
		{
			FServerDetailsButton prevButton = selectedServer;
			if (prevButton != null)
			{
				prevButton.SetLabelColors(Color.black);
			}

			selectedServer = button;
			if (selectedServer != null)
			{
				selectedServer.SetLabelColors(Color.green);
			}
		}

		public void OnClick_ConnectToServer()
		{
			if (Client.IsConnectionReady() &&
				selectedServer != null)
			{
				// connect to the world server
				Client.ConnectToServer(selectedServer.Details.Address, selectedServer.Details.Port);

				SetConnectToServerLocked(true);
			}
		}

		public void OnClick_Refresh()
		{
			// TODO -- there should be a timer on the server too
			if (nextRefresh < 0)
			{
				nextRefresh = RefreshRate;

				// request an updated server list
				RequestServerListBroadcast requestServerList = new RequestServerListBroadcast();
				Client.NetworkManager.ClientManager.Broadcast(requestServerList, Channel.Reliable);
			}
		}

		public void OnClick_QuitToLogin()
		{
			StopAllCoroutines();

			// we should go back to login..
			Client.QuitToLogin();
		}

		public void OnClick_Quit()
		{
			StopAllCoroutines();
			Client.Quit();
		}

		/// <summary>
		/// Sets locked state for signing in.
		/// </summary>
		private void SetConnectToServerLocked(bool locked)
		{
			connectToServerButton.interactable = !locked;
		}
	}
}
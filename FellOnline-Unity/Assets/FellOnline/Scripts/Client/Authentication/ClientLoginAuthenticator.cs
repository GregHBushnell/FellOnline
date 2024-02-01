﻿using FishNet.Authenticating;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Managing;
using FishNet.Transporting;
using System;
using System.Security.Cryptography;
using UnityEngine;
using SecureRemotePassword;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class ClientLoginAuthenticator : Authenticator
	{
		private string username = "";
		private string password = "";
		private bool register;
		private ClientSrpData SrpData;

		/// <summary>
		/// Client authentication event. Subscribe to this if you want something to happen after receiving authentication result from the server.
		/// </summary>
		public event Action<ClientAuthenticationResult> OnClientAuthenticationResult;

		/// <summary>
		/// We override this but never use it on the client...
		/// </summary>
#pragma warning disable CS0067
		public override event Action<NetworkConnection, bool> OnAuthenticationResult;
#pragma warning restore CS0067

		public Client Client { get; private set; }

		public override void InitializeOnce(NetworkManager networkManager)
		{
			base.InitializeOnce(networkManager);

			base.NetworkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
			base.NetworkManager.ClientManager.RegisterBroadcast<SrpVerifyBroadcast>(OnClientSrpVerifyBroadcastReceived);
			base.NetworkManager.ClientManager.RegisterBroadcast<SrpProofBroadcast>(OnClientSrpProofBroadcastReceived);
			base.NetworkManager.ClientManager.RegisterBroadcast<ClientAuthResultBroadcast>(OnClientAuthResultBroadcastReceived);
		}

		public void SetClient(Client client)
		{
			Client = client;
		}

		/// <summary>
		/// Initial sign in to the login server.
		/// </summary>
		public void SetLoginCredentials(string username, string password, bool register = false)
		{
			this.username = username;
			this.password = password;
			this.register = register;
		}

		/// <summary>
		/// Called when a connection state changes for the local client.
		/// We wait for the connection to be ready before proceeding with authentication.
		/// </summary>
		private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
		{
			/* If anything but the started state then exit early.
			 * Only try to authenticate on started state. The server
			* doesn't have to send an authentication request before client
			* can authenticate, that is entirely optional and up to you. In this
			* example the client tries to authenticate soon as they connect. */
			if (args.ConnectionState != LocalConnectionState.Started)
				return;

			SrpData = new ClientSrpData(SrpParameters.Create2048<SHA512>());

			// register a new account?
			if (register)
			{
				SrpData.GetSaltAndVerifier(username, password, out string salt, out string verifier);
				CreateAccountBroadcast msg = new CreateAccountBroadcast()
				{
					username = this.username,
					salt = salt,
					verifier = verifier,
				};
				Client.Broadcast(msg, Channel.Reliable);
			}
			else
			{
				SrpVerifyBroadcast msg = new SrpVerifyBroadcast()
				{
					s = this.username,
					publicEphemeral = SrpData.ClientEphemeral.Public,
				};
				Client.Broadcast(msg, Channel.Reliable);
			}
		}

		private void OnClientSrpVerifyBroadcastReceived(SrpVerifyBroadcast msg, Channel channel)
		{
			if (SrpData == null)
			{
				return;
			}

			if (SrpData.GetProof(this.username, this.password, msg.s, msg.publicEphemeral, out string proof))
			{
				Client.Broadcast(new SrpProofBroadcast()
				{
					proof = proof,
				}, Channel.Reliable);
			}
			else
			{
				Client.ForceDisconnect();
			}
			//Debug.Log("Srp: " + proof);
		}

		private void OnClientSrpProofBroadcastReceived(SrpProofBroadcast msg, Channel channel)
		{
			if (SrpData == null)
			{
				return;
			}

			if (SrpData.Verify(msg.proof, out string result))
			{
				Client.Broadcast(new SrpSuccess(), Channel.Reliable);
			}
			else
			{
				Client.ForceDisconnect();
			}
			//Debug.Log("Srp: " + result);
		}

		/// <summary>
		/// Received on client after server sends an authentication response.
		/// </summary>
		private void OnClientAuthResultBroadcastReceived(ClientAuthResultBroadcast msg, Channel channel)
		{
			// invoke result on the client
			OnClientAuthenticationResult(msg.result);

			if (NetworkManagerExtensions.CanLog(LoggingType.Common))
				Debug.Log(msg.result);
		}
	}
}
﻿using System.Security;
using SecureRemotePassword;

namespace FellOnline.Server
{
	public class FServerSrpData
	{
		public string UserName { get; private set; }
		public string PublicClientEphemeral { get; private set; }
		public SrpServer SrpServer { get; private set; }
		public string Salt { get; private set; }
		public string Verifier { get; private set; }
		public SrpEphemeral ServerEphemeral { get; private set; }
		public SrpSession Session { get; private set; }
		public FSrpState State { get; set; }

		public FServerSrpData(SrpParameters parameters, string username, string publicClientEphemeral, string salt, string verifier)
		{
			UserName = username;
			PublicClientEphemeral = publicClientEphemeral;
			SrpServer = new SrpServer(parameters);
			this.Salt = salt;
			this.Verifier = verifier;
			ServerEphemeral = SrpServer.GenerateEphemeral(this.Verifier);
			State = FSrpState.SrpVerify;
		}

		public bool GetProof(string proof, out string serverProof)
		{
			try
			{
				Session = SrpServer.DeriveSession(ServerEphemeral.Secret,
												  PublicClientEphemeral,
												  Salt,
												  UserName,
												  Verifier,
												  proof);
				serverProof = Session.Proof;
				return true;
			}
			catch (SecurityException e)
			{
				serverProof = e.Message;
				return false;
			}
		}
	}
}
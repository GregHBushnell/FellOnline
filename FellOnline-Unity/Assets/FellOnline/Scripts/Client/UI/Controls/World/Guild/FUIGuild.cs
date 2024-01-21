using FishNet.Transporting;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class FUIGuild : FUICharacterControl
	{
		public TMP_Text GuildLabel;
		public RectTransform GuildMemberParent;
		public FUIGuildMember GuildMemberPrefab;
		public Dictionary<long, FUIGuildMember> Members = new Dictionary<long, FUIGuildMember>();

		public override void OnDestroying()
		{
			OnLeaveGuild();
		}

		public void OnLeaveGuild()
		{
			if (GuildLabel != null)
			{
				GuildLabel.text = "Guild";
			}
			foreach (FUIGuildMember member in new List<FUIGuildMember>(Members.Values))
			{
				Destroy(member.gameObject);
			}
			Members.Clear();
		}

		public void OnGuildAddMember(long characterID, GuildRank rank, string location)
		{
			if (GuildMemberPrefab != null && GuildMemberParent != null)
			{
				if (!Members.TryGetValue(characterID, out FUIGuildMember guildMember))
				{
					Members.Add(characterID, guildMember = Instantiate(GuildMemberPrefab, GuildMemberParent));
				}
				if (guildMember.Name != null)
				{
					FClientNamingSystem.SetName(FNamingSystemType.CharacterName, characterID, (n) =>
					{
						guildMember.Name.text = n;
					});
				}
				if (guildMember.Rank != null)
					guildMember.Rank.text = rank.ToString();
				if (guildMember.Location != null)
					guildMember.Location.text = location;
			}
		}

		public void OnGuildRemoveMember(long characterID)
		{
			if (Members.TryGetValue(characterID, out FUIGuildMember member))
			{
				Members.Remove(characterID);
				Destroy(member.gameObject);
			}
		}

		public void OnButtonCreateGuild()
		{
					if (Character != null && Character.GuildController.ID.Value < 1 && Client.NetworkManager.IsClientStarted)
			{
				if (FUIManager.TryGet("UIInputConfirmationTooltip", out FUIInputConfirmationTooltip tooltip))
				{
					tooltip.Open("Please type the name of your new guild!", (s) =>
					{
						if (Constants.Authentication.IsAllowedGuildName(s))
						{
							Client.NetworkManager.ClientManager.Broadcast(new GuildCreateBroadcast()
							{
								guildName = s,
							}, Channel.Reliable);
						}
					}, null);
				}
			}
		}

		public void OnButtonLeaveGuild()
		{
			if (Character != null && Character.GuildController.ID.Value > 0 && Client.NetworkManager.IsClientStarted)
			{
				if (FUIManager.TryGet("UIConfirmationTooltip", out FUIConfirmationTooltip tooltip))
				{
					tooltip.Open("Are you sure you want to leave your guild?", () =>
					{
						Client.NetworkManager.ClientManager.Broadcast(new GuildLeaveBroadcast(), Channel.Reliable);
					}, null);
				}
			}
		}

		public void OnButtonInviteToGuild()
		{
		if (Character != null && Character.GuildController.ID.Value > 0 && Client.NetworkManager.IsClientStarted)
			{
				if (Character.TargetController.Current.Target != null)
				{
					Character targetCharacter = Character.TargetController.Current.Target.GetComponent<Character>();
					if (targetCharacter != null)
					{
						Client.NetworkManager.ClientManager.Broadcast(new GuildInviteBroadcast()
						{
						targetCharacterID = targetCharacter.ID.Value
						}, Channel.Reliable);
					}
				}
			}
		}
	}
}
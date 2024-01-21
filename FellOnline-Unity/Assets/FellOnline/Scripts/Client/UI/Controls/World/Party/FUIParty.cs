using FishNet.Transporting;
using System.Collections.Generic;
using UnityEngine;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class UIParty : FUICharacterControl
	{
		public FCharacterAttributeTemplate HealthTemplate;
		public RectTransform PartyMemberParent;
		public FUIPartyMember PartyMemberPrefab;
		public Dictionary<long, FUIPartyMember> Members = new Dictionary<long, FUIPartyMember>();

		public override void OnDestroying()
		{
			foreach (FUIPartyMember member in new List<FUIPartyMember>(Members.Values))
			{
				Destroy(member.gameObject);
			}
			Members.Clear();
		}

		public void OnPartyCreated(string location)
		{
			if (Character != null && PartyMemberPrefab != null && PartyMemberParent != null)
			{
				FUIPartyMember member = Instantiate(PartyMemberPrefab, PartyMemberParent);
				if (member != null)
				{
					if (member.Name != null)
						member.Name.text = Character.CharacterName;
					if (member.Rank != null)
						member.Rank.text = "Rank: " + Character.PartyController.Rank.ToString();
					if (member.Health != null)
						member.Health.value = Character.AttributeController.GetResourceAttributeCurrentPercentage(HealthTemplate);
					Members.Add(Character.ID.Value, member);
				}
			}
		}

		public void OnLeaveParty()
		{
			foreach (FUIPartyMember member in new List<FUIPartyMember>(Members.Values))
			{
				Destroy(member.gameObject);
			}
			Members.Clear();
		}

		public void OnPartyAddMember(long characterID, PartyRank rank, float healthPCT)
		{
			if (PartyMemberPrefab != null && PartyMemberParent != null)
			{
				if (!Members.TryGetValue(characterID, out FUIPartyMember partyMember))
				{
					Members.Add(characterID, partyMember = Instantiate(PartyMemberPrefab, PartyMemberParent));
				}
				if (partyMember.Name != null)
				{
					FClientNamingSystem.SetName(FNamingSystemType.CharacterName, characterID, (n) =>
					{
						partyMember.Name.text = n;
					});
				}
				if (partyMember.Rank != null)
					partyMember.Rank.text = "Rank: " + rank.ToString();
				if (partyMember.Health != null)
					partyMember.Health.value = healthPCT;
			}
		}

		public void OnPartyRemoveMember(long characterID)
		{
			if (Members.TryGetValue(characterID, out FUIPartyMember member))
			{
				Members.Remove(characterID);
				Destroy(member.gameObject);
			}
		}

		public void OnButtonCreateParty()
		{
			if (Character != null && Character.PartyController.ID < 1)
			{
				Client.NetworkManager.ClientManager.Broadcast(new PartyCreateBroadcast(), Channel.Reliable);
			}
		}

		public void OnButtonLeaveParty()
		{
			if (Character != null && Character.PartyController.ID > 0)
			{
				if (FUIManager.TryGet("UIConfirmationTooltip", out FUIConfirmationTooltip tooltip))
				{
					tooltip.Open("Are you sure you want to leave your party?", () =>
					{
						Client.NetworkManager.ClientManager.Broadcast(new PartyLeaveBroadcast(), Channel.Reliable);
					}, null);
				}
			}
		}

		public void OnButtonInviteToParty()
		{
		if (Character != null && Character.PartyController.ID > 0 && Client.NetworkManager.IsClientStarted)
			{
				if (Character.TargetController.Current.Target != null)
				{
					Character targetCharacter = Character.TargetController.Current.Target.GetComponent<Character>();
					if (targetCharacter != null)
					{
						Client.NetworkManager.ClientManager.Broadcast(new PartyInviteBroadcast()
						{
							targetCharacterID = targetCharacter.ID.Value,
						}, Channel.Reliable);
					}
				}
				else if (FUIManager.TryGet("UIInputConfirmationTooltip", out FUIInputConfirmationTooltip tooltip))
				{
					tooltip.Open("Please type the name of the person you wish to invite.", (s) =>
					{
						if (Constants.Authentication.IsAllowedCharacterName(s) &&
							FClientNamingSystem.GetCharacterID(s, out long id))
						{
							Client.NetworkManager.ClientManager.Broadcast(new PartyInviteBroadcast()
							{
								targetCharacterID = id,
							}, Channel.Reliable);
						}
						else if (FUIManager.TryGet("UIChat", out FUIChat chat))
						{
							chat.InstantiateChatMessage(ChatChannel.System, "", "A person with that name could not be found. Are you sure you have encountered them?");
						}
					}, null);
				}
			}
		}
	}
}
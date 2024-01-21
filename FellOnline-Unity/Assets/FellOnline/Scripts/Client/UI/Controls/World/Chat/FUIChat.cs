using FishNet.Transporting;
using UnityEngine;
using System;
using System.Collections.Generic;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class FUIChat : FUICharacterControl, IChatHelper
	{
		public const int MAX_LENGTH = 128;

		public string WelcomeMessage = "Welcome to " + Constants.Configuration.ProjectName + "!\r\nChat channels are available.";
		public Transform chatViewParent;
		public FUIChatMessage chatMessagePrefab;
		public Transform chatTabViewParent;
		public FChatTab chatTabPrefab;

		public Dictionary<string, string> ErrorCodes = new Dictionary<string, string>()
		{
			{ FChatHelper.ERROR_TARGET_OFFLINE, " is not online." },
			{ FChatHelper.ERROR_MESSAGE_SELF, "... Are you messaging yourself again?" },
		};

		public FUIChatChannelColorDictionary ChannelColors = new FUIChatChannelColorDictionary()
		{
			{ ChatChannel.Say,		Color.white },
			{ ChatChannel.World,	Color.cyan },
			{ ChatChannel.Region,	Color.blue },
			{ ChatChannel.Party,	Color.red },
			{ ChatChannel.Guild,	Color.green},
			{ ChatChannel.Tell,		Color.magenta },
			{ ChatChannel.Trade,	Color.black },
			{ ChatChannel.System,	Color.yellow },
		};

		public List<FChatTab> initialTabs = new List<FChatTab>();
		public Dictionary<string, FChatTab> tabs = new Dictionary<string, FChatTab>();
		public string currentTab = "";

		public delegate void ChatMessageChange(FUIChatMessage message);
		public event ChatMessageChange OnMessageAdded;
		public event ChatMessageChange OnMessageRemoved;
		public List<FUIChatMessage> messages = new List<FUIChatMessage>();
		public bool AllowRepeatMessages = false;
		[Tooltip("The rate at which messages can be sent in milliseconds.")]
		public float MessageRateLimit = 0.0f;

		private ChatChannel previousChannel = ChatChannel.Command;
		private string previousName = "";

		public override void OnStarting()
		{
			FChatHelper.InitializeOnce(GetChannelCommand);

			if (initialTabs != null && initialTabs.Count > 0)
			{
				// activate the first tab
				ActivateTab(initialTabs[0]);
				// add all the tabs to our list and add our events
				foreach (FChatTab tab in initialTabs)
				{
					if (!tabs.ContainsKey(tab.name))
					{
						tabs.Add(tab.name, tab);
					}
				}
			}

			InstantiateChatMessage(ChatChannel.System, "", WelcomeMessage);

			foreach (KeyValuePair<ChatChannel, List<string>> pair in FChatHelper.ChannelCommandMap)
			{
				string newLine = pair.Key.ToString() + ": ";
				foreach (string command in pair.Value)
				{
					newLine += command + ", ";
				}
				InstantiateChatMessage(ChatChannel.System, "", newLine, ChannelColors[pair.Key]);
			}

			Client.NetworkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
		}

		public override void OnDestroying()
		{
			if (initialTabs != null && initialTabs.Count > 0)
			{
				foreach (FChatTab tab in initialTabs)
				{
					if (!tabs.ContainsKey(tab.name))
					{
						tabs.Remove(tab.name);
					}
				}
			}

			Client.NetworkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
		}

		public void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
		{
			if (args.ConnectionState == LocalConnectionState.Started)
			{
				Client.NetworkManager.ClientManager.RegisterBroadcast<ChatBroadcast>(OnClientChatBroadcastReceived);
			}
			else if (args.ConnectionState == LocalConnectionState.Stopped)
			{
				Client.NetworkManager.ClientManager.UnregisterBroadcast<ChatBroadcast>(OnClientChatBroadcastReceived);
			}
		}

		void Update()
		{
			ValidateMessages();
		}

		public void ValidateMessages()
		{
			if (tabs.TryGetValue(currentTab, out FChatTab tab))
			{
				foreach (FUIChatMessage message in messages)
				{
					if (message == null) continue;

					if (tab.activeChannels.Contains(message.Channel))
					{
						message.gameObject.SetActive(true);
					}
					else
					{
						message.gameObject.SetActive(false);
					}
				}
			}
		}

		public void OnSubmit(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return;
			}

			// remove Rich Text Tags if any exist
			input = FChatHelper.Sanitize(input);

			if (Client.NetworkManager.IsClientStarted)
			{
				if (input.Length > MAX_LENGTH)
				{
					input = input.Substring(0, MAX_LENGTH);
				}
				if (Character != null)
				{
					if (MessageRateLimit > 0)
					{
						if (Character.NextChatMessageTime > DateTime.UtcNow)
						{
							return;
						}
						Character.NextChatMessageTime = DateTime.UtcNow.AddMilliseconds(MessageRateLimit);
					}
					if (!AllowRepeatMessages)
					{
						if (Character.LastChatMessage.Equals(input))
						{
							return;
						}
						Character.LastChatMessage = input;
					}
				}
				ChatBroadcast message = new ChatBroadcast() { text = input };
				// send the message to the server
				Client.NetworkManager.ClientManager.Broadcast(message, Channel.Reliable);
			}
			InputField.text = "";
		}

		public void AddTab()
		{
			const int MAX_TABS = 12;

			if (tabs.Count > MAX_TABS) return;

			FChatTab newTab = Instantiate(chatTabPrefab, chatTabViewParent);
			string newTabName = "New Tab";
			newTab.label.text = newTabName;
			for (int i = 0; tabs.ContainsKey(newTab.label.text); ++i)
			{
				newTab.label.text = newTabName + " " + i;
			}
			newTab.name = newTab.label.text;
			tabs.Add(newTab.label.text, newTab);
		}

		public void ToggleChannel(ChatChannel channel, bool value)
		{
			if (tabs.TryGetValue(currentTab, out FChatTab tab))
			{
				tab.ToggleActiveChannel(channel, value);
			}
		}

		public bool RenameCurrentTab(string newName)
		{
			if (tabs.ContainsKey(newName))
			{
				return false;
			}
			else if (tabs.TryGetValue(currentTab, out FChatTab tab))
			{
				tabs.Remove(currentTab);
				tab.name = newName;
				tab.label.text = newName;
				tabs.Add(tab.name, tab);
				ActivateTab(tab);
				return true;
			}
			return false; // something went wrong
		}

		public void RemoveTab(FChatTab tab)
		{

		}

		public void ActivateTab(FChatTab tab)
		{
			currentTab = tab.name;
		}

		private void AddMessage(FUIChatMessage message)
		{
			const int MAX_MESSAGES = 128;

			messages.Add(message);
			OnMessageAdded?.Invoke(message);

			if (messages.Count > MAX_MESSAGES)
			{
				// messages are FIFO.. remove the first message when we hit our limit.
				FUIChatMessage oldMessage = messages[0];
				messages.RemoveAt(0);
				//OnWriteMessageToDisk?.Invoke(oldMessage); can we add logging to disc later?
				OnMessageRemoved?.Invoke(oldMessage);
				Destroy(oldMessage.gameObject);
			}
		}

		public void InstantiateChatMessage(ChatChannel channel, string name, string message, Color? color = null)
		{
			FUIChatMessage newMessage = Instantiate(chatMessagePrefab, chatViewParent);
			newMessage.Channel = channel;
			newMessage.CharacterName.color = color ?? ChannelColors[channel];
			newMessage.CharacterName.text = "[" + channel.ToString() + "] ";
			if (!string.IsNullOrWhiteSpace(name))
			{
				if (previousName.Equals(name) && previousChannel == channel)
				{
					newMessage.CharacterName.enabled = false;
				}
				else
				{
					newMessage.CharacterName.text += name;
					previousName = name;
				}
			}
			else if (previousChannel == channel && channel == ChatChannel.System)
			{
				newMessage.CharacterName.enabled = false;
			}
			newMessage.Text.color = color ?? ChannelColors[channel];
			newMessage.Text.text = message;
			AddMessage(newMessage);
			
			previousChannel = channel;
		}

		public ChatCommand GetChannelCommand(ChatChannel channel)
		{
			switch (channel)
			{
				case ChatChannel.World: return OnWorldChat;
				case ChatChannel.Region: return OnRegionChat;
				case ChatChannel.Party: return OnPartyChat;
				case ChatChannel.Guild: return OnGuildChat;
				case ChatChannel.Tell: return OnTellChat;
				case ChatChannel.Trade: return OnTradeChat;
				case ChatChannel.Say: return OnSayChat;
				case ChatChannel.System: return OnSystemChat;
				default: return OnSayChat;
			}
		}

		private void OnClientChatBroadcastReceived(ChatBroadcast msg, Channel channel)
		{
			if (!string.IsNullOrWhiteSpace(currentTab) && tabs.TryGetValue(currentTab, out FChatTab tab))
			{
				if (tab.activeChannels.Contains(msg.channel))
				{
					// parse the local message
					ParseLocalMessage(Character, msg);
				}
			}
		}

		private void ParseLocalMessage(Character localCharacter, ChatBroadcast msg)
		{
			// validate message length
			if (string.IsNullOrWhiteSpace(msg.text) || msg.text.Length > MAX_LENGTH)
			{
				return;
			}

			ChatCommand command = FChatHelper.ParseChatChannel(msg.channel);
			if (command != null)
			{
				command?.Invoke(localCharacter, msg);
			}
		}

		public bool OnWorldChat(Character localCharacter, ChatBroadcast msg)
		{
			FClientNamingSystem.SetName(FNamingSystemType.CharacterName, msg.senderID, (s) =>
			{
				InstantiateChatMessage(msg.channel, s, msg.text);
			});
			
			return true;
		}

		public bool OnRegionChat(Character localCharacter, ChatBroadcast msg)
		{
			FClientNamingSystem.SetName(FNamingSystemType.CharacterName, msg.senderID, (s) =>
			{
				InstantiateChatMessage(msg.channel, s, msg.text);
			});
			return true;
		}

		public bool OnPartyChat(Character localCharacter, ChatBroadcast msg)
		{
			FClientNamingSystem.SetName(FNamingSystemType.CharacterName, msg.senderID, (s) =>
			{
				InstantiateChatMessage(msg.channel, s, msg.text);
			});
			return true;
		}

		public bool OnGuildChat(Character localCharacter, ChatBroadcast msg)
		{
			FClientNamingSystem.SetName(FNamingSystemType.CharacterName, msg.senderID, (s) =>
			{
				InstantiateChatMessage(msg.channel, s, msg.text);
			});
			return true;
		}

		public bool OnTellChat(Character localCharacter, ChatBroadcast msg)
		{
			string cmd = FChatHelper.GetWordAndTrimmed(msg.text, out string trimmed);

			// check if we have any special messages
			if (!string.IsNullOrWhiteSpace(cmd))
			{
				// returned message
				if (cmd.Equals(FChatHelper.RELAYED))
				{
					FClientNamingSystem.SetName(FNamingSystemType.CharacterName, msg.senderID, (s) =>
					{
						InstantiateChatMessage(msg.channel, "[To: " + s + "]", trimmed);
					});
					return true;
				}
				// target offline
				else if (cmd.Equals(FChatHelper.ERROR_TARGET_OFFLINE) &&
						 ErrorCodes.TryGetValue(FChatHelper.ERROR_TARGET_OFFLINE, out string offlineMsg))
				{
					FChatHelper.GetWordAndTrimmed(trimmed, out string targetName);
					if (!string.IsNullOrWhiteSpace(targetName))
					{
						FClientNamingSystem.SetName(FNamingSystemType.CharacterName, msg.senderID, (s) =>
						{
							InstantiateChatMessage(msg.channel, s, targetName + offlineMsg);
						});
						return true;
					}
				}
				// messaging ourself??
				else if (cmd.Equals(FChatHelper.ERROR_MESSAGE_SELF) &&
						 ErrorCodes.TryGetValue(FChatHelper.ERROR_MESSAGE_SELF, out string errorMsg))
				{
					FClientNamingSystem.SetName(FNamingSystemType.CharacterName, msg.senderID, (s) =>
					{
						InstantiateChatMessage(msg.channel, s, errorMsg);
					});
					return true;
				}
			}
			// we received a tell from someone else
			if (localCharacter == null || msg.senderID != localCharacter.ID.Value)
			{
				FClientNamingSystem.SetName(FNamingSystemType.CharacterName, msg.senderID, (s) =>
				{
					InstantiateChatMessage(msg.channel, "[From: " + s + "]", msg.text);
				});
			}
			return true;
		}

		public bool OnTradeChat(Character localCharacter, ChatBroadcast msg)
		{
			FClientNamingSystem.SetName(FNamingSystemType.CharacterName, msg.senderID, (s) =>
			{
				InstantiateChatMessage(msg.channel, s, msg.text);
			});
			return true;
		}

		public bool OnSayChat(Character localCharacter, ChatBroadcast msg)
		{
			FClientNamingSystem.SetName(FNamingSystemType.CharacterName, msg.senderID, (s) =>
			{
				InstantiateChatMessage(msg.channel, s, msg.text);
			});
			return true;
		}

		public bool OnSystemChat(Character localCharacter, ChatBroadcast msg)
		{
			FClientNamingSystem.SetName(FNamingSystemType.CharacterName, msg.senderID, (s) =>
			{
				InstantiateChatMessage(msg.channel, s, msg.text);
			});
			return true;
		}
	}
}
﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FellOnline.Shared
{
	/// <summary>
	/// Return true if it should write the chat message to the database.
	/// </summary>
	public delegate bool ChatCommand(Character character, ChatBroadcast msg);
	public struct ChatCommandDetails
	{
		public ChatChannel Channel;
		public ChatCommand Func;
	}

	public static class ChatHelper
	{
		public const string RELAYED = "$(|)";
		public const string ERROR_TARGET_OFFLINE = "$(_)";
		public const string ERROR_MESSAGE_SELF = "$(<)";

		#region Regex
		private const string AlignPattern = @"<align=[^>]+?>|<\/align>";
		private const string AllCapsPattern = @"<allcaps>|<\/allcaps>";
		private const string AlphaPattern = @"<alpha=[^>]+?>|<\/alpha>";
		private const string BoldPattern = @"<b>|<\/b>";
		private const string BrPattern = @"<br>|<\/br>";
		private const string ColorPattern = @"<color=[^>]+?>|<\/color>";
		private const string CspacePattern = @"<cspace=[^>]+?>|<\/cspace>";
		private const string FontPattern = @"<font=[^>]+?>|<\/font>";
		private const string FontWeightPattern = @"<font-weight=[^>]+?>|<\/font-weight>";
		private const string GradientPattern = @"<gradient=[^>]+?>|<\/gradient>";
		private const string ItalicPattern = @"<i>|<\/i>";
		private const string IndentPattern = @"<indent=[^>]+?>|<\/indent>";
		private const string LineHeightPattern = @"<line-height=[^>]+?>|<\/line-height>";
		private const string LineIndentPattern = @"<line-indent=[^>]+?>|<\/line-indent>";
		private const string LinkPattern = @"<link=[^>]+?>|<\/link>";
		private const string LowercasePattern = @"<lowercase>|<\/lowercase>";
		private const string MarginPattern = @"<margin=[^>]+?>|<\/margin>";
		private const string MarkPattern = @"<mark=[^>]+?>|<\/mark>";
		private const string MspacePattern = @"<mspace=[^>]+?>|<\/mspace>";
		private const string NobrPattern = @"<nobr>|<\/nobr>";
		private const string NoparsePattern = @"<noparse>|<\/noparse>";
		private const string PagePattern = @"<page=[^>]+?>|<\/page>";
		private const string PosPattern = @"<pos=[^>]+?>|<\/pos>";
		private const string RotatePattern = @"<rotate=[^>]+?>|<\/rotate>";
		private const string SPattern = @"<s>|<\/s>";
		private const string SizePattern = @"<size=[^>]+?>|<\/size>";
		private const string SmallcapsPattern = @"<smallcaps>|<\/smallcaps>";
		private const string SpacePattern = @"<space=[^>]+?>|<\/space>";
		private const string SpritePattern = @"<sprite=[^>]+?\/>";
		private const string StrikethroughPattern = @"<strikethrough>|<\/strikethrough>";
		private const string StylePattern = @"<style=[^>]+?>|<\/style>";
		private const string SubPattern = @"<sub>|<\/sub>";
		private const string SupPattern = @"<sup>|<\/sup>";
		private const string UPattern = @"<u>|<\/u>";
		private const string UppercasePattern = @"<uppercase>|<\/uppercase>";
		private const string VoffsetPattern = @"<voffset=[^>]+?>|<\/voffset>";
		private const string WidthPattern = @"<width=[^>]+?>|<\/width>";

		private static readonly string CombinedRTTPattern = $"{AlignPattern}|{AllCapsPattern}|{AlphaPattern}|{BoldPattern}|{BrPattern}|{ColorPattern}|{CspacePattern}|{FontPattern}|{FontWeightPattern}|{GradientPattern}|{ItalicPattern}|{IndentPattern}|{LineHeightPattern}|{LineIndentPattern}|{LinkPattern}|{LowercasePattern}|{MarginPattern}|{MarkPattern}|{MspacePattern}|{NobrPattern}|{NoparsePattern}|{PagePattern}|{PosPattern}|{RotatePattern}|{SPattern}|{SizePattern}|{SmallcapsPattern}|{SpacePattern}|{SpritePattern}|{StrikethroughPattern}|{StylePattern}|{SubPattern}|{SupPattern}|{UPattern}|{UppercasePattern}|{VoffsetPattern}|{WidthPattern}";
		#endregion

		private static bool initialized = false;

		public static Dictionary<string, ChatCommand> DirectCommands { get; private set; }
		public static Dictionary<string, ChatCommandDetails> Commands { get; private set; }
		public static Dictionary<ChatChannel, ChatCommandDetails> ChannelCommands { get; private set; }

		public static Dictionary<ChatChannel, List<string>> ChannelCommandMap = new Dictionary<ChatChannel, List<string>>()
	{
		{ ChatChannel.World, new List<string>() { "/w", "/world", } },
		{ ChatChannel.Region, new List<string>() { "/r", "/region", } },
		{ ChatChannel.Party, new List<string>() { "/p", "/party", } },
		{ ChatChannel.Guild, new List<string>() { "/g", "/guild", } },
		{ ChatChannel.Tell, new List<string>() { "/tell", } },
		{ ChatChannel.Trade, new List<string>() { "/t", "/trade", } },
		{ ChatChannel.Say, new List<string>() { "/s", "/say", } },
	};

		static ChatHelper()
		{
			DirectCommands = new Dictionary<string, ChatCommand>();
			Commands = new Dictionary<string, ChatCommandDetails>();
			ChannelCommands = new Dictionary<ChatChannel, ChatCommandDetails>();
		}

		public static void InitializeOnce(Func<ChatChannel, ChatCommand> onGetChannelCommand)
		{
			if (initialized) return;
			initialized = true;


			foreach (KeyValuePair<ChatChannel, List<string>> pair in ChatHelper.ChannelCommandMap)
			{
				AddChatCommandDetails(pair.Value, new ChatCommandDetails()
				{
					Channel = pair.Key,
					Func = onGetChannelCommand?.Invoke(pair.Key),
				});
			}
		}

		public static void AddDirectCommands(Dictionary<string, ChatCommand> commands)
		{
			if (commands == null)
				return;

			foreach (KeyValuePair<string, ChatCommand> pair in commands)
			{
				Debug.Log("ChatHelper: Added Command[" + pair.Key + "]");
				DirectCommands[pair.Key] = pair.Value;
			}
		}

		internal static void AddChatCommandDetails(List<string> commands, ChatCommandDetails details)
		{
			foreach (string command in commands)
			{
				Debug.Log("ChatHelper: Added Chat Command[" + command + "]");
				Commands[command] = details;
				ChannelCommands[details.Channel] = details;
			}
		}

		public static ChatCommand ParseChatChannel(ChatChannel channel)
		{
			ChatCommand command = null;
			if (ChatHelper.ChannelCommands.TryGetValue(channel, out ChatCommandDetails sayCommand))
			{
				command = sayCommand.Func;
			}
			return command;
		}

		public static bool TryParseDirectCommand(string cmd,Character sender, ChatBroadcast msg)
		{
			// try to find the command
			if (ChatHelper.DirectCommands.TryGetValue(cmd, out ChatCommand command))
			{
				command?.Invoke(sender, msg);
				return true;
			}
			return false;
		}

		public static ChatCommand ParseChatCommand(string cmd, ref ChatChannel channel)
		{
			ChatCommand command = null;
			// parse our command or send the message to our /say channel
			if (ChatHelper.Commands.TryGetValue(cmd, out ChatCommandDetails commandDetails))
			{
				channel = commandDetails.Channel;
				command = commandDetails.Func;
			}
			// default is say chat, if we have no command the text goes to say
			else if (ChatHelper.ChannelCommands.TryGetValue(ChatChannel.Say, out ChatCommandDetails sayCommand))
			{
				channel = sayCommand.Channel;
				command = sayCommand.Func;
			}
			return command;
		}

		/// <summary>
		/// Attempts to get the command from the text. If no commands are found it returns an empty string.
		/// </summary>
		public static string GetCommandAndTrim(ref string text)
		{
			if (!text.StartsWith("/"))
			{
				return "";
			}
			int firstSpace = text.IndexOf(' ');
			if (firstSpace < 0)
			{
				return "";
			}
			string cmd = text.Substring(0, firstSpace);
			text = text.Substring(firstSpace, text.Length - firstSpace).Trim();
			return cmd;
		}

		/// <summary>
		/// Attempts to get and remove the first single space separated word from the rest of the text. If no targets are found it returns an empty string.
		/// </summary>
		public static string GetWordAndTrimmed(string text, out string trimmed)
		{
			int firstSpace = text.IndexOf(' ');
			if (firstSpace < 0)
			{
				// no target?
				trimmed = text;
				return "";
			}
			string word = text.Substring(0, firstSpace);
			trimmed = text.Substring(firstSpace, text.Length - firstSpace).Trim();
			return word;
		}

		/// <summary>
		/// Attempts to sanitize a chat message. This will attempt to remove any Rich Text.
		/// </summary>
		public static string Sanitize(string message)
		{
			return Regex.Replace(message, CombinedRTTPattern, "");
		}
	}
}
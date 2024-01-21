using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	[RequireComponent(typeof(Character))]
	public class FQuestController : NetworkBehaviour
	{
		private Dictionary<string, FQuestInstance> quests = new Dictionary<string, FQuestInstance>();

		public Character Character;

		public override void OnStartClient()
		{
			base.OnStartClient();

			if (!base.IsOwner)
			{
				enabled = false;
				return;
			}
		}

		public Dictionary<string, FQuestInstance> Quests
		{
			get
			{
				return this.quests;
			}
		}

		public bool TryGetQuest(string name, out FQuestInstance quest)
		{
			return this.quests.TryGetValue(name, out quest);
		}

		void Update()
		{
		}

		public void Acquire(FQuestTemplate quest)
		{

		}
	}
}
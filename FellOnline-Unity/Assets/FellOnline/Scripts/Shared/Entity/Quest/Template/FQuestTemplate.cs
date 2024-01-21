using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "FNew Quest", menuName = "FellOnline/Character/Quest/Quest", order = 1)]
	public class FQuestTemplate : FCachedScriptableObject<FQuestTemplate>, FICachedObject
	{
		public string Description;
		public uint TimeToCompleteInSeconds;
		public Texture2D Icon;
		public List<FQuestAttributeRequirement> CharacterAttributeRequirements;
		public List<FQuestTemplate> CompletedQuestRequirements;
		public List<FQuestTemplate> AutoProgression;
		public List<FQuestObjective> Objectives;

		public string Name { get { return this.name; } }

		public bool CanAcceptQuest(Character character)
		{
			if (CharacterAttributeRequirements != null && CharacterAttributeRequirements.Count > 0)
			{
				FCharacterAttributeController characterAttributes = character.GetComponent<FCharacterAttributeController>();
				if (characterAttributes == null)
				{
					return false;
				}
				foreach (FQuestAttributeRequirement attributeRequirement in CharacterAttributeRequirements)
				{
					if (!attributeRequirement.MeetsRequirements(characterAttributes))
					{
						return false;
					}
				}
			}
			if (CompletedQuestRequirements != null && CompletedQuestRequirements.Count > 0)
			{
				FQuestController questController = character.GetComponent<FQuestController>();
				if (questController == null)
				{
					return false;
				}
				foreach (FQuestTemplate questRequirement in CompletedQuestRequirements)
				{
					FQuestInstance quest;
					if (!questController.TryGetQuest(questRequirement.Name, out quest) || quest.Status != QuestStatus.Completed)
					{
						return false;
					}
				}
			}
			return true;
		}

		public void AcceptQuest(Character character)
		{
			FQuestController questController = character.GetComponent<FQuestController>();
			if (questController == null)
			{
				return;
			}

			FQuestInstance quest;
			if (questController.TryGetQuest(this.Name, out quest))
			{
				return;
			}

			questController.Acquire(this);
		}

		public void TryCompleteQuest(FQuestInstance questInstance)
		{

		}
	}

	/*[CreateAssetMenu(fileName = "New Quest", menuName = "Character/Quest/Quest Objective/Kill Creature Objective", order = 1)]
	public class QuestKillCreatureObjective : QuestObjective
	{
		public Creature CreatureToKill;
	}*/

	[CreateAssetMenu(fileName = "New Quest", menuName = "Character/Quest/Quest Objective/Harvest Objective", order = 1)]
	public class QuestHarvestObjective : FQuestObjective
	{
		public FBaseItemTemplate ItemToHarvest;
	}

	[CreateAssetMenu(fileName = "New Quest", menuName = "Character/Quest/Quest Objective/Craft Objective", order = 1)]
	public class QuestCraftObjective : FQuestObjective
	{
		public FBaseItemTemplate ItemToCraft;
	}

	[CreateAssetMenu(fileName = "New Quest", menuName = "Character/Quest/Quest Objective/Enchant Objective", order = 1)]
	public class QuestEnchantObjective : FQuestObjective
	{
	}

	[CreateAssetMenu(fileName = "New Quest", menuName = "Character/Quest/Quest Objective/Purchase Objective", order = 1)]
	public class QuestPurchaseObjective : FQuestObjective
	{
		public FBaseItemTemplate ItemToPurchase;
	}

	[CreateAssetMenu(fileName = "New Quest", menuName = "Character/Quest/Quest Objective/CharacterAttribute Objective", order = 1)]
	public class QuestCharacterAttributeObjective : FQuestObjective
	{
	}

	[CreateAssetMenu(fileName = "New Quest", menuName = "Character/Quest/Quest Objective/Interact Objective", order = 1)]
	public class QuestInteractObjective : FQuestObjective
	{
	}

	[CreateAssetMenu(fileName = "New Quest", menuName = "Character/Quest/Quest Objective/Socialize Objective", order = 1)]
	public class QuestSocializeObjective : FQuestObjective
	{
	}

	[CreateAssetMenu(fileName = "New Quest", menuName = "Character/Quest/Quest Objective/Explore Objective", order = 1)]
	public class QuestExploreObjective : FQuestObjective
	{
		//public BaseWorldScene SceneToExplore;
	}
}
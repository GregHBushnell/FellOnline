using System.Collections.Generic;
using System.Linq;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;
using FellOnline.Shared;

namespace FellOnline.Server.DatabaseServices
{
	public class FCharacterBuffService
	{
		/// <summary>
		/// Save a characters buffs to the database.
		/// </summary>
		public static void Save(NpgsqlDbContext dbContext, Character character)
		{
			if (character == null)
			{
				return;
			}

						var buffs = dbContext.CharacterBuffs.Where(c => c.CharacterID == character.ID.Value)
												.ToDictionary(k => k.TemplateID);

			// remove dead buffs
			foreach (CharacterBuffEntity dbBuff in new List<CharacterBuffEntity>(buffs.Values))
			{
				if (!character.BuffController.Buffs.ContainsKey(dbBuff.TemplateID))
				{
					buffs.Remove(dbBuff.TemplateID);
					dbContext.CharacterBuffs.Remove(dbBuff);
				}
			}

			foreach (FBuff buff in character.BuffController.Buffs.Values)
			{
				if (buffs.TryGetValue(buff.Template.ID, out CharacterBuffEntity dbBuff))
				{
					dbBuff.CharacterID = character.ID.Value;
					dbBuff.TemplateID = buff.Template.ID;
					dbBuff.RemainingTime = buff.RemainingTime;
					dbBuff.Stacks.Clear();
					foreach (FBuff stack in buff.Stacks)
					{
						CharacterBuffEntity dbStack = new CharacterBuffEntity();
						dbStack.CharacterID = character.ID.Value;
						dbStack.TemplateID = stack.Template.ID;
						dbStack.RemainingTime = stack.RemainingTime;
						dbBuff.Stacks.Add(dbStack);
					}
				}
				else
				{
					CharacterBuffEntity newBuff = new CharacterBuffEntity()
					{
						CharacterID = character.ID.Value,
						TemplateID = buff.Template.ID,
						RemainingTime = buff.RemainingTime,
					};
					foreach (FBuff stack in buff.Stacks)
					{
						CharacterBuffEntity dbStack = new CharacterBuffEntity();
						dbStack.CharacterID = character.ID.Value;
						dbStack.TemplateID = stack.Template.ID;
						dbStack.RemainingTime = stack.RemainingTime;
						newBuff.Stacks.Add(dbStack);
					}

					dbContext.CharacterBuffs.Add(newBuff);
				}
			}
			dbContext.SaveChanges();
		}

		/// <summary>
		/// KeepData is automatically true... This means we don't actually delete anything. Deleted is simply set to true just incase we need to reinstate a character..
		/// </summary>
		public static void Delete(NpgsqlDbContext dbContext, long characterID, bool keepData = true)
		{
			if (characterID == 0)
			{
				return;
			}
			if (!keepData)
			{
				var buffs = dbContext.CharacterBuffs.Where(c => c.CharacterID == characterID);
				if (buffs != null)
				{
					dbContext.CharacterBuffs.RemoveRange(buffs);
					dbContext.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Load characters buffs from the database.
		/// </summary>
		public static void Load(NpgsqlDbContext dbContext, Character character)
		{
			var buffs = dbContext.CharacterBuffs.Where(c => c.CharacterID == character.ID.Value);
			foreach (CharacterBuffEntity buff in buffs)
			{
				List<FBuff> stacks = new List<FBuff>();
				if (buff.Stacks == null || buff.Stacks.Count > 0)
				{
					foreach (CharacterBuffEntity stack in buff.Stacks)
					{
						FBuff newStack = new FBuff(stack.TemplateID, stack.RemainingTime);
						stacks.Add(newStack);
					}
				}
				FBuff newBuff = new FBuff(buff.TemplateID, buff.RemainingTime, stacks);
				character.BuffController.Apply(newBuff);
			};
		}
	}
}
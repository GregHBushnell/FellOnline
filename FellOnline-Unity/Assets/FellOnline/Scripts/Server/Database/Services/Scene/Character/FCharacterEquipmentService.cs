﻿using System.Linq;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;
using FellOnline.Shared;

namespace FellOnline.Server.DatabaseServices
{
	public class FCharacterEquipmentService
	{
		/// <summary>
		/// Updates a CharacterInventoryItem slot to new values or adds a new CharacterInventoryItem and initializes the Item with the new ID.
		/// </summary>
		public static void SetSlot(NpgsqlDbContext dbContext, long characterID, FItem item)
		{
			if (characterID == 0)
			{
				return;
			}
			if (item == null)
			{
				return;
			}

			var dbItem = dbContext.CharacterEquippedItems.FirstOrDefault(c => c.CharacterID == characterID && c.Slot == item.Slot);
			// update slot or add
			if (dbItem != null)
			{
				dbItem.CharacterID = characterID;
				dbItem.TemplateID = item.Template.ID;
				dbItem.Slot = item.Slot;
				dbItem.Seed = item.Generator != null ? item.Generator.Seed : 0;
				dbItem.Amount = item.IsStackable ? item.Stackable.Amount : 0;
				dbContext.SaveChanges();
			}
			else
			{
				dbItem = new CharacterEquipmentEntity()
				{
					CharacterID = characterID,
					TemplateID = item.Template.ID,
					Slot = item.Slot,
					Seed = item.Generator != null ? item.Generator.Seed : 0,
					Amount = item.IsStackable ? item.Stackable.Amount : 0,
				};
				dbContext.CharacterEquippedItems.Add(dbItem);
				dbContext.SaveChanges();
				item.Initialize(dbItem.ID, dbItem.Seed);
			}
		}

		/// <summary>
		/// KeepData is automatically false... This means we delete the item. TODO Deleted field is simply set to true just incase we need to reinstate a character..
		/// </summary>
		public static void Delete(NpgsqlDbContext dbContext, long characterID, bool keepData = false)
		{
			if (characterID == 0)
			{
				return;
			}


			if (!keepData)
			{
				var dbEquippedItems = dbContext.CharacterEquippedItems.Where(c => c.CharacterID == characterID);
				if (dbEquippedItems != null)
				{
					dbContext.CharacterEquippedItems.RemoveRange(dbEquippedItems);
					dbContext.SaveChanges();
				}
			}
		}

		/// <summary>
		/// KeepData is automatically false... This means we delete the item. TODO Deleted field is simply set to true just incase we need to reinstate a character..
		/// </summary>
		public static void Delete(NpgsqlDbContext dbContext, long characterID, long slot, bool keepData = false)
		{
			if (characterID == 0)
			{
				return;
			}
			if (!keepData)
			{
				var dbItem = dbContext.CharacterEquippedItems.FirstOrDefault(c => c.CharacterID == characterID && c.Slot == slot);
				if (dbItem != null)
				{
					dbContext.CharacterEquippedItems.Remove(dbItem);
					dbContext.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Load character equipment from the database.
		/// </summary>
		public static void Load(NpgsqlDbContext dbContext, Character character)
		{
			if (character == null)
			{
				return;
			}
			var dbEquippedItems = dbContext.CharacterEquippedItems.Where(c => c.CharacterID == character.ID.Value);
			foreach (CharacterEquipmentEntity dbItem in dbEquippedItems)
			{
				FBaseItemTemplate template = FBaseItemTemplate.Get<FBaseItemTemplate>(dbItem.TemplateID);
				if (template == null)
				{
					return;
				}
				FItem item = new FItem(dbItem.ID, dbItem.Seed, template, dbItem.Amount);
				if (item == null)
				{
					return;
				}
				character.EquipmentController.SetItemSlot(item, dbItem.Slot);
			};
		}
	}
}
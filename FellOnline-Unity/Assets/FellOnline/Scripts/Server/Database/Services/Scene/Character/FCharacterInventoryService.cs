﻿using System.Linq;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;
using FellOnline.Shared;

namespace FellOnline.Server.DatabaseServices
{
	public class FCharacterInventoryService
	{
		/// <summary>
		/// Updates an existing item by ID.
		/// </summary>
		public static void Update(NpgsqlDbContext dbContext, long characterID, FItem item)
		{
			if (characterID == 0)
			{
				return;
			}
			if (item == null)
			{
				return;
			}

			var dbItem = dbContext.CharacterInventoryItems.FirstOrDefault(c => c.CharacterID == characterID && c.ID == item.ID);
			// update slot
			if (dbItem != null)
			{
				dbItem.CharacterID = characterID;
				dbItem.TemplateID = item.Template.ID;
				dbItem.Slot = item.Slot;
				dbItem.Seed = item.IsGenerated ? item.Generator.Seed : 0;
				dbItem.Amount = item.IsStackable ? item.Stackable.Amount : 0;
				dbContext.SaveChanges();
			}
		}





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

			var dbItem = dbContext.CharacterInventoryItems.FirstOrDefault(c => c.CharacterID == characterID && c.Slot == item.Slot);

			// update slot or add
			if (dbItem != null)
			{
				dbItem.CharacterID = characterID;
				dbItem.TemplateID = item.Template.ID;
				dbItem.Slot = item.Slot;
				dbItem.Amount = item.IsStackable ? item.Stackable.Amount : 0;
				dbContext.SaveChanges();
			}
			else
			{
				dbItem = new CharacterInventoryEntity()
				{
					CharacterID = characterID,
					TemplateID = item.Template.ID,
					Slot = item.Slot,
					Amount = item.IsStackable ? item.Stackable.Amount : 0,
				};
				dbContext.CharacterInventoryItems.Add(dbItem);
				dbContext.SaveChanges();
				item.Initialize(dbItem.ID, dbItem.Seed);
			}
		}

		/// <summary>
		/// Save a characters inventory to the database.
		/// </summary>
		public static void Save(NpgsqlDbContext dbContext, Character character)
		{
			if (character == null)
			{
				return;
			}

			var dbInventoryItems = dbContext.CharacterInventoryItems.Where(c => c.CharacterID == character.ID.Value)
																	.ToDictionary(k => k.Slot);

			foreach (FItem item in character.InventoryController.Items)
			{
				if (dbInventoryItems.TryGetValue(item.Slot, out CharacterInventoryEntity dbItem))
				{
					dbItem.CharacterID = character.ID.Value;
					dbItem.TemplateID = item.Template.ID;
					dbItem.Slot = item.Slot;
					dbItem.Amount = item.IsStackable ? item.Stackable.Amount : 0;
				}
				else
				{
					dbContext.CharacterInventoryItems.Add(new CharacterInventoryEntity()
					{
						CharacterID = character.ID.Value,
						TemplateID = item.Template.ID,
						Slot = item.Slot,
						Amount = item.IsStackable ? item.Stackable.Amount : 0,
					});
				}
			}
			dbContext.SaveChanges();
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
				var dbInventoryItems = dbContext.CharacterInventoryItems.Where(c => c.CharacterID == characterID);
				if (dbInventoryItems != null)
				{
					dbContext.CharacterInventoryItems.RemoveRange(dbInventoryItems);
					dbContext.SaveChanges();
				}
			}
		}

		/// <summary>
		/// KeepData is automatically false... This means we delete the item. TODO Deleted field is simply set to true just incase we need to reinstate a character..
		/// </summary>
			public static void Delete(NpgsqlDbContext dbContext, long characterID, long slot, bool keepData = false)		{
			if (characterID == 0)
			{
				return;
			}
			if (!keepData)
			{
				var dbItem = dbContext.CharacterInventoryItems.FirstOrDefault(c => c.CharacterID == characterID && c.Slot == slot);				if (dbItem != null)
				{
					dbContext.CharacterInventoryItems.Remove(dbItem);
					dbContext.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Load character inventory from the database.
		/// </summary>
		public static void Load(NpgsqlDbContext dbContext, Character character)
		{
			var dbInventoryItems = dbContext.CharacterInventoryItems.Where(c => c.CharacterID == character.ID.Value);
			foreach (CharacterInventoryEntity dbItem in dbInventoryItems)
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
				character.InventoryController.SetItemSlot(item, dbItem.Slot);
			};
		}
	}
}
﻿using System.Linq;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;
using FellOnline.Shared;

namespace FellOnline.Server.DatabaseServices
{
	public class FCharacterAttributeService
	{
		/// <summary>
		/// Save a character attributes to the database.
		/// </summary>
		public static void Save(NpgsqlDbContext dbContext, Character character)
		{
			if (character == null ||
				character.AttributeController == null)
			{
				return;
			}

			var attributes = dbContext.CharacterAttributes.Where(c => c.CharacterID == character.ID.Value)
														  .ToDictionary(k => k.TemplateID);

			foreach (FCharacterAttribute attribute in character.AttributeController.Attributes.Values)
			{
				// is looping resources separately faster than boxing?
				if (attribute.Template.IsResourceAttribute)
				{
					continue;
				}
				if (attributes.TryGetValue(attribute.Template.ID, out CharacterAttributeEntity dbAttribute))
				{
					dbAttribute.CharacterID = character.ID.Value;
					dbAttribute.TemplateID = attribute.Template.ID;
					dbAttribute.BaseValue = attribute.BaseValue;
					dbAttribute.Modifier = attribute.Modifier;
					dbAttribute.CurrentValue = 0;
				}
				else
				{
					dbContext.CharacterAttributes.Add(new CharacterAttributeEntity()
					{
						CharacterID = character.ID.Value,
						TemplateID = attribute.Template.ID,
						BaseValue = attribute.BaseValue,
						Modifier = attribute.Modifier,
						CurrentValue = 0,
					});
				}
			}
			// is looping resources separately faster than boxing?
			foreach (FCharacterResourceAttribute attribute in character.AttributeController.ResourceAttributes.Values)
			{
				if (attributes.TryGetValue(attribute.Template.ID, out CharacterAttributeEntity dbAttribute))
				{
					dbAttribute.CharacterID = character.ID.Value;
					dbAttribute.TemplateID = attribute.Template.ID;
					dbAttribute.BaseValue = attribute.BaseValue;
					dbAttribute.Modifier = attribute.Modifier;
					dbAttribute.CurrentValue = attribute.CurrentValue;
				}
				else
				{
					dbContext.CharacterAttributes.Add(new CharacterAttributeEntity()
					{
						CharacterID = character.ID.Value,
						TemplateID = attribute.Template.ID,
						BaseValue = attribute.BaseValue,
						Modifier = attribute.Modifier,
						CurrentValue = attribute.CurrentValue,
					});
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
				var attributes = dbContext.CharacterAttributes.Where(c => c.CharacterID == characterID);
				if (attributes != null)
				{
					dbContext.CharacterAttributes.RemoveRange(attributes);
					dbContext.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Load character attributes from the database.
		/// </summary>
		public static void Load(NpgsqlDbContext dbContext, Character character)
		{
			var attributes = dbContext.CharacterAttributes.Where(c => c.CharacterID == character.ID.Value);
			foreach (CharacterAttributeEntity attribute in attributes)
			{
				FCharacterAttributeTemplate template = FCharacterAttributeTemplate.Get<FCharacterAttributeTemplate>(attribute.TemplateID);
				if (template != null)
				{
					if (template.IsResourceAttribute)
					{
						character.AttributeController.SetResourceAttribute(template.ID, attribute.BaseValue, attribute.Modifier, attribute.CurrentValue);
					}
					else
					{
						character.AttributeController.SetAttribute(template.ID, attribute.BaseValue, attribute.Modifier);
					}
				}
			};
		}
	}
}
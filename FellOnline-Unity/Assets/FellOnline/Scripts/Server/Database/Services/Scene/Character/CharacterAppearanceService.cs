using System.Collections.Generic;
using System.Linq;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;
using FellOnline.Shared;
using UnityEngine;

namespace FellOnline.Server.DatabaseServices
{
	public class CharacterAppearanceService
	{
		/// <summary>
		/// Save a characters appearance to the database.
		/// </summary>
		public static void Save(NpgsqlDbContext dbContext, Character character )
		{
			if (character == null)
			{
				return;
			};

			var appearance = dbContext.CharacterAppearance.FirstOrDefault(a => a.CharacterID == character.ID.Value);
			if (appearance == null)
			{
				appearance = new CharacterAppearanceEntity()
				{
					CharacterID = character.ID.Value,
					SkinColor = character.AppearanceController._appearanceDetails.Value.SkinColor,
					HairID = character.AppearanceController._appearanceDetails.Value.HairID,
					HairColor = character.AppearanceController._appearanceDetails.Value.HairColor,
				};
				dbContext.CharacterAppearance.Add(appearance);
			}
			else
			{
				appearance.CharacterID = character.ID.Value;
				appearance.HairID = character.AppearanceController._appearanceDetails.Value.HairID;
				appearance.HairColor = character.AppearanceController._appearanceDetails.Value.HairColor;
				appearance.SkinColor = character.AppearanceController._appearanceDetails.Value.SkinColor;

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
		}

		/// <summary>
		/// Load characters appearance from the database.
		/// </summary>
		public static void Load(NpgsqlDbContext dbContext, Character character)
		{
			if (character == null)
			{
				return;
			}
			var appearance = dbContext.CharacterAppearance.Where(c => c.CharacterID == character.ID.Value);
				Debug.Log(appearance.ToString());
                //make appearance templete to be added
				//FBuff newBuff = new FBuff(buff.TemplateID, buff.RemainingTime, stacks);
				//character.BuffController.Apply(newBuff);
			

				if (appearance == null)
				{
					return;
				}
				foreach (CharacterAppearanceEntity entity in appearance)
				{
						Debug.Log(entity.ToString());
					// if(character.AppearanceController._appearanceDetails != null){
					// 	character.AppearanceController._appearanceDetails.Value = new _CharacterAppearanceDetails(entity.SkinColor,entity.HairID,entity.HairColor);
					// }
					
					character.AppearanceController.AppearanceDetails.HairID = entity.HairID;
					character.AppearanceController.AppearanceDetails.HairColor = entity.HairColor;
					character.AppearanceController.AppearanceDetails.SkinColor = entity.SkinColor;

						character.AppearanceController._appearanceDetails.Value = new CharacterAppearanceDetails(entity.SkinColor,entity.HairID,entity.HairColor);
					break;
				};
				
			
		}
	}
}
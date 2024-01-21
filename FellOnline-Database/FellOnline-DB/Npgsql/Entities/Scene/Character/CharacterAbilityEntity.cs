﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FellOnline.Database.Npgsql.Entities
{
	[Table("character_abilities", Schema = "fell_online_postgresql")]
	[Index(nameof(CharacterID))]
	public class CharacterAbilityEntity
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long ID { get; set; }
		public long CharacterID { get; set; }
		public CharacterEntity Character { get; set; }
		public int TemplateID { get; set; }
		public List<int> AbilityEvents { get; set; }
	}
}
﻿using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FellOnline.Database.Npgsql.Entities
{
    [Table("character_bank", Schema = "fell_online_postgresql")]
    [Index(nameof(CharacterID))]
    [Index(nameof(CharacterID), nameof(Slot))]
    public class CharacterBankEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public long CharacterID { get; set; }
        public CharacterEntity Character { get; set; }
        public int TemplateID { get; set; }
        public int Slot { get; set; }
        public int Seed { get; set; }
        public uint Amount { get; set; }
    }
}

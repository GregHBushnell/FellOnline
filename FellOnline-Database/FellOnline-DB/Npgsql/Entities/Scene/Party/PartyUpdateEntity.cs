﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FellOnline.Database.Npgsql.Entities
{
    [Table("party_updates", Schema = "fell_online_postgresql")]
    [Index(nameof(PartyID))]
    public class PartyUpdateEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public long PartyID { get; set; }
        public DateTime TimeCreated { get; set; }
    }
}
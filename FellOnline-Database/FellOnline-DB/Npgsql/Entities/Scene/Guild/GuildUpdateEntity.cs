using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FellOnline.Database.Npgsql.Entities
{
    [Table("guild_updates", Schema = "fell_online_postgresql")]
    [Index(nameof(GuildID))]
    public class GuildUpdateEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public long GuildID { get; set; }
        public DateTime TimeCreated { get; set; }
    }
}
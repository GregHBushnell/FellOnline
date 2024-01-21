using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FellOnline.Database.Npgsql.Entities
{
    [Table("scene_servers", Schema = "fell_online_postgresql")]
    public class SceneServerEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public DateTime LastPulse { get; set; }
        public string Address { get; set; }
        public ushort Port { get; set; }
        public int CharacterCount { get; set; }
        public bool Locked { get; set; }
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FellOnline.Database.Npgsql.Entities
{
    [Table("pending_scenes", Schema = "fell_online_postgresql")]
    [Index(nameof(WorldServerID))]
    public class PendingSceneEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public long WorldServerID { get; set; }
        public string SceneName { get; set; }
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FellOnline.Database.Npgsql.Entities
{
    [Table("loaded_scenes", Schema = "fell_online_postgresql")]
    [Index(nameof(SceneServerID))]
    [Index(nameof(WorldServerID))]
    public class LoadedSceneEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public long SceneServerID { get; set; }
        public long WorldServerID { get; set; }
        public string SceneName { get; set; }
        public int SceneHandle { get; set; }
        public int CharacterCount { get; set; }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FellOnline.Database.Npgsql.Entities
{
    [Table("parties", Schema = "fell_online_postgresql")]
    public class PartyEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public List<CharacterPartyEntity> Characters { get; set; }
    }
}
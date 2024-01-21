using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace FellOnline.Database.Npgsql.Entities
{
    [Table("character_appearance", Schema = "fell_online_postgresql")]
    [Index(nameof(CharacterID), IsUnique = true)]
    public class CharacterAppearanceEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public long CharacterID { get; set; }
        public CharacterEntity Character { get; set; }
        // public int TemplateID { get; set; }

        public int HairID { get; set; }
        public int SkinColor { get; set; }
        public int HairColor { get; set; }

        //hair color
        //skin color



    }
}
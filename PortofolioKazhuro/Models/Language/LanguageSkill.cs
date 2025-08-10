using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortofolioKazhuro.Models.Language
{
    public class LanguageSkill
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string LanguageName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public int LanguageLevelId { get; set; }
        public LanguageLevel LanguageLevel { get; set; } = null!;


    }


}

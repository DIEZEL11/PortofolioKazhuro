using System.ComponentModel.DataAnnotations;

namespace PortofolioKazhuro.Models.Language
{
    public class LanguageLevel
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<LanguageSkill> Skills { get; set; } = new List<LanguageSkill>();
    }

}

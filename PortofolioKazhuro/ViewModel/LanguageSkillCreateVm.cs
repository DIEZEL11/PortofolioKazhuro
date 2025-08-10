using System.ComponentModel.DataAnnotations;

namespace PortofolioKazhuro.ViewModel
{
    public class LanguageSkillCreateVm
    {
        [Required, MaxLength(50)]
        public string LanguageName { get; set; } = string.Empty;

        [Required]
        public int LanguageLevelId { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

}

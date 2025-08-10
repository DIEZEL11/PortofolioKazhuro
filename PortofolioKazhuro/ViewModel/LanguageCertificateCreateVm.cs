using System.ComponentModel.DataAnnotations;

namespace PortofolioKazhuro.ViewModel
{
    public class LanguageCertificateCreateVm
    {
        [Required, MaxLength(100)]
        public string CertificateName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IssuedBy { get; set; }

        public DateTime? DateIssued { get; set; }

        public IFormFile? File { get; set; }

        [Required]
        public int LanguageSkillId { get; set; }
    }
}

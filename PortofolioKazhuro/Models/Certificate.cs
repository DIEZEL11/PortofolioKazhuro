using System.ComponentModel.DataAnnotations;

namespace PortofolioKazhuro.Models
{
    public class Certificate
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Название сертификата")]
        public string Name { get; set; }

        [Display(Name = "Дата выдачи")]
        public DateTime? IssueDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Путь к файлу")]
        public string? FilePath { get; set; }
    }
}

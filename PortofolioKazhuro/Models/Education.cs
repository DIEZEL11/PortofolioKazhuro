using System.ComponentModel.DataAnnotations;

namespace PortofolioKazhuro.Models
{
    public class Education
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Учебное заведение")]
        public string Institution { get; set; }

        [StringLength(200)]
        [Display(Name = "Степень / Специальность")]
        public string Degree { get; set; }

        [Required]
        [Display(Name = "Дата начала")]
        public DateTime? DateStart { get; set; }

        [Display(Name = "Дата окончания")]
        public DateTime? DateEnd { get; set; }

        [StringLength(1000)]
        [Display(Name = "Описание")]
        public string Description { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace PortofolioKazhuro.Models
{
    public class JobOfferViewModel
    {
        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string JobDescription { get; set; }

        [Required, EmailAddress]
        public string ContactEmail { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal SalaryFrom { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal SalaryTo { get; set; }

        [Required]
        public string Currency { get; set; } // "BYN" or "USD"

        [Required, DataType(DataType.Date)]
        public DateTime ResponseDeadline { get; set; }

        [Required]
        public string WorkFormat { get; set; } // "Удалённо", "Офис", "Гибрид"
    }

}

using System.ComponentModel.DataAnnotations;

namespace PortofolioKazhuro.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        [Display(Name = "GitHub URL")]
        [Url]
        public string? GitHubUrl { get; set; }

        // Дополнительные поля (необязательно, см. ниже)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? ImagePath { get; set; } // Для обложки проекта (если захочешь)
    }

}

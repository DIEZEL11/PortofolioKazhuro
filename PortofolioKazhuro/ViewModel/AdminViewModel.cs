using PortofolioKazhuro.Models;
using System.ComponentModel.DataAnnotations;

namespace PortofolioKazhuro.ViewModel
{
    public class AdminViewModel
    {
        public Profile Profile { get; set; }
        public IFormFile? PhotoFile { get; set; }
        public List<Education> educations { get; set; }
        public List<Project> Projects { get; set; }
        public List<Skill> Skills { get; set; }
        public List<Certificate> Certificates { get; set; }
        public List<Experience> experiences { get; set; }
        public VisitorStatsViewModel visitorStats { get; set; }
        public List<Logs> Logs { get; set; } // Добавлено для логов
    }
}

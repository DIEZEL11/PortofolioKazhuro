namespace PortofolioKazhuro.Models
{
    public class WorkExperience
    {
        public int Id { get; set; }
        public string Company { get; set; } = null!;
        public string Position { get; set; } = null!;
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string? Description { get; set; }
    }

}

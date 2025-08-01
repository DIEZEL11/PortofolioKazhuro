namespace PortofolioKazhuro.Models
{
    public class VisitorStatGroup
    {
        public string IpAddress { get; set; } = default!;
        public int VisitsCount { get; set; }
        public DateTime LastVisitTime { get; set; }
    }
}

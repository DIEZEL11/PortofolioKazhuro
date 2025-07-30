namespace PortofolioKazhuro.Models
{
    public class VisitorStat
    {
        public int Id { get; set; }
        public string IpAddress { get; set; } = default!;
        public DateTime VisitTime { get; set; }
    }
}

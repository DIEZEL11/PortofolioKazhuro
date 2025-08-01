using PortofolioKazhuro.Models;

namespace PortofolioKazhuro.ViewModel
{
    public class VisitorStatsViewModel
    {
        // Список IP, кол-во визитов и время последнего
        public List<VisitorStatGroup> Groups { get; set; } = new();

        // Общее число всех визитов
        public int TotalVisits { get; set; }

        public static implicit operator List<object>(VisitorStatsViewModel v)
        {
            throw new NotImplementedException();
        }
    }
}

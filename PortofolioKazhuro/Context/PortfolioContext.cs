using Microsoft.EntityFrameworkCore;

namespace PortofolioKazhuro.Context
{
    public class PortfolioContext: DbContext
    {
        public PortfolioContext(DbContextOptions<PortfolioContext> options) : base(options) { }

        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<Experience> Experiences => Set<Experience>();
        public DbSet<VisitorStat> VisitorStats => Set<VisitorStat>();
    }
}

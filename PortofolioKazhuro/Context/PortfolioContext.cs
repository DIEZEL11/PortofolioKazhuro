using Microsoft.EntityFrameworkCore;
using PortofolioKazhuro.Models;

namespace PortofolioKazhuro.Context
{
    public class PortfolioContext: DbContext
    {
        public PortfolioContext(DbContextOptions<PortfolioContext> options) : base(options) { }
        public DbSet<Profile> Profiles => Set<Profile>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Certificate> Certificates => Set<Certificate>();
        public DbSet<SkillCategory> skillCategories => Set<SkillCategory>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<Experience> Experiences => Set<Experience>();
        public DbSet<VisitorStat> VisitorStats => Set<VisitorStat>();
        public DbSet<Education>  Educations => Set<Education>();


    }
}

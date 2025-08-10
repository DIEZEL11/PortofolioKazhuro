using Microsoft.EntityFrameworkCore;
using PortofolioKazhuro.Models;
using PortofolioKazhuro.Models.Language;

public class PortfolioContext : DbContext
{
    public PortfolioContext(DbContextOptions<PortfolioContext> options) : base(options) { }

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<WorkExperience> WorkExperiences =>Set<WorkExperience>();

    public DbSet<VisitorStat> VisitorStats => Set<VisitorStat>();
    public DbSet<Education> Educations => Set<Education>();

    public DbSet<LanguageLevel> LanguageLevels => Set<LanguageLevel>();
    public DbSet<LanguageSkill> LanguageSkills => Set<LanguageSkill>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Связь LanguageSkill -> LanguageLevel (многие к одному)
        modelBuilder.Entity<LanguageSkill>()
         .HasIndex(x => new { x.LanguageName, x.LanguageLevelId })
         .IsUnique();
        modelBuilder.Entity<LanguageSkill>()
    .Property(x => x.LanguageName)
    .IsRequired()
    .HasMaxLength(50);
        modelBuilder.Entity<LanguageLevel>()
    .Property(x => x.Name)
    .IsRequired()
    .HasMaxLength(50);

        modelBuilder.Entity<LanguageSkill>()
            .Property(x => x.Description)
            .HasMaxLength(250);

        modelBuilder.Entity<LanguageLevel>().HasData(
    new LanguageLevel { Id = 1, Name = "A1 — Beginner" },
    new LanguageLevel { Id = 2, Name = "A2 — Elementary" },
    new LanguageLevel { Id = 3, Name = "B1 — Intermediate" },
    new LanguageLevel { Id = 4, Name = "B2 — Upper Intermediate" },
    new LanguageLevel { Id = 5, Name = "C1 — Advanced" },
    new LanguageLevel { Id = 6, Name = "C2 — Proficient" },
    new LanguageLevel { Id = 7, Name = "Native" }
);


    }
}

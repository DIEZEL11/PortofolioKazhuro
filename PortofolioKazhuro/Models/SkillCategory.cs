namespace PortofolioKazhuro.Models
{
    public class SkillCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;

        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}

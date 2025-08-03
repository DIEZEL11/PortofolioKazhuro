namespace PortofolioKazhuro.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public int Proficiency { get; set; }  // от 0 до 100
        public int SkillCategoryId { get; set; }
        public SkillCategory SkillCategory { get; set; } = default!;
    }

}

namespace PortofolioKazhuro.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? Patronymic { get; set; }
        public byte[]? PhotoData { get; set; }
        public string? PhotoMimeType { get; set; }
        public string? GitHubUrl { get; set; }
        public string? LeetCodeUrl { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? TelegramUrl { get; set; }
        public string? About { get; set; }
        public string? Email { get; set; }
        
    }
}

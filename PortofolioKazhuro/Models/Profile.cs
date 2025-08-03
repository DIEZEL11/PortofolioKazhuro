using System.ComponentModel.DataAnnotations;

namespace PortofolioKazhuro.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? Patronymic { get; set; }
        public DateTime Birthday { get; set; }
        public byte[]? PhotoData { get; set; }
        public string? PhotoMimeType { get; set; }
        public string? GitHubUrl { get; set; }
        public string? LeetCodeUrl { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? TelegramUrl { get; set; }
        public string? About { get; set; }
        public string? Email { get; set; }
        [Phone(ErrorMessage = "Введите корректный номер телефона")]
        [StringLength(13, ErrorMessage = "Номер телефона не должен превышать 20 символов")]
        public string? PhoneNumber { get; set; }

    }
}

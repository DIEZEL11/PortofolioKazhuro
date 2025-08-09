using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortofolioKazhuro.Context;
using PortofolioKazhuro.Models;
using PortofolioKazhuro.ViewModel;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace PortofolioKazhuro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PortfolioContext _context;

        public HomeController(ILogger<HomeController> logger, PortfolioContext context)
        {
            _context = context;


            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminViewModel
            {
                Profile = await _context.Profiles.FirstOrDefaultAsync(),
                educations = await _context.Educations.ToListAsync(),
                Projects = await _context.Projects.ToListAsync(),
                Skills = await _context.skillCategories.ToListAsync(),
                Certificates = await _context.Certificates.ToListAsync(),
                experiences = await _context.Experiences.ToListAsync(),
            };
            return View(model);
        }
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> SubmitJobOffer(JobOfferViewModel offer)
        {
            if (!ModelState.IsValid)
                return BadRequest("Некорректные данные.");

            // Получаем профиль пользователя (можно через сервис или из БД)
            var profile = await _context.Profiles.FirstOrDefaultAsync(); // или .FindAsync(id)

            if (profile == null)
                return NotFound("Профиль не найден.");

            var message = $"🔔 Новое предложение работы\n\n" +
                          $"🏢 Компания: {offer.CompanyName}\n" +
                          $"📝 Описание: {offer.JobDescription}\n" +
                          $"📬 Контакт отправителя: {offer.ContactEmail}";

            // Отправка на Email
            if (!string.IsNullOrWhiteSpace(profile.Email))
            {
                await SendEmailAsync(profile.Email,profile.RabEmail,profile.RabEmailPass, "Новое предложение работы", message);
            }

            // Отправка в Telegram
            if (!string.IsNullOrWhiteSpace(profile.TelegramUrl))
            {
                var username = profile.TelegramUrl.Replace("https://t.me/", "").TrimStart('@');
                await SendTelegramAsync("YOUR_BOT_TOKEN", "@" + username, message);
            }

            return Ok("Предложение отправлено.");
        }

        private async Task SendEmailAsync(string toEmail,string rabMail,string rabMailPass ,string subject, string body)
        {
            try
            {
                using var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(rabMail, rabMailPass),
                    EnableSsl = true
                };
                var mail = new MailMessage("Sergevm88@gmail.com", toEmail, subject, body);
                await smtp.SendMailAsync(mail);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex.Message);
            }
        }
        private async Task SendTelegramAsync(string botToken, string chatId, string message)
        {
            var url = $"https://api.telegram.org/bot{botToken}/sendMessage";
            using var client = new HttpClient();
            var payload = new Dictionary<string, string>
    {
        { "chat_id", chatId },
        { "text", message }
    };
            await client.PostAsync(url, new FormUrlEncodedContent(payload));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortofolioKazhuro.Context;
using PortofolioKazhuro.Models;
using PortofolioKazhuro.Serviceces;
using PortofolioKazhuro.ViewModel;
using System.Diagnostics;

namespace PortofolioKazhuro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PortfolioContext _context;
        private readonly TelegramService _telegram;
        private readonly MailService _mail;
        public HomeController(ILogger<HomeController> logger, TelegramService telegram, MailService mail, PortfolioContext context)
        {
            _context = context;
            _telegram = telegram;
            _mail = mail;
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

        public async Task<IActionResult> SubmitJobOffer(JobOfferViewModel offer)
        {
            if (offer == null || !ModelState.IsValid)
                return BadRequest("Некорректные данные.");

            var profile = await _context.Profiles.FirstOrDefaultAsync();
            if (profile == null)
                return NotFound("Профиль не найден.");

            var message = $"""
        🔔 *Новое предложение работы*

        🏢 *Компания:* {offer.CompanyName}
        📝 *Описание:* {offer.JobDescription}
        💰 *Зарплата:* {offer.SalaryFrom}–{offer.SalaryTo} {offer.Currency}
        📍 *Формат работы:* {offer.WorkFormat}
        📅 *Срок отклика:* {offer.ResponseDeadline:dd.MM.yyyy}
        📬 *Контакт:* {offer.ContactEmail}
        """;

            bool telegramSent = false;
            bool mailSent = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(profile.RabEmail) &&
                    !string.IsNullOrWhiteSpace(profile.RabEmailPass) &&
                    !string.IsNullOrWhiteSpace(profile.Email))
                {
                    mailSent = await _mail.SendEmailAsync(
                         toEmail: profile.Email,
                       rabMail: profile.RabEmail,
                        rabMailPass: profile.RabEmailPass,
                        subject: "Новое предложение работы",
                        body: message
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке email.");
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(profile.TelegramTokenBot) &&
                    !string.IsNullOrWhiteSpace(profile.TelegramChatIdBot))
                {
                    telegramSent = await _telegram.SendMessageAsync(
                       botToken: profile.TelegramTokenBot,
                        chatId: profile.TelegramChatIdBot,
                        message: message
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке сообщения в Telegram.");
            }

            TempData["Success"] = telegramSent
                ? "Предложение отправлено через Telegram."
                : "Предложение не удалось отправить через Telegram.";

            if (!mailSent)
                TempData["MailWarning"] = "Email не был отправлен.";

            return RedirectToAction("Index");
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private async Task<PartialViewResult> RenderListAsync()
        {
            var items = await _context.LanguageSkills
                .Include(x => x.LanguageLevel)
                .OrderBy(x => x.LanguageName)
                .ToListAsync();

            return PartialView("_LanguageList", items);
        }
        public async Task<IActionResult> Index()
        {
            var model = new AdminViewModel
            {
                Profile = await _context.Profiles.FirstOrDefaultAsync(),
                educations = await _context.Educations.ToListAsync(),
                Projects = await _context.Projects.ToListAsync(),
                Skills = await _context.SkillCategories.Include(s => s.Skills).ToListAsync(),
                Certificates = await _context.Certificates.ToListAsync(),
                experiences = await _context.WorkExperiences.ToListAsync(),
                LanguageSkills = await _context.LanguageSkills
                        .Include(ls => ls.LanguageLevel)
                        .ToListAsync()
            };
            return View(model);
        }
        [HttpPost]

        public async Task<IActionResult> SubmitJobOffer(JobOfferViewModel offer)
        {
            if (offer == null || !ModelState.IsValid)
            {
                _logger.LogWarning("Получены некорректные данные для предложения работы.");
                return BadRequest("Некорректные данные.");
            }

            var profile = await _context.Profiles.FirstOrDefaultAsync();
            if (profile == null)
            {
                _logger.LogWarning("Профиль пользователя не найден.");
                return NotFound("Профиль не найден.");
            }

            var message = GenerateJobOfferMessage(offer);

            bool telegramSent = false;
            bool mailSent = false;
            bool telegramFileSent = false;

            //try
            //{
            //    if (!string.IsNullOrWhiteSpace(profile.RabEmail) &&
            //        !string.IsNullOrWhiteSpace(profile.RabEmailPass) &&
            //        !string.IsNullOrWhiteSpace(profile.Email))
            //    {
            //        mailSent = await _mail.SendEmailAsync(
            //            toEmail: profile.Email,
            //            rabMail: profile.RabEmail,
            //            rabMailPass: profile.RabEmailPass,
            //            subject: "Новое предложение работы",
            //            body: message
            //        );
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Ошибка при отправке email.");
            //}

            try
            {
                if (!string.IsNullOrWhiteSpace(profile.TelegramTokenBot) && !string.IsNullOrWhiteSpace(profile.TelegramChatIdBot))
                {
                    telegramSent = await _telegram.SendMessageAsync(
                        botToken: profile.TelegramTokenBot,
                        chatId: profile.TelegramChatIdBot,
                        message: message
                    );

                    // Отправка файла в Telegram
                    if (offer.Attachment != null && offer.Attachment.Length > 0)
                    {
                        using var stream = offer.Attachment.OpenReadStream();
                        telegramFileSent = await _telegram.SendDocumentAsync(
                            botToken: profile.TelegramTokenBot,
                            chatId: profile.TelegramChatIdBot,
                            fileStream: stream,
                            fileName: offer.Attachment.FileName,
                            caption: "Вложение к предложению работы"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке сообщения в Telegram.");
            }

            TempData["Success"] = (telegramSent || mailSent || telegramFileSent)
                ? "Предложение успешно отправлено."
                : "Не удалось отправить предложение. Проверьте настройки.";

            return RedirectToAction("Index");
        }

        private string GenerateJobOfferMessage(JobOfferViewModel offer)
        {
            var message =
                $"🔔 *Новое предложение работы*\n" +
                $"🏢 *Компания:* {offer.CompanyName}\n" +
                $"📝 *Описание:* {offer.JobDescription}\n" +
                $"💰 *Зарплата:* {offer.SalaryFrom}–{offer.SalaryTo} {offer.Currency}\n" +
                $"📍 *Формат работы:* {offer.WorkFormat}\n" +
                $"📅 *Срок отклика:* {offer.ResponseDeadline:dd.MM.yyyy}\n" +
                $"📬 *Контакт:* {offer.ContactEmail}";

            if (offer.Attachment != null && offer.Attachment.Length > 0)
            {
                message += $"\n📎 *Вложение:* {offer.Attachment.FileName}";
            }

            return message;
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

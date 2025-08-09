using System.Net;
using System.Net.Mail;

namespace PortofolioKazhuro.Serviceces
{
    public class MailService
    {
        private readonly ILogger<MailService> _logger;


        public MailService(ILogger<MailService> logger)
        {
            _logger = logger;

        }
        public async Task<bool> SendEmailAsync(string toEmail, string rabMail, string rabMailPass, string subject, string body)
        {
            try
            {
                var smtp = new SmtpClient("smtp.yandex.ru", 587);


                smtp.UseDefaultCredentials = false;                      // отключаем автодетект доменной учётки
                smtp.Credentials = new NetworkCredential(rabMail, rabMailPass);
                smtp.EnableSsl = true;                                         // STARTTLS на порту 587

                var mail = new MailMessage
                {
                    From = new MailAddress(rabMail, "JobBot"), // Можно указать имя отправителя
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false // Или true, если используешь HTML
                };

                mail.To.Add(toEmail);

                await smtp.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }

        }
    }
}

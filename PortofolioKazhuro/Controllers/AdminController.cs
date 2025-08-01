using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortofolioKazhuro.Context;
using PortofolioKazhuro.Models;
using PortofolioKazhuro.ViewModel;

namespace PortofolioKazhuro.Controllers
{
    public class AdminController : Controller
    {
        private readonly PortfolioContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(PortfolioContext context,
                               ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
            _logger.LogInformation("Инициализация AdminController");
        }

        // GET: /Admin
        public async Task<IActionResult> Index(string logLevel)
        {
            _logger.LogInformation("Загрузка данных для админской панели");

            var model = new AdminViewModel
            {
                Profile = await _context.Profiles.FirstOrDefaultAsync(),
                educations = await _context.educations.ToListAsync(),
                Projects = await _context.Projects.ToListAsync(),
                Skills = await _context.Skills.ToListAsync(),
                Certificates = await _context.Certificates.ToListAsync(),
                experiences = await _context.Experiences.ToListAsync(),
                visitorStats = await _context.VisitorStats.ToListAsync(),
              
            };
            if(model.Profile == null)
            {
                _logger.LogWarning("Профиль не найден, создайте профиль в базе данных");
                model.Profile = new Profile();
                TempData["ErrorMessage"] = "Профиль не найден. Пожалуйста, создайте профиль в базе данных.";
                //return RedirectToAction("CreateProfile", "Profile");
            }
            // Получение строки подключения
            var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
            var sqliteConnectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=log.db;";

            // Получение строки подключения для логов
            var logDbPath = Path.Combine(AppContext.BaseDirectory, "logs.db");
            var logConnectionString = $"Data Source={logDbPath}";

            // Считываем логи из отдельной БД SQLite
            var logs = new List<Logs>();
            using (var connection = new SqliteConnection(logConnectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    if (string.IsNullOrEmpty(logLevel))
                    {
                        command.CommandText = "SELECT Timestamp, Level, Exception, RenderedMessage, Properties FROM Logs ORDER BY Timestamp DESC LIMIT 100";
                    }
                    else
                    {
                        command.CommandText = "SELECT Timestamp, Level, Exception, RenderedMessage, Properties FROM Logs WHERE Level = $level ORDER BY Timestamp DESC LIMIT 100";
                        command.Parameters.AddWithValue("$level", logLevel);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            logs.Add(new Logs
                            {
                                Timestamp = reader.GetDateTime(0),
                                Level = reader.GetString(1),
                                Exception = reader.GetString(2),
                                RenderedMessage = reader.GetString(3),
                                Properties= reader.GetString(4)
                            });
                        }
                    }
                }
            }
            model.Logs = logs;

            _logger.LogInformation("Данные для админской панели успешно загружены");
            return View(model);
        }

        // POST: /Admin/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(AdminViewModel vm)
        {
            //if (!ModelState.IsValid)
            //{
            //    TempData["ErrorMessage"] = "Проверьте заполненные поля.";
            //    return RedirectToAction(nameof(Index));
            //}

            // Получаем текущую запись
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == vm.Profile.Id);
            if (profile == null)
            {
                profile = new Profile
                {
                    Name = vm.Profile.Name,
                    Surname = vm.Profile.Surname,
                    Patronymic = vm.Profile.Patronymic,
                    Email = vm.Profile.Email,
                    GitHubUrl = vm.Profile.GitHubUrl,
                    LeetCodeUrl = vm.Profile.LeetCodeUrl,
                    LinkedinUrl = vm.Profile.LinkedinUrl,
                    About = vm.Profile.About
                };
                //TempData["ErrorMessage"] = "Профиль не найден.";
                //return RedirectToAction(nameof(Index));
            }
            else
            {
                // Обновляем текстовые поля
                profile.Name = vm.Profile.Name;
                profile.Surname = vm.Profile.Surname;
                profile.Patronymic = vm.Profile.Patronymic;
                profile.Email = vm.Profile.Email;
                profile.GitHubUrl = vm.Profile.GitHubUrl;
                profile.LeetCodeUrl = vm.Profile.LeetCodeUrl;
                profile.LinkedinUrl = vm.Profile.LinkedinUrl;
                profile.About = vm.Profile.About;
            }
            // Если загружен новый файл
            if (vm.PhotoFile != null && vm.PhotoFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await vm.PhotoFile.CopyToAsync(ms);

                profile.PhotoData = ms.ToArray();
                profile.PhotoMimeType = vm.PhotoFile.ContentType;
            }

            try
            {
                _context.Profiles.Update(profile);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Профиль обновлён (Id: {Id})", profile.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления профиля");
                TempData["ErrorMessage"] = "Не удалось сохранить профиль.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ----- ОБРАЗОВАНИЕ -----

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEducation(string name, DateTime? dateStart, DateTime? dateEnd)
        {
            _logger.LogInformation("Добавление образования: {Name} ({Start}–{End})",
                                   name, dateStart, dateEnd);

            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Попытка добавить образование без названия");
                TempData["ErrorMessage"] = "Укажите название учебного заведения.";
            }
            else
            {
                var edu = new Education
                {
                    name = name,
                    DateStart = dateStart,
                    DateEnd = dateEnd
                };

                try
                {
                    await _context.educations.AddAsync(edu);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Образование добавлено (Id: {Id})", edu.id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении образования: {Name}", name);
                    TempData["ErrorMessage"] = "Не удалось добавить образование.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEducation(int id)
        {
            _logger.LogInformation("Удаление образования (Id: {Id})", id);

            var edu = await _context.educations.FindAsync(id);
            if (edu == null)
            {
                _logger.LogWarning("Образование не найдено для удаления (Id: {Id})", id);
            }
            else
            {
                try
                {
                    _context.educations.Remove(edu);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Образование удалено (Id: {Id})", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при удалении образования (Id: {Id})", id);
                    TempData["ErrorMessage"] = "Не удалось удалить образование.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // ----- ПРОЕКТЫ -----

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProject(string title, string gitHubUrl, string description)
        {
            _logger.LogInformation("Добавление проекта: {Title}", title);

            if (string.IsNullOrWhiteSpace(title))
            {
                _logger.LogWarning("Попытка добавить проект без названия");
                TempData["ErrorMessage"] = "Название проекта не может быть пустым.";
            }
            else
            {
                var project = new Project
                {
                    Title = title,
                    GitHubUrl = gitHubUrl,
                    Description = description
                };

                try
                {
                    await _context.Projects.AddAsync(project);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Проект добавлен (Id: {Id})", project.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении проекта: {Title}", title);
                    TempData["ErrorMessage"] = "Не удалось добавить проект.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int id)
        {
            _logger.LogInformation("Удаление проекта (Id: {Id})", id);

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                _logger.LogWarning("Проект не найден для удаления (Id: {Id})", id);
            }
            else
            {
                try
                {
                    _context.Projects.Remove(project);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Проект удалён (Id: {Id})", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при удалении проекта (Id: {Id})", id);
                    TempData["ErrorMessage"] = "Не удалось удалить проект.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // ----- НАВЫКИ -----

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSkill(string name)
        {
            _logger.LogInformation("Добавление навыка: {Name}", name);

            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Попытка добавить навык без названия");
            }
            else
            {
                try
                {
                    await _context.Skills.AddAsync(new Skill { Name = name });
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Навык добавлен: {Name}", name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при добавлении навыка: {Name}", name);
                    TempData["ErrorMessage"] = "Не удалось добавить навык.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            _logger.LogInformation("Удаление навыка (Id: {Id})", id);

            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
            {
                _logger.LogWarning("Навык не найден для удаления (Id: {Id})", id);
            }
            else
            {
                try
                {
                    _context.Skills.Remove(skill);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Навык удалён (Id: {Id})", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при удалении навыка (Id: {Id})", id);
                    TempData["ErrorMessage"] = "Не удаётся удалить навык.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // ----- СЕРТИФИКАТЫ -----

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCertificate(string name)
        {
            _logger.LogInformation("Добавление сертификата: {Name}", name);

            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Попытка добавить сертификат без названия");
            }
            else
            {
                try
                {
                    await _context.Certificates.AddAsync(new Certificate { Name = name });
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Сертификат добавлен: {Name}", name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при добавлении сертификата: {Name}", name);
                    TempData["ErrorMessage"] = "Не удалось добавить сертификат.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCertificate(int id)
        {
            _logger.LogInformation("Удаление сертификата (Id: {Id})", id);

            var cert = await _context.Certificates.FindAsync(id);
            if (cert == null)
            {
                _logger.LogWarning("Сертификат не найден для удаления (Id: {Id})", id);
            }
            else
            {
                try
                {
                    _context.Certificates.Remove(cert);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Сертификат удалён (Id: {Id})", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при удалении сертификата (Id: {Id})", id);
                    TempData["ErrorMessage"] = "Не удалось удалить сертификат.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // ----- ОПЫТ РАБОТЫ -----

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExperience(string description)
        {
            _logger.LogInformation("Добавление опыта: {Description}", description);

            if (string.IsNullOrWhiteSpace(description))
            {
                _logger.LogWarning("Попытка добавить пустой опыт работы");
            }
            else
            {
                try
                {
                    await _context.Experiences.AddAsync(new Experience { Description = description });
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Опыт работы добавлен");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при добавлении опыта работы");
                    TempData["ErrorMessage"] = "Не удалось добавить опыт работы.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExperience(int id)
        {
            _logger.LogInformation("Удаление опыта работы (Id: {Id})", id);

            var exp = await _context.Experiences.FindAsync(id);
            if (exp == null)
            {
                _logger.LogWarning("Опыт работы не найден для удаления (Id: {Id})", id);
            }
            else
            {
                try
                {
                    _context.Experiences.Remove(exp);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Опыт работы удалён (Id: {Id})", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при удалении опыта работы (Id: {Id})", id);
                    TempData["ErrorMessage"] = "Не удалось удалить опыт работы.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

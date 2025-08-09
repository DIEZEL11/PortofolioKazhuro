using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PortofolioKazhuro.Context;
using PortofolioKazhuro.Models;
using PortofolioKazhuro.ViewModel;

namespace PortofolioKazhuro.Controllers
{
    [IpFilter("192.168.31.149", "::1")]
    public class AdminController : Controller
    {
        private readonly PortfolioContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IWebHostEnvironment _environment;

        public AdminController(PortfolioContext context, ILogger<AdminController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _logger.LogInformation("Инициализация AdminController");
            _environment = environment;
        }

        // GET: /Admin
        public async Task<IActionResult> Index(string logLevel)
        {
            _logger.LogInformation("Загрузка данных для админской панели");

            // 1. Получаем профиль и остальные справочники
            var profile = await _context.Profiles.FirstOrDefaultAsync();
            var educations = await _context.Educations.ToListAsync();
            var projects = await _context.Projects.ToListAsync();
            var skills = await _context.skillCategories.Include(s => s.Skills).ToListAsync();
            var certificates = await _context.Certificates.ToListAsync();
            var experiences = await _context.Experiences.ToListAsync();

            // 2. Группируем статистику по IP
            var allVisits = _context.VisitorStats.AsNoTracking();
            var groups = await allVisits
                .GroupBy(v => v.IpAddress)
                .Select(g => new VisitorStatGroup
                {
                    IpAddress = g.Key,
                    VisitsCount = g.Count(),
                    LastVisitTime = g.Max(v => v.VisitTime)
                })
                .ToListAsync();
            var total = await allVisits.CountAsync();

            // 3. Считываем логи из отдельной БД SQLite
            var logs = new List<Logs>();
            var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
            var logDbPath = Path.Combine(AppContext.BaseDirectory, "logs.db");
            var logConnectionStr = $"Data Source={logDbPath}";
            using (var conn = new SqliteConnection(logConnectionStr))
            {
                await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = string.IsNullOrEmpty(logLevel)
                    ? "SELECT Timestamp, Level, Exception, RenderedMessage, Properties FROM Logs ORDER BY Timestamp DESC LIMIT 100"
                    : "SELECT Timestamp, Level, Exception, RenderedMessage, Properties FROM Logs WHERE Level = $level ORDER BY Timestamp DESC LIMIT 100";
                if (!string.IsNullOrEmpty(logLevel))
                    cmd.Parameters.AddWithValue("$level", logLevel);

                using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    logs.Add(new Logs
                    {
                        Timestamp = rdr.GetDateTime(0),
                        Level = rdr.GetString(1),
                        Exception = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                        RenderedMessage = rdr.GetString(3),
                        Properties = rdr.GetString(4)
                    });
                }
            }

            // 4. Собираем модель и возвращаем View
            var model = new AdminViewModel
            {
                Profile = profile ?? new Profile(),
                educations = educations,
                Projects = projects,
                Skills = skills,
                Certificates = certificates,
                experiences = experiences,
                visitorStats = new VisitorStatsViewModel
                {
                    Groups = groups,
                    TotalVisits = total
                },
                Logs = logs
            };

            if (profile == null)
            {
                _logger.LogWarning("Профиль не найден, создайте профиль в базе данных");
                TempData["ErrorMessage"] = "Профиль не найден. Пожалуйста, создайте профиль в базе данных.";
            }

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
                    Birthday = vm.Profile.Birthday,
                    Email = vm.Profile.Email,
                    RabEmail = vm.Profile.RabEmail,
                    RabEmailPass = vm.Profile.RabEmailPass,
                    GitHubUrl = vm.Profile.GitHubUrl,
                    LeetCodeUrl = vm.Profile.LeetCodeUrl,
                    LinkedinUrl = vm.Profile.LinkedinUrl,
                    TelegramUrl = vm.Profile.TelegramUrl,
                    PhoneNumber = vm.Profile.PhoneNumber,
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
                profile.Birthday = vm.Profile.Birthday;
                profile.Email = vm.Profile.Email;
                profile.RabEmail = vm.Profile.RabEmail;
                profile.RabEmailPass = vm.Profile.RabEmailPass;
                profile.PhoneNumber = vm.Profile.PhoneNumber;
                profile.GitHubUrl = vm.Profile.GitHubUrl;
                profile.LeetCodeUrl = vm.Profile.LeetCodeUrl;
                profile.LinkedinUrl = vm.Profile.LinkedinUrl;
                profile.TelegramUrl = vm.Profile.TelegramUrl;
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
        public IActionResult AddEducation(Education model)
        {
            if (model.DateEnd.HasValue && model.DateStart.HasValue && model.DateEnd < model.DateStart)
            {
                ModelState.AddModelError("DateEnd", "Дата окончания не может быть раньше даты начала.");
            }

            if (!ModelState.IsValid)
            {
                var educations = _context.Educations.ToList();
                ViewBag.Error = "Пожалуйста, исправьте ошибки.";
                return View("Index", educations);
            }

            _context.Educations.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteEducation(int id)
        {
            var education = _context.Educations.Find(id);
            if (education != null)
            {
                _context.Educations.Remove(education);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditEducation(Education model)
        {
            if (!ModelState.IsValid)
            {
                var educations = _context.Educations.ToList();
                ViewBag.Error = "Ошибка при редактировании.";
                return View("Index", educations);
            }

            var edu = _context.Educations.Find(model.Id);
            if (edu != null)
            {
                edu.Institution = model.Institution;
                edu.Degree = model.Degree;
                edu.DateStart = model.DateStart;
                edu.DateEnd = model.DateEnd;
                edu.Description = model.Description;

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearLogs()
        {
            try
            {
                // Формируем путь и строку подключения к базе логов
                var logDbPath = Path.Combine(AppContext.BaseDirectory, "logs.db");
                var connectionString = $"Data Source={logDbPath}";

                // Открываем соединение и выполняем очистку таблицы
                await using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                await using (var deleteCmd = connection.CreateCommand())
                {
                    deleteCmd.CommandText = "DELETE FROM Logs;";
                    var deletedCount = await deleteCmd.ExecuteNonQueryAsync();
                    TempData["Success"] = $"Удалено {deletedCount} записей логов.";
                }

                // Опционально: уменьшаем размер файла после большого удаления
                await using (var vacuumCmd = connection.CreateCommand())
                {
                    vacuumCmd.CommandText = "VACUUM;";
                    await vacuumCmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Ошибка при очистке логов");
                TempData["Error"] = "Не удалось очистить логи. Проверьте настройки БД.";
            }

            return RedirectToAction(nameof(Index));
        }


        // ----- ПРОЕКТЫ -----

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProject(string Title, string? Description, string? GitHubUrl)
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                ModelState.AddModelError("Title", "Название обязательно");
            }

            if (!ModelState.IsValid)
            {
                var all = _context.Projects.ToList();
                return View("Index", all);
            }

            var project = new Project
            {
                Title = Title,
                Description = Description,
                GitHubUrl = GitHubUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult EditProject(int id)
        {
            var project = _context.Projects.Find(id);
            return project == null ? NotFound() : View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProject(int id, string Title, string? Description, string? GitHubUrl)
        {
            var project = _context.Projects.Find(id);
            if (project == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(Title))
            {
                ModelState.AddModelError("Title", "Название обязательно");
            }

            if (!ModelState.IsValid)
            {
                return View(project);
            }

            project.Title = Title;
            project.Description = Description;
            project.GitHubUrl = GitHubUrl;

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProject(int id)
        {
            var project = _context.Projects.Find(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }



        // ----- НАВЫКИ -----

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> CreateSkillAndCategory(string CategoryName, string SkillName, int Proficiency)
        {
            if (string.IsNullOrWhiteSpace(CategoryName) || string.IsNullOrWhiteSpace(SkillName) || Proficiency < 0 || Proficiency > 100)
            {
                TempData["ErrorMessage"] = "Введите корректные данные.";
                return RedirectToAction("Index");
            }

            var category = new SkillCategory { Name = CategoryName };
            var skill = new Skill
            {
                Name = SkillName,
                Proficiency = Proficiency,
                SkillCategory = category
            };

            _context.skillCategories.Add(category);
            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Категория и навык успешно добавлены.";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> AddSkill(string Name, int Proficiency, int SkillCategoryId)
        {
            if (string.IsNullOrWhiteSpace(Name) || Proficiency < 0 || Proficiency > 100)
            {
                TempData["ErrorMessage"] = "Введите корректные данные.";
                return RedirectToAction("Index");
            }
            var result = await _context.skillCategories
                .FirstOrDefaultAsync(c => c.Id == SkillCategoryId);
            var skill = new Skill
            {
                Name = Name,
                Proficiency = Proficiency,
                SkillCategory = result
            };
            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Навык успешно добавлен.";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteSkill(int SkillId)
        {
            var skill = await _context.Skills.FindAsync(SkillId);
            if (skill != null)
            {
                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditSkill(int SkillId, string NewName, int NewProficiency)
        {
            var skill = await _context.Skills.FindAsync(SkillId);
            if (skill != null)
            {
                skill.Name = NewName;
                skill.Proficiency = NewProficiency;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(int CategoryId, string NewName)
        {
            var category = await _context.skillCategories.FindAsync(CategoryId);
            if (category != null)
            {
                category.Name = NewName;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int CategoryId)
        {
            var category = await _context.skillCategories
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(c => c.Id == CategoryId);

            if (category != null)
            {
                _context.Skills.RemoveRange(category.Skills); // сначала удалить навыки
                _context.skillCategories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> AddCategory(string CategoryName)
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                TempData["ErrorMessage"] = "Название категории не может быть пустым.";
                return RedirectToAction("Index");
            }

            var exists = await _context.skillCategories.AnyAsync(c => c.Name == CategoryName);
            if (exists)
            {
                TempData["ErrorMessage"] = "Такая категория уже существует.";
                return RedirectToAction("Index");
            }

            var category = new SkillCategory { Name = CategoryName };
            _context.skillCategories.Add(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Категория успешно добавлена.";
            return RedirectToAction("Index");
        }

        // ----- СЕРТИФИКАТЫ -----

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCertificate(string Name, DateTime? IssueDate, IFormFile? File)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("Name", "Название обязательно");
            }

            if (!ModelState.IsValid)
            {
                var allCerts = _context.Certificates.ToList();
                return View("Index", allCerts);
            }

            string? filePath = null;

            if (File != null && File.Length > 0)
            {
                var uploads = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid() + Path.GetExtension(File.FileName);
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    File.CopyTo(stream);
                }

                filePath = "/uploads/" + fileName;
            }

            var cert = new Certificate
            {
                Name = Name,
                IssueDate = IssueDate,
                FilePath = filePath
            };

            _context.Certificates.Add(cert);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        // GET: /Certificates/EditCertificate/5
        public IActionResult EditCertificate(int id)
        {
            var cert = _context.Certificates.Find(id);
            if (cert == null)
                return NotFound();

            return View(cert);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCertificate(int id, string Name, DateTime? IssueDate, IFormFile? File)
        {
            var cert = _context.Certificates.Find(id);
            if (cert == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("Name", "Название обязательно");
            }

            if (!ModelState.IsValid)
            {
                return View(cert);
            }

            cert.Name = Name;
            cert.IssueDate = IssueDate;

            if (File != null && File.Length > 0)
            {
                // Удалить старый файл, если был
                if (!string.IsNullOrEmpty(cert.FilePath))
                {
                    var oldPath = Path.Combine(_environment.WebRootPath, cert.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var uploads = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid() + Path.GetExtension(File.FileName);
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    File.CopyTo(stream);
                }

                cert.FilePath = "/uploads/" + fileName;
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCertificate(int id)
        {
            var cert = _context.Certificates.Find(id);
            if (cert == null)
                return NotFound();

            // Удаление файла
            if (!string.IsNullOrEmpty(cert.FilePath))
            {
                var fullPath = Path.Combine(_environment.WebRootPath, cert.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            _context.Certificates.Remove(cert);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Certificates/DownloadCertificate/5
        public IActionResult DownloadCertificate(int id)
        {
            var cert = _context.Certificates.Find(id);
            if (cert == null || string.IsNullOrEmpty(cert.FilePath))
                return NotFound();

            var filePath = Path.Combine(_environment.WebRootPath, cert.FilePath.TrimStart('/'));
            var contentType = "application/octet-stream";
            var fileName = Path.GetFileName(filePath);

            return PhysicalFile(filePath, contentType, fileName);
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
                    TempData["Error"] = "Не удалось добавить опыт работы.";
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
                    TempData["Error"] = "Не удалось удалить опыт работы.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

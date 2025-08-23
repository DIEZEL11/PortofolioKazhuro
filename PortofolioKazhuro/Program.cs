using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortofolioKazhuro.Serviceces;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

    // 👇 Формируем путь к файлу БД и подключаем её
    var dbFileName = "portfolio.db";
    var dbLogFileName = "Logs.db";
    var dbPath = Path.Combine(AppContext.BaseDirectory + "\\DB", dbFileName);
    //var dbPath = Path.Combine(AppContext.BaseDirectory, dbFileName);
    var dbLogsPath = Path.Combine(AppContext.BaseDirectory, dbLogFileName);
    var connectionString = $"Data Source={dbPath}";
    builder.Services.AddDbContext<PortfolioContext>(options =>
        options.UseSqlite(connectionString));

    // 👇 Настройка Serilog
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.SQLite(
            sqliteDbPath: dbLogsPath,
            tableName: "Logs",
            batchSize: 1)
        .Filter.ByExcluding(logEvent =>
            logEvent.Properties.ContainsKey("SourceContext") &&
            (
                logEvent.Properties["SourceContext"].ToString().StartsWith("\"Microsoft") ||
                logEvent.Properties["SourceContext"].ToString().StartsWith("\"System")
            )
        ).CreateLogger();

    builder.Host.UseSerilog();

    Log.Information("Старт приложения");

    // 👇 Регистрация сервисов
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllersWithViews();
    builder.Services.AddHttpClient<TelegramService>();
    builder.Services.AddSingleton<MailService>();
    //builder.WebHost.UseUrls("https://0.0.0.0:6688");
    var app = builder.Build();
    // Program.cs или Startup.cs
    //using (var scope = app.Services.CreateScope())
    //{
    //    var db = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
    //    db.Database.Migrate();

    //}


    // 👇 Конфигурация пайплайна
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseStaticFiles();
    app.UseMiddleware<VisitorLoggingMiddleware>();


    app.MapStaticAssets();

    // 👇 Маршруты

    app.MapControllerRoute(
    name: "home",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


    app.MapControllerRoute(
        name: "admin",
        pattern: "{controller=Admin}/{action=Index}/{id?}")
        .WithStaticAssets();



    app.Run();
}
catch (Exception ex)
{
    Log.Error(ex, "Ошибка при запуске приложения");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
// Этот код запускает приложение ASP.NET Core с использованием Serilog для логирования и SQLite в качестве базы данных.
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Получаем строку подключения к SQLite
var sqliteConnectionString = builder.Configuration.GetConnectionString("LogDb") ?? "Data Source=log.db;";

// Настройка Serilog для записи логов в SQLite
Log.Logger = new LoggerConfiguration()
    .WriteTo.SQLite(sqliteConnectionString, tableName: "Logs", batchSize: 1)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.WebHost.UseUrls("https://0.0.0.0:6688");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

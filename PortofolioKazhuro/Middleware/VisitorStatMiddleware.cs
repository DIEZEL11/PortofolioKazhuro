using Microsoft.EntityFrameworkCore;
using PortofolioKazhuro.Context;
using PortofolioKazhuro.Models;

public class VisitorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    public VisitorLoggingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, PortfolioContext db)
    {
        var path = context.Request.Path.Value;
        if (path.EndsWith(".css")
            || path.EndsWith(".js")
            || path.EndsWith(".png")
            || path.EndsWith(".jpg")
            || path.StartsWith("/lib"))
        {
            await _next(context);
            return;
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;

        var lastVisitTime = await db.VisitorStats
            .Where(v => v.IpAddress == ip)
            .OrderByDescending(v => v.VisitTime)
            .Select(v => v.VisitTime)
            .FirstOrDefaultAsync();

        if (lastVisitTime == default || (now - lastVisitTime) > TimeSpan.FromSeconds(10))
        {
            db.VisitorStats.Add(new VisitorStat
            {
                IpAddress = ip,
                VisitTime = now
            });
            await db.SaveChangesAsync();
        }

        await _next(context);
    }
}

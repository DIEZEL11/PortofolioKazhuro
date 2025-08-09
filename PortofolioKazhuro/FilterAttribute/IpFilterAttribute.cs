using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

public class IpFilterAttribute : ActionFilterAttribute
{
    private readonly string[] allowedIps;

    public IpFilterAttribute(params string[] ips)
    {
        allowedIps = ips;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();

        if (!allowedIps.Contains(ip))
        {
            context.Result = new StatusCodeResult(403); // Доступ запрещён
        }

        base.OnActionExecuting(context);
    }
}

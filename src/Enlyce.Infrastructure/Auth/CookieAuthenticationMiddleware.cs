using Microsoft.AspNetCore.Http;

namespace Enlyce.Infrastructure.Auth;

public sealed class CookieAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public CookieAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            if (context.Request.Cookies.TryGetValue("_enlyce_auth", out var token))
            {
                context.Request.Headers["Authorization"] = $"Bearer {token}";
            }
        }

        await _next(context);
    }
}

namespace Enlyce.Api.Middleware;

public sealed class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
            // En desarrollo, Swagger necesita cargar su propio JS/CSS: no se le aplica CSP.
            var allowSwaggerDev = environment.IsDevelopment() && context.Request.Path.StartsWithSegments("/swagger");
            if (!allowSwaggerDev)
            {
                // Las rutas /api solo devuelven JSON: portero estricto que no permite cargar nada.
                // El resto (documento HTML de la SPA) debe poder cargar su propio bundle, estilos y fuentes;
                // sin esta distincion, default-src 'none' bloquea el script de la SPA y la pagina queda en blanco.
                var isApi = context.Request.Path.StartsWithSegments("/api");
                headers["Content-Security-Policy"] = isApi
                    ? "default-src 'none'; frame-ancestors 'none'; base-uri 'none'"
                    : "default-src 'none'; " +
                      "script-src 'self'; " +
                      "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                      "font-src 'self' https://fonts.gstatic.com; " +
                      "img-src 'self' data: blob:; " +
                      "connect-src 'self'; " +
                      "base-uri 'none'; frame-ancestors 'none'; form-action 'none'";
            }
            return Task.CompletedTask;
        });

        return next(context);
    }
}

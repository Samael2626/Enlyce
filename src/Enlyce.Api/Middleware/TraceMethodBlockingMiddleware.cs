namespace Enlyce.Api.Middleware;

// Rechaza TRACE y TRACK con 405 en cualquier ruta. Vive en la API porque el
// hosting (Railway) no ofrece un proxy configurable y el fallback de la SPA
// respondia 200 a cualquier metodo.
public sealed class TraceMethodBlockingMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        if (HttpMethods.IsTrace(method) || string.Equals(method, "TRACK", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            context.Response.Headers.Allow = "GET, HEAD, POST, PUT, PATCH, DELETE, OPTIONS";
            return Task.CompletedTask;
        }

        return next(context);
    }
}

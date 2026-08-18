using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Api.Endpoints.Health;

public static class HealthCheck
{
    public static void MapHealth(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async (EnlyceDbContext context) =>
        {
            var canConnect = await context.Database.CanConnectAsync();
            return canConnect
                ? Results.Ok(new { status = "healthy", database = "connected" })
                : Results.StatusCode(503);
        })
        .WithName("HealthCheck")
        .AllowAnonymous();
    }
}

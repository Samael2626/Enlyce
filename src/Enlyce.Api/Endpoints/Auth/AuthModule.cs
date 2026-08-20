using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Enlyce.Application.Commands.Login;
using Enlyce.Application.Commands.RegisterAsesor;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Api.Endpoints.Auth;

public static class AuthModule
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (
            LoginCommand command,
            LoginCommandHandler handler,
            HttpContext http) =>
        {
            var result = await handler.HandleAsync(command);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                MaxAge = TimeSpan.FromHours(8)
            };

            http.Response.Cookies.Append("_enlyce_auth", result.Token, cookieOptions);

            return Results.Ok(new { result.Token, result.Rol, result.Nombre });
        })
        .WithName("Login");

        group.MapPost("/register", async (
            RegisterAsesorCommand command,
            RegisterAsesorCommandHandler handler) =>
        {
            var result = await handler.HandleAsync(command);
            return Results.Created($"/api/asesores/{result.Id}", result);
        })
        .RequireAuthorization("Administrador")
        .WithName("RegisterAsesor");

        group.MapPost("/logout", (HttpContext http) =>
        {
            http.Response.Cookies.Delete("_enlyce_auth");
            return Results.Ok(new { message = "Sesion cerrada." });
        })
        .RequireAuthorization()
        .WithName("Logout");

        group.MapGet("/me", (HttpContext http) =>
        {
            var user = http.User;
            return Results.Ok(new
            {
                Id = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
                Email = user.FindFirst(ClaimTypes.Email)?.Value,
                Nombre = user.FindFirst("nombre")?.Value,
                Rol = user.FindFirst(ClaimTypes.Role)?.Value
            });
        })
        .RequireAuthorization()
        .WithName("Me");

        // Seed endpoint - crear admin por defecto si no existe.
        // Solo en Development: crea un admin con credenciales conocidas sin autenticacion.
        group.MapPost("/seed", async (EnlyceDbContext db, IWebHostEnvironment env) =>
        {
            if (!env.IsDevelopment())
                return Results.NotFound();

            var adminEmail = "admin@enlyce.com";
            var adminPassword = "Admin123!";

            if (await db.Asesores.AnyAsync(a => a.Correo.Value == adminEmail))
                return Results.Ok(new { message = "Admin ya existe.", email = adminEmail });

            var correo = Email.Create(adminEmail);
            var hash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
            var admin = Asesor.Crear("Administrador", correo, hash, "Administrador");
            db.Asesores.Add(admin);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                message = "Admin creado.",
                email = adminEmail,
                password = adminPassword
            });
        })
        .WithName("SeedAdmin");
    }
}

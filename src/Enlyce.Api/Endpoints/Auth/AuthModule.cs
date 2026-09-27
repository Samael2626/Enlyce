using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net;
using Enlyce.Application.Commands.Login;
using Enlyce.Application.Commands.RegisterAsesor;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Enlyce.Application.Auth;
using Enlyce.Api.Security;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Enlyce.Api.Endpoints.Auth;

public static class AuthModule
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        // Publico: el usuario necesita obtener el token antes de autenticarse.
        group.MapPost("/login", async (
            LoginCommand command,
            LoginCommandHandler handler,
            LoginAttemptGuard attempts,
            IOptions<JwtSettings> jwtSettings,
            HttpContext http) =>
        {
            if (command.Correo.Length > 320 || command.Password.Length > 200)
                return Results.BadRequest(new { message = "Datos de acceso no válidos." });

            if (!attempts.CanAttempt(command.Correo, out var retryAfter))
            {
                http.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString();
                return Results.Problem(statusCode: StatusCodes.Status429TooManyRequests,
                    title: "Demasiados intentos. Intenta más tarde.");
            }

            LoginResult result;
            try
            {
                result = await handler.HandleAsync(command);
                attempts.Reset(command.Correo);
            }
            catch (UnauthorizedAccessException)
            {
                attempts.RegisterFailure(command.Correo);
                throw;
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                MaxAge = TimeSpan.FromMinutes(jwtSettings.Value.ExpirationMinutes)
            };

            http.Response.Cookies.Append("_enlyce_auth", result.Token, cookieOptions);

            return Results.Ok(new { result.Rol, result.Nombre });
        })
        .AllowAnonymous()
        .RequireRateLimiting("auth-login")
        .WithMetadata(new RequestSizeLimitAttribute(16 * 1024))
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

        group.MapPost("/logout", async (HttpContext http, IAsesorRepository asesores) =>
        {
            var subject = http.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(subject, out var asesorId))
            {
                var asesor = await asesores.ObtenerPorIdAsync(asesorId);
                if (asesor is not null)
                {
                    asesor.RevocarSesiones();
                    await asesores.GuardarAsync(asesor);
                }
            }

            http.Response.Cookies.Delete("_enlyce_auth", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
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

        // Publico solo para bootstrap local explicito; Development por si solo no habilita el seed.
        group.MapPost("/seed", async (
            EnlyceDbContext db,
            IWebHostEnvironment env,
            IConfiguration config,
            HttpContext http,
            ILoggerFactory loggerFactory) =>
        {
            var password = config["Auth:DevelopmentSeedPassword"];
            if (!env.IsDevelopment() ||
                !config.GetValue<bool>("Auth:AllowDevelopmentSeed") ||
                !IPAddress.IsLoopback(http.Connection.RemoteIpAddress ?? IPAddress.None) ||
                string.IsNullOrWhiteSpace(password))
            {
                loggerFactory.CreateLogger("Auth.Seed")
                    .LogWarning("Intento de invocar el seed de administracion sin habilitacion local explicita");
                return Results.NotFound();
            }

            var adminEmail = "admin@enlyce.com";

            if (await db.Asesores.AnyAsync(a => a.Correo.Value == adminEmail))
                return Results.Ok(new { message = "Admin ya existe.", email = adminEmail });

            var correo = Email.Create(adminEmail);
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var admin = Asesor.Crear("Administrador", correo, hash, "Administrador");
            db.Asesores.Add(admin);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                message = "Admin creado.",
                email = adminEmail
            });
        })
        .AllowAnonymous()
        .WithName("SeedAdmin");
    }
}

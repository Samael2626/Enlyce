using System.Security.Claims;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.WebsiteConfig;

public static class WebsiteConfigModule
{
    public static void MapWebsiteConfig(this IEndpointRouteBuilder app)
    {
        // Public endpoint consumed by Next.js website
        app.MapGet("/api/public/sitio-web", async (
            IPublicWebsiteConfigRepository repository,
            CancellationToken ct) =>
        {
            var config = await repository.GetAsync(ct);
            return Results.Ok(ToResponse(config));
        })
        .AllowAnonymous()
        .WithName("GetPublicWebsiteConfig")
        .Produces<PublicWebsiteConfigResponse>();

        // Admin CRM endpoints
        app.MapGet("/api/configuracion/sitio-web", async (
            IPublicWebsiteConfigRepository repository,
            CancellationToken ct) =>
        {
            var config = await repository.GetAsync(ct);
            return Results.Ok(ToResponse(config));
        })
        .RequireAuthorization("Administrador")
        .WithName("GetAdminWebsiteConfig")
        .Produces<PublicWebsiteConfigResponse>();

        app.MapPut("/api/configuracion/sitio-web", async (
            [FromBody] UpdateWebsiteConfigRequest request,
            IPublicWebsiteConfigRepository repository,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.AgencyName) ||
                string.IsNullOrWhiteSpace(request.HeroTitle) ||
                string.IsNullOrWhiteSpace(request.ContactPhone) ||
                string.IsNullOrWhiteSpace(request.ContactEmail))
            {
                return Results.BadRequest(new { error = "El nombre de la agencia, título principal, teléfono y correo son obligatorios." });
            }

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? updatedByUserId = Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : null;

            var config = new PublicWebsiteConfig
            {
                Id = 1,
                AgencyName = request.AgencyName.Trim(),
                Slogan = request.Slogan?.Trim() ?? string.Empty,
                HeroTitle = request.HeroTitle.Trim(),
                HeroSubtitle = request.HeroSubtitle?.Trim() ?? string.Empty,
                HeroBannerUrl = string.IsNullOrWhiteSpace(request.HeroBannerUrl) ? null : request.HeroBannerUrl.Trim(),
                AboutTitle = request.AboutTitle?.Trim() ?? "Quiénes somos",
                AboutText = request.AboutText?.Trim() ?? string.Empty,
                ContactPhone = request.ContactPhone.Trim(),
                ContactWhatsapp = request.ContactWhatsapp?.Trim() ?? string.Empty,
                ContactEmail = request.ContactEmail.Trim(),
                Address = request.Address?.Trim() ?? string.Empty,
                InstagramUrl = string.IsNullOrWhiteSpace(request.InstagramUrl) ? null : request.InstagramUrl.Trim(),
                FacebookUrl = string.IsNullOrWhiteSpace(request.FacebookUrl) ? null : request.FacebookUrl.Trim(),
                SeoTitle = request.SeoTitle?.Trim() ?? request.AgencyName.Trim(),
                SeoDescription = request.SeoDescription?.Trim() ?? string.Empty,
                PrimaryColor = request.PrimaryColor?.Trim() ?? "#0f1a3d",
                AccentColor = request.AccentColor?.Trim() ?? "#b78a2d",
                UpdatedAtUtc = DateTime.UtcNow,
                UpdatedByUserId = updatedByUserId
            };

            await repository.UpdateAsync(config, ct);
            return Results.Ok(ToResponse(config));
        })
        .RequireAuthorization("Administrador")
        .WithName("UpdateWebsiteConfig")
        .Produces<PublicWebsiteConfigResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static PublicWebsiteConfigResponse ToResponse(PublicWebsiteConfig config) => new(
        config.AgencyName,
        config.Slogan,
        config.HeroTitle,
        config.HeroSubtitle,
        config.HeroBannerUrl,
        config.AboutTitle,
        config.AboutText,
        config.ContactPhone,
        config.ContactWhatsapp,
        config.ContactEmail,
        config.Address,
        config.InstagramUrl,
        config.FacebookUrl,
        config.SeoTitle,
        config.SeoDescription,
        config.PrimaryColor,
        config.AccentColor,
        config.UpdatedAtUtc);
}

public sealed record PublicWebsiteConfigResponse(
    string AgencyName,
    string Slogan,
    string HeroTitle,
    string HeroSubtitle,
    string? HeroBannerUrl,
    string AboutTitle,
    string AboutText,
    string ContactPhone,
    string ContactWhatsapp,
    string ContactEmail,
    string Address,
    string? InstagramUrl,
    string? FacebookUrl,
    string SeoTitle,
    string SeoDescription,
    string? PrimaryColor,
    string? AccentColor,
    DateTime UpdatedAtUtc);

public sealed record UpdateWebsiteConfigRequest(
    string AgencyName,
    string? Slogan,
    string HeroTitle,
    string? HeroSubtitle,
    string? HeroBannerUrl,
    string? AboutTitle,
    string? AboutText,
    string ContactPhone,
    string? ContactWhatsapp,
    string ContactEmail,
    string? Address,
    string? InstagramUrl,
    string? FacebookUrl,
    string? SeoTitle,
    string? SeoDescription,
    string? PrimaryColor,
    string? AccentColor);

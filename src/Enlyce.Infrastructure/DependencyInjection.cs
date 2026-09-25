using Enlyce.Application.Auth;
using Enlyce.Application.Billing;
using Enlyce.Application.PublicCatalog;
using Enlyce.Domain.Ports;
using Enlyce.Infrastructure.Auth;
using Enlyce.Infrastructure.Billing;
using Enlyce.Infrastructure.Email;
using Enlyce.Infrastructure.Media;
using Enlyce.Infrastructure.Persistence;
using Enlyce.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EnlyceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IPropertyPublicationRepository, PropertyPublicationRepository>();
        services.AddScoped<IInmuebleRepository, InmuebleRepository>();
        services.AddScoped<IPropietarioRepository, PropietarioRepository>();
        services.AddScoped<IAsesorRepository, AsesorRepository>();
        services.AddScoped<IConsentimientoRepository, ConsentimientoRepository>();
        services.AddScoped<IPoliticaTratamientoRepository, PoliticaTratamientoRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IInteraccionRepository, InteraccionRepository>();
        services.AddScoped<IVisitaRepository, VisitaRepository>();
        services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
        services.AddScoped<IPublicPropertyReadRepository, PublicPropertyRepository>();
        services.Configure<MediaStorageOptions>(configuration.GetSection(MediaStorageOptions.SectionName));
        services.AddSingleton<IMediaStorage, LocalMediaStorage>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.Configure<WompiOptions>(configuration.GetSection(WompiOptions.SectionName));
        services.AddSingleton<WompiPaymentGateway>();
        services.AddSingleton<IPaymentCheckoutGateway>(provider => provider.GetRequiredService<WompiPaymentGateway>());
        services.AddSingleton<IPaymentEventVerifier>(provider => provider.GetRequiredService<WompiPaymentGateway>());

        return services;
    }
}

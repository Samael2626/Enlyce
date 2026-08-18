using Enlyce.Application.Auth;
using Enlyce.Domain.Ports;
using Enlyce.Infrastructure.Auth;
using Enlyce.Infrastructure.Email;
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
        services.AddScoped<IInmuebleRepository, InmuebleRepository>();
        services.AddScoped<IPropietarioRepository, PropietarioRepository>();
        services.AddScoped<IAsesorRepository, AsesorRepository>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}

using Enlyce.Application.Abstractions;
using Enlyce.Application.UseCases.CreateInmueble;
using Enlyce.Application.UseCases.CreateLead;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateLeadCommand, CreateLeadResponse>, CreateLeadHandler>();
        services.AddScoped<ICommandHandler<CreateInmuebleCommand, CreateInmuebleResponse>, CreateInmuebleHandler>();

        return services;
    }
}

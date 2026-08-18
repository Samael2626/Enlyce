using Enlyce.Application.Abstractions;
using Enlyce.Application.Commands.Login;
using Enlyce.Application.Commands.RegisterAsesor;
using Enlyce.Application.UseCases.CreateInmueble;
using Enlyce.Application.UseCases.CreateLead;
using Enlyce.Application.UseCases.GetInmuebleById;
using Enlyce.Application.UseCases.GetLeadById;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateLeadCommand, CreateLeadResponse>, CreateLeadHandler>();
        services.AddScoped<ICommandHandler<CreateInmuebleCommand, CreateInmuebleResponse>, CreateInmuebleHandler>();
        services.AddScoped<IQueryHandler<GetLeadByIdQuery, GetLeadByIdResponse?>, GetLeadByIdHandler>();
        services.AddScoped<IQueryHandler<GetInmuebleByIdQuery, GetInmuebleByIdResponse?>, GetInmuebleByIdHandler>();

        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<RegisterAsesorCommandHandler>();

        return services;
    }
}

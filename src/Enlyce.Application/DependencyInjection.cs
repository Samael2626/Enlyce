using Enlyce.Application.Abstractions;
using Enlyce.Application.Billing;
using Enlyce.Application.Commands.Consentimiento;
using Enlyce.Application.Commands.Lead;
using Enlyce.Application.Commands.Login;
using Enlyce.Application.Commands.RegisterAsesor;
using Enlyce.Application.Queries.Lead;
using Enlyce.Application.Queries.Politica;
using Enlyce.Application.PublicCatalog;
using Enlyce.Application.UseCases.CreateInmueble;
using Enlyce.Application.UseCases.CreateLead;
using Enlyce.Application.UseCases.GetInmuebleById;
using Enlyce.Application.UseCases.GetLeadById;
using Enlyce.Application.UseCases.UploadPublicationPhoto;
using Microsoft.Extensions.DependencyInjection;

namespace Enlyce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<ICommandHandler<CreateCheckoutSessionCommand, CreateCheckoutSessionResponse>, CreateCheckoutSessionHandler>();
        services.AddScoped<ProcessPaymentNotificationHandler>();
        services.AddScoped<ICommandHandler<CreateLeadCommand, CreateLeadResponse>, CreateLeadHandler>();
        services.AddScoped<ICommandHandler<CreateInmuebleCommand, CreateInmuebleResponse>, CreateInmuebleHandler>();
        services.AddScoped<ICommandHandler<UploadPublicationPhotoCommand, UploadPublicationPhotoResponse>, UploadPublicationPhotoHandler>();
        services.AddScoped<IQueryHandler<GetLeadByIdQuery, GetLeadByIdResponse?>, GetLeadByIdHandler>();
        services.AddScoped<IQueryHandler<GetInmuebleByIdQuery, GetInmuebleByIdResponse?>, GetInmuebleByIdHandler>();
        services.AddScoped<IQueryHandler<GetPublicPropertiesQuery, PublicPropertyPageResponse>, GetPublicPropertiesHandler>();
        services.AddScoped<IQueryHandler<GetPublicPropertyBySlugQuery, PublicPropertyDetailResponse?>, GetPublicPropertyBySlugHandler>();

        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<RegisterAsesorCommandHandler>();

        services.AddScoped<RegistrarConsentimientoHandler>();
        services.AddScoped<ObtenerPoliticaActivaHandler>();
        services.AddScoped<ConsultarDatosLeadHandler>();
        services.AddScoped<SuprimirDatosLeadHandler>();

        services.AddScoped<MoverEtapaHandler>();
        services.AddScoped<AsignarLeadHandler>();
        services.AddScoped<ReasignarLeadHandler>();
        services.AddScoped<RegistrarInteraccionHandler>();
        services.AddScoped<RegistrarVisitaHandler>();
        services.AddScoped<ConsultarPipelineHandler>();
        services.AddScoped<ObtenerAlertasHandler>();

        return services;
    }
}

using Amazon.S3;
using Enlyce.Application.Auth;
using Enlyce.Application.Billing;
using Enlyce.Application.PublicCatalog;
using Enlyce.Application.Publications;
using Enlyce.Domain.Ports;
using Enlyce.Application.Analytics;
using Enlyce.Application.Media;
using Enlyce.Infrastructure.Auth;
using Enlyce.Infrastructure.Billing;
using Enlyce.Infrastructure.Email;
using Enlyce.Infrastructure.Imports;
using Enlyce.Infrastructure.Media;
using Enlyce.Infrastructure.Persistence;
using Enlyce.Infrastructure.Persistence.Repositories;
using Enlyce.Application.Imports;
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
        services.AddScoped<ILeadAssignmentHistoryRepository, LeadAssignmentHistoryRepository>();
        services.AddScoped<ILeadStageHistoryRepository, LeadStageHistoryRepository>();
        services.AddScoped<ILeadSlaRuleRepository, LeadSlaRuleRepository>();
        services.AddScoped<ILeadSlaAlertsRepository, LeadSlaAlertsRepository>();
        services.AddScoped<ILeadDistributionSettingsRepository, LeadDistributionSettingsRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<ICustomerCatalogRepository, CustomerCatalogRepository>();
        services.AddScoped<ICustomerFunnelStageRepository, CustomerFunnelStageRepository>();
        services.AddScoped<ICommercialTaskRepository, CommercialTaskRepository>();
        services.AddScoped<ICustomerDemandRepository, CustomerDemandRepository>();
        services.AddScoped<IPropertyPublicationRepository, PropertyPublicationRepository>();
        services.AddScoped<IInmuebleRepository, InmuebleRepository>();
        services.AddScoped<Enlyce.Application.UseCases.ListInmuebles.IInmuebleListReadRepository, InmuebleListReadRepository>();
        services.AddScoped<IPropertyPhotoReferenceReadRepository, PropertyPhotoReferenceReadRepository>();
        services.AddScoped<IPropietarioRepository, PropietarioRepository>();
        services.AddScoped<IAsesorRepository, AsesorRepository>();
        services.AddScoped<IConsentimientoRepository, ConsentimientoRepository>();
        services.AddScoped<IPoliticaTratamientoRepository, PoliticaTratamientoRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IInteraccionRepository, InteraccionRepository>();
        services.AddScoped<IVisitaRepository, VisitaRepository>();
        services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
        services.AddScoped<IPublicPropertyReadRepository, PublicPropertyRepository>();
        services.AddScoped<IWebAnalyticsRepository, WebAnalyticsRepository>();
        services.AddScoped<IRealizedDealRepository, RealizedDealRepository>();
        services.AddScoped<IRealizedDealsReportReadRepository, RealizedDealsReportReadRepository>();
        services.AddScoped<IMarketAnalysisReadRepository, MarketAnalysisReadRepository>();
        services.AddScoped<IAdvisorPerformanceReportReadRepository, AdvisorPerformanceReportReadRepository>();
        services.AddScoped<IOwnerPerformanceReportReadRepository, OwnerPerformanceReportReadRepository>();
        services.AddScoped<ISentEmailReportReadRepository, SentEmailReportReadRepository>();
        services.AddScoped<IWebPropertyVisitsReportReadRepository, WebPropertyVisitsReportReadRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<IPublicWebsiteConfigRepository, PublicWebsiteConfigRepository>();
        services.AddScoped<IBlogPostRepository, BlogPostRepository>();
        services.AddScoped<Enlyce.Application.Imports.IImportedListingsReadRepository, ImportedListingsReadRepository>();
        services.AddSingleton<IClientImportTableReader, ClientImportTableReader>();
        services.AddSingleton<IPropertyImportTableReader, PropertyImportTableReader>();
        services.AddScoped<IPropertyPublicationAdminReadRepository, PropertyPublicationAdminReadRepository>();
        services.Configure<MediaStorageOptions>(configuration.GetSection(MediaStorageOptions.SectionName));
        services.AddSingleton<ImageVariantProcessor>();
        AddMediaStorage(services, configuration);
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddSingleton<SmtpEmailSender>();
        services.AddSingleton<IEmailSender>(provider => provider.GetRequiredService<SmtpEmailSender>());
        services.AddSingleton<IEmailAttachmentSender>(provider => provider.GetRequiredService<SmtpEmailSender>());
        services.Configure<WasiOptions>(configuration.GetSection(WasiOptions.SectionName));
        services.AddHttpClient<IExternalListingSource, WasiListingSource>(client =>
            client.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<ITokenService, JwtTokenService>();
        services.Configure<WompiOptions>(configuration.GetSection(WompiOptions.SectionName));
        services.AddSingleton<WompiPaymentGateway>();
        services.AddSingleton<IPaymentCheckoutGateway>(provider => provider.GetRequiredService<WompiPaymentGateway>());
        services.AddSingleton<IPaymentEventVerifier>(provider => provider.GetRequiredService<WompiPaymentGateway>());

        return services;
    }

    // Elige el adaptador de medios segun MediaStorage:Provider. "s3" registra un
    // cliente S3-compatible (Cloudflare R2) y el adaptador S3; cualquier otro
    // valor usa el adaptador local en disco.
    private static void AddMediaStorage(IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration[$"{MediaStorageOptions.SectionName}:Provider"] ?? "local";
        if (!string.Equals(provider, "s3", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IMediaStorage, LocalMediaStorage>();
            return;
        }

        var s3 = configuration.GetSection($"{MediaStorageOptions.SectionName}:S3").Get<S3StorageOptions>() ?? new S3StorageOptions();
        if (string.IsNullOrWhiteSpace(s3.ServiceUrl) || string.IsNullOrWhiteSpace(s3.AccessKey) || string.IsNullOrWhiteSpace(s3.SecretKey))
            throw new InvalidOperationException("Con MediaStorage:Provider=s3 configura ServiceUrl, AccessKey y SecretKey fuera del repositorio.");

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = s3.ServiceUrl,
                ForcePathStyle = s3.ForcePathStyle,
                AuthenticationRegion = s3.Region
            };
            return new AmazonS3Client(s3.AccessKey, s3.SecretKey, config);
        });
        services.AddSingleton<IMediaStorage, S3MediaStorage>();
        services.AddSingleton<IOrphanMediaStorage>(sp => (IOrphanMediaStorage)sp.GetRequiredService<IMediaStorage>());
        services.AddScoped<ICleanupOrphanMediaHandler, CleanupOrphanMediaHandler>();
    }
}

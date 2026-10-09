using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using Enlyce.Api.Endpoints.Billing;
using Enlyce.Api.Endpoints.Analytics;
using Enlyce.Api.Development;
using Enlyce.Api.Endpoints.Alertas;
using Enlyce.Api.Endpoints.Advisors;
using Enlyce.Api.Endpoints.Auth;
using Enlyce.Api.Endpoints.Contacts;
using Enlyce.Api.Endpoints.CsvExport;
using Enlyce.Api.Endpoints.Demands;
using Enlyce.Api.Endpoints.Tasks;
using Enlyce.Api.Endpoints.DatosPersonales;
using Enlyce.Api.Endpoints.Health;
using Enlyce.Api.Endpoints.Inmuebles;
using Enlyce.Api.Endpoints.Interacciones;
using Enlyce.Api.Endpoints.Leads;
using Enlyce.Api.Endpoints.LeadDistribution;
using Enlyce.Api.Endpoints.Pipeline;
using Enlyce.Api.Endpoints.Politica;
using Enlyce.Api.Endpoints.PublicationMedia;
using Enlyce.Api.Endpoints.Publications;
using Enlyce.Api.Endpoints.PublicCatalog;
using Enlyce.Api.Endpoints.Visitas;
using Enlyce.Api.Middleware;
using Enlyce.Api.Security;
using Enlyce.Application;
using Enlyce.Application.Auth;
using Enlyce.Domain.Ports;
using Enlyce.Infrastructure;
using Enlyce.Infrastructure.Auth;
using Enlyce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = 12 * 1024 * 1024;
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = false;
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var subject = context.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                ?? context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var version = context.Principal?.FindFirst("session_version")?.Value;
            if (!Guid.TryParse(subject, out var asesorId) || !int.TryParse(version, out var sessionVersion))
            {
                context.Fail("Sesion invalida.");
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<EnlyceDbContext>();
            var valid = await db.Asesores.AsNoTracking().AnyAsync(asesor =>
                asesor.Id == asesorId && asesor.Activo && asesor.SessionVersion == sessionVersion);
            if (!valid)
                context.Fail("Sesion revocada.");
        }
    };
});
builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<JwtSettings>>().Value;
    return new ConfigureNamedOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Administrador", policy => policy.RequireRole("Administrador"))
    .AddPolicy("Asesor", policy => policy.RequireRole("Asesor"));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<LoginAttemptGuard>();
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(180);
    options.IncludeSubDomains = true;
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, _) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString();

        return ValueTask.CompletedTask;
    };
    options.AddPolicy("auth-login", context => CreateIpLimiter(context, 10));
    options.AddPolicy("public-leads", context => CreateIpLimiter(context, 40));
    options.AddPolicy("public-analytics", context => CreateIpLimiter(context, 120));
    options.AddPolicy("owner-details", context => CreateIpLimiter(context, 20));
    options.AddPolicy("wompi-webhook", context => CreateIpLimiter(context, 120));
    options.AddPolicy("billing-checkout", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 8,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    options.AddPolicy("billing-status", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var localCorsOrigins = new[]
{
    "http://localhost:5173",
    "http://127.0.0.1:5173",
    "http://localhost:4173",
    "http://localhost:3000",
    "http://127.0.0.1:3000"
};
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // Origenes explicitos: AllowAnyOrigin es incompatible con AllowCredentials
        policy.WithOrigins(localCorsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();

        if (builder.Configuration.GetValue<bool>("Cors:AllowTrycloudflareOrigins"))
        {
            policy.SetIsOriginAllowed(origin =>
            {
                if (localCorsOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                    return true;

                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                    uri.Scheme != Uri.UriSchemeHttps ||
                    !uri.IsDefaultPort ||
                    uri.UserInfo.Length != 0 ||
                    uri.AbsolutePath != "/" ||
                    uri.Query.Length != 0 ||
                    uri.Fragment.Length != 0)
                    return false;

                const string suffix = ".trycloudflare.com";
                if (!uri.Host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    return false;

                var subdomain = uri.Host[..^suffix.Length];
                return subdomain.Length > 0 && !subdomain.Contains('.');
            });
        }
    });
});

// Solo se confia en X-Forwarded-For cuando hay un proxy declarado delante (el
// tunel de Cloudflare del demo). Sin esa configuracion el header se ignora: de
// lo contrario cualquiera podria falsear su IP en la auditoria de consentimiento.
var trustedProxies = builder.Configuration
    .GetSection("Forwarding:KnownProxies").Get<string[]>() ?? [];
var trustForwardedHeaders = builder.Configuration.GetValue<bool>("Forwarding:Enabled");

if (trustForwardedHeaders)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();

        foreach (var proxy in trustedProxies)
            if (IPAddress.TryParse(proxy, out var parsed))
                options.KnownProxies.Add(parsed);
    });
}

var app = builder.Build();

if (trustForwardedHeaders)
    app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

var signingKey = app.Services.GetRequiredService<IOptions<JwtSettings>>().Value.SecretKey;
if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32)
    throw new InvalidOperationException("Configura JwtSettings:SecretKey fuera del repositorio (minimo 32 bytes).");

if (app.Environment.IsDevelopment() &&
    app.Configuration.GetValue<bool>("DemoData:SeedPublicCatalog"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
    var mediaStorage = scope.ServiceProvider.GetRequiredService<IMediaStorage>();
    var photoSource = app.Configuration.GetValue<string>("DemoData:PhotoSourceDirectory")
        ?? Path.Combine(app.Environment.ContentRootPath, "..", "..", "website", "assets");

    await db.Database.MigrateAsync();
    await DemoCatalogSeeder.SeedAsync(db, mediaStorage, Path.GetFullPath(photoSource));
}

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

// Sirve las variantes generadas por LocalMediaStorage bajo /media.
var mediaRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "media");
Directory.CreateDirectory(mediaRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaRoot),
    RequestPath = "/media"
});

app.UseCors();
app.UseRateLimiter();
app.UseMiddleware<CookieAuthenticationMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealth();
app.MapLeads();
app.MapCsvExport();
app.MapLeadDistribution();
app.MapContacts();
app.MapTasks();
app.MapDemands();
app.MapInmuebles();
app.MapAuth();
app.MapPolitica();
app.MapDatosPersonales();
app.MapPipeline();
app.MapLeadHistory();
app.MapAdvisors();
app.MapInteracciones();
app.MapVisitas();
app.MapAlertas();
app.MapPublicCatalog();
app.MapPublicationMedia();
app.MapPropertyPublications();
app.MapBilling();
app.MapAnalytics();

static RateLimitPartition<string> CreateIpLimiter(HttpContext context, int permitLimit) =>
    RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });

app.Run();

public partial class Program { }

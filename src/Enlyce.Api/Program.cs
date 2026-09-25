using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using Enlyce.Api.Endpoints.Billing;
using Enlyce.Api.Development;
using Enlyce.Api.Endpoints.Alertas;
using Enlyce.Api.Endpoints.Auth;
using Enlyce.Api.Endpoints.DatosPersonales;
using Enlyce.Api.Endpoints.Health;
using Enlyce.Api.Endpoints.Inmuebles;
using Enlyce.Api.Endpoints.Interacciones;
using Enlyce.Api.Endpoints.Leads;
using Enlyce.Api.Endpoints.Pipeline;
using Enlyce.Api.Endpoints.Politica;
using Enlyce.Api.Endpoints.PublicationMedia;
using Enlyce.Api.Endpoints.PublicCatalog;
using Enlyce.Api.Endpoints.Visitas;
using Enlyce.Api.Middleware;
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

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = _ => Task.CompletedTask
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
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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
        options.KnownNetworks.Clear();

        foreach (var proxy in trustedProxies)
            if (IPAddress.TryParse(proxy, out var parsed))
                options.KnownProxies.Add(parsed);
    });
}

var app = builder.Build();

if (trustForwardedHeaders)
    app.UseForwardedHeaders();

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

app.UseMiddleware<ExceptionHandlerMiddleware>();
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
app.MapInmuebles();
app.MapAuth();
app.MapPolitica();
app.MapDatosPersonales();
app.MapPipeline();
app.MapInteracciones();
app.MapVisitas();
app.MapAlertas();
app.MapPublicCatalog();
app.MapPublicationMedia();
app.MapBilling();

app.Run();

public partial class Program { }

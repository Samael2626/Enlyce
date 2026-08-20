using System.Text;
using Enlyce.Api.Endpoints.Alertas;
using Enlyce.Api.Endpoints.Auth;
using Enlyce.Api.Endpoints.DatosPersonales;
using Enlyce.Api.Endpoints.Health;
using Enlyce.Api.Endpoints.Inmuebles;
using Enlyce.Api.Endpoints.Interacciones;
using Enlyce.Api.Endpoints.Leads;
using Enlyce.Api.Endpoints.Pipeline;
using Enlyce.Api.Endpoints.Politica;
using Enlyce.Api.Endpoints.Visitas;
using Enlyce.Api.Middleware;
using Enlyce.Application;
using Enlyce.Application.Auth;
using Enlyce.Infrastructure;
using Enlyce.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // Origenes explicitos: AllowAnyOrigin es incompatible con AllowCredentials
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:4173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();

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

app.Run();

public partial class Program { }

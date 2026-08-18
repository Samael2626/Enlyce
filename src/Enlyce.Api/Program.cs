using Enlyce.Api.Endpoints.Health;
using Enlyce.Api.Endpoints.Inmuebles;
using Enlyce.Api.Endpoints.Leads;
using Enlyce.Api.Middleware;
using Enlyce.Application;
using Enlyce.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapHealth();
app.MapLeads();
app.MapInmuebles();

app.Run();

public partial class Program { }

using Enlyce.Application.Abstractions;
using Enlyce.Application.UseCases.CreateInmueble;
using Enlyce.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Inmuebles;

public static class InmueblesModule
{
    public static void MapInmuebles(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inmuebles").WithTags("Inmuebles");

        group.MapPost("/", async (
            [FromBody] CreateInmuebleRequest request,
            ICommandHandler<CreateInmuebleCommand, CreateInmuebleResponse> handler) =>
        {
            var command = new CreateInmuebleCommand(
                request.Nombre, request.Descripcion, request.Tipo,
                request.Modalidad, request.Calle, request.Ciudad,
                request.Barrio, request.Precio, request.Moneda,
                request.MetrosCuadrados, request.Habitaciones,
                request.Banos, request.Parqueaderos, request.PropietarioId);

            var result = await handler.HandleAsync(command);
            return Results.Created($"/api/inmuebles/{result.Id}", result);
        })
        .WithName("CreateInmueble")
        .Produces<CreateInmuebleResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

public record CreateInmuebleRequest(
    string Nombre,
    string? Descripcion,
    TipoInmueble Tipo,
    ModalidadInmueble Modalidad,
    string Calle,
    string Ciudad,
    string? Barrio,
    decimal Precio,
    string Moneda,
    int MetrosCuadrados,
    int Habitaciones,
    int Banos,
    int Parqueaderos,
    Guid PropietarioId);

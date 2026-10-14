using Enlyce.Application.Abstractions;
using Enlyce.Application.UseCases.CreateInmueble;
using Enlyce.Application.UseCases.GetInmuebleById;
using Enlyce.Application.UseCases.ListInmuebles;
using Enlyce.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Inmuebles;

public static class InmueblesModule
{
    public static void MapInmuebles(this IEndpointRouteBuilder app)
    {
        // Inmueble solo referencia al propietario; no tiene asesor responsable.
        var group = app.MapGroup("/api/inmuebles").WithTags("Inmuebles").RequireAuthorization();

        group.MapGet("/", async (int? page, int? pageSize, string? search, string? modalidad, string? estado,
            IQueryHandler<ListInmueblesQuery, ListInmueblesResponse> handler, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(new ListInmueblesQuery(page ?? 1, pageSize ?? 20, search, modalidad, estado), ct)))
            .WithName("ListInmuebles");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IQueryHandler<GetInmuebleByIdQuery, GetInmuebleByIdResponse?> handler) =>
        {
            var result = await handler.HandleAsync(new GetInmuebleByIdQuery(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetInmuebleById")
        .Produces<GetInmuebleByIdResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
            [FromBody] CreateInmuebleRequest request,
            ICommandHandler<CreateInmuebleCommand, CreateInmuebleResponse> handler) =>
        {
            var command = new CreateInmuebleCommand(
                request.Nombre, request.Descripcion, request.Tipo,
                request.Modalidad, request.Calle, request.Ciudad,
                request.Barrio, request.Precio, request.Moneda,
                request.MetrosCuadrados, request.Habitaciones,
                request.Banos, request.Parqueaderos, request.PropietarioId, request.Pais, request.Departamento);

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
    Guid PropietarioId,
    string Pais = "Colombia",
    string? Departamento = null);

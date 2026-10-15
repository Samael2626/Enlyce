using Enlyce.Application.Abstractions;
using Enlyce.Application.UseCases.CreateInmueble;
using Enlyce.Application.UseCases.GetInmuebleById;
using Enlyce.Application.UseCases.ListInmuebles;
using Enlyce.Application.UseCases.UpdateInmueble;
using Enlyce.Application.UseCases.ManageInmuebleTrash;
using Enlyce.Domain.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Inmuebles;

public static class InmueblesModule
{
    public static void MapInmuebles(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inmuebles").WithTags("Inmuebles").RequireAuthorization();

        group.MapGet("/", async (int? page, int? pageSize, string? search, string? modalidad, string? estado,
            string? tipo, string? pais, string? departamento, string? ciudad, string? barrio, string? moneda,
            decimal? precioDesde, decimal? precioHasta, int? areaDesde, int? areaHasta,
            int? habitaciones, int? banos, int? parqueaderos, string? orden, bool? papelera, string? codigoWasi,
            int? construidoDesde, string? etiqueta, Guid? asesorResponsableId,
            IQueryHandler<ListInmueblesQuery, ListInmueblesResponse> handler, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var isAdministrator = user.IsInRole("Administrador");
            var currentAdvisorId = EndpointAccess.AdvisorId(user);
            if (!isAdministrator && user.IsInRole("Asesor") && currentAdvisorId is null)
                return Results.Unauthorized();

            if (page is < 1 || pageSize is < 1 or > 100 || search?.Length > 200 ||
                !ValidEnum<ModalidadInmueble>(modalidad) || !ValidEnum<EstadoInmueble>(estado) ||
                !ValidEnum<TipoInmueble>(tipo) || !ValidRange(precioDesde, precioHasta) ||
                !ValidRange(areaDesde, areaHasta) || habitaciones < 0 || banos < 0 || parqueaderos < 0 ||
                new[] { pais, departamento, ciudad, barrio }.Any(value => value?.Length > 100) ||
                moneda?.Length > 3 || codigoWasi?.Length > 100 || construidoDesde is < 1 or > 9999 || etiqueta?.Length > 50 ||
                ((precioDesde.HasValue || precioHasta.HasValue) && string.IsNullOrWhiteSpace(moneda)) ||
                orden is not (null or "recent" or "oldest" or "priceAsc" or "priceDesc"))
                return Results.BadRequest(new { error = "Los filtros del inventario no son validos." });

            return Results.Ok(await handler.HandleAsync(new ListInmueblesQuery(
                page ?? 1, pageSize ?? 20, search, modalidad, estado, tipo, pais, departamento, ciudad, barrio,
                moneda?.ToUpperInvariant(), precioDesde, precioHasta, areaDesde, areaHasta,
                habitaciones, banos, parqueaderos, orden ?? "recent", papelera ?? false, codigoWasi, construidoDesde, etiqueta,
                isAdministrator ? asesorResponsableId : currentAdvisorId,
                !isAdministrator && user.IsInRole("Asesor")), ct));
        })
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
            ICommandHandler<CreateInmuebleCommand, CreateInmuebleResponse> handler, HttpContext http) =>
        {
            if (request.AnioConstruccion is < 1 or > 9999 || request.EstadoFisico?.Length > 100)
                return Results.BadRequest(new { error = "Revise el año de construcción y el estado físico del inmueble." });

            var advisorId = http.User.IsInRole("Administrador")
                ? request.AsesorResponsableId
                : EndpointAccess.AdvisorId(http.User);
            if (http.User.IsInRole("Asesor") && advisorId is null)
                return Results.Unauthorized();

            var command = new CreateInmuebleCommand(
                request.Nombre, request.Descripcion, request.Tipo,
                request.Modalidad, request.Calle, request.Ciudad,
                request.Barrio, request.Precio, request.Moneda,
                request.MetrosCuadrados, request.Habitaciones,
                request.Banos, request.Parqueaderos, request.PropietarioId, request.Pais, request.Departamento,
                request.CaracteristicasInternas, request.CaracteristicasExternas, request.AnioConstruccion,
                request.EstadoFisico, request.Etiquetas, advisorId);

            CreateInmuebleResponse result;
            try { result = await handler.HandleAsync(command); }
            catch (InvalidOperationException error) { return Results.BadRequest(new { error = error.Message }); }
            return Results.Created($"/api/inmuebles/{result.Id}", result);
        })
        .WithName("CreateInmueble")
        .Produces<CreateInmuebleResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (
            Guid id, [FromBody] UpdateInmuebleRequest request,
            ICommandHandler<UpdateInmuebleCommand, CreateInmuebleResponse?> handler, HttpContext http, CancellationToken ct) =>
        {
            if (request.AnioConstruccion is < 1 or > 9999 || request.EstadoFisico?.Length > 100)
                return Results.BadRequest(new { error = "Revise el año de construcción y el estado físico del inmueble." });

            var command = new UpdateInmuebleCommand(id, request.Nombre, request.Descripcion, request.Tipo,
                request.Modalidad, request.Calle, request.Ciudad, request.Barrio, request.Precio,
                request.Moneda, request.MetrosCuadrados, request.Habitaciones, request.Banos,
                request.Parqueaderos, request.Pais, request.Departamento,
                request.CaracteristicasInternas, request.CaracteristicasExternas, request.AnioConstruccion,
                request.EstadoFisico, request.Etiquetas,
                http.User.IsInRole("Administrador") ? request.AsesorResponsableId : null,
                http.User.IsInRole("Administrador"));
            try
            {
                var result = await handler.HandleAsync(command, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (Enlyce.Domain.Errors.DomainError error) { return Results.BadRequest(new { error = error.Message }); }
            catch (InvalidOperationException error) { return Results.BadRequest(new { error = error.Message }); }
        })
        .WithName("UpdateInmueble")
        .Produces<CreateInmuebleResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/papelera", ChangeTrashStateAsync(false))
            .WithName("MoveInmuebleToTrash")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/restaurar", ChangeTrashStateAsync(true))
            .WithName("RestoreInmuebleFromTrash")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/lote/papelera", ChangeTrashStatesAsync(false))
            .WithName("MoveInmueblesToTrash")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/lote/restaurar", ChangeTrashStatesAsync(true))
            .WithName("RestoreInmueblesFromTrash")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static Func<Guid, ICommandHandler<ChangeInmuebleTrashStateCommand, bool>, CancellationToken, Task<IResult>>
        ChangeTrashStateAsync(bool restore) => async (id, handler, ct) =>
    {
        var changed = await handler.HandleAsync(new ChangeInmuebleTrashStateCommand(id, restore), ct);
        return changed ? Results.NoContent() : Results.NotFound();
    };

    private static Func<BatchInmuebleTrashRequest, ICommandHandler<ChangeInmueblesTrashStateCommand, bool>, CancellationToken, Task<IResult>>
        ChangeTrashStatesAsync(bool restore) => async (request, handler, ct) =>
    {
        if (request.Ids is not { Length: > 0 and <= 100 } || request.Ids.Distinct().Count() != request.Ids.Length)
            return Results.BadRequest(new { error = "Envie entre 1 y 100 inmuebles sin IDs repetidos." });

        var changed = await handler.HandleAsync(new ChangeInmueblesTrashStateCommand(request.Ids, restore), ct);
        return changed ? Results.NoContent() : Results.Conflict(new { error = "La seleccion cambio o contiene inmuebles inexistentes; no se modifico ninguno." });
    };

    private static bool ValidEnum<T>(string? value) where T : struct, Enum =>
        string.IsNullOrWhiteSpace(value) || Enum.TryParse<T>(value, true, out var parsed) && Enum.IsDefined(parsed);

    private static bool ValidRange<T>(T? from, T? to) where T : struct, IComparable<T> =>
        (!from.HasValue || from.Value.CompareTo(default) >= 0) &&
        (!to.HasValue || to.Value.CompareTo(default) >= 0) &&
        (!from.HasValue || !to.HasValue || from.Value.CompareTo(to.Value) <= 0);
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
    string? Departamento = null,
    string[]? CaracteristicasInternas = null,
    string[]? CaracteristicasExternas = null,
    int? AnioConstruccion = null,
    string? EstadoFisico = null, string[]? Etiquetas = null, Guid? AsesorResponsableId = null);

public record UpdateInmuebleRequest(
    string Nombre, string? Descripcion, TipoInmueble Tipo, ModalidadInmueble Modalidad,
    string Calle, string Ciudad, string? Barrio, decimal Precio, string Moneda,
    int MetrosCuadrados, int Habitaciones, int Banos, int Parqueaderos,
    string Pais = "Colombia", string? Departamento = null,
    string[]? CaracteristicasInternas = null, string[]? CaracteristicasExternas = null,
    int? AnioConstruccion = null, string? EstadoFisico = null, string[]? Etiquetas = null,
    Guid? AsesorResponsableId = null);

public record BatchInmuebleTrashRequest(Guid[]? Ids);

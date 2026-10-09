using Enlyce.Application.Abstractions;
using Enlyce.Application.Publications;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Publications;

public static class PropertyPublicationsModule
{
    public static void MapPropertyPublications(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/publicaciones")
            .WithTags("Publicaciones")
            .RequireAuthorization();

        group.MapGet("/", async (
            HttpContext http,
            IQueryHandler<GetPropertyPublicationsQuery, IReadOnlyList<PropertyPublicationAdminData>> handler,
            CancellationToken ct) =>
        {
            var advisorId = ResolveAdvisorScope(http);
            if (!http.User.IsInRole("Administrador") && advisorId is null)
                return Results.Forbid();

            return Results.Ok(await handler.HandleAsync(new(advisorId), ct));
        })
        .WithName("GetPropertyPublications")
        .Produces<IReadOnlyList<PropertyPublicationAdminData>>();

        group.MapGet("/opciones", async (
            HttpContext http,
            IQueryHandler<GetPropertyPublicationOptionsQuery, PropertyPublicationOptions> handler,
            CancellationToken ct) =>
        {
            var advisorId = ResolveAdvisorScope(http);
            if (!http.User.IsInRole("Administrador") && advisorId is null)
                return Results.Forbid();

            return Results.Ok(await handler.HandleAsync(new(advisorId), ct));
        })
        .WithName("GetPropertyPublicationOptions")
        .Produces<PropertyPublicationOptions>();

        group.MapPost("/", async (
            [FromBody] CreatePropertyPublicationRequest request,
            HttpContext http,
            ICommandHandler<CreatePropertyPublicationCommand, PropertyPublicationMutationResponse> handler,
            CancellationToken ct) =>
        {
            var advisorId = http.User.IsInRole("Administrador")
                ? request.AdvisorId
                : EndpointAccess.AdvisorId(http.User);
            if (advisorId is null || advisorId == Guid.Empty)
                return Results.Forbid();

            var result = await handler.HandleAsync(new(
                request.PropertyId,
                advisorId.Value,
                request.Slug,
                request.PublicTitle,
                request.PublicDescription,
                request.PriceAmount,
                request.PriceCurrency,
                request.Municipality,
                request.Neighborhood,
                request.ApproximateLatitude,
                request.ApproximateLongitude), ct);
            return Results.Created($"/api/publicaciones/{result.Id}", result);
        })
        .WithName("CreatePropertyPublication")
        .Produces<PropertyPublicationMutationResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdatePropertyPublicationRequest request,
            HttpContext http,
            IPropertyPublicationRepository publications,
            ICommandHandler<UpdatePropertyPublicationCommand, PropertyPublicationMutationResponse> handler,
            CancellationToken ct) =>
        {
            if (!await EndpointAccess.CanAccessPublicationAsync(http.User, id, publications, ct))
                return Results.Forbid();

            var result = await handler.HandleAsync(new(
                id,
                request.Slug,
                request.PublicTitle,
                request.PublicDescription,
                request.PriceAmount,
                request.PriceCurrency,
                request.Municipality,
                request.Neighborhood,
                request.ApproximateLatitude,
                request.ApproximateLongitude), ct);
            return Results.Ok(result);
        })
        .WithName("UpdatePropertyPublication")
        .Produces<PropertyPublicationMutationResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}/estado", async (
            Guid id,
            [FromBody] ChangePropertyPublicationStatusRequest request,
            HttpContext http,
            IPropertyPublicationRepository publications,
            ICommandHandler<ChangePropertyPublicationStatusCommand, PropertyPublicationMutationResponse> handler,
            CancellationToken ct) =>
        {
            if (!await EndpointAccess.CanAccessPublicationAsync(http.User, id, publications, ct))
                return Results.Forbid();

            return Results.Ok(await handler.HandleAsync(new(id, request.Action), ct));
        })
        .WithName("ChangePropertyPublicationStatus")
        .Produces<PropertyPublicationMutationResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static Guid? ResolveAdvisorScope(HttpContext http) =>
        http.User.IsInRole("Administrador") ? null : EndpointAccess.AdvisorId(http.User);
}

public sealed record CreatePropertyPublicationRequest(
    Guid PropertyId,
    Guid? AdvisorId,
    string Slug,
    string PublicTitle,
    string? PublicDescription,
    decimal PriceAmount,
    string PriceCurrency,
    string Municipality,
    string Neighborhood,
    decimal ApproximateLatitude,
    decimal ApproximateLongitude);

public sealed record UpdatePropertyPublicationRequest(
    string Slug,
    string PublicTitle,
    string? PublicDescription,
    decimal PriceAmount,
    string PriceCurrency,
    string Municipality,
    string Neighborhood,
    decimal ApproximateLatitude,
    decimal ApproximateLongitude);

public sealed record ChangePropertyPublicationStatusRequest(string Action);

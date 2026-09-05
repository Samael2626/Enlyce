using Enlyce.Application.Abstractions;
using Enlyce.Application.PublicCatalog;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.PublicCatalog;

public static class PublicCatalogModule
{
    private const int CacheSeconds = 300;

    public static void MapPublicCatalog(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public/inmuebles").WithTags("Catálogo público");

        group.MapGet("", GetPropertiesAsync)
            .WithName("GetPublicProperties")
            .Produces<PublicPropertyPageResponse>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{slug}", GetPropertyBySlugAsync)
            .WithName("GetPublicPropertyBySlug")
            .Produces<PublicPropertyDetailResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetPropertiesAsync(
        [AsParameters] PublicPropertySearchRequest request,
        IQueryHandler<GetPublicPropertiesQuery, PublicPropertyPageResponse> handler,
        HttpContext context,
        CancellationToken ct)
    {
        try
        {
            var result = await handler.HandleAsync(request.ToQuery(), ct);
            SetPublicCache(context.Response);
            return Results.Ok(result);
        }
        catch (PublicCatalogValidationException exception)
        {
            context.Response.Headers.CacheControl = "no-store";
            return Results.ValidationProblem(
                exception.Errors,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parámetros de búsqueda inválidos");
        }
    }

    private static async Task<IResult> GetPropertyBySlugAsync(
        string slug,
        IQueryHandler<GetPublicPropertyBySlugQuery, PublicPropertyDetailResponse?> handler,
        HttpContext context,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetPublicPropertyBySlugQuery(slug), ct);
        if (result is null)
        {
            context.Response.Headers.CacheControl = "no-store";
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Inmueble no encontrado");
        }

        SetPublicCache(context.Response);
        return Results.Ok(result);
    }

    private static void SetPublicCache(HttpResponse response)
    {
        response.Headers.CacheControl = $"public,max-age={CacheSeconds}";
    }
}

public sealed class PublicPropertySearchRequest
{
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public string? Operation { get; init; }
    public string? PropertyType { get; init; }
    public string? Municipality { get; init; }
    public string? Neighborhood { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? MinArea { get; init; }
    public int? MaxArea { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public int? ParkingSpaces { get; init; }
    public string? Sort { get; init; }

    public GetPublicPropertiesQuery ToQuery() =>
        new(
            Page ?? 1,
            PageSize ?? 12,
            Operation,
            PropertyType,
            Municipality,
            Neighborhood,
            MinPrice,
            MaxPrice,
            MinArea,
            MaxArea,
            Bedrooms,
            Bathrooms,
            ParkingSpaces,
            Sort);
}

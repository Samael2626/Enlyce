using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.CustomerCatalogs;

public static class CustomerCatalogsModule
{
    public static void MapCustomerCatalogs(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/configuracion/catalogos-clientes")
            .WithTags("Configuracion de clientes").RequireAuthorization();

        group.MapGet("/", async (ICustomerCatalogRepository catalogs, CancellationToken ct) =>
        {
            var stored = await catalogs.GetAllAsync(ct);
            var values = stored.GroupBy(item => item.Catalog)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(item => item.Name).ToArray());
            return Results.Ok(new CustomerCatalogsResponse(
                MergeBuiltIns("CustomerType", ["Comprador", "Vendedor", "Arrendador", "Arrendatario"], values),
                values.GetValueOrDefault("CustomerLabel", []),
                MergeBuiltIns("CustomerFunnel", ["Clientes"], values),
                values.GetValueOrDefault("AcquisitionMedium", [])));
        }).WithName("GetCustomerCatalogs");

        group.MapPost("/", async Task<IResult> (
            [FromBody] AddCustomerCatalogItemRequest request,
            ICustomerCatalogRepository catalogs,
            CancellationToken ct) =>
        {
            try
            {
                var item = CustomerCatalogItem.Create(request.Catalog, request.Name);
                if (IsBuiltIn(item) || !await catalogs.AddAsync(item, ct))
                    return Results.Conflict(new { error = "Ese valor ya existe en el catalogo." });
                return Results.Created("/api/configuracion/catalogos-clientes", new CustomerCatalogItemResponse(item.Catalog, item.Name));
            }
            catch (DomainError error)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["catalogo"] = [error.Message] });
            }
        }).RequireAuthorization("Administrador").WithName("AddCustomerCatalogItem");
    }

    private static string[] MergeBuiltIns(string catalog, string[] builtIns,
        IReadOnlyDictionary<string, string[]> values) =>
        builtIns.Concat(values.GetValueOrDefault(catalog, []))
            .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value).ToArray();

    private static bool IsBuiltIn(CustomerCatalogItem item) => item.Catalog switch
    {
        "CustomerType" => new[] { "Comprador", "Vendedor", "Arrendador", "Arrendatario" }
            .Contains(item.Name, StringComparer.OrdinalIgnoreCase),
        "CustomerFunnel" => string.Equals(item.Name, "Clientes", StringComparison.OrdinalIgnoreCase),
        _ => false
    };
}

public sealed record AddCustomerCatalogItemRequest(string Catalog, string Name);
public sealed record CustomerCatalogItemResponse(string Catalog, string Name);
public sealed record CustomerCatalogsResponse(
    IReadOnlyList<string> Types, IReadOnlyList<string> Labels,
    IReadOnlyList<string> Funnels, IReadOnlyList<string> AcquisitionMedia);

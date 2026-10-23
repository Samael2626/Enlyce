using Enlyce.Application.Analytics;
using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class MarketAnalysisReadRepository(EnlyceDbContext context) : IMarketAnalysisReadRepository
{
    public async Task<MarketComparativeAnalysisResult> AnalyzeAsync(MarketAnalysisFilter filter, CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(nowUtc);

        // 1. Query Active Properties (Disponible / Reservado)
        var activeQuery = context.Inmuebles.AsNoTracking()
            .Where(p => p.Estado == EstadoInmueble.Disponible || p.Estado == EstadoInmueble.Reservado);

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var cityNormalized = filter.City.Trim().ToLower();
            activeQuery = activeQuery.Where(p => p.Direccion.Ciudad.ToLower() == cityNormalized);
        }

        if (!string.IsNullOrWhiteSpace(filter.Neighborhood))
        {
            var neighborhoodNormalized = filter.Neighborhood.Trim().ToLower();
            activeQuery = activeQuery.Where(p => p.Direccion.Barrio != null && p.Direccion.Barrio.ToLower().Contains(neighborhoodNormalized));
        }

        if (filter.PropertyType.HasValue)
        {
            activeQuery = activeQuery.Where(p => p.Tipo == filter.PropertyType.Value);
        }

        if (filter.Operation.HasValue)
        {
            if (filter.Operation.Value == ModalidadInmueble.Venta)
            {
                activeQuery = activeQuery.Where(p => p.Modalidad == ModalidadInmueble.Venta || p.Modalidad == ModalidadInmueble.VentaYArriendo);
            }
            else if (filter.Operation.Value == ModalidadInmueble.Arriendo)
            {
                activeQuery = activeQuery.Where(p => p.Modalidad == ModalidadInmueble.Arriendo || p.Modalidad == ModalidadInmueble.VentaYArriendo);
            }
            else
            {
                activeQuery = activeQuery.Where(p => p.Modalidad == filter.Operation.Value);
            }
        }

        if (filter.Bedrooms.HasValue)
        {
            activeQuery = activeQuery.Where(p => p.Habitaciones >= filter.Bedrooms.Value);
        }

        if (filter.Bathrooms.HasValue)
        {
            activeQuery = activeQuery.Where(p => p.Banos >= filter.Bathrooms.Value);
        }

        var activeList = await activeQuery
            .OrderByDescending(p => p.FechaCreacion)
            .Take(100)
            .Select(p => new
            {
                p.Id,
                p.Nombre,
                Tipo = p.Tipo.ToString(),
                Modalidad = p.Modalidad.ToString(),
                p.Direccion.Ciudad,
                p.Direccion.Barrio,
                p.MetrosCuadrados,
                p.Habitaciones,
                p.Banos,
                Price = p.Precio.Monto,
                Currency = p.Precio.Moneda,
                p.FechaCreacion
            })
            .ToListAsync(ct);

        // 2. Query Closed Deals (RealizedDeals joined with Inmuebles)
        var dealsQuery = from deal in context.RealizedDeals.AsNoTracking()
                         join property in context.Inmuebles.AsNoTracking() on deal.PropertyId equals property.Id
                         select new { deal, property };

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var cityNormalized = filter.City.Trim().ToLower();
            dealsQuery = dealsQuery.Where(r => r.property.Direccion.Ciudad.ToLower() == cityNormalized);
        }

        if (!string.IsNullOrWhiteSpace(filter.Neighborhood))
        {
            var neighborhoodNormalized = filter.Neighborhood.Trim().ToLower();
            dealsQuery = dealsQuery.Where(r => r.property.Direccion.Barrio != null && r.property.Direccion.Barrio.ToLower().Contains(neighborhoodNormalized));
        }

        if (filter.PropertyType.HasValue)
        {
            dealsQuery = dealsQuery.Where(r => r.property.Tipo == filter.PropertyType.Value);
        }

        if (filter.Operation.HasValue)
        {
            var dealOp = filter.Operation.Value == ModalidadInmueble.Arriendo
                ? RealizedDealOperation.Rent
                : RealizedDealOperation.Sale;
            dealsQuery = dealsQuery.Where(r => r.deal.Operation == dealOp);
        }

        if (filter.Bedrooms.HasValue)
        {
            dealsQuery = dealsQuery.Where(r => r.property.Habitaciones >= filter.Bedrooms.Value);
        }

        if (filter.Bathrooms.HasValue)
        {
            dealsQuery = dealsQuery.Where(r => r.property.Banos >= filter.Bathrooms.Value);
        }

        var dealsList = await dealsQuery
            .OrderByDescending(r => r.deal.ClosedOn)
            .Take(100)
            .Select(r => new
            {
                DealId = r.deal.Id,
                r.property.Id,
                r.property.Nombre,
                Tipo = r.property.Tipo.ToString(),
                Operation = r.deal.Operation.ToString(),
                r.property.Direccion.Ciudad,
                r.property.Direccion.Barrio,
                r.property.MetrosCuadrados,
                r.property.Habitaciones,
                r.property.Banos,
                InitialPrice = r.property.Precio.Monto,
                ClosedPrice = r.deal.GrossAmount,
                Currency = r.deal.Currency,
                r.deal.ClosedOn,
                r.property.FechaCreacion
            })
            .ToListAsync(ct);

        // 3. Compute Active Inventory Metrics
        int activeSampleSize = activeList.Count;
        decimal activeAvgPrice = activeSampleSize > 0 ? decimal.Round(activeList.Average(p => p.Price), 2) : 0m;
        decimal activeMedianPrice = CalculateMedian(activeList.Select(p => p.Price));
        decimal activeMinPrice = activeSampleSize > 0 ? activeList.Min(p => p.Price) : 0m;
        decimal activeMaxPrice = activeSampleSize > 0 ? activeList.Max(p => p.Price) : 0m;

        var activePricesM2 = activeList
            .Where(p => p.MetrosCuadrados > 0)
            .Select(p => decimal.Round(p.Price / p.MetrosCuadrados, 2))
            .ToList();

        decimal activeAvgPriceM2 = activePricesM2.Count > 0 ? decimal.Round(activePricesM2.Average(), 2) : 0m;
        decimal activeMedianPriceM2 = CalculateMedian(activePricesM2);
        decimal activeMinPriceM2 = activePricesM2.Count > 0 ? activePricesM2.Min() : 0m;
        decimal activeMaxPriceM2 = activePricesM2.Count > 0 ? activePricesM2.Max() : 0m;

        double activeAvgDays = activeSampleSize > 0
            ? Math.Round(activeList.Average(p => Math.Max(0, (nowUtc.Date - p.FechaCreacion.Date).TotalDays)), 1)
            : 0;

        var activeMetrics = new MarketInventoryMetrics(
            activeSampleSize,
            activeAvgPrice,
            activeMedianPrice,
            activeMinPrice,
            activeMaxPrice,
            activeAvgPriceM2,
            activeMedianPriceM2,
            activeMinPriceM2,
            activeMaxPriceM2,
            activeAvgDays);

        // 4. Compute Closed Deals Metrics
        int closedSampleSize = dealsList.Count;
        decimal closedAvgPrice = closedSampleSize > 0 ? decimal.Round(dealsList.Average(d => d.ClosedPrice), 2) : 0m;
        decimal closedMedianPrice = CalculateMedian(dealsList.Select(d => d.ClosedPrice));

        var closedPricesM2 = dealsList
            .Where(d => d.MetrosCuadrados > 0)
            .Select(d => decimal.Round(d.ClosedPrice / d.MetrosCuadrados, 2))
            .ToList();

        decimal closedAvgPriceM2 = closedPricesM2.Count > 0 ? decimal.Round(closedPricesM2.Average(), 2) : 0m;
        decimal closedMedianPriceM2 = CalculateMedian(closedPricesM2);

        double closedAvgDays = closedSampleSize > 0
            ? Math.Round(dealsList.Average(d => Math.Max(0, d.ClosedOn.DayNumber - DateOnly.FromDateTime(d.FechaCreacion).DayNumber)), 1)
            : 0;

        decimal? avgDiscount = null;
        var discounts = dealsList
            .Where(d => d.InitialPrice > 0)
            .Select(d => ((d.ClosedPrice - d.InitialPrice) / d.InitialPrice) * 100m)
            .ToList();
        if (discounts.Count > 0)
        {
            avgDiscount = decimal.Round(discounts.Average(), 2);
        }

        var closedMetrics = new MarketClosedDealsMetrics(
            closedSampleSize,
            closedAvgPrice,
            closedMedianPrice,
            closedAvgPriceM2,
            closedMedianPriceM2,
            closedAvgDays,
            avgDiscount);

        // 5. Compute Valuation Estimate (if TargetAreaM2 > 0)
        MarketValuationEstimate? valuationEstimate = null;
        if (filter.TargetAreaM2.HasValue && filter.TargetAreaM2.Value > 0)
        {
            int targetArea = filter.TargetAreaM2.Value;
            decimal referenceM2 = 0m;

            if (closedMedianPriceM2 > 0 && closedSampleSize >= 3)
            {
                referenceM2 = closedMedianPriceM2;
            }
            else if (activeMedianPriceM2 > 0)
            {
                referenceM2 = activeMedianPriceM2;
            }
            else if (closedMedianPriceM2 > 0)
            {
                referenceM2 = closedMedianPriceM2;
            }

            if (referenceM2 > 0)
            {
                decimal targetVal = decimal.Round(targetArea * referenceM2, 2);
                decimal lowVal = decimal.Round(targetVal * 0.95m, 2);
                decimal highVal = decimal.Round(targetVal * 1.05m, 2);

                int totalSamples = activeSampleSize + closedSampleSize;
                string confidence = totalSamples >= 8 ? "Alta"
                    : totalSamples >= 3 ? "Media"
                    : "Baja";

                string explanation = $"Avalúo referencial para {targetArea} m² con valor base de {referenceM2:N0} por m² según {totalSamples} comparables en inventario y cierres históricos. Rango comercial sugerido ±5%.";

                valuationEstimate = new MarketValuationEstimate(
                    targetArea,
                    referenceM2,
                    lowVal,
                    targetVal,
                    highVal,
                    confidence,
                    explanation);
            }
        }

        // 6. Build Comparables Lists (Top 15 each)
        var activeComparables = activeList.Take(15).Select(p =>
        {
            int days = Math.Max(0, (nowUtc.Date - p.FechaCreacion.Date).Days);
            decimal m2Price = p.MetrosCuadrados > 0 ? decimal.Round(p.Price / p.MetrosCuadrados, 2) : 0m;
            return new MarketComparableActiveProperty(
                p.Id,
                p.Nombre,
                p.Tipo,
                p.Modalidad,
                p.Ciudad,
                p.Barrio,
                p.MetrosCuadrados,
                p.Habitaciones,
                p.Banos,
                p.Price,
                m2Price,
                days,
                p.Currency);
        }).ToList();

        var closedComparables = dealsList.Take(15).Select(d =>
        {
            int days = Math.Max(0, d.ClosedOn.DayNumber - DateOnly.FromDateTime(d.FechaCreacion).DayNumber);
            decimal m2Price = d.MetrosCuadrados > 0 ? decimal.Round(d.ClosedPrice / d.MetrosCuadrados, 2) : 0m;
            return new MarketComparableClosedDeal(
                d.DealId,
                d.Id,
                d.Nombre,
                d.Tipo,
                d.Operation,
                d.Ciudad,
                d.Barrio,
                d.MetrosCuadrados,
                d.Habitaciones,
                d.Banos,
                d.ClosedPrice,
                m2Price,
                days,
                d.ClosedOn,
                d.Currency);
        }).ToList();

        return new MarketComparativeAnalysisResult(
            filter,
            activeMetrics,
            closedMetrics,
            valuationEstimate,
            activeComparables,
            closedComparables);
    }

    private static decimal CalculateMedian(IEnumerable<decimal> values)
    {
        var list = values.OrderBy(v => v).ToList();
        if (list.Count == 0) return 0m;
        int count = list.Count;
        if (count % 2 == 1) return list[count / 2];
        return decimal.Round((list[(count / 2) - 1] + list[count / 2]) / 2m, 2);
    }
}

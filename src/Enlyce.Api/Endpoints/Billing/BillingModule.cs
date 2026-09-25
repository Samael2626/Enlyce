using System.Text.Json;
using Enlyce.Application.Abstractions;
using Enlyce.Application.Billing;
using Enlyce.Domain.Billing;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Billing;

public static class BillingModule
{
    public static void MapBilling(this IEndpointRouteBuilder app)
    {
        var billing = app.MapGroup("/api/billing")
            .WithTags("Billing")
            .RequireAuthorization("Administrador");

        billing.MapGet("/pricing", () => Results.Ok(new BillingPricingResponse(
            SaasPricing.BasePlanInCents,
            SaasPricing.AdditionalAdvisorInCents,
            SaasPricing.SetupInCents,
            SaasPricing.WhatsAppInCents,
            SaasPricing.PortalInCents,
            SaasPricing.AdvancedReportsInCents,
            SubscriptionSelection.MaximumAdditionalAdvisors)))
        .WithName("GetBillingPricing")
        .Produces<BillingPricingResponse>();

        billing.MapPost("/checkout-sessions", async Task<IResult> (
            [FromBody] CreateCheckoutSessionRequest request,
            ICommandHandler<CreateCheckoutSessionCommand, CreateCheckoutSessionResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new CreateCheckoutSessionCommand(
                request.CompanyName,
                request.TaxId,
                request.CustomerEmail,
                request.AdditionalAdvisors,
                request.IncludeSetup,
                request.IncludeWhatsApp,
                request.IncludePortal,
                request.IncludeAdvancedReports), cancellationToken);

            return Results.Created($"/api/billing/orders/{result.Checkout.Reference}", result);
        })
        .RequireRateLimiting("billing-checkout")
        .WithName("CreateBillingCheckoutSession")
        .Produces<CreateCheckoutSessionResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status429TooManyRequests)
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        billing.MapGet("/orders/{reference}", async Task<IResult> (
            string reference,
            IPaymentOrderRepository orders,
            CancellationToken cancellationToken) =>
        {
            var order = await orders.GetByReferenceAsync(reference, cancellationToken);
            return order is null
                ? Results.NotFound()
                : Results.Ok(new PaymentOrderStatusResponse(
                    order.Reference,
                    order.AmountInCents,
                    order.Currency,
                    order.Status.ToString()));
        })
        .RequireRateLimiting("billing-status")
        .WithName("GetPaymentOrderStatus")
        .Produces<PaymentOrderStatusResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/webhooks/wompi", async Task<IResult> (
            [FromBody] JsonElement eventBody,
            IPaymentEventVerifier verifier,
            ProcessPaymentNotificationHandler handler,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var notification = verifier.VerifyAndParse(eventBody);
            var processed = await handler.HandleAsync(notification, cancellationToken);
            if (!processed)
                loggerFactory.CreateLogger("WompiWebhook")
                    .LogWarning("Evento de Wompi para referencia desconocida {Reference}", notification.Reference);

            return Results.Ok(new { processed });
        })
        .AllowAnonymous()
        .WithName("ReceiveWompiWebhook")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}

public sealed record CreateCheckoutSessionRequest(
    string CompanyName,
    string TaxId,
    string CustomerEmail,
    int AdditionalAdvisors = 0,
    bool IncludeSetup = true,
    bool IncludeWhatsApp = false,
    bool IncludePortal = false,
    bool IncludeAdvancedReports = false);

public sealed record PaymentOrderStatusResponse(
    string Reference,
    long AmountInCents,
    string Currency,
    string Status);

public sealed record BillingPricingResponse(
    long BasePlanInCents,
    long AdditionalAdvisorInCents,
    long SetupInCents,
    long WhatsAppInCents,
    long PortalInCents,
    long AdvancedReportsInCents,
    int MaximumAdditionalAdvisors);

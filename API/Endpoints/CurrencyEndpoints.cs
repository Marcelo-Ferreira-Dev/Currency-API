using API.Application.Currencies.Commands.CreateCurrency;
using API.Application.Currencies.Queries.GetCurrencies;
using API.Application.CurrencyConversion.ConvertCurrency;
using API.Contracts;

namespace API.Endpoints;

public static class CurrencyEndpoints
{
    public static void MapCurrencyEndpoints(this WebApplication app)
    {
        var currencies = app.MapGroup("/currencies").WithTags("Currencies");
        currencies.MapGet("", async (bool? isActive, GetCurrenciesQueryHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(new(isActive), ct)))
            .WithSummary("Listar monedas").Produces<List<CurrencyResponse>>();
        currencies.MapPost("", async (CreateCurrencyCommand command, CreateCurrencyCommandHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(command, ct)).ToHttp(currency => Results.Created("/currencies", currency)))
            .WithSummary("Crear moneda").Produces<CurrencyResponse>(201).ProducesValidationProblem().Produces(409);
        app.MapPost("/currency/convert", async (ConvertCurrencyCommand command, ConvertCurrencyCommandHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(command, ct)).ToHttp())
            .WithTags("Conversion").WithSummary("Convertir un importe entre monedas")
            .Produces<ConversionResponse>().ProducesValidationProblem().Produces(404).Produces(409);
    }
}

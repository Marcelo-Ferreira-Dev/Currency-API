using API.Application.Common;
using API.Contracts;
using API.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API.Application.CurrencyConversion.ConvertCurrency;

public class ConvertCurrencyCommandHandler(AppDbContext context, IValidator<ConvertCurrencyCommand> validator)
{
    public async Task<OperationResult<ConversionResponse>> HandleAsync(ConvertCurrencyCommand command, CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return new(Errors: validation.ToDictionary());
        }

        var fromCode = command.FromCurrencyCode.Trim().ToUpperInvariant();
        var toCode = command.ToCurrencyCode.Trim().ToUpperInvariant();
        var from = await context.Currencies.AsNoTracking().SingleOrDefaultAsync(c => c.Code == fromCode, ct);
        var to = await context.Currencies.AsNoTracking().SingleOrDefaultAsync(c => c.Code == toCode, ct);
        if (from is null || to is null)
        {
            return new(NotFound: true);
        }

        if (!from.IsActive || !to.IsActive)
        {
            return new(Conflict: "Las monedas deben estar activas para convertir.");
        }

        if (from.RateToBase <= 0 || to.RateToBase <= 0)
        {
            return new(Conflict: "Las tasas almacenadas deben ser positivas.");
        }

        try
        {
            return new(new(from.Code, to.Code, command.Amount,
            CurrencyCalculator.Convert(command.Amount, from.RateToBase, to.RateToBase)));
        }
        catch (OverflowException)
        {
            return new(Errors: new Dictionary<string, string[]> { ["amount"] = ["El importe excede el rango permitido."] });
        }
    }
}

using API.Application.Common;
using API.Contracts;
using API.Data;
using API.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Currencies.Commands.CreateCurrency;

public class CreateCurrencyCommandHandler(AppDbContext context, IValidator<CreateCurrencyCommand> validator)
{
    public async Task<OperationResult<CurrencyResponse>> HandleAsync(CreateCurrencyCommand command, CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return new(Errors: validation.ToDictionary());
        }

        var code = command.Code.Trim().ToUpperInvariant();
        if (await context.Currencies.AnyAsync(c => c.Code == code, ct))
        {
            return new(Conflict: "El código ya existe.");
        }

        var currency = new Currency
        {
            Code = code,
            Name = command.Name.Trim(),
            RateToBase = command.RateToBase,
            IsActive = command.IsActive
        };
        context.Currencies.Add(currency);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (DatabaseErrors.IsUniqueViolation(ex))
        {
            return new(Conflict: "El código ya existe.");
        }
        return new(new(currency.Id, currency.Code, currency.Name, currency.RateToBase, currency.IsActive));
    }
}

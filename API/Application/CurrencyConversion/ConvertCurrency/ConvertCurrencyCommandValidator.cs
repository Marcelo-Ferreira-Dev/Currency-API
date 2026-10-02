using FluentValidation;

namespace API.Application.CurrencyConversion.ConvertCurrency;

public class ConvertCurrencyCommandValidator : AbstractValidator<ConvertCurrencyCommand>
{
    public ConvertCurrencyCommandValidator()
    {
        RuleFor(command => command.FromCurrencyCode)
            .NotEmpty();

        RuleFor(command => command.ToCurrencyCode)
            .NotEmpty();

        RuleFor(command => command.Amount)
            .GreaterThan(0m);
    }
}

using API.Application.Currencies.Commands.CreateCurrency;
using API.Application.CurrencyConversion.ConvertCurrency;
using FluentValidation.TestHelper;

namespace API.Tests.Currencies;

public class CurrencyValidationTests
{
    [Fact]
    public void Create_accepts_valid_currency()
    {
        new CreateCurrencyCommandValidator().TestValidate(new CreateCurrencyCommand
        {
            Code = "PYG",
            Name = "Guaraní",
            RateToBase = 1
        }).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_empty_code_and_name(string? value)
    {
        var result = new CreateCurrencyCommandValidator().TestValidate(new CreateCurrencyCommand
        {
            Code = value!,
            Name = value!,
            RateToBase = 1
        });
        result.ShouldHaveValidationErrorFor(x => x.Code);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_rate(int rate)
    {
        new CreateCurrencyCommandValidator().TestValidate(new CreateCurrencyCommand
        {
            Code = "PYG",
            Name = "Guaraní",
            RateToBase = rate
        }).ShouldHaveValidationErrorFor(x => x.RateToBase);
    }

    [Fact]
    public void Convert_accepts_valid_request()
    {
        new ConvertCurrencyCommandValidator().TestValidate(new ConvertCurrencyCommand
        {
            FromCurrencyCode = "USD",
            ToCurrencyCode = "PYG",
            Amount = 100
        }).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Convert_rejects_empty_codes(string? value)
    {
        var result = new ConvertCurrencyCommandValidator().TestValidate(new ConvertCurrencyCommand
        {
            FromCurrencyCode = value!,
            ToCurrencyCode = value!,
            Amount = 100
        });
        result.ShouldHaveValidationErrorFor(x => x.FromCurrencyCode);
        result.ShouldHaveValidationErrorFor(x => x.ToCurrencyCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Convert_rejects_non_positive_amount(int amount)
    {
        new ConvertCurrencyCommandValidator().TestValidate(new ConvertCurrencyCommand
        {
            FromCurrencyCode = "USD",
            ToCurrencyCode = "PYG",
            Amount = amount
        }).ShouldHaveValidationErrorFor(x => x.Amount);
    }
}

using API.Application.CurrencyConversion.ConvertCurrency;

namespace API.Tests.Currencies;

public class CurrencyCalculatorTests
{
    [Fact]
    public void Converts_usd_to_pyg() => Assert.Equal(600000m, CurrencyCalculator.Convert(100m, 6000m, 1m));

    [Fact]
    public void Converts_pyg_to_usd() => Assert.Equal(100m, CurrencyCalculator.Convert(600000m, 1m, 6000m));

    [Fact]
    public void Same_currency_keeps_amount() => Assert.Equal(123.45m, CurrencyCalculator.Convert(123.45m, 6000m, 6000m));

    [Fact]
    public void Does_not_round_result() => Assert.Equal(2.5m, CurrencyCalculator.Convert(1m, 5m, 2m));

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 0)]
    [InlineData(-1, 1, 1)]
    public void Rejects_non_positive_values(int amount, int from, int to) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => CurrencyCalculator.Convert(amount, from, to));

    [Fact]
    public void Detects_overflow() =>
        Assert.Throws<OverflowException>(() => CurrencyCalculator.Convert(decimal.MaxValue, 2m, 1m));
}

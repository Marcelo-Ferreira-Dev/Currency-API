namespace API.Application.CurrencyConversion.ConvertCurrency;

public static class CurrencyCalculator
{
    public static decimal Convert(decimal amount, decimal fromRate, decimal toRate)
    {
        if (amount <= 0 || fromRate <= 0 || toRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Importe y tasas deben ser positivos.");
        }

        return checked(amount * fromRate / toRate);
    }
}

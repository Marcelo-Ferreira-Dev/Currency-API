namespace API.Application.CurrencyConversion.ConvertCurrency;

public class ConvertCurrencyCommand
{
    public string FromCurrencyCode { get; set; } = string.Empty;

    public string ToCurrencyCode { get; set; } = string.Empty;

    public decimal Amount
    {
        get; set;
    }
}

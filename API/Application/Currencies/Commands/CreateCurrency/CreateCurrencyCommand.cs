namespace API.Application.Currencies.Commands.CreateCurrency;

public class CreateCurrencyCommand
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal RateToBase
    {
        get; set;
    }
    public bool IsActive { get; set; } = true;
}

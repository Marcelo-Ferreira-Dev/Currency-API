namespace API.Contracts;

public record ConversionResponse(string FromCurrency, string ToCurrency, decimal OriginalAmount, decimal ConvertedAmount);

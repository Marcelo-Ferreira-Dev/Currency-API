namespace API.Contracts;

public record CurrencyResponse(int Id, string Code, string Name, decimal RateToBase, bool IsActive);

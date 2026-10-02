using API.Contracts;
using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Currencies.Queries.GetCurrencies;

public class GetCurrenciesQueryHandler(AppDbContext context)
{
    public async Task<List<CurrencyResponse>> HandleAsync(GetCurrenciesQuery query, CancellationToken ct = default)
    {
        var currencies = context.Currencies.AsNoTracking();
        if (query.IsActive.HasValue)
        {
            currencies = currencies.Where(c => c.IsActive == query.IsActive.Value);
        }

        return await currencies.OrderBy(c => c.Code)
            .Select(c => new CurrencyResponse(c.Id, c.Code, c.Name, c.RateToBase, c.IsActive)).ToListAsync(ct);
    }
}

using API.Application.Currencies.Commands.CreateCurrency;
using API.Application.Currencies.Queries.GetCurrencies;
using API.Application.CurrencyConversion.ConvertCurrency;
using API.Data;
using API.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Currencies;

public class CurrencyHandlerTests : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly AppDbContext context;

    public CurrencyHandlerTests()
    {
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        context.Database.EnsureCreated();
    }

    private CreateCurrencyCommandHandler CreateHandler() => new(context, new CreateCurrencyCommandValidator());
    private ConvertCurrencyCommandHandler ConvertHandler() => new(context, new ConvertCurrencyCommandValidator());

    private async Task SeedAsync(bool fromActive = true, bool toActive = true, decimal fromRate = 6000m)
    {
        context.Currencies.AddRange(
            new Currency { Code = "USD", Name = "Dólar", RateToBase = fromRate, IsActive = fromActive },
            new Currency { Code = "PYG", Name = "Guaraní", RateToBase = 1m, IsActive = toActive });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Create_normalizes_code_and_name()
    {
        var result = await CreateHandler().HandleAsync(new()
        {
            Code = " pyg ",
            Name = " Guaraní ",
            RateToBase = 1
        });
        Assert.NotNull(result.Value);
        Assert.Equal("PYG", result.Value.Code);
        Assert.Equal("Guaraní", result.Value.Name);
        Assert.True(result.Value.IsActive);
    }

    [Fact]
    public async Task Create_rejects_duplicate_code_even_when_inactive()
    {
        await CreateHandler().HandleAsync(new()
        {
            Code = "PYG",
            Name = "Guaraní",
            RateToBase = 1,
            IsActive = false
        });
        var result = await CreateHandler().HandleAsync(new()
        {
            Code = " pyg ",
            Name = "Guaraní",
            RateToBase = 1
        });
        Assert.NotNull(result.Conflict);
        Assert.Equal(1, await context.Currencies.CountAsync());
    }

    [Fact]
    public async Task Invalid_creation_does_not_save()
    {
        var result = await CreateHandler().HandleAsync(new()
        {
            Code = "",
            Name = "",
            RateToBase = 0
        });
        Assert.NotNull(result.Errors);
        Assert.Empty(await context.Currencies.ToListAsync());
    }

    [Theory]
    [InlineData(null, 2)]
    [InlineData(true, 1)]
    [InlineData(false, 1)]
    public async Task List_filters_by_state(bool? isActive, int count)
    {
        await SeedAsync(fromActive: false);
        var result = await new GetCurrenciesQueryHandler(context).HandleAsync(new(isActive));
        Assert.Equal(count, result.Count);
        if (isActive.HasValue)
        {
            Assert.All(result, currency => Assert.Equal(isActive.Value, currency.IsActive));
        }
    }

    [Fact]
    public async Task Convert_normalizes_codes_and_returns_result()
    {
        await SeedAsync();
        var result = await ConvertHandler().HandleAsync(new()
        {
            FromCurrencyCode = " usd ",
            ToCurrencyCode = "pyg",
            Amount = 100
        });
        Assert.NotNull(result.Value);
        Assert.Equal(600000m, result.Value.ConvertedAmount);
        Assert.Equal("USD", result.Value.FromCurrency);
        Assert.Equal("PYG", result.Value.ToCurrency);
        Assert.Equal(100m, result.Value.OriginalAmount);
    }

    [Theory]
    [InlineData("EUR", "PYG")]
    [InlineData("USD", "EUR")]
    public async Task Convert_missing_currency_returns_not_found(string from, string to)
    {
        await SeedAsync();
        var result = await ConvertHandler().HandleAsync(new()
        {
            FromCurrencyCode = from,
            ToCurrencyCode = to,
            Amount = 100
        });
        Assert.True(result.NotFound);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Convert_inactive_currency_returns_conflict(bool fromActive, bool toActive)
    {
        await SeedAsync(fromActive, toActive);
        var result = await ConvertHandler().HandleAsync(new()
        {
            FromCurrencyCode = "USD",
            ToCurrencyCode = "PYG",
            Amount = 100
        });
        Assert.NotNull(result.Conflict);
    }

    [Fact]
    public async Task Convert_invalid_stored_rate_returns_conflict()
    {
        await SeedAsync(fromRate: 0);
        var result = await ConvertHandler().HandleAsync(new()
        {
            FromCurrencyCode = "USD",
            ToCurrencyCode = "PYG",
            Amount = 100
        });
        Assert.NotNull(result.Conflict);
    }

    [Fact]
    public async Task Convert_overflow_returns_validation_error()
    {
        await SeedAsync();
        var result = await ConvertHandler().HandleAsync(new()
        {
            FromCurrencyCode = "USD",
            ToCurrencyCode = "PYG",
            Amount = decimal.MaxValue
        });
        Assert.NotNull(result.Errors);
        Assert.Contains("amount", result.Errors.Keys);
    }

    public void Dispose()
    {
        context.Dispose();
        connection.Dispose();
    }
}

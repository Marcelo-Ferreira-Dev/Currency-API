using API.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace API.Tests.Addresses;

public class IsActiveMigrationTests
{
    [Fact]
    public async Task Migration_marks_existing_addresses_and_currencies_as_active()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).Options);
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync("20261002213409_InitialCreate");
        await context.Database.ExecuteSqlRawAsync("""
            INSERT INTO Users (Name, Email, Password) VALUES ('Marcelo Ferreira', 'marcelod.ferreira.dev@gmail.com', 'hash-de-prueba');
            INSERT INTO Addresses (UserId, Street, City, Country) VALUES (1, 'Av. España', 'Asunción', 'Paraguay');
            INSERT INTO Currencies (Code, Name, RateToBase) VALUES ('PYG', 'Guaraní', '1');
            """);

        await migrator.MigrateAsync();

        Assert.True((await context.Addresses.SingleAsync()).IsActive);
        Assert.True((await context.Currencies.SingleAsync()).IsActive);
    }
}

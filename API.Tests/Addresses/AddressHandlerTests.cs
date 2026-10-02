using API.Application.Addresses.Commands.CreateAddress;
using API.Application.Addresses.Commands.DeleteAddress;
using API.Application.Addresses.Commands.UpdateAddress;
using API.Application.Addresses.Queries.GetUserAddresses;
using API.Data;
using API.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Addresses;

public class AddressHandlerTests : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly AppDbContext context;

    public AddressHandlerTests()
    {
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        context.Database.EnsureCreated();
    }

    private async Task<User> UserAsync(bool isActive = true)
    {
        var user = new User
        {
            Name = "Marcelo Ferreira",
            Email = $"usuario{Guid.NewGuid():N}@example.com",
            Password = "hash-de-prueba",
            IsActive = isActive
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private CreateAddressCommand Command(int userId) => new()
    {
        UserId = userId,
        Street = " Av. Mariscal López 1234 ",
        City = " Asunción ",
        Country = " Paraguay "
    };

    private CreateAddressCommandHandler CreateHandler() => new(context, new CreateAddressCommandValidator());
    private DeleteAddressCommandHandler DeleteHandler() => new(context, new DeleteAddressCommandValidator());

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Create_accepts_existing_user_and_normalizes_fields(bool isActive)
    {
        var user = await UserAsync(isActive);
        var result = await CreateHandler().HandleAsync(Command(user.Id));
        Assert.NotNull(result.Value);
        Assert.Equal("Av. Mariscal López 1234", result.Value.Street);
        Assert.Equal("Asunción", result.Value.City);
        Assert.Equal("Paraguay", result.Value.Country);
        Assert.Null(result.Value.ZipCode);
        Assert.True(result.Value.IsActive);
        Assert.Equal(user.Id, result.Value.UserId);
    }

    [Fact]
    public async Task Create_missing_user_returns_not_found_without_saving()
    {
        var result = await CreateHandler().HandleAsync(Command(123));
        Assert.True(result.NotFound);
        Assert.Empty(await context.Addresses.ToListAsync());
    }

    [Fact]
    public async Task Create_invalid_data_returns_errors_without_saving()
    {
        var user = await UserAsync();
        var command = Command(user.Id);
        command.Street = " ";
        var result = await CreateHandler().HandleAsync(command);
        Assert.NotNull(result.Errors);
        Assert.Empty(await context.Addresses.ToListAsync());
    }

    [Fact]
    public async Task List_existing_user_without_addresses_returns_empty_list()
    {
        var user = await UserAsync();
        var result = await new GetUserAddressesQueryHandler(context).HandleAsync(new(user.Id));
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task List_missing_user_returns_not_found()
    {
        var result = await new GetUserAddressesQueryHandler(context).HandleAsync(new(123));
        Assert.True(result.NotFound);
    }

    [Theory]
    [InlineData(null, 2)]
    [InlineData(true, 1)]
    [InlineData(false, 1)]
    public async Task List_filters_by_state_and_does_not_include_another_users_addresses(bool? isActive, int count)
    {
        var user = await UserAsync();
        var other = await UserAsync();
        var first = await CreateHandler().HandleAsync(Command(user.Id));
        await CreateHandler().HandleAsync(Command(user.Id));
        await CreateHandler().HandleAsync(Command(other.Id));
        await DeleteHandler().HandleAsync(new()
        {
            Id = first.Value!.Id
        });

        var result = await new GetUserAddressesQueryHandler(context).HandleAsync(new(user.Id, isActive));
        Assert.Equal(count, result.Value!.Count);
        Assert.All(result.Value, address => Assert.Equal(user.Id, address.UserId));
        if (isActive.HasValue)
        {
            Assert.All(result.Value, address => Assert.Equal(isActive.Value, address.IsActive));
        }
    }

    [Fact]
    public async Task Delete_preserves_address_and_user_and_can_be_repeated()
    {
        var user = await UserAsync();
        var address = (await CreateHandler().HandleAsync(Command(user.Id))).Value!;
        var handler = DeleteHandler();
        Assert.True((await handler.HandleAsync(new()
        {
            Id = address.Id
        })).Value);
        Assert.True((await handler.HandleAsync(new()
        {
            Id = address.Id
        })).Value);
        context.ChangeTracker.Clear();
        Assert.False((await context.Addresses.SingleAsync()).IsActive);
        Assert.True((await context.Users.SingleAsync()).IsActive);
    }

    [Fact]
    public async Task Update_can_reactivate_address_without_changing_owner()
    {
        var user = await UserAsync();
        var address = (await CreateHandler().HandleAsync(Command(user.Id))).Value!;
        await DeleteHandler().HandleAsync(new()
        {
            Id = address.Id
        });
        var result = await new UpdateAddressCommandHandler(context, new UpdateAddressCommandValidator())
            .HandleAsync(new()
            {
                Id = address.Id,
                Street = " Av. España 456 ",
                City = "Asunción",
                Country = "Paraguay",
                ZipCode = " 1209 ",
                IsActive = true
            });
        Assert.NotNull(result.Value);
        Assert.True(result.Value.IsActive);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Equal("Av. España 456", result.Value.Street);
        Assert.Equal("1209", result.Value.ZipCode);
    }

    [Fact]
    public async Task Update_and_delete_missing_address_return_not_found()
    {
        var update = await new UpdateAddressCommandHandler(context, new UpdateAddressCommandValidator())
            .HandleAsync(new()
            {
                Id = 123,
                Street = "Av. España",
                City = "Asunción",
                Country = "Paraguay"
            });
        Assert.True(update.NotFound);
        Assert.True((await DeleteHandler().HandleAsync(new()
        {
            Id = 123
        })).NotFound);
    }

    public void Dispose()
    {
        context.Dispose();
        connection.Dispose();
    }
}

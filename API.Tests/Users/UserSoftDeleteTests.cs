using API.Application.Users.Commands.DeleteUser;
using API.Application.Users.Queries.GetUsers;
using API.Application.Users.Queries.GetUserById;
using API.Data;
using API.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Users;

public class UserSoftDeleteTests : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly AppDbContext context;

    public UserSoftDeleteTests()
    {
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).Options);
        context.Database.EnsureCreated();
    }

    private async Task<User> CreateUserAsync(bool isActive = true)
    {
        var user = new User
        {
            Name = "Marcelo Ferreira", Email = "marcelod.ferreira.dev@gmail.com",
            Password = "hash-de-prueba", IsActive = isActive
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private DeleteUserCommandHandler DeleteHandler() =>
        new(context, new DeleteUserCommandValidator());

    [Fact]
    public async Task Delete_deactivates_user_and_preserves_address()
    {
        var user = await CreateUserAsync();
        context.Addresses.Add(new Address
        {
            UserId = user.Id, Street = "Av. Mariscal López", City = "Asunción", Country = "Paraguay"
        });
        await context.SaveChangesAsync();

        var result = await DeleteHandler().HandleAsync(new DeleteUserCommand { Id = user.Id });
        context.ChangeTracker.Clear();

        Assert.True(result.Value);
        var stored = await context.Users.SingleAsync();
        Assert.False(stored.IsActive);
        Assert.Equal("hash-de-prueba", stored.Password);
        Assert.Equal(user.Id, (await context.Addresses.SingleAsync()).UserId);
    }

    [Fact]
    public async Task Delete_again_succeeds_without_removing_user()
    {
        var user = await CreateUserAsync();
        var handler = DeleteHandler();
        await handler.HandleAsync(new DeleteUserCommand { Id = user.Id });
        var result = await handler.HandleAsync(new DeleteUserCommand { Id = user.Id });

        Assert.True(result.Value);
        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task Delete_missing_user_returns_not_found()
    {
        var result = await DeleteHandler().HandleAsync(new DeleteUserCommand { Id = 123 });
        Assert.True(result.NotFound);
    }

    [Fact]
    public async Task List_without_filter_includes_inactive_users()
    {
        var user = await CreateUserAsync();
        await DeleteHandler().HandleAsync(new DeleteUserCommand { Id = user.Id });
        var users = await new GetUsersQueryHandler(context).HandleAsync(new GetUsersQuery());

        Assert.False(Assert.Single(users).IsActive);
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task List_filters_soft_deleted_user_by_state(bool isActive, int count)
    {
        var user = await CreateUserAsync();
        await DeleteHandler().HandleAsync(new DeleteUserCommand { Id = user.Id });
        var users = await new GetUsersQueryHandler(context).HandleAsync(new GetUsersQuery(isActive));

        Assert.Equal(count, users.Count);
        Assert.All(users, response => Assert.Equal(isActive, response.IsActive));
    }

    [Fact]
    public async Task Get_by_id_can_read_soft_deleted_user()
    {
        var user = await CreateUserAsync();
        await DeleteHandler().HandleAsync(new DeleteUserCommand { Id = user.Id });
        var result = await new GetUserByIdQueryHandler(context).HandleAsync(new GetUserByIdQuery(user.Id));

        Assert.NotNull(result);
        Assert.False(result.IsActive);
    }

    public void Dispose()
    {
        context.Dispose();
        connection.Dispose();
    }
}

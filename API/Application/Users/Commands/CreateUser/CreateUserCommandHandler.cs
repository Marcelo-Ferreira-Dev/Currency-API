using API.Data;
using API.Entities;
using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler(AppDbContext context, IValidator<CreateUserCommand> validator)
{
    public async Task<CreateUserResult> HandleAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return new(null, validation.ToDictionary());
        }

        var email = command.Email.Trim().ToLowerInvariant();
        if (await context.Users.AnyAsync(user => user.Email == email, ct))
        {
            return new(null, null, true);
        }

        var user = new User
        {
            Name = command.Name.Trim(),
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(command.Password),
            IsActive = true
        };

        context.Users.Add(user);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            return new(null, null, true);
        }

        return new(user, null);
    }
}

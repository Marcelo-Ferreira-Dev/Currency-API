using API.Data;
using API.Entities;
using API.Contracts;
using API.Application.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
namespace API.Application.Users.Commands.UpdateUser;
public class UpdateUserCommandHandler(AppDbContext context, IValidator<UpdateUserCommand> validator)
{
    public async Task<OperationResult<UserResponse>> HandleAsync(UpdateUserCommand command, CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid) return new(Errors: validation.ToDictionary());
        var user = await context.Users.FindAsync([command.Id], ct);
        if (user is null) return new(NotFound: true);
        var email = command.Email.Trim().ToLowerInvariant();
        if (await context.Users.AnyAsync(u => u.Id != command.Id && u.Email == email, ct))
            return new(Conflict: "El email ya está registrado.");
        user.Name = command.Name.Trim(); user.Email = email; user.IsActive = command.IsActive;
        try { await context.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (DatabaseErrors.IsUniqueViolation(ex))
        { return new(Conflict: "El email ya está registrado."); }
        return new(new(user.Id, user.Name, user.Email, user.IsActive));
    }
}

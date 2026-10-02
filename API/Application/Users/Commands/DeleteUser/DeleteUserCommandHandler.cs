using API.Data;
using API.Entities;
using API.Contracts;
using API.Application.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
namespace API.Application.Users.Commands.DeleteUser;
public class DeleteUserCommandHandler(AppDbContext context, IValidator<DeleteUserCommand> validator)
{
    public async Task<OperationResult<bool>> HandleAsync(DeleteUserCommand command, CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid) return new(Errors: validation.ToDictionary());
        var user = await context.Users.FindAsync([command.Id], ct);
        if (user is null) return new(NotFound: true);
        user.IsActive = false;
        await context.SaveChangesAsync(ct);
        return new(true);
    }
}

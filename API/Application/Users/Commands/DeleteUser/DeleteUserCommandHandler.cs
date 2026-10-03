using API.Application.Common;
using API.Data;
using FluentValidation;

namespace API.Application.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler(AppDbContext context, IValidator<DeleteUserCommand> validator)
{
    public async Task<OperationResult<bool>> HandleAsync(DeleteUserCommand command, CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return new(Errors: validation.ToDictionary());
        }

        var user = await context.Users.FindAsync([command.Id], ct);
        if (user is null)
        {
            return new(NotFound: true);
        }

        user.IsActive = false;
        await context.SaveChangesAsync(ct);
        return new(true);
    }
}

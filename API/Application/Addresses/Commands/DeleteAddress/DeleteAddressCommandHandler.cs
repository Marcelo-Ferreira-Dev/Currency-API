using API.Application.Common;
using API.Data;
using FluentValidation;

namespace API.Application.Addresses.Commands.DeleteAddress;

public class DeleteAddressCommandHandler(AppDbContext context, IValidator<DeleteAddressCommand> validator)
{
    public async Task<OperationResult<bool>> HandleAsync(DeleteAddressCommand command, CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return new(Errors: validation.ToDictionary());
        }

        var address = await context.Addresses.FindAsync([command.Id], ct);
        if (address is null)
        {
            return new(NotFound: true);
        }

        address.IsActive = false;
        await context.SaveChangesAsync(ct);
        return new(true);
    }
}

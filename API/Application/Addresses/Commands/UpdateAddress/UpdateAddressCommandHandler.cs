using API.Data;
using API.Entities;
using API.Contracts;
using API.Application.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
namespace API.Application.Addresses.Commands.UpdateAddress;
public class UpdateAddressCommandHandler(AppDbContext context, IValidator<UpdateAddressCommand> validator)
{
    public async Task<OperationResult<AddressResponse>> HandleAsync(UpdateAddressCommand command, CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid) return new(Errors: validation.ToDictionary());
        var address = await context.Addresses.FindAsync([command.Id], ct);
        if (address is null) return new(NotFound: true);
        address.Street = command.Street.Trim(); address.City = command.City.Trim();
        address.Country = command.Country.Trim(); address.ZipCode = command.ZipCode?.Trim();
        address.IsActive = command.IsActive;
        await context.SaveChangesAsync(ct);
        return new(new(address.Id, address.UserId, address.Street, address.City, address.Country, address.ZipCode, address.IsActive));
    }
}

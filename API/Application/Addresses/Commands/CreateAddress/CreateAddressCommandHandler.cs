using API.Data;
using API.Entities;
using API.Contracts;
using API.Application.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
namespace API.Application.Addresses.Commands.CreateAddress;
public class CreateAddressCommandHandler(AppDbContext context, IValidator<CreateAddressCommand> validator)
{
    public async Task<OperationResult<AddressResponse>> HandleAsync(CreateAddressCommand command, CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid) return new(Errors: validation.ToDictionary());
        if (!await context.Users.AnyAsync(u => u.Id == command.UserId, ct)) return new(NotFound: true);
        var address = new Address { UserId = command.UserId, Street = command.Street.Trim(),
            City = command.City.Trim(), Country = command.Country.Trim(), ZipCode = command.ZipCode?.Trim() };
        context.Addresses.Add(address);
        try { await context.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException { SqliteExtendedErrorCode: 787 })
        { return new(NotFound: true); }
        return new(new(address.Id, address.UserId, address.Street, address.City, address.Country, address.ZipCode, address.IsActive));
    }
}

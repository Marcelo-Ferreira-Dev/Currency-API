using API.Application.Common;
using API.Contracts;
using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Addresses.Queries.GetUserAddresses;

public class GetUserAddressesQueryHandler(AppDbContext context)
{
    public async Task<OperationResult<List<AddressResponse>>> HandleAsync(GetUserAddressesQuery query, CancellationToken ct = default)
    {
        if (!await context.Users.AnyAsync(u => u.Id == query.UserId, ct))
        {
            return new(NotFound: true);
        }

        var addressesQuery = context.Addresses.AsNoTracking().Where(a => a.UserId == query.UserId);
        if (query.IsActive.HasValue)
        {
            addressesQuery = addressesQuery.Where(a => a.IsActive == query.IsActive.Value);
        }

        var addresses = await addressesQuery.OrderBy(a => a.Id)
            .Select(a => new AddressResponse(a.Id, a.UserId, a.Street, a.City, a.Country, a.ZipCode, a.IsActive)).ToListAsync(ct);
        return new(addresses);
    }
}

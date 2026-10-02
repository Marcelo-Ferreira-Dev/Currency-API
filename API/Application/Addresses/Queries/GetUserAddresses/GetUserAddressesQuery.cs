namespace API.Application.Addresses.Queries.GetUserAddresses;

public record GetUserAddressesQuery(int UserId, bool? IsActive = null);

namespace API.Contracts;

public record AddressResponse(int Id, int UserId, string Street, string City, string Country, string? ZipCode, bool IsActive);

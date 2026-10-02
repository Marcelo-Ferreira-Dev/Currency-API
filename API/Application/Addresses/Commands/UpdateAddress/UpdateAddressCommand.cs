namespace API.Application.Addresses.Commands.UpdateAddress;

public class UpdateAddressCommand
{
    [System.Text.Json.Serialization.JsonIgnore]
    public int Id
    {
        get; set;
    }

    public string Street { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string? ZipCode
    {
        get; set;
    }
    public bool IsActive { get; set; } = true;
}

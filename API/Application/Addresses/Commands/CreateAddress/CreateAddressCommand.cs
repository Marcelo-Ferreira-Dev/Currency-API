namespace API.Application.Addresses.Commands.CreateAddress
{
    public class CreateAddressCommand
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int UserId { get; set; }

        public string Street { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string? ZipCode { get; set; }
    }
}

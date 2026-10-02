namespace API.Entities;

public class Address
{
    public int Id
    {
        get; set;
    }
    public required int UserId
    {
        get; set;
    }
    public required string Street
    {
        get; set;
    }
    public required string City
    {
        get; set;
    }
    public required string Country
    {
        get; set;
    }
    public string? ZipCode
    {
        get; set;
    }
    public bool IsActive { get; set; } = true;
    public User User { get; set; } = null!;
}

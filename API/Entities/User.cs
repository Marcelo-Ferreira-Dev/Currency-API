namespace API.Entities;

public class User
{
    public int Id
    {
        get; set;
    }
    public required string Name
    {
        get; set;
    }
    public required string Email
    {
        get; set;
    }
    public bool IsActive { get; set; } = true;
    public required string Password
    {
        get; set;
    }
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}

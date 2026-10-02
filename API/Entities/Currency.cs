namespace API.Entities;

public class Currency
{
    public int Id
    {
        get; set;
    }
    public required string Code
    {
        get; set;
    }
    public required string Name
    {
        get; set;
    }
    public required decimal RateToBase
    {
        get; set;
    }
    public bool IsActive { get; set; } = true;
}

namespace API.Application.Users.Commands.UpdateUser;

public class UpdateUserCommand
{
    [System.Text.Json.Serialization.JsonIgnore]
    public int Id
    {
        get; set;
    }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive
    {
        get; set;
    }
}

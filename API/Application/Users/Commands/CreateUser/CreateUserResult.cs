using API.Entities;

namespace API.Application.Users.Commands.CreateUser;

public record CreateUserResult(
    User? User,
    IDictionary<string, string[]>? Errors,
    bool EmailAlreadyExists = false);

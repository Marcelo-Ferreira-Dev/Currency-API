using API.Application.Users.Commands.CreateUser;
using API.Application.Users.Commands.DeleteUser;
using API.Application.Users.Commands.UpdateUser;
using API.Application.Users.Queries.GetUserById;
using API.Application.Users.Queries.GetUsers;
using API.Contracts;

namespace API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var users = app.MapGroup("/users").WithTags("Users");
        users.MapPost("", CreateAsync).WithSummary("Crear usuario")
            .Produces<UserResponse>(201).ProducesValidationProblem().Produces(409);
        users.MapGet("", async (bool? isActive, GetUsersQueryHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(new(isActive), ct)))
            .WithSummary("Listar usuarios; filtro isActive opcional").Produces<List<UserResponse>>();
        users.MapGet("/{id:int}", async (int id, GetUserByIdQueryHandler handler, CancellationToken ct) =>
        {
            var user = await handler.HandleAsync(new(id), ct);
            return user is null ? Results.NotFound() : Results.Ok(user);
        }).WithSummary("Obtener usuario").Produces<UserResponse>().Produces(404);
        users.MapPut("/{id:int}", async (int id, UpdateUserCommand command, UpdateUserCommandHandler handler, CancellationToken ct) =>
        {
            command.Id = id;
            return (await handler.HandleAsync(command, ct)).ToHttp();
        }).WithSummary("Actualizar usuario").Produces<UserResponse>().ProducesValidationProblem().Produces(404).Produces(409);
        users.MapDelete("/{id:int}", async (int id, DeleteUserCommandHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(new()
            {
                Id = id
            }, ct)).ToHttp(_ => Results.NoContent()))
            .WithSummary("Eliminar usuario de forma lógica (IsActive = false)").Produces(204).ProducesValidationProblem().Produces(404);
    }
    private static async Task<IResult> CreateAsync(CreateUserCommand command, CreateUserCommandHandler handler, CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);
        if (result.Errors is not null)
        {
            return Results.ValidationProblem(result.Errors);
        }

        if (result.EmailAlreadyExists)
        {
            return Results.Conflict(new
            {
                message = "El email ya está registrado."
            });
        }

        var user = result.User!;
        return Results.Created($"/users/{user.Id}", new UserResponse(user.Id, user.Name, user.Email, user.IsActive));
    }
}

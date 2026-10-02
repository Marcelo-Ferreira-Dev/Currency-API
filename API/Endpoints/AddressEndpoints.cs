using API.Application.Addresses.Commands.CreateAddress;
using API.Application.Addresses.Commands.DeleteAddress;
using API.Application.Addresses.Commands.UpdateAddress;
using API.Application.Addresses.Queries.GetUserAddresses;
using API.Contracts;

namespace API.Endpoints;

public static class AddressEndpoints
{
    public static void MapAddressEndpoints(this WebApplication app)
    {
        var nested = app.MapGroup("/users/{userId:int}/addresses").WithTags("Addresses");
        nested.MapPost("", async (int userId, CreateAddressCommand command, CreateAddressCommandHandler handler, CancellationToken ct) =>
        {
            command.UserId = userId;
            return (await handler.HandleAsync(command, ct)).ToHttp(address =>
                Results.Created($"/users/{userId}/addresses", address));
        }).WithSummary("Crear dirección para un usuario").Produces<AddressResponse>(201).ProducesValidationProblem().Produces(404);
        nested.MapGet("", async (int userId, bool? isActive, GetUserAddressesQueryHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(new(userId, isActive), ct)).ToHttp())
            .WithSummary("Listar direcciones de un usuario").Produces<List<AddressResponse>>().Produces(404);
        var addresses = app.MapGroup("/addresses").WithTags("Addresses");
        addresses.MapPut("/{id:int}", async (int id, UpdateAddressCommand command, UpdateAddressCommandHandler handler, CancellationToken ct) =>
        {
            command.Id = id;
            return (await handler.HandleAsync(command, ct)).ToHttp();
        }).WithSummary("Actualizar dirección").Produces<AddressResponse>().ProducesValidationProblem().Produces(404);
        addresses.MapDelete("/{id:int}", async (int id, DeleteAddressCommandHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(new()
            {
                Id = id
            }, ct)).ToHttp(_ => Results.NoContent()))
            .WithSummary("Eliminar dirección de forma lógica (IsActive = false)").Produces(204).ProducesValidationProblem().Produces(404);
    }
}

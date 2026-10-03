using API.Application.Addresses.Commands.CreateAddress;
using API.Application.Addresses.Commands.DeleteAddress;
using API.Application.Addresses.Commands.UpdateAddress;
using API.Application.Addresses.Queries.GetUserAddresses;
using API.Application.Currencies.Commands.CreateCurrency;
using API.Application.Currencies.Queries.GetCurrencies;
using API.Application.CurrencyConversion.ConvertCurrency;
using API.Application.Users.Commands.CreateUser;
using API.Application.Users.Commands.DeleteUser;
using API.Application.Users.Commands.UpdateUser;
using API.Application.Users.Queries.GetUserById;
using API.Application.Users.Queries.GetUsers;
using FluentValidation;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRequestValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();
        return services;
    }

    public static IServiceCollection AddUserHandlers(this IServiceCollection services)
    {
        services.AddScoped<CreateUserCommandHandler>();
        services.AddScoped<UpdateUserCommandHandler>();
        services.AddScoped<DeleteUserCommandHandler>();
        services.AddScoped<GetUsersQueryHandler>();
        services.AddScoped<GetUserByIdQueryHandler>();
        return services;
    }

    public static IServiceCollection AddAddressHandlers(this IServiceCollection services)
    {
        services.AddScoped<CreateAddressCommandHandler>();
        services.AddScoped<UpdateAddressCommandHandler>();
        services.AddScoped<DeleteAddressCommandHandler>();
        services.AddScoped<GetUserAddressesQueryHandler>();
        return services;
    }

    public static IServiceCollection AddCurrencyHandlers(this IServiceCollection services)
    {
        services.AddScoped<CreateCurrencyCommandHandler>();
        services.AddScoped<GetCurrenciesQueryHandler>();
        services.AddScoped<ConvertCurrencyCommandHandler>();
        return services;
    }
}

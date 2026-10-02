using API.Data;
using API.Application.Users.Commands.CreateUser;
using API.Application.Users.Commands.UpdateUser;
using API.Application.Users.Commands.DeleteUser;
using API.Application.Users.Queries.GetUsers;
using API.Application.Users.Queries.GetUserById;
using API.Endpoints;
using API.Application.Addresses.Commands.CreateAddress;
using API.Application.Addresses.Commands.UpdateAddress;
using API.Application.Addresses.Commands.DeleteAddress;
using API.Application.Addresses.Queries.GetUserAddresses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se configuró DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();
builder.Services.AddScoped<CreateUserCommandHandler>();
builder.Services.AddScoped<UpdateUserCommandHandler>();
builder.Services.AddScoped<DeleteUserCommandHandler>();
builder.Services.AddScoped<GetUsersQueryHandler>();
builder.Services.AddScoped<GetUserByIdQueryHandler>();
builder.Services.AddScoped<CreateAddressCommandHandler>();
builder.Services.AddScoped<UpdateAddressCommandHandler>();
builder.Services.AddScoped<DeleteAddressCommandHandler>();
builder.Services.AddScoped<GetUserAddressesQueryHandler>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapUserEndpoints();
app.MapAddressEndpoints();

app.Run();

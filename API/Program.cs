using API.Data;
using API.Application.Users.Commands.CreateUser;
using API.Application.Users.Commands.UpdateUser;
using API.Application.Users.Commands.DeleteUser;
using API.Application.Users.Queries.GetUsers;
using API.Application.Users.Queries.GetUserById;
using API.Endpoints;
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

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapUserEndpoints();

app.Run();

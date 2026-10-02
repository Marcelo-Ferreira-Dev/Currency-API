using API.Application.Users.Commands.CreateUser;
using API.Application.Users.Commands.DeleteUser;
using API.Application.Users.Commands.UpdateUser;
using FluentValidation.TestHelper;

namespace API.Tests.Users;

public class UserValidationTests
{
    private static CreateUserCommand ValidCreate() => new()
    {
        Name = "Marcelo Ferreira",
        Email = "marcelod.ferreira.dev@gmail.com",
        Password = "P@sswordsegura"
    };

    [Fact]
    public void Create_accepts_valid_user()
    {
        new CreateUserCommandValidator().TestValidate(ValidCreate()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_empty_name(string? name)
    {
        var command = ValidCreate();
        command.Name = name!;
        new CreateUserCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sin-arroba")]
    public void Create_rejects_invalid_email(string? email)
    {
        var command = ValidCreate();
        command.Email = email!;
        new CreateUserCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_empty_password_without_throwing(string? password)
    {
        var command = ValidCreate();
        command.Password = password!;
        new CreateUserCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData('a', 72, true)]
    [InlineData('a', 73, false)]
    [InlineData('á', 36, true)]
    [InlineData('á', 37, false)]
    public void Create_checks_password_limit_in_utf8_bytes(char character, int count, bool valid)
    {
        var command = ValidCreate();
        command.Password = new string(character, count);
        var result = new CreateUserCommandValidator().TestValidate(command);
        if (valid)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Update_accepts_both_active_states(bool isActive)
    {
        var command = new UpdateUserCommand
        {
            Id = 1,
            Name = "Marcelo Ferreira",
            Email = "marcelod.ferreira.dev@gmail.com",
            IsActive = isActive
        };
        new UpdateUserCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Update_rejects_invalid_id_name_and_email()
    {
        var result = new UpdateUserCommandValidator().TestValidate(new UpdateUserCommand
        {
            Id = 0,
            Name = " ",
            Email = "invalido"
        });
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Delete_rejects_non_positive_id(int id)
    {
        new DeleteUserCommandValidator().TestValidate(new DeleteUserCommand { Id = id })
            .ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Delete_accepts_positive_id()
    {
        new DeleteUserCommandValidator().TestValidate(new DeleteUserCommand { Id = 1 })
            .ShouldNotHaveAnyValidationErrors();
    }
}

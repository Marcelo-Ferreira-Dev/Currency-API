using System.Text;
using FluentValidation;

namespace API.Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty();
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
        RuleFor(command => command.Password)
            .NotEmpty()
            .Must(password => password is null || Encoding.UTF8.GetByteCount(password) <= 72)
            .WithMessage("La contraseña no puede superar 72 bytes UTF-8.");
    }
}

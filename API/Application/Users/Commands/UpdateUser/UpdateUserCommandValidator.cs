using FluentValidation;

namespace API.Application.Users.Commands.UpdateUser
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(command => command.Id)
                .GreaterThan(0);

            RuleFor(command => command.Name)
                .NotEmpty();

            RuleFor(command => command.Email)
                .NotEmpty().EmailAddress();
        }
    }
}

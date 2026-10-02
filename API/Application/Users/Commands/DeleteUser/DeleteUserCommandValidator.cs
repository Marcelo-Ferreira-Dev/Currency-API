using FluentValidation;

namespace API.Application.Users.Commands.DeleteUser
{
    public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserCommandValidator()
        {
            RuleFor(command => command.Id)
                .GreaterThan(0);
        }
    }
}

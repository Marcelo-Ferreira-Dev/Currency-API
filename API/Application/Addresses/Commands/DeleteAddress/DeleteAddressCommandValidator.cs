using FluentValidation;

namespace API.Application.Addresses.Commands.DeleteAddress
{
    public class DeleteAddressCommandValidator : AbstractValidator<DeleteAddressCommand>
    {
        public DeleteAddressCommandValidator()
        {
            RuleFor(command => command.Id)
                .GreaterThan(0);
        }
    }
}

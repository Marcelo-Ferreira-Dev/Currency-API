using FluentValidation;

namespace API.Application.Addresses.Commands.CreateAddress
{
    public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
    {
        public CreateAddressCommandValidator()
        {
            RuleFor(command => command.UserId)
                .GreaterThan(0);

            RuleFor(command => command.Street)
                .NotEmpty();

            RuleFor(command => command.City)
                .NotEmpty();

            RuleFor(command => command.Country)
                .NotEmpty();
        }
    }
}

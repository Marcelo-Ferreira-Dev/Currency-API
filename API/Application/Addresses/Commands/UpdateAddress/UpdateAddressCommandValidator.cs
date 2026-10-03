using FluentValidation;

namespace API.Application.Addresses.Commands.UpdateAddress;

public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0);

        RuleFor(command => command.Street)
            .NotEmpty();

        RuleFor(command => command.City)
            .NotEmpty();

        RuleFor(command => command.Country)
            .NotEmpty();
    }
}

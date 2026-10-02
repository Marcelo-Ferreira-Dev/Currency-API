using API.Application.Addresses.Commands.CreateAddress;
using API.Application.Addresses.Commands.DeleteAddress;
using API.Application.Addresses.Commands.UpdateAddress;
using FluentValidation.TestHelper;

namespace API.Tests.Addresses;

public class AddressValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("1209")]
    public void Create_accepts_address_with_optional_zip_code(string? zipCode)
    {
        var command = new CreateAddressCommand
        {
            UserId = 1,
            Street = "Av. Mariscal López 1234",
            City = "Asunción",
            Country = "Paraguay",
            ZipCode = zipCode
        };
        new CreateAddressCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_empty_required_fields(string? value)
    {
        var result = new CreateAddressCommandValidator().TestValidate(new CreateAddressCommand
        {
            UserId = 1,
            Street = value!,
            City = value!,
            Country = value!
        });
        result.ShouldHaveValidationErrorFor(x => x.Street);
        result.ShouldHaveValidationErrorFor(x => x.City);
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_user_id(int id)
    {
        new CreateAddressCommandValidator().TestValidate(new CreateAddressCommand
        {
            UserId = id,
            Street = "Av. Mariscal López",
            City = "Asunción",
            Country = "Paraguay"
        }).ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Update_accepts_both_active_states(bool isActive)
    {
        new UpdateAddressCommandValidator().TestValidate(new UpdateAddressCommand
        {
            Id = 1,
            Street = "Av. España 456",
            City = "Asunción",
            Country = "Paraguay",
            IsActive = isActive
        }).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Update_rejects_invalid_id_and_required_fields()
    {
        var result = new UpdateAddressCommandValidator().TestValidate(new UpdateAddressCommand
        {
            Id = 0,
            Street = "",
            City = " ",
            Country = null!
        });
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Street);
        result.ShouldHaveValidationErrorFor(x => x.City);
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Delete_rejects_non_positive_id(int id)
    {
        new DeleteAddressCommandValidator().TestValidate(new DeleteAddressCommand { Id = id })
            .ShouldHaveValidationErrorFor(x => x.Id);
    }
}

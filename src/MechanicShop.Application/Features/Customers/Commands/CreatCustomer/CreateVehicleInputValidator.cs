using FluentValidation;
namespace MechanicShop.Application.Features.Customers.Commands.CreatCustomer;
public sealed class CreateVehicleInputValidator : AbstractValidator<CreateVehicleInput>
{
    public CreateVehicleInputValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Model)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.LicensePlate)
            .NotEmpty()
            .MaximumLength(10);
    }
}

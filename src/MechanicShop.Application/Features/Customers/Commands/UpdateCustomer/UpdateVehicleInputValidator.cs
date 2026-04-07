using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateVehicleInputValidator : AbstractValidator<UpdateVehicleInput>
{
	public UpdateVehicleInputValidator()
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
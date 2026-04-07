using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
	public UpdateCustomerCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.MaximumLength(100);

		RuleFor(x => x.Email)
			.NotEmpty()
			.EmailAddress()
			.MaximumLength(100);

		RuleFor(x => x.PhoneNumber)
			.NotEmpty()
			.Matches(@"^\+?\d{7,15}$");

		RuleFor(x => x.Vehicles)
			.NotNull()
			.NotEmpty()
			.Must(HaveUniqueLicensePlates)
			.WithMessage("Vehicles must not contain duplicate LicensePlate values.");

		RuleForEach(x => x.Vehicles)
			.SetValidator(new UpdateVehicleInputValidator());
	}

	private static bool HaveUniqueLicensePlates(List<UpdateVehicleInput>? vehicles)
	{
		if (vehicles is null)
		{
			return true;
		}

		var normalizedPlates = vehicles
			.Select(vehicle => vehicle.LicensePlate?.Trim())
			.Where(licensePlate => !string.IsNullOrWhiteSpace(licensePlate))
			.Select(licensePlate => licensePlate!.ToUpperInvariant())
			.ToList();

		return normalizedPlates.Distinct().Count() == normalizedPlates.Count;
	}
}
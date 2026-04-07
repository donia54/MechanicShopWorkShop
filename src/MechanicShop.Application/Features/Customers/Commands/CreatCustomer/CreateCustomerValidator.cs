using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.CreatCustomer;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
	public CreateCustomerCommandValidator()
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
			.SetValidator(new CreateVehicleInputValidator());
	}

	private static bool HaveUniqueLicensePlates(List<CreateVehicleInput>? vehicles)
	{
		if (vehicles is null)
		{
			return true;
		}

		var normalizedPlates = vehicles
			.Select(vehicle => vehicle.LicensePlate?.Trim())
			.Where(licensePlate => !string.IsNullOrWhiteSpace(licensePlate))
			.Select(licensePlate => licensePlate!.ToUpperInvariant());

		return normalizedPlates.Distinct().Count() == normalizedPlates.Count();
	}
}

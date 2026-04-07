namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed record UpdateVehicleInput(
	string Make,
	string Model,
	int Year,
	string LicensePlate);
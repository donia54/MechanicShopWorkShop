namespace MechanicShop.Application.Features.Customers.Commands.CreatCustomer;

public sealed record CreateVehicleInput(
    string Make,
    string Model,
    int Year,
    string LicensePlate);
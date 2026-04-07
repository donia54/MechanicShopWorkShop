using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Customers;

namespace MechanicShop.Application.Features.Customers.Mappers;

public static class CustomerMapper
{
	public static CustomerDto ToDto(this Customer customer)
	{
		return new CustomerDto(
			customer.Id,
			customer.Name,
			customer.PhoneNumber,
			customer.Email,
			customer.Vehicles
				.Select(vehicle => new VehicleDto(
					vehicle.Id,
					vehicle.Make,
					vehicle.Model,
					vehicle.Year,
					vehicle.LicensePlate))
				.ToList());
	}
}

using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;

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
    
       public static List<CustomerDto> ToDtos(this IEnumerable<Customer> entities)
    {
        return [.. entities.Select(e => e.ToDto())];
    }

    public static VehicleDto ToDto(this Vehicle entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new VehicleDto(entity.Id, entity.Make!, entity.Model!, entity.Year, entity.LicensePlate!);
    }

    public static List<VehicleDto> ToDtos(this IEnumerable<Vehicle> entities)
    {
        return [.. entities.Select(e => e.ToDto())];
    }
}

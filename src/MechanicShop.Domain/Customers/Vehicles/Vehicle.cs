using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;

namespace MechanicShop.Domain.Customers.Vehicles;

public sealed class Vehicle : AuditableEntity
{
    public Guid CustomerId { get; private set; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public string LicensePlate { get; private set; }
    public Customer? Customer { get; private set; }

    public string VehicleInfo => $"{Make} | {Model} | {Year}";

    private Vehicle()
    {
        Make = string.Empty;
        Model = string.Empty;
        LicensePlate = string.Empty;
    }

    private Vehicle(Guid id, string make, string model, int year, string licensePlate)
        : base(id)
    {
        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;
    }

    public static Result<Vehicle> Create(Guid id, string make, string model, int year, string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(make))
        {
            return VehicleErrors.MakeRequired;
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            return VehicleErrors.ModelRequired;
        }

        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return VehicleErrors.LicensePlateRequired;
        }

        if (year < 1886 || year > DateTime.UtcNow.Year)
        {
            return VehicleErrors.YearInvalid;
        }

        var vehicle = new Vehicle(id, make.Trim(), model.Trim(), year, licensePlate.Trim());
        vehicle.AddDomainEvent(new VehicleCreated(vehicle.Id, DateTimeOffset.UtcNow));

        return vehicle;
    }

    public Result<Updated> Update(string make, string model, int year, string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(make))
        {
            return VehicleErrors.MakeRequired;
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            return VehicleErrors.ModelRequired;
        }

        if (year < 1886 || year > DateTime.UtcNow.Year)
        {
            return VehicleErrors.YearInvalid;
        }

        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return VehicleErrors.LicensePlateRequired;
        }

        Make = make.Trim();
        Model = model.Trim();
        Year = year;
        LicensePlate = licensePlate.Trim();

        AddDomainEvent(new VehicleUpdated(Id, DateTimeOffset.UtcNow));

        return Result.Updated;
    }

    internal Result<Updated> AssignCustomer(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            return VehicleErrors.CustomerIdRequired;
        }

        CustomerId = customerId;
        return Result.Updated;
    }
}

public sealed class VehicleCreated : DomainEvent
{
    public Guid VehicleId { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public VehicleCreated(Guid vehicleId, DateTimeOffset occurredAtUtc)
    {
        VehicleId = vehicleId;
        OccurredAtUtc = occurredAtUtc;
    }
}

public sealed class VehicleUpdated : DomainEvent
{
    public Guid VehicleId { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public VehicleUpdated(Guid vehicleId, DateTimeOffset occurredAtUtc)
    {
        VehicleId = vehicleId;
        OccurredAtUtc = occurredAtUtc;
    }
}
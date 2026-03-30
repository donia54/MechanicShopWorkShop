using System.Net.Mail;
using System.Text.RegularExpressions;

using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;

namespace MechanicShop.Domain.Customers;

public sealed class Customer : AuditableEntity
{
    public string Name { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Email { get; private set; }

    private readonly List<Vehicle> _vehicles = [];
    public IReadOnlyList<Vehicle> Vehicles => _vehicles.AsReadOnly();

    private Customer()
    {
        Name = string.Empty;
        PhoneNumber = string.Empty;
        Email = string.Empty;
    }

    private Customer(Guid id, string name, string phoneNumber, string email, IEnumerable<Vehicle> vehicles)
        : base(id)
    {
        Name = name;
        PhoneNumber = phoneNumber;
        Email = email;
        _vehicles.AddRange(vehicles);
    }

    public static Result<Customer> Create(Guid id, string name, string phoneNumber, string email, List<Vehicle>? vehicles)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return CustomerErrors.NameRequired;
        }

        if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
        {
            return CustomerErrors.InvalidPhoneNumber;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return CustomerErrors.EmailRequired;
        }

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            return CustomerErrors.EmailInvalid;
        }

        var customer = new Customer(id, name.Trim(), phoneNumber.Trim(), email.Trim(), vehicles ?? []);
        customer.AddDomainEvent(new CustomerCreated(customer.Id, DateTimeOffset.UtcNow));

        return customer;
    }

    public Result<Updated> Update(string name, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return CustomerErrors.NameRequired;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return CustomerErrors.EmailRequired;
        }

        if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
        {
            return CustomerErrors.InvalidPhoneNumber;
        }

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            return CustomerErrors.EmailInvalid;
        }

        Name = name.Trim();
        Email = email.Trim();
        PhoneNumber = phoneNumber.Trim();

        AddDomainEvent(new CustomerUpdated(Id, DateTimeOffset.UtcNow));

        return Result.Updated;
    }

    public Result<Updated> AddVehicle(Vehicle vehicle)
    {
        if (vehicle is null)
        {
            return CustomerErrors.VehicleRequired;
        }

        if (_vehicles.Any(existingVehicle => existingVehicle.Id == vehicle.Id))
        {
            return CustomerErrors.DuplicateVehicle;
        }

        var assignResult = vehicle.AssignCustomer(Id);
        if (assignResult.IsError)
        {
            return assignResult.TopError;
        }

        _vehicles.Add(vehicle);
        AddDomainEvent(new CustomerUpdated(Id, DateTimeOffset.UtcNow));

        return Result.Updated;
    }

    public Result<Updated> RemoveVehicle(Guid vehicleId)
    {
        var existingVehicle = _vehicles.FirstOrDefault(vehicle => vehicle.Id == vehicleId);
        if (existingVehicle is null)
        {
            return CustomerErrors.VehicleNotFound;
        }

        _vehicles.Remove(existingVehicle);
        AddDomainEvent(new CustomerUpdated(Id, DateTimeOffset.UtcNow));

        return Result.Updated;
    }

    public Result<Updated> UpsertVehicles(List<Vehicle> incomingVehicle)
    {
        incomingVehicle ??= [];

        var removedVehicleIds = _vehicles
            .Where(existing => incomingVehicle.All(vehicle => vehicle.Id != existing.Id))
            .Select(vehicle => vehicle.Id)
            .ToList();

        foreach (var vehicleId in removedVehicleIds)
        {
            var removeResult = RemoveVehicle(vehicleId);
            if (removeResult.IsError)
            {
                return removeResult.TopError;
            }
        }

        foreach (var incoming in incomingVehicle)
        {
            var existing = _vehicles.FirstOrDefault(v => v.Id == incoming.Id);
            if (existing is null)
            {
                var addResult = AddVehicle(incoming);
                if (addResult.IsError)
                {
                    return addResult.TopError;
                }
            }
            else
            {
                var updateVehicleResult = existing.Update(incoming.Make, incoming.Model, incoming.Year, incoming.LicensePlate);

                if (updateVehicleResult.IsError)
                {
                    return updateVehicleResult.TopError;
                }

                AddDomainEvent(new CustomerUpdated(Id, DateTimeOffset.UtcNow));
            }
        }

        return Result.Updated;
    }
}

public sealed class CustomerCreated : DomainEvent
{
    public Guid CustomerId { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public CustomerCreated(Guid customerId, DateTimeOffset occurredAtUtc)
    {
        CustomerId = customerId;
        OccurredAtUtc = occurredAtUtc;
    }
}

public sealed class CustomerUpdated : DomainEvent
{
    public Guid CustomerId { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public CustomerUpdated(Guid customerId, DateTimeOffset occurredAtUtc)
    {
        CustomerId = customerId;
        OccurredAtUtc = occurredAtUtc;
    }
}
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Employees.Events;
using MechanicShop.Domain.Identity;

namespace MechanicShop.Domain.Employees;

public sealed class Employee : AuditableEntity
{
	public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Role Role { get; private set; }
    public string FullName => $"{FirstName} {LastName}";


    private Employee()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    private Employee(Guid id, string firstName, string lastName, Role role)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Role = role;
    }

    public static Result<Employee> Create(Guid id, string firstName, string lastName, Role role)
    {
        if (id == Guid.Empty)
        {
            return EmployeeErrors.IdRequired;
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            return EmployeeErrors.FirstNameRequired;
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return EmployeeErrors.LastNameRequired;
        }

        if (!Enum.IsDefined(role))
        {
            return EmployeeErrors.RoleInvalid;
        }

        return new Employee(id, firstName.Trim(), lastName.Trim(), role);
    }

    public Result<Updated> UpdateFirstName(string? firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return EmployeeErrors.FirstNameRequired;
        }

        FirstName = firstName.Trim();
        AddDomainEvent(new EmployeeUpdated(Id, DateTimeOffset.UtcNow));
        return Result.Updated;
    }

    public Result<Updated> UpdateLastName(string? lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
        {
            return EmployeeErrors.LastNameRequired;
        }

        LastName = lastName.Trim();
        AddDomainEvent(new EmployeeUpdated(Id, DateTimeOffset.UtcNow));
        return Result.Updated;
    }

    public Result<Updated> UpdateRole(Role role)
    {
        if (!Enum.IsDefined(role))
        {
            return EmployeeErrors.RoleInvalid;
        }

        Role = role;
        AddDomainEvent(new EmployeeUpdated(Id, DateTimeOffset.UtcNow));
        return Result.Updated;
    }

}


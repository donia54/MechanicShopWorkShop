using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.Employees.Events;

public sealed class EmployeeUpdated : DomainEvent
{
    public Guid EmployeeId { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public EmployeeUpdated(Guid employeeId, DateTimeOffset occurredAtUtc)
    {
        EmployeeId = employeeId;
        OccurredAtUtc = occurredAtUtc;
    }
}

using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.WorkOrders.Events;

public sealed class WorkOrderStarted : DomainEvent
{
    public Guid WorkOrderId { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public WorkOrderStarted(Guid workOrderId, DateTimeOffset startedAtUtc)
    {
        WorkOrderId = workOrderId;
        StartedAtUtc = startedAtUtc;
    }
}
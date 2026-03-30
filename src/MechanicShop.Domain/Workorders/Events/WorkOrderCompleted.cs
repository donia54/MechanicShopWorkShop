using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.WorkOrders.Events;

public sealed class WorkOrderCompleted : DomainEvent
{
    public Guid WorkOrderId { get; }

    public DateTimeOffset CompletedAtUtc { get; }

    public WorkOrderCompleted(Guid workOrderId, DateTimeOffset completedAtUtc)
    {
        WorkOrderId = workOrderId;
        CompletedAtUtc = completedAtUtc;
    }
}

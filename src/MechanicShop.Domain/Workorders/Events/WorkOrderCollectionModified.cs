using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.WorkOrders.Events;

public sealed class WorkOrderCollectionModified : DomainEvent
{
    public Guid WorkOrderId { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public WorkOrderCollectionModified(Guid workOrderId, DateTimeOffset occurredAtUtc)
    {
        WorkOrderId = workOrderId;
        OccurredAtUtc = occurredAtUtc;
    }
}

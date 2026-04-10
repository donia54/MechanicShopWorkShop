using MechanicShop.Domain.Common;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Domain.WorkOrders.Events;

public sealed class WorkOrderRelocated : DomainEvent
{
    public Guid WorkOrderId { get; }

    public DateTimeOffset StartAtUtc { get; }

    public DateTimeOffset EndAtUtc { get; }

    public Spot Spot { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public WorkOrderRelocated(
        Guid workOrderId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        Spot spot,
        DateTimeOffset occurredAtUtc)
    {
        WorkOrderId = workOrderId;
        StartAtUtc = startAtUtc;
        EndAtUtc = endAtUtc;
        Spot = spot;
        OccurredAtUtc = occurredAtUtc;
    }
}
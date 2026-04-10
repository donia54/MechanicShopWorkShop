using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.WorkOrders.Events;

public sealed class WorkOrderCancelled : DomainEvent
{
    public Guid WorkOrderId { get; }

    public DateTimeOffset CancelledAtUtc { get; }

    public WorkOrderCancelled(Guid workOrderId, DateTimeOffset cancelledAtUtc)
    {
        WorkOrderId = workOrderId;
        CancelledAtUtc = cancelledAtUtc;
    }
}
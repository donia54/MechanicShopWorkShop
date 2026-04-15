using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Common.Interfaces;

public interface IWorkOrderNotifier
{
	Task NotifyWorkOrderStartedAsync(Guid workOrderId, DateTimeOffset startedAtUtc, CancellationToken cancellationToken);

	Task NotifyWorkOrderCancelledAsync(Guid workOrderId, DateTimeOffset cancelledAtUtc, CancellationToken cancellationToken);

	Task NotifyWorkOrderRelocatedAsync(
		Guid workOrderId,
		DateTimeOffset startAtUtc,
		DateTimeOffset endAtUtc,
		Spot spot,
		CancellationToken cancellationToken);
}
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Events;

using MediatR;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers;

public sealed class WorkOrderCancelledEventHandler : INotificationHandler<WorkOrderCancelled>
{
	private readonly IEnumerable<IWorkOrderNotifier> _workOrderNotifiers;
	private readonly ILogger<WorkOrderCancelledEventHandler> _logger;

	public WorkOrderCancelledEventHandler(
		IEnumerable<IWorkOrderNotifier> workOrderNotifiers,
		ILogger<WorkOrderCancelledEventHandler> logger)
	{
		_workOrderNotifiers = workOrderNotifiers;
		_logger = logger;
	}

	public async Task Handle(WorkOrderCancelled notification, CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"WorkOrder cancelled event received. WorkOrderId: {WorkOrderId}, CancelledAtUtc: {CancelledAtUtc}",
			notification.WorkOrderId,
			notification.CancelledAtUtc);

		foreach (var notifier in _workOrderNotifiers)
		{
			await notifier.NotifyWorkOrderCancelledAsync(notification.WorkOrderId, notification.CancelledAtUtc, cancellationToken);
		}

		_logger.LogInformation("WorkOrder cancelled event handled successfully. WorkOrderId: {WorkOrderId}", notification.WorkOrderId);
	}
}
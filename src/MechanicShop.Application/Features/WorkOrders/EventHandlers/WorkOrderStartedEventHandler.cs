using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Events;

using MediatR;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers;

public sealed class WorkOrderStartedEventHandler : INotificationHandler<WorkOrderStarted>
{
	private readonly IEnumerable<IWorkOrderNotifier> _workOrderNotifiers;
	private readonly ILogger<WorkOrderStartedEventHandler> _logger;

	public WorkOrderStartedEventHandler(
		IEnumerable<IWorkOrderNotifier> workOrderNotifiers,
		ILogger<WorkOrderStartedEventHandler> logger)
	{
		_workOrderNotifiers = workOrderNotifiers;
		_logger = logger;
	}

	public async Task Handle(WorkOrderStarted notification, CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"WorkOrder started event received. WorkOrderId: {WorkOrderId}, StartedAtUtc: {StartedAtUtc}",
			notification.WorkOrderId,
			notification.StartedAtUtc);

		foreach (var notifier in _workOrderNotifiers)
		{
			await notifier.NotifyWorkOrderStartedAsync(notification.WorkOrderId, notification.StartedAtUtc, cancellationToken);
		}

		_logger.LogInformation("WorkOrder started event handled successfully. WorkOrderId: {WorkOrderId}", notification.WorkOrderId);
	}
}
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Events;

using MediatR;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers;

public sealed class WorkOrderRelocatedEventHandler : INotificationHandler<WorkOrderRelocated>
{
	private readonly IEnumerable<IWorkOrderNotifier> _workOrderNotifiers;
	private readonly ILogger<WorkOrderRelocatedEventHandler> _logger;

	public WorkOrderRelocatedEventHandler(
		IEnumerable<IWorkOrderNotifier> workOrderNotifiers,
		ILogger<WorkOrderRelocatedEventHandler> logger)
	{
		_workOrderNotifiers = workOrderNotifiers;
		_logger = logger;
	}

	public async Task Handle(WorkOrderRelocated notification, CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"WorkOrder relocated event received. WorkOrderId: {WorkOrderId}, StartAtUtc: {StartAtUtc}, EndAtUtc: {EndAtUtc}, Spot: {Spot}",
			notification.WorkOrderId,
			notification.StartAtUtc,
			notification.EndAtUtc,
			notification.Spot);

		foreach (var notifier in _workOrderNotifiers)
		{
			await notifier.NotifyWorkOrderRelocatedAsync(
				notification.WorkOrderId,
				notification.StartAtUtc,
				notification.EndAtUtc,
				notification.Spot,
				cancellationToken);
		}

		_logger.LogInformation("WorkOrder relocated event handled successfully. WorkOrderId: {WorkOrderId}", notification.WorkOrderId);
	}
}
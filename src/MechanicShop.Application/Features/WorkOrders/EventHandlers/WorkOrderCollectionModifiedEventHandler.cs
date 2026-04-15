using MechanicShop.Application.Features.WorkOrders.Queries;
using MechanicShop.Domain.WorkOrders.Events;

using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers;

public sealed class WorkOrderCollectionModifiedEventHandler : INotificationHandler<WorkOrderCollectionModified>
{
	private readonly HybridCache _cache;
	private readonly ILogger<WorkOrderCollectionModifiedEventHandler> _logger;

	public WorkOrderCollectionModifiedEventHandler(
		HybridCache cache,
		ILogger<WorkOrderCollectionModifiedEventHandler> logger)
	{
		_cache = cache;
		_logger = logger;
	}

	public async Task Handle(WorkOrderCollectionModified notification, CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"WorkOrder collection modified event received. WorkOrderId: {WorkOrderId}, OccurredAtUtc: {OccurredAtUtc}",
			notification.WorkOrderId,
			notification.OccurredAtUtc);

		await _cache.RemoveByTagAsync(WorkOrderQueryCacheConstants.WorkOrderTag, cancellationToken: cancellationToken);

		_logger.LogInformation("WorkOrder cache invalidated for collection modified event. WorkOrderId: {WorkOrderId}", notification.WorkOrderId);
	}
}
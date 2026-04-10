using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;

public sealed class DeleteWorkOrderCommandHandler : IRequestHandler<DeleteWorkOrderCommand, Result<Deleted>>
{
	private const string WorkOrderCacheTag = "workorder";

	private readonly IAppDbContext _dbContext;
	private readonly ILogger<DeleteWorkOrderCommandHandler> _logger;
	private readonly HybridCache _cache;

	public DeleteWorkOrderCommandHandler(
		IAppDbContext dbContext,
		ILogger<DeleteWorkOrderCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Deleted>> Handle(DeleteWorkOrderCommand request, CancellationToken cancellationToken)
	{
		var workOrder = await _dbContext.WorkOrders
			.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);

		if (workOrder is null)
		{
			_logger.LogWarning("Delete workorder failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		if (workOrder.State is WorkOrderState.InProgress or WorkOrderState.Completed)
		{
			_logger.LogInformation("Delete workorder failed due to state rule. WorkOrderId: {WorkOrderId}, State: {State}", request.WorkOrderId, workOrder.State);
			return Error.Conflict(
				"ApplicationErrors.WorkOrder.NotDeletable",
				"WorkOrder cannot be deleted when it is InProgress or Completed.");
		}

		_dbContext.WorkOrders.Remove(workOrder);
		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderCacheTag, cancellationToken: cancellationToken);

		_logger.LogInformation("Workorder deleted successfully. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
		return Result.Deleted;
	}
}
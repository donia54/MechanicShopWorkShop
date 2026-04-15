using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Commands.RemoveRepairTaskFromWorkOrder;

public sealed class RemoveRepairTaskFromWorkOrderCommandHandler : IRequestHandler<RemoveRepairTaskFromWorkOrderCommand, Result<Updated>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<RemoveRepairTaskFromWorkOrderCommandHandler> _logger;
	private readonly HybridCache _cache;

	public RemoveRepairTaskFromWorkOrderCommandHandler(
		IAppDbContext dbContext,
		ILogger<RemoveRepairTaskFromWorkOrderCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Updated>> Handle(RemoveRepairTaskFromWorkOrderCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"Removing repair task from workorder. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}",
			request.WorkOrderId,
			request.RepairTaskId);

		var workOrder = await _dbContext.WorkOrders
			.Include(order => order.RepairTasks)
				.ThenInclude(task => task.Parts)
			.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);

		if (workOrder is null)
		{
			_logger.LogWarning("Remove repair task failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		var remainingTasks = workOrder.RepairTasks
			.Where(task => task.Id != request.RepairTaskId)
			.ToList();

		if (remainingTasks.Count == workOrder.RepairTasks.Count)
		{
			_logger.LogWarning(
				"Remove repair task failed. RepairTask is not linked to WorkOrder. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}",
				request.WorkOrderId,
				request.RepairTaskId);
			return ApplicationErrors.RepairTask.NotFound(request.RepairTaskId);
		}

		var clearResult = workOrder.ClearRepairTasks();
		if (clearResult.IsError)
		{
			_logger.LogInformation("Remove repair task failed while clearing tasks. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return clearResult.Errors;
		}

		foreach (var task in remainingTasks)
		{
			var addResult = workOrder.AddRepairTask(task);
			if (addResult.IsError)
			{
				_logger.LogInformation(
					"Remove repair task failed while re-adding remaining tasks. WorkOrderId: {WorkOrderId}",
					request.WorkOrderId);
				return addResult.Errors;
			}
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderQueryCacheConstants.WorkOrderTag, cancellationToken: cancellationToken);

		_logger.LogInformation(
			"Repair task removed from workorder successfully. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}",
			request.WorkOrderId,
			request.RepairTaskId);

		return Result.Updated;
	}
}
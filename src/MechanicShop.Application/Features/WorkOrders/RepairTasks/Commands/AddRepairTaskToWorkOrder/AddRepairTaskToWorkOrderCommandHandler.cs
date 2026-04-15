using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Commands.AddRepairTaskToWorkOrder;

public sealed class AddRepairTaskToWorkOrderCommandHandler : IRequestHandler<AddRepairTaskToWorkOrderCommand, Result<Updated>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<AddRepairTaskToWorkOrderCommandHandler> _logger;
	private readonly HybridCache _cache;

	public AddRepairTaskToWorkOrderCommandHandler(
		IAppDbContext dbContext,
		ILogger<AddRepairTaskToWorkOrderCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Updated>> Handle(AddRepairTaskToWorkOrderCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"Adding repair task to workorder. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}",
			request.WorkOrderId,
			request.RepairTaskId);

		var workOrder = await _dbContext.WorkOrders
			.Include(order => order.RepairTasks)
				.ThenInclude(task => task.Parts)
			.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);

		if (workOrder is null)
		{
			_logger.LogWarning("Add repair task failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		var repairTask = await _dbContext.RepairTasks
			.Include(task => task.Parts)
			.FirstOrDefaultAsync(task => task.Id == request.RepairTaskId, cancellationToken);

		if (repairTask is null)
		{
			_logger.LogWarning("Add repair task failed. RepairTask not found: {RepairTaskId}", request.RepairTaskId);
			return ApplicationErrors.RepairTask.NotFound(request.RepairTaskId);
		}

		var addResult = workOrder.AddRepairTask(repairTask);
		if (addResult.IsError)
		{
			_logger.LogInformation(
				"Add repair task failed by domain validation. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}",
				request.WorkOrderId,
				request.RepairTaskId);
			return addResult.Errors;
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderQueryCacheConstants.WorkOrderTag, cancellationToken: cancellationToken);

		_logger.LogInformation(
			"Repair task added to workorder successfully. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}",
			request.WorkOrderId,
			request.RepairTaskId);

		return Result.Updated;
	}
}
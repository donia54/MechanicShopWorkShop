using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public sealed class UpdateWorkOrderRepairTasksCommandHandler : IRequestHandler<UpdateWorkOrderRepairTasksCommand, Result<Updated>>
{
	private const string WorkOrderCacheTag = "workorder";
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<UpdateWorkOrderRepairTasksCommandHandler> _logger;
	private readonly HybridCache _cache;

	public UpdateWorkOrderRepairTasksCommandHandler(
		IAppDbContext dbContext,
		ILogger<UpdateWorkOrderRepairTasksCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Updated>> Handle(UpdateWorkOrderRepairTasksCommand request, CancellationToken cancellationToken)
	{
		var workOrder = await _dbContext.WorkOrders
			.Include(order => order.RepairTasks)
			.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);

		if (workOrder is null)
		{
			_logger.LogWarning("Update repair tasks failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		var repairTaskIds = request.RepairTaskIds.Distinct().ToList();
		var repairTasks = await _dbContext.RepairTasks
			.Where(task => repairTaskIds.Contains(task.Id))
			.ToListAsync(cancellationToken);

		if (repairTasks.Count != repairTaskIds.Count)
		{
			var missingRepairTaskId = repairTaskIds.Except(repairTasks.Select(task => task.Id)).First();
			_logger.LogWarning("Update repair tasks failed. RepairTask not found: {RepairTaskId}", missingRepairTaskId);
			return ApplicationErrors.RepairTask.NotFound(missingRepairTaskId);
		}

		var clearResult = workOrder.ClearRepairTasks();
		if (clearResult.IsError)
		{
			_logger.LogInformation("Update repair tasks failed while clearing existing tasks. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return clearResult.Errors;
		}

		foreach (var repairTask in repairTasks)
		{
			var addResult = workOrder.AddRepairTask(repairTask);
			if (addResult.IsError)
			{
				_logger.LogInformation("Update repair tasks failed while adding task. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}", request.WorkOrderId, repairTask.Id);
				return addResult.Errors;
			}
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderCacheTag, cancellationToken: cancellationToken);
		_logger.LogInformation("Workorder repair tasks updated successfully. WorkOrderId: {WorkOrderId}", request.WorkOrderId);

		return Result.Updated;
	}
}
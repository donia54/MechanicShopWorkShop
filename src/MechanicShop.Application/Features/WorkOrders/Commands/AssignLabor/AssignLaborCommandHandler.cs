using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;

public sealed class AssignLaborCommandHandler : IRequestHandler<AssignLaborCommand, Result<Updated>>
{
	private const string WorkOrderCacheTag = "workorder";

	private readonly IAppDbContext _dbContext;
	private readonly IWorkOrderPolicy _policy;
	private readonly ILogger<AssignLaborCommandHandler> _logger;
	private readonly HybridCache _cache;

	public AssignLaborCommandHandler(
		IAppDbContext dbContext,
		IWorkOrderPolicy policy,
		ILogger<AssignLaborCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_policy = policy;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Updated>> Handle(AssignLaborCommand request, CancellationToken cancellationToken)
	{
		var workOrder = await _dbContext.WorkOrders
			.Include(order => order.RepairTasks)
			.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);

		if (workOrder is null)
		{
			_logger.LogWarning("Assign labor failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}
         var labor = await _dbContext.Employees.FindAsync([request.LaborId], cancellationToken);

        if (labor is null)
        {
            _logger.LogError("Invalid LaborId: {LaborId}", request.LaborId);
            return ApplicationErrors.Labor.NotFound(request.LaborId);
        }


		var endAtUtc = CalculateEndAtUtc(workOrder); //FLage to Review:

		var schedulingValidationResult = await _policy.ValidateSchedulingAsync(
			request.LaborId,
			workOrder.VehicleId,
			workOrder.Spot,
			workOrder.StartAtUtc,
			endAtUtc,
			excludeWorkOrderId: workOrder.Id,
			ct: cancellationToken);

		if (schedulingValidationResult.IsError)
		{
			_logger.LogInformation(
				"Assign labor failed due to scheduling policy. WorkOrderId: {WorkOrderId}, LaborId: {LaborId}, StartAtUtc: {StartAtUtc}, EndAtUtc: {EndAtUtc}",
				request.WorkOrderId,
				request.LaborId,
				workOrder.StartAtUtc,
				endAtUtc);

			return schedulingValidationResult.Errors;
		}

		var updateResult = workOrder.UpdateLabor(request.LaborId);
		if (updateResult.IsError)
		{
			_logger.LogInformation("Assign labor business validation failed. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return updateResult.Errors;
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderCacheTag, cancellationToken: cancellationToken);

		_logger.LogInformation("Labor assigned successfully. WorkOrderId: {WorkOrderId}, LaborId: {LaborId}", request.WorkOrderId, request.LaborId);
		return Result.Updated;
	}

	private static DateTimeOffset CalculateEndAtUtc(Domain.WorkOrders.WorkOrder workOrder)
	{
		if (workOrder.EndAtUtc.HasValue)
		{
			return workOrder.EndAtUtc.Value;
		}

		var estimatedDurationMinutes = workOrder.RepairTasks.Sum(task => (int)task.EstimatedDurationInMins);
		if (estimatedDurationMinutes <= 0)
		{
			estimatedDurationMinutes = 15;
		}

		return workOrder.StartAtUtc.AddMinutes(estimatedDurationMinutes);
	}
}
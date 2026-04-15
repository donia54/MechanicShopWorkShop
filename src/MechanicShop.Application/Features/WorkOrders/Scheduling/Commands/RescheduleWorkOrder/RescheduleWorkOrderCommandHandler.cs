using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Commands.RescheduleWorkOrder;

public sealed class RescheduleWorkOrderCommandHandler : IRequestHandler<RescheduleWorkOrderCommand, Result<Updated>>
{
	private readonly IAppDbContext _dbContext;
	private readonly IWorkOrderPolicy _policy;
	private readonly ILogger<RescheduleWorkOrderCommandHandler> _logger;
	private readonly HybridCache _cache;

	public RescheduleWorkOrderCommandHandler(
		IAppDbContext dbContext,
		IWorkOrderPolicy policy,
		ILogger<RescheduleWorkOrderCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_policy = policy;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Updated>> Handle(RescheduleWorkOrderCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"Rescheduling workorder. WorkOrderId: {WorkOrderId}, StartAtUtc: {StartAtUtc}, EndAtUtc: {EndAtUtc}, Spot: {Spot}",
			request.WorkOrderId,
			request.StartAtUtc,
			request.EndAtUtc,
			request.Spot);

		var workOrder = await _dbContext.WorkOrders
			.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);

		if (workOrder is null)
		{
			_logger.LogWarning("Reschedule workorder failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		var scheduleValidation = await _policy.ValidateSchedulingAsync(
			workOrder.LaborId,
			workOrder.VehicleId,
			request.Spot,
			request.StartAtUtc,
			request.EndAtUtc,
			excludeWorkOrderId: workOrder.Id,
			ct: cancellationToken);

		if (scheduleValidation.IsError)
		{
			_logger.LogInformation("Reschedule workorder failed due to policy validation. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return scheduleValidation.Errors;
		}

		var timingResult = workOrder.UpdateTiming(request.StartAtUtc, request.EndAtUtc, markCollectionModified: false);
		if (timingResult.IsError)
		{
			_logger.LogInformation("Reschedule workorder failed during timing update. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return timingResult.Errors;
		}

		var spotResult = workOrder.UpdateSpot(request.Spot);
		if (spotResult.IsError)
		{
			_logger.LogInformation("Reschedule workorder failed during spot update. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return spotResult.Errors;
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderQueryCacheConstants.WorkOrderTag, cancellationToken: cancellationToken);

		_logger.LogInformation("Workorder rescheduled successfully. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
		return Result.Updated;
	}
}
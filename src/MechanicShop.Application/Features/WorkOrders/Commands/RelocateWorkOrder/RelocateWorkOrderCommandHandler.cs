using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;

public sealed class RelocateWorkOrderCommandHandler : IRequestHandler<RelocateWorkOrderCommand, Result<Updated>>
{
	private const string WorkOrderCacheTag = "workorder";

	private readonly IAppDbContext _dbContext;
	private readonly IWorkOrderPolicy _policy;
	private readonly ILogger<RelocateWorkOrderCommandHandler> _logger;
	private readonly HybridCache _cache;

	public RelocateWorkOrderCommandHandler(
		IAppDbContext dbContext,
		IWorkOrderPolicy policy,
		ILogger<RelocateWorkOrderCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_policy = policy;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Updated>> Handle(RelocateWorkOrderCommand request, CancellationToken cancellationToken)
	{
		var workOrder = await _dbContext.WorkOrders
			.Include(order => order.RepairTasks)
			.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);

		if (workOrder is null)
		{
			_logger.LogWarning("Relocate workorder failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		var duration = workOrder.GetDuration();
		var newStartAtUtc = request.NewStartAt;
		var newEndAtUtc = newStartAtUtc.Add(duration);

		var schedulingValidationResult = await _policy.ValidateSchedulingAsync(
			workOrder.LaborId,
			workOrder.VehicleId,
			request.NewSpot,
			newStartAtUtc,
			newEndAtUtc,
			excludeWorkOrderId: workOrder.Id,
			ct: cancellationToken);

		if (schedulingValidationResult.IsError)
		{
			LogSchedulingValidationFailure(request, workOrder, newStartAtUtc, newEndAtUtc, schedulingValidationResult.TopError.Code);
			return schedulingValidationResult.Errors;
		}

		var relocateResult = workOrder.Relocate(newStartAtUtc, newEndAtUtc, request.NewSpot);
		if (relocateResult.IsError)
		{
			_logger.LogInformation("Reschedule workorder failed during domain relocate. WorkOrderId: {WorkOrderId}, Spot: {Spot}", request.WorkOrderId, request.NewSpot);
			return relocateResult.Errors;
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderCacheTag, cancellationToken: cancellationToken);
		_logger.LogInformation(
			"Workorder rescheduled successfully. WorkOrderId: {WorkOrderId}, StartAtUtc: {StartAtUtc}, EndAtUtc: {EndAtUtc}, Spot: {Spot}",
			request.WorkOrderId,
			newStartAtUtc,
			newEndAtUtc,
			request.NewSpot);

		return Result.Updated;
	}

	private void LogSchedulingValidationFailure(
		RelocateWorkOrderCommand request,
		Domain.WorkOrders.WorkOrder workOrder,
		DateTimeOffset newStartAtUtc,
		DateTimeOffset newEndAtUtc,
		string errorCode)
	{
		if (errorCode.Contains("ServiceBayDoubleBooked", StringComparison.OrdinalIgnoreCase)
			|| errorCode.Contains("SpotDoubleBooked", StringComparison.OrdinalIgnoreCase))
		{
			_logger.LogInformation(
				"Reschedule workorder failed due to spot conflict. WorkOrderId: {WorkOrderId}, Spot: {Spot}, StartAtUtc: {StartAtUtc}, EndAtUtc: {EndAtUtc}",
				request.WorkOrderId,
				request.NewSpot,
				newStartAtUtc,
				newEndAtUtc);
			return;
		}

		if (errorCode.Contains("TechnicianDoubleBooked", StringComparison.OrdinalIgnoreCase)
			|| errorCode.Contains("Labor", StringComparison.OrdinalIgnoreCase))
		{
			_logger.LogInformation(
				"Reschedule workorder failed due to labor conflict. WorkOrderId: {WorkOrderId}, LaborId: {LaborId}, StartAtUtc: {StartAtUtc}, EndAtUtc: {EndAtUtc}",
				request.WorkOrderId,
				workOrder.LaborId,
				newStartAtUtc,
				newEndAtUtc);
			return;
		}

		if (errorCode.Contains("VehicleSchedulingConflict", StringComparison.OrdinalIgnoreCase)
			|| errorCode.Contains("Vehicle", StringComparison.OrdinalIgnoreCase))
		{
			_logger.LogInformation(
				"Reschedule workorder failed due to vehicle conflict. WorkOrderId: {WorkOrderId}, VehicleId: {VehicleId}, StartAtUtc: {StartAtUtc}, EndAtUtc: {EndAtUtc}",
				request.WorkOrderId,
				workOrder.VehicleId,
				newStartAtUtc,
				newEndAtUtc);
			return;
		}

		_logger.LogInformation(
			"Reschedule workorder failed due to scheduling validation. WorkOrderId: {WorkOrderId}, StartAtUtc: {StartAtUtc}, EndAtUtc: {EndAtUtc}, ErrorCode: {ErrorCode}",
			request.WorkOrderId,
			newStartAtUtc,
			newEndAtUtc,
			errorCode);
	}
}
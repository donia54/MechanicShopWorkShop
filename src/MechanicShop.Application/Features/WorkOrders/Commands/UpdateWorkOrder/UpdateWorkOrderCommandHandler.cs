using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrder;

public sealed class UpdateWorkOrderCommandHandler : IRequestHandler<UpdateWorkOrderCommand, Result<WorkOrderDto>>
{
	private const string WorkOrderCacheTag = "workorder";

	private readonly IAppDbContext _dbContext;
	private readonly IWorkOrderPolicy _workOrderPolicy;
	private readonly ILogger<UpdateWorkOrderCommandHandler> _logger;
	private readonly HybridCache _cache;

	public UpdateWorkOrderCommandHandler(
		IAppDbContext dbContext,
		IWorkOrderPolicy workOrderPolicy,
		ILogger<UpdateWorkOrderCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_workOrderPolicy = workOrderPolicy;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<WorkOrderDto>> Handle(UpdateWorkOrderCommand request, CancellationToken cancellationToken)
	{
		var workOrder = await _dbContext.WorkOrders
			.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);

		if (workOrder is null)
		{
			_logger.LogWarning("Update workorder failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		var minimumRequirementResult = _workOrderPolicy.ValidateMinimumRequirement(request.StartAtUtc, request.EndAtUtc);
		if (minimumRequirementResult.IsError)
		{
			_logger.LogInformation("Update workorder failed. Minimum requirement validation failed. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return minimumRequirementResult.Errors;
		}

		if (_workOrderPolicy.IsOutsideOperatingHours(request.StartAtUtc, request.EndAtUtc - request.StartAtUtc))
		{
			_logger.LogInformation("Update workorder failed. Outside operating hours. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.Scheduling.OutsideOperatingHours(request.StartAtUtc, request.EndAtUtc);
		}

		if (await _workOrderPolicy.IsLaborOccupied(workOrder.LaborId, workOrder.Id, request.StartAtUtc, request.EndAtUtc))
		{
			_logger.LogInformation("Update workorder failed. Labor is occupied. WorkOrderId: {WorkOrderId}, LaborId: {LaborId}", request.WorkOrderId, workOrder.LaborId);
			return ApplicationErrors.Scheduling.TechnicianDoubleBooked(workOrder.LaborId, request.StartAtUtc, request.EndAtUtc);
		}

		if (await _workOrderPolicy.IsVehicleAlreadyScheduled(workOrder.VehicleId, request.StartAtUtc, request.EndAtUtc, workOrder.Id))
		{
			_logger.LogInformation("Update workorder failed. Vehicle scheduling conflict. WorkOrderId: {WorkOrderId}, VehicleId: {VehicleId}", request.WorkOrderId, workOrder.VehicleId);
			return ApplicationErrors.Scheduling.VehicleSchedulingConflict(workOrder.VehicleId, request.StartAtUtc, request.EndAtUtc);
		}

		var spotAvailabilityResult = await _workOrderPolicy.CheckSpotAvailabilityAsync(
			request.Spot,
			request.StartAtUtc,
			request.EndAtUtc,
			excludeWorkOrderId: workOrder.Id,
			ct: cancellationToken);

		if (spotAvailabilityResult.IsError)
		{
			_logger.LogInformation("Update workorder failed. Spot is not available. WorkOrderId: {WorkOrderId}, Spot: {Spot}", request.WorkOrderId, request.Spot);
			return spotAvailabilityResult.Errors;
		}

		var timingResult = workOrder.UpdateTiming(request.StartAtUtc, request.EndAtUtc);
		if (timingResult.IsError)
		{
			_logger.LogInformation("Update workorder timing failed. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return timingResult.Errors;
		}

		var spotResult = workOrder.UpdateSpot(request.Spot);
		if (spotResult.IsError)
		{
			_logger.LogInformation("Update workorder spot failed. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return spotResult.Errors;
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderCacheTag, cancellationToken: cancellationToken);

		_logger.LogInformation("Workorder updated successfully. WorkOrderId: {WorkOrderId}", workOrder.Id);
		return workOrder.ToDto();
	}
}
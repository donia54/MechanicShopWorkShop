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
	private readonly IWorkOrderPolicy _workOrderPolicy;
	private readonly ILogger<RelocateWorkOrderCommandHandler> _logger;
	private readonly HybridCache _cache;

	public RelocateWorkOrderCommandHandler(
		IAppDbContext dbContext,
		IWorkOrderPolicy workOrderPolicy,
		ILogger<RelocateWorkOrderCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_workOrderPolicy = workOrderPolicy;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Updated>> Handle(RelocateWorkOrderCommand request, CancellationToken cancellationToken)
	{
		var workOrder = await _dbContext.WorkOrders.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);
		if (workOrder is null)
		{
			_logger.LogWarning("Relocate workorder failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		var endAtUtc = workOrder.EndAtUtc ?? workOrder.StartAtUtc;
		var spotAvailabilityResult = await _workOrderPolicy.CheckSpotAvailabilityAsync(
			request.NewSpot,
			workOrder.StartAtUtc,
			endAtUtc,
			excludeWorkOrderId: workOrder.Id,
			ct: cancellationToken);

		if (spotAvailabilityResult.IsError)
		{
			_logger.LogInformation("Relocate workorder failed. Spot is not available. WorkOrderId: {WorkOrderId}, Spot: {Spot}", request.WorkOrderId, request.NewSpot);
			return spotAvailabilityResult.Errors;
		}

		var relocateResult = workOrder.UpdateSpot(request.NewSpot);
		if (relocateResult.IsError)
		{
			_logger.LogInformation("Relocate workorder business validation failed. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return relocateResult.Errors;
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderCacheTag, cancellationToken: cancellationToken);
		_logger.LogInformation("Workorder relocated successfully. WorkOrderId: {WorkOrderId}, Spot: {Spot}", request.WorkOrderId, request.NewSpot);

		return Result.Updated;
	}
}
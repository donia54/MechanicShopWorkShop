using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.StartWorkOrder;

public sealed class StartWorkOrderCommandHandler : IRequestHandler<StartWorkOrderCommand, Result<Updated>>
{
	private const string WorkOrderCacheTag = "workorder";
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<StartWorkOrderCommandHandler> _logger;
	private readonly HybridCache _cache;

	public StartWorkOrderCommandHandler(
		IAppDbContext dbContext,
		ILogger<StartWorkOrderCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Updated>> Handle(StartWorkOrderCommand request, CancellationToken cancellationToken)
	{
		var workOrder = await _dbContext.WorkOrders.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);
		if (workOrder is null)
		{
			_logger.LogWarning("Start workorder failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		var transitionResult = workOrder.Start();
		if (transitionResult.IsError)
		{
			_logger.LogInformation("Start workorder business validation failed. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return transitionResult.Errors;
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderCacheTag, cancellationToken: cancellationToken);
		_logger.LogInformation("Workorder started successfully. WorkOrderId: {WorkOrderId}", request.WorkOrderId);

		return Result.Updated;
	}
}

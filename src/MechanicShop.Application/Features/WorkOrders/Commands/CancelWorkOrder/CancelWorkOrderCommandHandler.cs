using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CancelWorkOrder;

public sealed class CancelWorkOrderCommandHandler : IRequestHandler<CancelWorkOrderCommand, Result<Updated>>
{
	private const string WorkOrderCacheTag = "workorder";
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<CancelWorkOrderCommandHandler> _logger;
	private readonly HybridCache _cache;

	public CancelWorkOrderCommandHandler(
		IAppDbContext dbContext,
		ILogger<CancelWorkOrderCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Updated>> Handle(CancelWorkOrderCommand request, CancellationToken cancellationToken)
	{
		var workOrder = await _dbContext.WorkOrders.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);
		if (workOrder is null)
		{
			_logger.LogWarning("Cancel workorder failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		var cancelResult = workOrder.Cancel();
		if (cancelResult.IsError)
		{
			_logger.LogInformation("Cancel workorder business validation failed. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return cancelResult.Errors;
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderCacheTag, cancellationToken: cancellationToken);
		_logger.LogInformation("Workorder cancelled successfully. WorkOrderId: {WorkOrderId}", request.WorkOrderId);

		return Result.Updated;
	}
}
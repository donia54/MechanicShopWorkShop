using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Application.Features.WorkOrders.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Queries.GetWorkOrderRepairTasks;

public sealed class GetWorkOrderRepairTasksQueryHandler : IRequestHandler<GetWorkOrderRepairTasksQuery, Result<IReadOnlyList<RepairTaskDto>>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<GetWorkOrderRepairTasksQueryHandler> _logger;

	public GetWorkOrderRepairTasksQueryHandler(
		IAppDbContext dbContext,
		ILogger<GetWorkOrderRepairTasksQueryHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<Result<IReadOnlyList<RepairTaskDto>>> Handle(GetWorkOrderRepairTasksQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting workorder repair tasks. WorkOrderId: {WorkOrderId}", request.WorkOrderId);

		var workOrder = await _dbContext.WorkOrders
			.AsNoTracking()
			.Include(order => order.RepairTasks)
				.ThenInclude(task => task.Parts)
			.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);

		if (workOrder is null)
		{
			_logger.LogWarning("Get workorder repair tasks failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		var result = workOrder.RepairTasks
			.Select(task => task.ToRepairTaskDto())
			.ToList();

		_logger.LogInformation(
			"Workorder repair tasks retrieved successfully. WorkOrderId: {WorkOrderId}, Count: {Count}",
			request.WorkOrderId,
			result.Count);

		return result;
	}
}
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;

public sealed class GetWorkOrderByIdQueryHandler : IRequestHandler<GetWorkOrderByIdQuery, Result<WorkOrderDetailsDto>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<GetWorkOrderByIdQueryHandler> _logger;

	public GetWorkOrderByIdQueryHandler(
		IAppDbContext dbContext,
		ILogger<GetWorkOrderByIdQueryHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<Result<WorkOrderDetailsDto>> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting workorder details. WorkOrderId: {WorkOrderId}", request.WorkOrderId);

		var workOrder = await _dbContext.WorkOrders
			.AsNoTracking()
			.Include(order => order.Vehicle)
			.Include(order => order.Labor)
			.Include(order => order.RepairTasks)
				.ThenInclude(task => task.Parts)
			.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);

		if (workOrder is null)
		{
			_logger.LogWarning("Workorder details not found. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		var dto = workOrder.ToDetailsDto();

		_logger.LogInformation("Workorder details retrieved successfully. WorkOrderId: {WorkOrderId}", request.WorkOrderId);
		return dto;
	}
}
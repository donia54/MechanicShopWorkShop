using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrdersByLabor;

public sealed class GetWorkOrdersByLaborQueryHandler : IRequestHandler<GetWorkOrdersByLaborQuery, Result<IReadOnlyList<WorkOrderListItemDto>>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<GetWorkOrdersByLaborQueryHandler> _logger;

	public GetWorkOrdersByLaborQueryHandler(
		IAppDbContext dbContext,
		ILogger<GetWorkOrdersByLaborQueryHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<Result<IReadOnlyList<WorkOrderListItemDto>>> Handle(GetWorkOrdersByLaborQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting workorders by labor. LaborId: {LaborId}", request.LaborId);

		var workOrders = await _dbContext.WorkOrders
			.AsNoTracking()
			.Where(order => order.LaborId == request.LaborId)
			.OrderByDescending(order => order.StartAtUtc)
			.ToListAsync(cancellationToken);

		var result = workOrders
			.Select(order => order.ToListItemDto())
			.ToList();

		_logger.LogInformation(
			"Workorders by labor retrieved successfully. LaborId: {LaborId}, Count: {Count}",
			request.LaborId,
			result.Count);

		return result;
	}
}
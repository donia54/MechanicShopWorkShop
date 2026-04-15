using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrdersByDate;

public sealed class GetWorkOrdersByDateQueryHandler : IRequestHandler<GetWorkOrdersByDateQuery, Result<IReadOnlyList<WorkOrderListItemDto>>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<GetWorkOrdersByDateQueryHandler> _logger;

	public GetWorkOrdersByDateQueryHandler(
		IAppDbContext dbContext,
		ILogger<GetWorkOrdersByDateQueryHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<Result<IReadOnlyList<WorkOrderListItemDto>>> Handle(GetWorkOrdersByDateQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting workorders by date range. FromDate: {FromDate}, ToDate: {ToDate}", request.FromDate, request.ToDate);

		var workOrders = await _dbContext.WorkOrders
			.AsNoTracking()
			.Where(order => order.StartAtUtc >= request.FromDate && order.StartAtUtc <= request.ToDate)
			.OrderByDescending(order => order.StartAtUtc)
			.ToListAsync(cancellationToken);

		var result = workOrders
			.Select(order => order.ToListItemDto())
			.ToList();

		_logger.LogInformation(
			"Workorders by date range retrieved successfully. FromDate: {FromDate}, ToDate: {ToDate}, Count: {Count}",
			request.FromDate,
			request.ToDate,
			result.Count);

		return result;
	}
}
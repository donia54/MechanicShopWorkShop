using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrdersByVehicle;

public sealed class GetWorkOrdersByVehicleQueryHandler : IRequestHandler<GetWorkOrdersByVehicleQuery, Result<IReadOnlyList<WorkOrderListItemDto>>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<GetWorkOrdersByVehicleQueryHandler> _logger;

	public GetWorkOrdersByVehicleQueryHandler(
		IAppDbContext dbContext,
		ILogger<GetWorkOrdersByVehicleQueryHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<Result<IReadOnlyList<WorkOrderListItemDto>>> Handle(GetWorkOrdersByVehicleQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting workorders by vehicle. VehicleId: {VehicleId}", request.VehicleId);

		var workOrders = await _dbContext.WorkOrders
			.AsNoTracking()
			.Where(order => order.VehicleId == request.VehicleId)
			.OrderByDescending(order => order.StartAtUtc)
			.ToListAsync(cancellationToken);

		var result = workOrders
			.Select(order => order.ToListItemDto())
			.ToList();

		_logger.LogInformation(
			"Workorders by vehicle retrieved successfully. VehicleId: {VehicleId}, Count: {Count}",
			request.VehicleId,
			result.Count);

		return result;
	}
}
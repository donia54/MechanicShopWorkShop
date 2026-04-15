using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Application.Features.WorkOrders.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Queries.GetLaborSchedule;

public sealed class GetLaborScheduleQueryHandler : IRequestHandler<GetLaborScheduleQuery, Result<IReadOnlyList<ScheduleItemDto>>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<GetLaborScheduleQueryHandler> _logger;

	public GetLaborScheduleQueryHandler(
		IAppDbContext dbContext,
		ILogger<GetLaborScheduleQueryHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<Result<IReadOnlyList<ScheduleItemDto>>> Handle(GetLaborScheduleQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting labor schedule. LaborId: {LaborId}", request.LaborId);

		var workOrders = await _dbContext.WorkOrders
			.AsNoTracking()
			.Include(order => order.Labor)
			.Include(order => order.Vehicle)
			.Where(order => order.LaborId == request.LaborId && order.State != WorkOrderState.Cancelled)
			.OrderBy(order => order.StartAtUtc)
			.ToListAsync(cancellationToken);

		var result = workOrders
			.Select(order => order.ToScheduleItemDto())
			.ToList();

		_logger.LogInformation("Labor schedule retrieved successfully. LaborId: {LaborId}, Count: {Count}", request.LaborId, result.Count);
		return result;
	}
}
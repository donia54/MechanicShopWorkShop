using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Application.Features.WorkOrders.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Queries.GetDailySchedule;

public sealed class GetDailyScheduleQueryHandler : IRequestHandler<GetDailyScheduleQuery, Result<IReadOnlyList<ScheduleItemDto>>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<GetDailyScheduleQueryHandler> _logger;

	public GetDailyScheduleQueryHandler(
		IAppDbContext dbContext,
		ILogger<GetDailyScheduleQueryHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<Result<IReadOnlyList<ScheduleItemDto>>> Handle(GetDailyScheduleQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting daily schedule. Date: {Date}", request.Date);

		var startOfDayUtc = request.Date.ToUniversalTime().Date;
		var endOfDayUtc = startOfDayUtc.AddDays(1);

		var workOrders = await _dbContext.WorkOrders
			.AsNoTracking()
			.Include(order => order.Labor)
			.Include(order => order.Vehicle)
			.Where(order => order.StartAtUtc >= startOfDayUtc && order.StartAtUtc < endOfDayUtc)
			.OrderBy(order => order.StartAtUtc)
			.ToListAsync(cancellationToken);

		var result = workOrders
			.Select(order => order.ToScheduleItemDto())
			.ToList();

		_logger.LogInformation("Daily schedule retrieved successfully. Date: {Date}, Count: {Count}", request.Date, result.Count);
		return result;
	}
}
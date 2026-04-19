using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;

public sealed class GetWorkOrderStatsQueryHandler : IRequestHandler<GetWorkOrderStatsQuery, Result<WorkOrderStatsDto>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<GetWorkOrderStatsQueryHandler> _logger;

	public GetWorkOrderStatsQueryHandler(
		IAppDbContext dbContext,
		ILogger<GetWorkOrderStatsQueryHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<Result<WorkOrderStatsDto>> Handle(GetWorkOrderStatsQuery request, CancellationToken cancellationToken)
	{
		var startUtc = new DateTimeOffset(request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
		var endUtc = startUtc.AddDays(1);

		_logger.LogInformation(
			"Getting dashboard workorder statistics. Date: {Date}, StartUtc: {StartUtc}, EndUtc: {EndUtc}",
			request.Date,
			startUtc,
			endUtc);

		var statsProjection = await _dbContext.WorkOrders
			.AsNoTracking()
			.Where(workOrder => workOrder.StartAtUtc >= startUtc && workOrder.StartAtUtc < endUtc)
			.GroupBy(_ => 1)
			.Select(group => new
			{
				TotalWorkOrders = group.Count(),
				CompletedWorkOrders = group.Count(workOrder => workOrder.State == WorkOrderState.Completed),
				PendingWorkOrders = group.Count(workOrder => workOrder.State == WorkOrderState.Scheduled),
				InProgressWorkOrders = group.Count(workOrder => workOrder.State == WorkOrderState.InProgress),
				CancelledWorkOrders = group.Count(workOrder => workOrder.State == WorkOrderState.Cancelled),
				AverageCompletionHours = group
					.Where(workOrder => workOrder.State == WorkOrderState.Completed && workOrder.EndAtUtc.HasValue)
					.Select(workOrder => (double?)(workOrder.EndAtUtc!.Value - workOrder.StartAtUtc).TotalHours)
					.Average() ?? 0d,
				TotalRevenue = group
					.Select(workOrder => workOrder.Invoice != null ? (decimal?)workOrder.Invoice.Total : 0m)
					.Sum() ?? 0m,
				UniqueVehicles = group
					.Select(workOrder => workOrder.VehicleId)
					.Distinct()
					.Count(),
				UniqueCustomers = group
					.Where(workOrder => workOrder.Vehicle != null)
					.Select(workOrder => (Guid?)workOrder.Vehicle!.CustomerId)
					.Distinct()
					.Count(customerId => customerId.HasValue)
			})
			.FirstOrDefaultAsync(cancellationToken);

		if (statsProjection is null)
		{
			_logger.LogInformation(
				"Dashboard workorder statistics retrieved successfully with empty dataset. Date: {Date}, StartUtc: {StartUtc}, EndUtc: {EndUtc}",
				request.Date,
				startUtc,
				endUtc);
			return new WorkOrderStatsDto(0, 0, 0, 0, 0, 0d, 0m, 0, 0);
		}

		var result = new WorkOrderStatsDto(
			statsProjection.TotalWorkOrders,
			statsProjection.CompletedWorkOrders,
			statsProjection.PendingWorkOrders,
			statsProjection.InProgressWorkOrders,
			statsProjection.CancelledWorkOrders,
			statsProjection.AverageCompletionHours,
			statsProjection.TotalRevenue,
			statsProjection.UniqueVehicles,
			statsProjection.UniqueCustomers);

		_logger.LogInformation(
			"Dashboard workorder statistics retrieved successfully. Date: {Date}, Total: {Total}, Completed: {Completed}, Pending: {Pending}, InProgress: {InProgress}, Cancelled: {Cancelled}, AvgCompletionHours: {AverageCompletionHours}, TotalRevenue: {TotalRevenue}, UniqueVehicles: {UniqueVehicles}, UniqueCustomers: {UniqueCustomers}",
			request.Date,
			result.TotalWorkOrders,
			result.CompletedWorkOrders,
			result.PendingWorkOrders,
			result.InProgressWorkOrders,
			result.CancelledWorkOrders,
			result.AverageCompletionHours,
			result.TotalRevenue,
			result.UniqueVehicles,
			result.UniqueCustomers);

		return result;
	}
}
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries;
using MechanicShop.Application.Features.WorkOrders.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Queries.GetDailySchedule;

public sealed record GetDailyScheduleQuery(DateTimeOffset Date) : ICachedQuery<Result<IReadOnlyList<ScheduleItemDto>>>
{
	public string CacheKey => $"{WorkOrderQueryCacheConstants.GetDailyScheduleCacheKeyPrefix}:{Date.ToUniversalTime():yyyyMMdd}";

	public string[] Tags => [WorkOrderQueryCacheConstants.WorkOrderTag];

	public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}
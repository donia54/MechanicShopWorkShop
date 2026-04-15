using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries;
using MechanicShop.Application.Features.WorkOrders.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Queries.GetLaborSchedule;

public sealed record GetLaborScheduleQuery(Guid LaborId) : ICachedQuery<Result<IReadOnlyList<ScheduleItemDto>>>
{
	public string CacheKey => $"{WorkOrderQueryCacheConstants.GetLaborScheduleCacheKeyPrefix}:{LaborId}";

	public string[] Tags => [WorkOrderQueryCacheConstants.WorkOrderTag];

	public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}
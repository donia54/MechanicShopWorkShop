using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries;
using MechanicShop.Application.Features.WorkOrders.Labor.Dtos;

namespace MechanicShop.Application.Features.WorkOrders.Labor.Queries.GetAvailableLabors;

public sealed record GetAvailableLaborsQuery(
	DateTimeOffset StartAt,
	DateTimeOffset EndAt) : ICachedQuery<List<LaborDto>>
{
	public string CacheKey =>
		$"{WorkOrderQueryCacheConstants.GetAvailableLaborsCacheKeyPrefix}:{StartAt.ToUniversalTime():O}:{EndAt.ToUniversalTime():O}";

	public string[] Tags => [WorkOrderQueryCacheConstants.WorkOrderTag];

	public TimeSpan Expiration => TimeSpan.FromMinutes(5);
};
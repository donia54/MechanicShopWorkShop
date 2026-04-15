using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries;
using MechanicShop.Application.Features.WorkOrders.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Queries.GetAvailableSlots;

public sealed record GetAvailableSlotsQuery(
	DateTimeOffset Date,
	Guid? LaborId = null) : ICachedQuery<Result<IReadOnlyList<AvailabilitySlotDto>>>
{
	public string CacheKey => $"{WorkOrderQueryCacheConstants.GetAvailableSlotsCacheKeyPrefix}:{Date.ToUniversalTime():yyyyMMdd}:{LaborId}";

	public string[] Tags => [WorkOrderQueryCacheConstants.WorkOrderTag];

	public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}
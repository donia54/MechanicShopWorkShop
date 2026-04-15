using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;

public sealed record GetWorkOrdersQuery(
	int PageNumber = 1,
	int PageSize = 20,
	WorkOrderState? State = null,
	DateTimeOffset? FromDate = null,
	DateTimeOffset? ToDate = null)
	: ICachedQuery<Result<IReadOnlyList<WorkOrderListItemDto>>>
{
	public string CacheKey
	{
		get
		{
			var stateValue = State?.ToString() ?? "all";
			var fromDateValue = FromDate?.ToUniversalTime().ToString("O") ?? "none";
			var toDateValue = ToDate?.ToUniversalTime().ToString("O") ?? "none";

			return $"{WorkOrderQueryCacheConstants.GetWorkOrdersCacheKeyPrefix}:{PageNumber}:{PageSize}:{stateValue}:{fromDateValue}:{toDateValue}";
		}
	}

	public string[] Tags => [WorkOrderQueryCacheConstants.WorkOrderTag];

	public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
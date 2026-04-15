using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries;
using MechanicShop.Application.Features.WorkOrders.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Queries.GetWorkOrderRepairTasks;

public sealed record GetWorkOrderRepairTasksQuery(Guid WorkOrderId)
	: ICachedQuery<Result<IReadOnlyList<RepairTaskDto>>>
{
	public string CacheKey => $"{WorkOrderQueryCacheConstants.GetWorkOrderRepairTasksCacheKeyPrefix}:{WorkOrderId}";

	public string[] Tags => [WorkOrderQueryCacheConstants.WorkOrderTag];

	public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
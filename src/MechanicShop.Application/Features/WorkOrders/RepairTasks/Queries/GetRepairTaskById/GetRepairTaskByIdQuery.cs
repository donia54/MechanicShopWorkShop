using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries;
using MechanicShop.Application.Features.WorkOrders.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Queries.GetRepairTaskById;

public sealed record GetRepairTaskByIdQuery(Guid RepairTaskId)
	: ICachedQuery<Result<RepairTaskDto>>
{
	public string CacheKey => $"{WorkOrderQueryCacheConstants.GetRepairTaskByIdCacheKeyPrefix}:{RepairTaskId}";

	public string[] Tags => [WorkOrderQueryCacheConstants.WorkOrderTag];

	public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
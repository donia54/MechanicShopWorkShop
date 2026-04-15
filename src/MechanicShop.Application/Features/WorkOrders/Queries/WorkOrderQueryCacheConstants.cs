namespace MechanicShop.Application.Features.WorkOrders.Queries;

internal static class WorkOrderQueryCacheConstants
{
	public const string WorkOrderTag = "workorder";
	public const string GetWorkOrdersCacheKeyPrefix = "workorders:list";
	public const string GetWorkOrderRepairTasksCacheKeyPrefix = "workorders:repairtasks";
	public const string GetRepairTaskByIdCacheKeyPrefix = "repairtasks:byid";
	public const string GetAvailableSlotsCacheKeyPrefix = "workorders:scheduling:slots";
	public const string GetDailyScheduleCacheKeyPrefix = "workorders:scheduling:daily";
	public const string GetLaborScheduleCacheKeyPrefix = "workorders:scheduling:labor";
	public const string GetAvailableLaborsCacheKeyPrefix = "workorders:labor:available";
}
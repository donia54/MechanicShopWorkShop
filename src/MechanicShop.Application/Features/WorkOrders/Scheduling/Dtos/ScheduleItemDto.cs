namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Dtos;

public sealed record ScheduleItemDto(
	Guid WorkOrderId,
	DateTimeOffset StartAt,
	DateTimeOffset? EndAt,
	string LaborName,
	string VehicleInfo,
	string Status);
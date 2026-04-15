namespace MechanicShop.Application.Features.WorkOrders.Dtos;

public sealed record WorkOrderDetailsDto(
	Guid WorkOrderId,
	Guid VehicleId,
	string VehicleMake,
	string VehicleModel,
	int VehicleYear,
	string VehicleLicensePlate,
	Guid LaborId,
	string LaborName,
	string Spot,
	string State,
	DateTimeOffset StartAtUtc,
	DateTimeOffset? EndAtUtc,
	decimal TotalPartsCost,
	decimal TotalLaborCost,
	decimal Total,
	IReadOnlyList<WorkOrderRepairTaskDto> RepairTasks);

public sealed record WorkOrderRepairTaskDto(
	Guid RepairTaskId,
	string Name,
	decimal LaborCost,
	int EstimatedDurationInMinutes,
	decimal TotalCost,
	IReadOnlyList<WorkOrderPartDto> Parts);

public sealed record WorkOrderPartDto(
	Guid PartId,
	string Name,
	decimal Cost,
	int Quantity,
	decimal TotalCost);
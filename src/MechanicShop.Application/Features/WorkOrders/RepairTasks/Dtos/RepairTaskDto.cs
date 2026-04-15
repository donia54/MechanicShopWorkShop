namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Dtos;

public sealed record RepairTaskDto(
	Guid Id,
	string Name,
	decimal LaborCost,
	int EstimatedDuration,
	IReadOnlyList<PartDto> Parts);
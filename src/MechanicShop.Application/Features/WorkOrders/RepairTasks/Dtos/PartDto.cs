namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Dtos;

public sealed record PartDto(
	Guid Id,
	string Name,
	decimal Cost,
	int Quantity);
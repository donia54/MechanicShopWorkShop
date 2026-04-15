namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Dtos;

public sealed record AvailabilitySlotDto(
	DateTimeOffset StartAt,
	DateTimeOffset EndAt,
	bool IsAvailable,
	string Spot);
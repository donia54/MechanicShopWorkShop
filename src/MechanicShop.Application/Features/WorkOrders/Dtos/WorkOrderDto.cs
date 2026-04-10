namespace MechanicShop.Application.Features.WorkOrders.Dtos;

public sealed record WorkOrderDto(
	Guid WorkOrderId,
	Guid VehicleId,
	Guid LaborId,
	string Spot,
	string State,
	DateTimeOffset StartAtUtc,
	DateTimeOffset? EndAtUtc,
	decimal Total);
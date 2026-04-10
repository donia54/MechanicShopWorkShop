using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed record CreateWorkOrderCommand(
	Guid VehicleId,
	Guid LaborId,
	DateTimeOffset StartAtUtc,
	DateTimeOffset EndAtUtc,
	Spot Spot,
	IReadOnlyList<Guid> RepairTaskIds) : IRequest<Result<WorkOrderDto>>;
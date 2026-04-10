using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrder;

public sealed record UpdateWorkOrderCommand(
	Guid WorkOrderId,
	DateTimeOffset StartAtUtc,
	DateTimeOffset EndAtUtc,
	Spot Spot) : IRequest<Result<WorkOrderDto>>;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Commands.RescheduleWorkOrder;

public sealed record RescheduleWorkOrderCommand(
	Guid WorkOrderId,
	DateTimeOffset StartAtUtc,
	DateTimeOffset EndAtUtc,
	Spot Spot) : IRequest<Result<Updated>>;
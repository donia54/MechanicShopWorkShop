using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Commands.ScheduleWorkOrder;

public sealed record ScheduleWorkOrderCommand(
	Guid WorkOrderId,
	DateTimeOffset StartAtUtc,
	DateTimeOffset EndAtUtc,
	Spot Spot) : IRequest<Result<Updated>>;
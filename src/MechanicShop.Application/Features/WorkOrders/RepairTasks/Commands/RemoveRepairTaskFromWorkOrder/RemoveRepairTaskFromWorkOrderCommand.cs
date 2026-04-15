using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Commands.RemoveRepairTaskFromWorkOrder;

public sealed record RemoveRepairTaskFromWorkOrderCommand(
	Guid WorkOrderId,
	Guid RepairTaskId) : IRequest<Result<Updated>>;
using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Commands.AddRepairTaskToWorkOrder;

public sealed record AddRepairTaskToWorkOrderCommand(
	Guid WorkOrderId,
	Guid RepairTaskId) : IRequest<Result<Updated>>;
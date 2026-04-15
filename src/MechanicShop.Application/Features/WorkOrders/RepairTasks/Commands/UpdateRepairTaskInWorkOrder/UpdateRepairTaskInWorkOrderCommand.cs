using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Commands.UpdateRepairTaskInWorkOrder;

public sealed record UpdateRepairTaskInWorkOrderCommand(
	Guid WorkOrderId,
	Guid RepairTaskId,
	string Name,
	decimal LaborCost,
	int EstimatedDuration,
	IReadOnlyList<UpdateRepairTaskPartInput> Parts) : IRequest<Result<Updated>>;

public sealed record UpdateRepairTaskPartInput(
	Guid PartId,
	string Name,
	decimal Cost,
	int Quantity);
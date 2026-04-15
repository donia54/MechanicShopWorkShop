using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Commands.RemoveRepairTaskFromWorkOrder;

public sealed class RemoveRepairTaskFromWorkOrderCommandValidator : AbstractValidator<RemoveRepairTaskFromWorkOrderCommand>
{
	public RemoveRepairTaskFromWorkOrderCommandValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
		RuleFor(x => x.RepairTaskId).NotEmpty();
	}
}
using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Commands.AddRepairTaskToWorkOrder;

public sealed class AddRepairTaskToWorkOrderCommandValidator : AbstractValidator<AddRepairTaskToWorkOrderCommand>
{
	public AddRepairTaskToWorkOrderCommandValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
		RuleFor(x => x.RepairTaskId).NotEmpty();
	}
}
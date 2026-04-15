using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Commands.UpdateRepairTaskInWorkOrder;

public sealed class UpdateRepairTaskInWorkOrderCommandValidator : AbstractValidator<UpdateRepairTaskInWorkOrderCommand>
{
	public UpdateRepairTaskInWorkOrderCommandValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
		RuleFor(x => x.RepairTaskId).NotEmpty();
		RuleFor(x => x.Name).NotEmpty();
		RuleFor(x => x.LaborCost).GreaterThan(0);
		RuleFor(x => x.EstimatedDuration).GreaterThan(0);
		RuleFor(x => x.Parts).NotNull();
		RuleForEach(x => x.Parts).SetValidator(new UpdateRepairTaskPartInputValidator());
	}
}

public sealed class UpdateRepairTaskPartInputValidator : AbstractValidator<UpdateRepairTaskPartInput>
{
	public UpdateRepairTaskPartInputValidator()
	{
		RuleFor(x => x.PartId).NotEmpty();
		RuleFor(x => x.Name).NotEmpty();
		RuleFor(x => x.Cost).GreaterThan(0);
		RuleFor(x => x.Quantity).GreaterThan(0);
	}
}
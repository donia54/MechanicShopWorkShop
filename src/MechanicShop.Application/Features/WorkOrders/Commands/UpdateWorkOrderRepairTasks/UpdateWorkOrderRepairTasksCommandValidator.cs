using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public sealed class UpdateWorkOrderRepairTasksCommandValidator : AbstractValidator<UpdateWorkOrderRepairTasksCommand>
{
	public UpdateWorkOrderRepairTasksCommandValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
		RuleFor(x => x.RepairTaskIds)
			.NotNull()
			.NotEmpty();
		RuleForEach(x => x.RepairTaskIds).NotEmpty();
	}
}

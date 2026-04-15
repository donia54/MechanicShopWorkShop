using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Queries.GetRepairTaskById;

public sealed class GetRepairTaskByIdQueryValidator : AbstractValidator<GetRepairTaskByIdQuery>
{
	public GetRepairTaskByIdQueryValidator()
	{
		RuleFor(x => x.RepairTaskId).NotEmpty();
	}
}
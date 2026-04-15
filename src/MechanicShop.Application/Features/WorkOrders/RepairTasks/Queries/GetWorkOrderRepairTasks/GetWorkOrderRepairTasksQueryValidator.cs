using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Queries.GetWorkOrderRepairTasks;

public sealed class GetWorkOrderRepairTasksQueryValidator : AbstractValidator<GetWorkOrderRepairTasksQuery>
{
	public GetWorkOrderRepairTasksQueryValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
	}
}
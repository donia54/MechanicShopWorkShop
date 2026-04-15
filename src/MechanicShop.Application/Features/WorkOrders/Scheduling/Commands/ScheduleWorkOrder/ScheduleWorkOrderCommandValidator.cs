using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Commands.ScheduleWorkOrder;

public sealed class ScheduleWorkOrderCommandValidator : AbstractValidator<ScheduleWorkOrderCommand>
{
	public ScheduleWorkOrderCommandValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
		RuleFor(x => x.StartAtUtc).LessThan(x => x.EndAtUtc);
		RuleFor(x => x.Spot).IsInEnum();
	}
}
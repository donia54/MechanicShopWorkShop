using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Commands.RescheduleWorkOrder;

public sealed class RescheduleWorkOrderCommandValidator : AbstractValidator<RescheduleWorkOrderCommand>
{
	public RescheduleWorkOrderCommandValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
		RuleFor(x => x.StartAtUtc).LessThan(x => x.EndAtUtc);
		RuleFor(x => x.Spot).IsInEnum();
	}
}
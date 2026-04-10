using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;

public sealed class RelocateWorkOrderCommandValidator : AbstractValidator<RelocateWorkOrderCommand>
{
	public RelocateWorkOrderCommandValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
		RuleFor(x => x.NewSpot).IsInEnum();
	}
}
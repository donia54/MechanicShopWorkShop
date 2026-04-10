using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CompleteWorkOrder;

public sealed class CompleteWorkOrderCommandValidator : AbstractValidator<CompleteWorkOrderCommand>
{
	public CompleteWorkOrderCommandValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
	}
}

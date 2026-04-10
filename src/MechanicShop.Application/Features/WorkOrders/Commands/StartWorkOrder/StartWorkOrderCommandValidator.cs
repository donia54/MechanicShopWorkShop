using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.StartWorkOrder;

public sealed class StartWorkOrderCommandValidator : AbstractValidator<StartWorkOrderCommand>
{
	public StartWorkOrderCommandValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
	}
}

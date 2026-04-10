using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrder;

public sealed class UpdateWorkOrderCommandValidator : AbstractValidator<UpdateWorkOrderCommand>
{
	public UpdateWorkOrderCommandValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
		RuleFor(x => x.StartAtUtc)
			.LessThan(x => x.EndAtUtc)
			.WithMessage("StartAtUtc must be earlier than EndAtUtc.");
		RuleFor(x => x.Spot).IsInEnum();
	}
}
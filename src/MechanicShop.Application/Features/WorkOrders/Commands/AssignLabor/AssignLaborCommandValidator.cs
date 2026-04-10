using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;

public sealed class AssignLaborCommandValidator : AbstractValidator<AssignLaborCommand>
{
	public AssignLaborCommandValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
		RuleFor(x => x.LaborId).NotEmpty();
	}
}
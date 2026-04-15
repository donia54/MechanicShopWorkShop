using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;

public sealed class GetWorkOrderByIdQueryValidator : AbstractValidator<GetWorkOrderByIdQuery>
{
	public GetWorkOrderByIdQueryValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
	}
}
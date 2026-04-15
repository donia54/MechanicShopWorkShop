using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrdersByLabor;

public sealed class GetWorkOrdersByLaborQueryValidator : AbstractValidator<GetWorkOrdersByLaborQuery>
{
	public GetWorkOrdersByLaborQueryValidator()
	{
		RuleFor(x => x.LaborId).NotEmpty();
	}
}
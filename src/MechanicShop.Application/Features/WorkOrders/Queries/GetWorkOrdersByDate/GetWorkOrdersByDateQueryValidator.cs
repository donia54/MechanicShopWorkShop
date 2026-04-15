using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrdersByDate;

public sealed class GetWorkOrdersByDateQueryValidator : AbstractValidator<GetWorkOrdersByDateQuery>
{
	public GetWorkOrdersByDateQueryValidator()
	{
		RuleFor(x => x.FromDate)
			.LessThan(x => x.ToDate)
			.WithMessage("FromDate must be earlier than ToDate.");
	}
}
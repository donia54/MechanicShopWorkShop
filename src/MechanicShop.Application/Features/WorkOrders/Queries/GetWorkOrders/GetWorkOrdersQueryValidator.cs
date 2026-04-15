using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;

public sealed class GetWorkOrdersQueryValidator : AbstractValidator<GetWorkOrdersQuery>
{
	public GetWorkOrdersQueryValidator()
	{
		RuleFor(x => x.PageNumber)
			.GreaterThan(0)
			.WithMessage("PageNumber must be greater than 0.");

		RuleFor(x => x.PageSize)
			.InclusiveBetween(1, 200)
			.WithMessage("PageSize must be between 1 and 200.");

		RuleFor(x => x.State)
			.IsInEnum()
			.When(x => x.State.HasValue);

		RuleFor(x => x.FromDate)
			.LessThan(x => x.ToDate)
			.When(x => x.FromDate.HasValue && x.ToDate.HasValue)
			.WithMessage("FromDate must be earlier than ToDate.");
	}
}
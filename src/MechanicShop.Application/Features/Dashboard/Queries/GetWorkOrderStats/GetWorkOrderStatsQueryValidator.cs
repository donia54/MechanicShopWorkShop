using FluentValidation;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;

public sealed class GetWorkOrderStatsQueryValidator : AbstractValidator<GetWorkOrderStatsQuery>
{
	public GetWorkOrderStatsQueryValidator()
	{
		RuleFor(x => x.Date)
			.NotEqual(default(DateOnly))
			.WithMessage("Date is required.");
	}
}
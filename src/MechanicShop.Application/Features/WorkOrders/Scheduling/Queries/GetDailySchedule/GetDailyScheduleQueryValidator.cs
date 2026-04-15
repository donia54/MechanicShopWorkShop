using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Queries.GetDailySchedule;

public sealed class GetDailyScheduleQueryValidator : AbstractValidator<GetDailyScheduleQuery>
{
	public GetDailyScheduleQueryValidator()
	{
		RuleFor(x => x.Date).NotEmpty();
	}
}
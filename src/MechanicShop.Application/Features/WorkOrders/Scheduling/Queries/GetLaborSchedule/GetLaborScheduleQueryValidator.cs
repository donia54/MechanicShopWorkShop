using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Queries.GetLaborSchedule;

public sealed class GetLaborScheduleQueryValidator : AbstractValidator<GetLaborScheduleQuery>
{
	public GetLaborScheduleQueryValidator()
	{
		RuleFor(x => x.LaborId).NotEmpty();
	}
}
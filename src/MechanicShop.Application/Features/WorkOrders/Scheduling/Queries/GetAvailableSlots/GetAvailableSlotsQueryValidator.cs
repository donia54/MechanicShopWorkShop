using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Queries.GetAvailableSlots;

public sealed class GetAvailableSlotsQueryValidator : AbstractValidator<GetAvailableSlotsQuery>
{
	public GetAvailableSlotsQueryValidator()
	{
		RuleFor(x => x.Date).NotEmpty();
		RuleFor(x => x.LaborId)
			.NotEmpty()
			.When(x => x.LaborId.HasValue);
	}
}
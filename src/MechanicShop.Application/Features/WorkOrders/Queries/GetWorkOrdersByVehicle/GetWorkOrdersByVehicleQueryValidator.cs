using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrdersByVehicle;

public sealed class GetWorkOrdersByVehicleQueryValidator : AbstractValidator<GetWorkOrdersByVehicleQuery>
{
	public GetWorkOrdersByVehicleQueryValidator()
	{
		RuleFor(x => x.VehicleId).NotEmpty();
	}
}
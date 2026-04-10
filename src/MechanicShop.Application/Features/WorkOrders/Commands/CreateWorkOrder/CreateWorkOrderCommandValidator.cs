using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
	public CreateWorkOrderCommandValidator()
	{
		RuleFor(x => x.VehicleId).NotEmpty();
		RuleFor(x => x.LaborId).NotEmpty();

		RuleFor(x => x.StartAtUtc)
			.LessThan(x => x.EndAtUtc)
			.WithMessage("StartAtUtc must be earlier than EndAtUtc.");

		RuleFor(x => x.Spot)
			.IsInEnum();

		RuleFor(x => x.RepairTaskIds)
			.NotNull()
			.NotEmpty();

		RuleForEach(x => x.RepairTaskIds)
			.NotEmpty();
	}
}
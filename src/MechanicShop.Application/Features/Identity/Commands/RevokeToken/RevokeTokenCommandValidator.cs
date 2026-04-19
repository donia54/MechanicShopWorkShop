using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Commands.RevokeToken;

public sealed class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
	public RevokeTokenCommandValidator()
	{
		RuleFor(x => x.RefreshToken).NotEmpty();
	}
}
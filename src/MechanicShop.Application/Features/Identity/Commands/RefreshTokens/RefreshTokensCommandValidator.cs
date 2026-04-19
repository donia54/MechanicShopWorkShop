using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Commands.RefreshTokens;

public sealed class RefreshTokensCommandValidator : AbstractValidator<RefreshTokensCommand>
{
	public RefreshTokensCommandValidator()
	{
		RuleFor(x => x.RefreshToken).NotEmpty();
	}
}
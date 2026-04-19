using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Commands.GenerateTokens;

public sealed class GenerateTokensCommandValidator : AbstractValidator<GenerateTokensCommand>
{
	public GenerateTokensCommandValidator()
	{
		RuleFor(x => x.Email).NotEmpty().EmailAddress();
		RuleFor(x => x.Password).NotEmpty();
	}
}
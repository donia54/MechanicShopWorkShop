using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Commands.GenerateTokens;

public sealed class GenerateTokensCommandHandler : IRequestHandler<GenerateTokensCommand, Result<TokenResponse>>
{
	private readonly IIdentityService _identityService;
	private readonly ITokenProvider _tokenProvider;
	private readonly ILogger<GenerateTokensCommandHandler> _logger;

	public GenerateTokensCommandHandler(
		IIdentityService identityService,
		ITokenProvider tokenProvider,
		ILogger<GenerateTokensCommandHandler> logger)
	{
		_identityService = identityService;
		_tokenProvider = tokenProvider;
		_logger = logger;
	}

	public async Task<Result<TokenResponse>> Handle(GenerateTokensCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Generate tokens started. Email: {Email}", request.Email);

		var userResult = await ValidateCredentialsAsync(request.Email, request.Password);
		if (userResult.IsError)
		{
			_logger.LogInformation("Generate tokens failed. Invalid credentials. Email: {Email}", request.Email);
			return userResult.Errors;
		}

		var tokenResponseResult = await GenerateTokenResponseAsync(userResult.Value, cancellationToken);
		if (tokenResponseResult.IsError)
		{
			_logger.LogWarning("Generate tokens failed. Token generation failed for user: {UserId}", userResult.Value.UserId);
			return tokenResponseResult.Errors;
		}

		_logger.LogInformation("Generate tokens succeeded. UserId: {UserId}", userResult.Value.UserId);
		return tokenResponseResult.Value;
	}

	private Task<Result<AppUserDto>> ValidateCredentialsAsync(string email, string password)
	{
		return _identityService.AuthenticateAsync(email, password);
	}

	private async Task<Result<TokenResponse>> GenerateTokenResponseAsync(AppUserDto user, CancellationToken cancellationToken)
	{
		var accessTokenResult = await _tokenProvider.GenerateAccessTokenAsync(user, cancellationToken);
		if (accessTokenResult.IsError)
		{
			return accessTokenResult.Errors;
		}

		var refreshTokenResult = await _tokenProvider.GenerateRefreshTokenAsync(user, cancellationToken);
		if (refreshTokenResult.IsError)
		{
			return refreshTokenResult.Errors;
		}

		return new TokenResponse(
			accessTokenResult.Value.Token,
			refreshTokenResult.Value.Token,
			accessTokenResult.Value.ExpiresAtUtc);
	}
}
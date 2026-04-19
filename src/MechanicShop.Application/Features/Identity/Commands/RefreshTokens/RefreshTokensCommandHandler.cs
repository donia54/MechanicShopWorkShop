using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Commands.RefreshTokens;

public sealed class RefreshTokensCommandHandler : IRequestHandler<RefreshTokensCommand, Result<TokenResponse>>
{
	private readonly ITokenProvider _tokenProvider;
	private readonly ILogger<RefreshTokensCommandHandler> _logger;

	public RefreshTokensCommandHandler(
		ITokenProvider tokenProvider,
		ILogger<RefreshTokensCommandHandler> logger)
	{
		_tokenProvider = tokenProvider;
		_logger = logger;
	}

	public async Task<Result<TokenResponse>> Handle(RefreshTokensCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Refresh tokens started.");

		var userResult = await ValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);
		if (userResult.IsError)
		{
			_logger.LogInformation("Refresh tokens failed. Invalid refresh token.");
			return userResult.Errors;
		}

		var tokenResponseResult = await GenerateTokenResponseAsync(userResult.Value, cancellationToken);
		if (tokenResponseResult.IsError)
		{
			_logger.LogWarning("Refresh tokens failed. Token generation failed for user: {UserId}", userResult.Value.UserId);
			return tokenResponseResult.Errors;
		}

		_logger.LogInformation("Refresh tokens succeeded. UserId: {UserId}", userResult.Value.UserId);
		return tokenResponseResult.Value;
	}

	private async Task<Result<AppUserDto>> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
	{
		var validationResult = await _tokenProvider.ValidateRefreshTokenAsync(refreshToken, cancellationToken);
		if (validationResult.IsError)
		{
			return ApplicationErrors.Auth.InvalidRefreshToken;
		}

		return validationResult.Value;
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
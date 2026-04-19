using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Commands.RevokeToken;

public sealed class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result<Updated>>
{
	private readonly ITokenProvider _tokenProvider;
	private readonly ILogger<RevokeTokenCommandHandler> _logger;

	public RevokeTokenCommandHandler(
		ITokenProvider tokenProvider,
		ILogger<RevokeTokenCommandHandler> logger)
	{
		_tokenProvider = tokenProvider;
		_logger = logger;
	}

	public async Task<Result<Updated>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Revoke refresh token started.");

		var revokeResult = await _tokenProvider.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
		if (revokeResult.IsError)
		{
			_logger.LogInformation("Revoke refresh token failed. Invalid token.");
			return ApplicationErrors.Auth.InvalidRefreshToken;
		}

		_logger.LogInformation("Revoke refresh token succeeded.");
		return Result.Updated;
	}
}
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Interfaces;

public sealed record AccessTokenDescriptor(string Token, DateTimeOffset ExpiresAtUtc);

public sealed record RefreshTokenDescriptor(string Token);

public interface ITokenProvider
{
	Task<Result<AccessTokenDescriptor>> GenerateAccessTokenAsync(AppUserDto user, CancellationToken cancellationToken);

	Task<Result<RefreshTokenDescriptor>> GenerateRefreshTokenAsync(AppUserDto user, CancellationToken cancellationToken);

	Task<Result<AppUserDto>> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

	Task<Result<Updated>> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
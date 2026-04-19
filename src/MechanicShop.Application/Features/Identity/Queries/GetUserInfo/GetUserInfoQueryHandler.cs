using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfo;

public sealed class GetUserInfoQueryHandler : IRequestHandler<GetUserInfoQuery, Result<UserDto>>
{
	private readonly IUser _currentUser;
	private readonly IIdentityService _identityService;
	private readonly ILogger<GetUserInfoQueryHandler> _logger;

	public GetUserInfoQueryHandler(
		IUser currentUser,
		IIdentityService identityService,
		ILogger<GetUserInfoQueryHandler> logger)
	{
		_currentUser = currentUser;
		_identityService = identityService;
		_logger = logger;
	}

	public async Task<Result<UserDto>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Get user info started.");

		var userIdResult = GetCurrentUserId();
		if (userIdResult.IsError)
		{
			_logger.LogInformation("Get user info failed. Missing current user id claim.");
			return userIdResult.Errors;
		}

		var appUserResult = await _identityService.GetUserByIdAsync(userIdResult.Value);
		if (appUserResult.IsError)
		{
			_logger.LogInformation("Get user info failed. User not found. UserId: {UserId}", userIdResult.Value);
			return appUserResult.Errors;
		}

		var userDto = MapToUserDto(appUserResult.Value);

		_logger.LogInformation("Get user info succeeded. UserId: {UserId}", userDto.Id);
		return userDto;
	}

	private Result<string> GetCurrentUserId()
	{
		if (string.IsNullOrWhiteSpace(_currentUser.Id))
		{
			return ApplicationErrors.Auth.InvalidUserIdClaim(_currentUser.Id);
		}

		return _currentUser.Id;
	}

	private static UserDto MapToUserDto(AppUserDto appUser)
	{
		var role = appUser.Roles.FirstOrDefault() ?? string.Empty;
		return new UserDto(appUser.UserId, appUser.Email, role);
	}
}
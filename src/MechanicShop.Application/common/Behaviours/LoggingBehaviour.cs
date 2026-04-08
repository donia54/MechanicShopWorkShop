using MechanicShop.Application.Common.Interfaces;

using MediatR.Pipeline;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviours;

public sealed class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
	where TRequest : notnull
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
	private readonly ILogger<LoggingBehaviour<TRequest>> _logger;

	public LoggingBehaviour(IUser user, IIdentityService identityService, ILogger<LoggingBehaviour<TRequest>> logger)
	{
		_user = user;
        _logger = logger;
        _identityService = identityService;
	}

	public Task Process(TRequest request, CancellationToken cancellationToken)
    {
          string? userName = string.Empty;

        if (!string.IsNullOrEmpty(_user.Id))
        {
            userName =  _identityService.GetUserNameAsync(_user.Id).GetAwaiter().GetResult() ?? "Unknown";
        }
		_logger.LogInformation(
			"Handling request {RequestName} for UserId: {UserId},{@UserName}",
			typeof(TRequest).Name,
			_user.Id ?? "Anonymous",
			userName);

		return Task.CompletedTask;
	}
}

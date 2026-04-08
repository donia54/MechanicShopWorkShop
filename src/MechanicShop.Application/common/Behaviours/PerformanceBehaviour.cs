using System.Diagnostics;

using MechanicShop.Application.Common.Interfaces;

using MediatR;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviours;

public sealed class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly IUser _user;
	private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;
	private readonly long _warningThresholdMilliseconds;

	public PerformanceBehaviour(
		IUser user,
		ILogger<PerformanceBehaviour<TRequest, TResponse>> logger,
		long warningThresholdMilliseconds = 500)
	{
		_user = user;
		_logger = logger;
		_warningThresholdMilliseconds = warningThresholdMilliseconds;
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		var stopwatch = Stopwatch.StartNew();
		var response = await next(cancellationToken);
		stopwatch.Stop();

		if (stopwatch.ElapsedMilliseconds > _warningThresholdMilliseconds)
		{
			_logger.LogWarning(
				"Long Running Request: {RequestName} ({ElapsedMilliseconds} ms) for UserId: {UserId}",
				typeof(TRequest).Name,
				stopwatch.ElapsedMilliseconds,
				_user.Id);
		}

		return response;
	}
}

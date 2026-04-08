using MediatR;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviours;

public sealed class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> _logger;

	public UnhandledExceptionBehaviour(ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
	{
		_logger = logger;
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		try
		{
			return await next(cancellationToken);
		}
		catch (Exception exception)
		{
			var correlationId = Guid.NewGuid();

			_logger.LogError(
				exception,
				"Unhandled exception while processing {RequestName}. CorrelationId: {CorrelationId}.",
				typeof(TRequest).Name,
				correlationId);

			throw;
		}
	}
}

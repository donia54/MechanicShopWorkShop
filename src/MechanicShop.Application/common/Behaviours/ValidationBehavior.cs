using FluentValidation;

using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Common.Behaviours;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
	where TResponse : IResult
{
	private readonly IEnumerable<IValidator<TRequest>> _validators;

	public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
	{
		_validators = validators;
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		if (!_validators.Any())
		{
			return await next(cancellationToken);
		}

		var context = new ValidationContext<TRequest>(request);
		var validationTasks = _validators.Select(validator => validator.ValidateAsync(context, cancellationToken));
		var validationResults = await Task.WhenAll(validationTasks);

		var failures = validationResults
			.SelectMany(result => result.Errors)
			.Where(error => error is not null)
			.Select(error => Error.Validation(error.ErrorCode, error.ErrorMessage))
			.ToList();

		if (failures.Count == 0)
		{
			return await next(cancellationToken);
		}

		if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
		{
			return (TResponse)Activator.CreateInstance(typeof(TResponse), null, failures, false)!;
		}

		throw new InvalidOperationException($"ValidationBehavior cannot build a failure response for {typeof(TResponse).FullName}.");
	}
}

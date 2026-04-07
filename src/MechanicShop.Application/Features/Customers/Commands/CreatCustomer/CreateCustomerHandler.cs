using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Utilities;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.CreatCustomer;

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<CreateCustomerCommandHandler> _logger;
	private readonly HybridCache _cache;
	private const string CustomerCacheTag = "customer";

	public CreateCustomerCommandHandler(
		IAppDbContext dbContext,
		ILogger<CreateCustomerCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
	{
		var normalizedName = InputNormalizer.NormalizeText(request.Name);
		var normalizedPhoneNumber = InputNormalizer.NormalizeText(request.PhoneNumber);
		var normalizedEmail = InputNormalizer.NormalizeEmail(request.Email);

		if (await CustomerExistsAsync(normalizedEmail, cancellationToken))
		{
			_logger.LogInformation("Customer creation skipped because email already exists. Email: {Email}", normalizedEmail);
			return CustomerErrors.CustomerExists;
		}

		var vehiclesResult = CreateVehicles(request.Vehicles);
		if (vehiclesResult.IsError)
		{
			_logger.LogInformation(
				"Customer creation failed because one or more vehicles are invalid. Email: {Email}, ErrorCount: {ErrorCount}",
				normalizedEmail,
				vehiclesResult.Errors.Count);
			return vehiclesResult.Errors;
		}

		var customerResult = Customer.Create(
			Guid.NewGuid(),
			normalizedName,
			normalizedPhoneNumber,
			normalizedEmail,
			vehiclesResult.Value.ToList());

		if (customerResult.IsError)
		{
			_logger.LogInformation(
				"Customer creation failed due to domain validation. Email: {Email}, ErrorCount: {ErrorCount}",
				normalizedEmail,
				customerResult.Errors.Count);
			return customerResult.Errors;
		}

		_dbContext.Customers.Add(customerResult.Value);
		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(CustomerCacheTag, cancellationToken: cancellationToken);

		_logger.LogInformation(
			"Customer created successfully. Email: {Email}, CustomerId: {CustomerId}",
			normalizedEmail,
			customerResult.Value.Id);

		return customerResult.Value.ToDto();
	}

	private async Task<bool> CustomerExistsAsync(string normalizedEmail, CancellationToken cancellationToken)
	{
		return await _dbContext.Customers
			.AnyAsync(
				customer => customer.Email != null && customer.Email == normalizedEmail,
				cancellationToken);
	}

	private static Result<IReadOnlyList<Vehicle>> CreateVehicles(List<CreateVehicleInput>? vehicleInputs)
	{
		if (vehicleInputs is null || vehicleInputs.Count == 0)
		{
			return new List<Error> { CustomerErrors.VehicleRequired };
		}

		var vehicles = new List<Vehicle>(vehicleInputs.Count);
		var errors = new List<Error>();

		foreach (var input in vehicleInputs)
		{
			var result = Vehicle.Create(
				Guid.NewGuid(),
				InputNormalizer.NormalizeText(input.Make),
				InputNormalizer.NormalizeText(input.Model),
				input.Year,
				InputNormalizer.NormalizeText(input.LicensePlate));

			if (result.IsError)
			{
				errors.AddRange(result.Errors);
				continue;
			}

			vehicles.Add(result.Value);
		}

		if (errors.Count > 0)
		{
			return errors;
		}

		return vehicles;
	}
}

using MechanicShop.Application.Common.Errors;
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

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<UpdateCustomerCommandHandler> _logger;
	private readonly HybridCache _cache;
	private const string CustomerCacheTag = "customer";

	public UpdateCustomerCommandHandler(
		IAppDbContext dbContext,
		ILogger<UpdateCustomerCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Updated>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
	{
		var normalizedName = InputNormalizer.NormalizeText(request.Name);
		var normalizedPhoneNumber = InputNormalizer.NormalizeText(request.PhoneNumber);
		var normalizedEmail = InputNormalizer.NormalizeEmail(request.Email);

		var customer = await _dbContext.Customers
			.Include(c => c.Vehicles)
			.FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

		if (customer is null)
		{
			_logger.LogInformation("Customer update failed because customer was not found. CustomerId: {CustomerId}", request.CustomerId);
			return ApplicationErrors.Customer.NotFound(request.CustomerId);
		}

		if (await ValidateDuplicateEmailAsync(customer.Id, normalizedEmail, cancellationToken))
		{
			_logger.LogInformation(
				"Customer update failed because email already exists for another customer. CustomerId: {CustomerId}, Email: {Email}",
				request.CustomerId,
				normalizedEmail);
			return CustomerErrors.CustomerExists;
		}

		var updateCustomerResult = customer.Update(normalizedName, normalizedEmail, normalizedPhoneNumber);
		if (updateCustomerResult.IsError)
		{
			return updateCustomerResult.Errors;
		}

		var vehiclesResult = CreateVehicles(request.Vehicles);
		if (vehiclesResult.IsError)
		{
			return vehiclesResult.Errors;
		}

		var upsertVehiclesResult = customer.UpsertVehicles(vehiclesResult.Value.ToList());
		if (upsertVehiclesResult.IsError)
		{
			return upsertVehiclesResult.Errors;
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(CustomerCacheTag, cancellationToken: cancellationToken);

		_logger.LogInformation(
			"Customer updated successfully. CustomerId: {CustomerId}, Email: {Email}",
			customer.Id,
			normalizedEmail);

		return Result.Updated;
	}

	private async Task<bool> ValidateDuplicateEmailAsync(Guid customerId, string normalizedEmail, CancellationToken cancellationToken)
	{
		return await _dbContext.Customers
			.AnyAsync(
				customer => customer.Id != customerId
					&& customer.Email != null
					&& customer.Email.Trim().ToLower() == normalizedEmail,
				cancellationToken);
	}

	private static Result<IReadOnlyList<Vehicle>> CreateVehicles(List<UpdateVehicleInput>? vehicleInputs)
	{
		if (vehicleInputs is null || vehicleInputs.Count == 0)
		{
			return new List<Error> { CustomerErrors.VehicleRequired };
		}

		var vehicles = new List<Vehicle>(vehicleInputs.Count);
		var errors = new List<Error>();

		foreach (var input in vehicleInputs)
		{
			var createVehicleResult = Vehicle.Create(
				Guid.NewGuid(),
				InputNormalizer.NormalizeText(input.Make),
				InputNormalizer.NormalizeText(input.Model),
				input.Year,
				InputNormalizer.NormalizeText(input.LicensePlate));

			if (createVehicleResult.IsError)
			{
				errors.AddRange(createVehicleResult.Errors);
				continue;
			}

			vehicles.Add(createVehicleResult.Value);
		}

		if (errors.Count > 0)
		{
			return errors;
		}

		return vehicles;
	}
}
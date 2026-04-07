using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.DeleteCustomer;

public sealed class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<Deleted>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<DeleteCustomerCommandHandler> _logger;
	private readonly HybridCache _cache;
	private const string CustomerCacheTag = "customer";

	public DeleteCustomerCommandHandler(
		IAppDbContext dbContext,
		ILogger<DeleteCustomerCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Deleted>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
	{
		var customer = await _dbContext.Customers
			.FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

		if (customer is null)
		{
			_logger.LogInformation("Customer deletion failed because customer was not found. CustomerId: {CustomerId}", request.CustomerId);
			return CustomerErrors.CustomerNotFound;
		}

		if (await HasWorkOrdersAsync(request.CustomerId, cancellationToken))
		{
			_logger.LogInformation(
				"Customer deletion failed because customer has work orders. CustomerId: {CustomerId}",
				request.CustomerId);
			return CustomerErrors.CannotDeleteCustomerWithWorkOrders;
		}

		_dbContext.Customers.Remove(customer);
		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(CustomerCacheTag, cancellationToken: cancellationToken);

		_logger.LogInformation("Customer deleted successfully. CustomerId: {CustomerId}", request.CustomerId);

		return Result.Deleted;
	}

	private async Task<bool> HasWorkOrdersAsync(Guid customerId, CancellationToken cancellationToken)
	{
		var vehicleIds = _dbContext.Vehicles
			.Where(vehicle => vehicle.CustomerId == customerId)
			.Select(vehicle => vehicle.Id);

		return await _dbContext.WorkOrders
			.AnyAsync(workOrder => vehicleIds.Contains(workOrder.VehicleId), cancellationToken);
	}
}
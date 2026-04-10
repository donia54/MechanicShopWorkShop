using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, Result<WorkOrderDto>>
{
	private const string WorkOrderCacheTag = "workorder";

	private readonly IAppDbContext _dbContext;
	private readonly IWorkOrderPolicy _policy;
	private readonly ILogger<CreateWorkOrderCommandHandler> _logger;
	private readonly HybridCache _cache;

	public CreateWorkOrderCommandHandler(
		IAppDbContext dbContext,
		IWorkOrderPolicy policy,
		ILogger<CreateWorkOrderCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_policy = policy;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<WorkOrderDto>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
	{
		var startAtUtc = request.StartAtUtc;
		var endAtUtc = request.EndAtUtc;

		var timeWindowValidationResult = ValidateTimeWindow(startAtUtc, endAtUtc);
		if (timeWindowValidationResult.IsError)
		{
			return timeWindowValidationResult.Errors;
		}

		var referenceValidationResult = await ValidateReferencesAsync(request, cancellationToken);
		if (referenceValidationResult.IsError)
		{
			return referenceValidationResult.Errors;
		}

		var schedulingValidationResult = await _policy.ValidateSchedulingAsync(
			request.LaborId,
			request.VehicleId,
			request.Spot,
			startAtUtc,
			endAtUtc,
			excludeWorkOrderId: null,
			ct: cancellationToken);

		if (schedulingValidationResult.IsError)
		{
			_logger.LogInformation(
				"Create workorder failed due to scheduling policy. VehicleId: {VehicleId}, LaborId: {LaborId}, Spot: {Spot}, StartAtUtc: {StartAtUtc}, EndAtUtc: {EndAtUtc}",
				request.VehicleId,
				request.LaborId,
				request.Spot,
				startAtUtc,
				endAtUtc);

			return schedulingValidationResult.Errors;
		}

		var repairTasksResult = await GetRepairTasksAsync(request.RepairTaskIds, cancellationToken);
		if (repairTasksResult.IsError)
		{
			return repairTasksResult.Errors;
		}

		var createResult = WorkOrder.Create(
			Guid.NewGuid(),
			request.VehicleId,
			request.LaborId,
			request.Spot,
			startAtUtc,
			endAtUtc,
			repairTasks: repairTasksResult.Value);

		if (createResult.IsError)
		{
			_logger.LogInformation("Create workorder business validation failed for VehicleId: {VehicleId}", request.VehicleId);
			return createResult.Errors;
		}

		_dbContext.WorkOrders.Add(createResult.Value);
		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderCacheTag, cancellationToken: cancellationToken);

		_logger.LogInformation("Workorder created successfully. WorkOrderId: {WorkOrderId}", createResult.Value.Id);
		return createResult.Value.ToDto();
	}

	private Result<Success> ValidateTimeWindow(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc)
	{
		var minimumRequirementResult = _policy.ValidateMinimumRequirement(startAtUtc, endAtUtc);
		if (minimumRequirementResult.IsError)
		{
			_logger.LogInformation("Create workorder failed. Minimum requirement validation failed.");
			return minimumRequirementResult.Errors;
		}

		var workingHoursResult = _policy.ValidateWorkingHours(startAtUtc, endAtUtc);
		if (workingHoursResult.IsError)
		{
			_logger.LogInformation("Create workorder failed. Outside operating hours.");
			return workingHoursResult.Errors;
		}

		return Result.success;
	}

	private async Task<Result<Success>> ValidateReferencesAsync(CreateWorkOrderCommand request, CancellationToken cancellationToken)
	{
		if (!await VehicleExistsAsync(request.VehicleId, cancellationToken))
		{
			_logger.LogWarning("Create workorder failed. Vehicle not found: {VehicleId}", request.VehicleId);
			return ApplicationErrors.Vehicle.NotFound(request.VehicleId);
		}

		if (!await LaborExistsAsync(request.LaborId, cancellationToken))
		{
			_logger.LogWarning("Create workorder failed. Labor not found: {LaborId}", request.LaborId);
			return ApplicationErrors.Labor.NotFound(request.LaborId);
		}

		return Result.success;
	}

	private async Task<Result<List<Domain.RepairTasks.RepairTask>>> GetRepairTasksAsync(
		IReadOnlyList<Guid> repairTaskIds,
		CancellationToken cancellationToken)
	{
		var distinctRepairTaskIds = repairTaskIds.Distinct().ToList();

		var repairTasks = await _dbContext.RepairTasks
			.Where(task => distinctRepairTaskIds.Contains(task.Id))
			.ToListAsync(cancellationToken);

		if (repairTasks.Count != distinctRepairTaskIds.Count)
		{
			var missingRepairTaskId = distinctRepairTaskIds.Except(repairTasks.Select(task => task.Id)).First();
			_logger.LogWarning("Create workorder failed. RepairTask not found: {RepairTaskId}", missingRepairTaskId);
			return ApplicationErrors.RepairTask.NotFound(missingRepairTaskId);
		}

		return repairTasks;
	}

	private Task<bool> VehicleExistsAsync(Guid vehicleId, CancellationToken cancellationToken)
	{
		return _dbContext.Vehicles.AnyAsync(vehicle => vehicle.Id == vehicleId, cancellationToken);
	}

	private Task<bool> LaborExistsAsync(Guid laborId, CancellationToken cancellationToken)
	{
		return _dbContext.Employees.AnyAsync(employee => employee.Id == laborId, cancellationToken);
	}
}
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Utilities;
using MechanicShop.Application.Features.WorkOrders.Queries;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Commands.UpdateRepairTaskInWorkOrder;

public sealed class UpdateRepairTaskInWorkOrderCommandHandler : IRequestHandler<UpdateRepairTaskInWorkOrderCommand, Result<Updated>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<UpdateRepairTaskInWorkOrderCommandHandler> _logger;
	private readonly HybridCache _cache;

	public UpdateRepairTaskInWorkOrderCommandHandler(
		IAppDbContext dbContext,
		ILogger<UpdateRepairTaskInWorkOrderCommandHandler> logger,
		HybridCache cache)
	{
		_dbContext = dbContext;
		_logger = logger;
		_cache = cache;
	}

	public async Task<Result<Updated>> Handle(UpdateRepairTaskInWorkOrderCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"Updating repair task in workorder. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}",
			request.WorkOrderId,
			request.RepairTaskId);

		var workOrder = await _dbContext.WorkOrders
			.Include(order => order.RepairTasks)
				.ThenInclude(task => task.Parts)
			.FirstOrDefaultAsync(order => order.Id == request.WorkOrderId, cancellationToken);

		if (workOrder is null)
		{
			_logger.LogWarning("Update repair task failed. WorkOrder not found: {WorkOrderId}", request.WorkOrderId);
			return ApplicationErrors.WorkOrder.NotFound(request.WorkOrderId);
		}

		var repairTask = workOrder.RepairTasks.FirstOrDefault(task => task.Id == request.RepairTaskId);
		if (repairTask is null)
		{
			_logger.LogWarning(
				"Update repair task failed. RepairTask is not linked to WorkOrder. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}",
				request.WorkOrderId,
				request.RepairTaskId);
			return ApplicationErrors.RepairTask.NotFound(request.RepairTaskId);
		}

		if (!Enum.IsDefined(typeof(RepairDurationInMinutes), request.EstimatedDuration))
		{
			_logger.LogInformation("Update repair task failed. EstimatedDuration is invalid: {EstimatedDuration}", request.EstimatedDuration);
			return Error.Validation(
				code: "ApplicationErrors.RepairTask.EstimatedDurationInvalid",
				description: $"Estimated duration '{request.EstimatedDuration}' is invalid.");
		}

		var normalizedName = InputNormalizer.NormalizeText(request.Name);
		var duration = (RepairDurationInMinutes)request.EstimatedDuration;

		var updateResult = repairTask.Update(normalizedName, request.LaborCost, duration);
		if (updateResult.IsError)
		{
			_logger.LogInformation(
				"Update repair task failed by domain validation. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}",
				request.WorkOrderId,
				request.RepairTaskId);
			return updateResult.Errors;
		}

		var partsResult = CreateParts(request.Parts);
		if (partsResult.IsError)
		{
			_logger.LogInformation(
				"Update repair task failed while creating parts. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}",
				request.WorkOrderId,
				request.RepairTaskId);
			return partsResult.Errors;
		}

		var upsertResult = repairTask.UpsertParts(partsResult.Value);
		if (upsertResult.IsError)
		{
			_logger.LogInformation(
				"Update repair task failed while upserting parts. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}",
				request.WorkOrderId,
				request.RepairTaskId);
			return upsertResult.Errors;
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
		await _cache.RemoveByTagAsync(WorkOrderQueryCacheConstants.WorkOrderTag, cancellationToken: cancellationToken);

		_logger.LogInformation(
			"Repair task updated in workorder successfully. WorkOrderId: {WorkOrderId}, RepairTaskId: {RepairTaskId}",
			request.WorkOrderId,
			request.RepairTaskId);

		return Result.Updated;
	}

	private static Result<List<Part>> CreateParts(IReadOnlyList<UpdateRepairTaskPartInput> inputParts)
	{
		var parts = new List<Part>(inputParts.Count);
		var errors = new List<Error>();

		foreach (var input in inputParts)
		{
			var createResult = Part.Create(
				input.PartId,
				InputNormalizer.NormalizeText(input.Name),
				input.Cost,
				input.Quantity);

			if (createResult.IsError)
			{
				errors.AddRange(createResult.Errors);
				continue;
			}

			parts.Add(createResult.Value);
		}

		if (errors.Count > 0)
		{
			return errors;
		}

		return parts;
	}
}
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Application.Features.WorkOrders.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.RepairTasks.Queries.GetRepairTaskById;

public sealed class GetRepairTaskByIdQueryHandler : IRequestHandler<GetRepairTaskByIdQuery, Result<RepairTaskDto>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<GetRepairTaskByIdQueryHandler> _logger;

	public GetRepairTaskByIdQueryHandler(
		IAppDbContext dbContext,
		ILogger<GetRepairTaskByIdQueryHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<Result<RepairTaskDto>> Handle(GetRepairTaskByIdQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting repair task by id. RepairTaskId: {RepairTaskId}", request.RepairTaskId);

		var repairTask = await _dbContext.RepairTasks
			.AsNoTracking()
			.Include(task => task.Parts)
			.FirstOrDefaultAsync(task => task.Id == request.RepairTaskId, cancellationToken);

		if (repairTask is null)
		{
			_logger.LogWarning("Get repair task by id failed. RepairTask not found: {RepairTaskId}", request.RepairTaskId);
			return ApplicationErrors.RepairTask.NotFound(request.RepairTaskId);
		}

		var dto = repairTask.ToRepairTaskDto();
		_logger.LogInformation("Repair task retrieved successfully. RepairTaskId: {RepairTaskId}", request.RepairTaskId);

		return dto;
	}
}
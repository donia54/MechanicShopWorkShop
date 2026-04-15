using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Labor.Dtos;
using MechanicShop.Application.Features.WorkOrders.Labor.Mapper;
using MechanicShop.Domain.Identity;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Labor.Queries.GetAvailableLabors;

public sealed class GetAvailableLaborsQueryHandler : IRequestHandler<GetAvailableLaborsQuery, List<LaborDto>>
{
	private readonly IAppDbContext _dbContext;
	private readonly IWorkOrderPolicy _workOrderPolicy;
	private readonly ILogger<GetAvailableLaborsQueryHandler> _logger;

	public GetAvailableLaborsQueryHandler(
		IAppDbContext dbContext,
		IWorkOrderPolicy workOrderPolicy,
		ILogger<GetAvailableLaborsQueryHandler> logger)
	{
		_dbContext = dbContext;
		_workOrderPolicy = workOrderPolicy;
		_logger = logger;
	}

	public async Task<List<LaborDto>> Handle(GetAvailableLaborsQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"Getting available labors. StartAt: {StartAt}, EndAt: {EndAt}",
			request.StartAt,
			request.EndAt);

		if (request.StartAt >= request.EndAt)
		{
			_logger.LogWarning(
				"Get available labors failed due to invalid range. StartAt: {StartAt}, EndAt: {EndAt}",
				request.StartAt,
				request.EndAt);
			return [];
		}

		var laborEmployees = await _dbContext.Employees
			.AsNoTracking()
			.Where(employee => employee.Role == Role.Labor)
			.ToListAsync(cancellationToken);

		var availableLabors = new List<LaborDto>();

		foreach (var labor in laborEmployees)
		{
			var isOccupied = await _workOrderPolicy.IsLaborOccupied(
				labor.Id,
				Guid.Empty,
				request.StartAt,
				request.EndAt);

			if (!isOccupied)
			{
				availableLabors.Add(labor.ToLaborDto());
			}
		}

		_logger.LogInformation(
			"Available labors retrieved successfully. Count: {Count}, StartAt: {StartAt}, EndAt: {EndAt}",
			availableLabors.Count,
			request.StartAt,
			request.EndAt);

		return availableLabors;
	}
}
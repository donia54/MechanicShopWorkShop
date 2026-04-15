using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;

public sealed class GetWorkOrdersQueryHandler : IRequestHandler<GetWorkOrdersQuery, Result<IReadOnlyList<WorkOrderListItemDto>>>
{
	private const int MinPageNumber = 1;
	private const int MinPageSize = 1;
	private const int MaxPageSize = 200;

	private readonly IAppDbContext _dbContext;
	private readonly ILogger<GetWorkOrdersQueryHandler> _logger;

	public GetWorkOrdersQueryHandler(
		IAppDbContext dbContext,
		ILogger<GetWorkOrdersQueryHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<Result<IReadOnlyList<WorkOrderListItemDto>>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"Getting workorders list. PageNumber: {PageNumber}, PageSize: {PageSize}, State: {State}, FromDate: {FromDate}, ToDate: {ToDate}",
			request.PageNumber,
			request.PageSize,
			request.State,
			request.FromDate,
			request.ToDate);

		var pageNumber = Math.Max(request.PageNumber, MinPageNumber);
		var pageSize = Math.Clamp(request.PageSize, MinPageSize, MaxPageSize);

		var queryable = _dbContext.WorkOrders
			.AsNoTracking()
			.AsQueryable();

		if (request.State.HasValue)
		{
			queryable = queryable.Where(order => order.State == request.State.Value);
		}

		if (request.FromDate.HasValue)
		{
			queryable = queryable.Where(order => order.StartAtUtc >= request.FromDate.Value);
		}

		if (request.ToDate.HasValue)
		{
			queryable = queryable.Where(order => order.StartAtUtc <= request.ToDate.Value);
		}

		var totalCount = await queryable.CountAsync(cancellationToken);

		var workOrders = await queryable
			.OrderByDescending(order => order.StartAtUtc)
			.Skip((pageNumber - MinPageNumber) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		var listItems = workOrders
			.Select(order => order.ToListItemDto())
			.ToList();

		var paginatedList = new PaginatedList<WorkOrderListItemDto>
		{
			PageNumber = pageNumber,
			PageSize = pageSize,
			TotalCount = totalCount,
			TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
			Items = listItems
		};

		_logger.LogInformation(
			"Workorders list retrieved successfully. PageNumber: {PageNumber}, PageSize: {PageSize}, ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
			paginatedList.PageNumber,
			paginatedList.PageSize,
			listItems.Count,
			paginatedList.TotalCount);

		return listItems;
	}
}
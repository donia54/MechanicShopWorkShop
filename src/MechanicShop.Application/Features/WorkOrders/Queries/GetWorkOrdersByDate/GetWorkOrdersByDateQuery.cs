using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrdersByDate;

public sealed record GetWorkOrdersByDateQuery(
	DateTimeOffset FromDate,
	DateTimeOffset ToDate) : IRequest<Result<IReadOnlyList<WorkOrderListItemDto>>>;
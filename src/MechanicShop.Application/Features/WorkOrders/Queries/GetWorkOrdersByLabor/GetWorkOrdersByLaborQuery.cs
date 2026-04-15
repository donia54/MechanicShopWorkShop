using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrdersByLabor;

public sealed record GetWorkOrdersByLaborQuery(Guid LaborId) : IRequest<Result<IReadOnlyList<WorkOrderListItemDto>>>;
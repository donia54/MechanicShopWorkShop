using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrdersByVehicle;

public sealed record GetWorkOrdersByVehicleQuery(Guid VehicleId) : IRequest<Result<IReadOnlyList<WorkOrderListItemDto>>>;
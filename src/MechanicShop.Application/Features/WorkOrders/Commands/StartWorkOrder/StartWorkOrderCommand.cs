using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Commands.StartWorkOrder;

public sealed record StartWorkOrderCommand(Guid WorkOrderId) : IRequest<Result<Updated>>;

using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CompleteWorkOrder;

public sealed record CompleteWorkOrderCommand(Guid WorkOrderId) : IRequest<Result<Updated>>;

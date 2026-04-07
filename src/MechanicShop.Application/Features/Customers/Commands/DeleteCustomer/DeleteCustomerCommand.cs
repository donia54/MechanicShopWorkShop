using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.DeleteCustomer;

public sealed record DeleteCustomerCommand(Guid CustomerId) : IRequest<Result<Deleted>>;
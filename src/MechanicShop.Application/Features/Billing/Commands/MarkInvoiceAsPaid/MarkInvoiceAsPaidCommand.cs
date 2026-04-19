using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.Billing.Commands.MarkInvoiceAsPaid;

public sealed record MarkInvoiceAsPaidCommand(Guid InvoiceId) : IRequest<Result<Updated>>;
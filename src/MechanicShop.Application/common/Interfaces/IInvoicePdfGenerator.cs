using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Common.Interfaces;

public interface IInvoicePdfGenerator
{
	Task<byte[]> GenerateAsync(Invoice invoice, CancellationToken cancellationToken);
}
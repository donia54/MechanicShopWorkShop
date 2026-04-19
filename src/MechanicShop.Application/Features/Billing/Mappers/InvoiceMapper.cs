using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Features.Billing.Mappers;

public static class InvoiceMapper
{
	public static InvoiceDto ToDto(this Invoice invoice)
	{
		return new InvoiceDto(
			invoice.Id,
			invoice.WorkOrderId,
			invoice.IssuedAtUtc,
			invoice.Status.ToString(),
			invoice.Subtotal,
			invoice.DiscountAmount,
			invoice.TaxAmount,
			invoice.Total,
			invoice.LineItems.Select(lineItem => lineItem.ToDto()).ToList());
	}

	public static InvoiceLineItemDto ToDto(this InvoiceLineItem lineItem)
	{
		return new InvoiceLineItemDto(
			lineItem.LineNumber,
			lineItem.Description ?? string.Empty,
			lineItem.Quantity,
			lineItem.UnitPrice,
			lineItem.LineTotal);
	}
}
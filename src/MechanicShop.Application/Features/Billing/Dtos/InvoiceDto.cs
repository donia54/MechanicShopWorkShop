namespace MechanicShop.Application.Features.Billing.Dtos;

public sealed record InvoiceDto(
	Guid Id,
	Guid WorkOrderId,
	DateTimeOffset IssuedAtUtc,
	string Status,
	decimal Subtotal,
	decimal DiscountAmount,
	decimal TaxAmount,
	decimal Total,
	List<InvoiceLineItemDto> LineItems);
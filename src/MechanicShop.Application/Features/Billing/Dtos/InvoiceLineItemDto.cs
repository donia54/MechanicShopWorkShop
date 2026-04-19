namespace MechanicShop.Application.Features.Billing.Dtos;

public sealed record InvoiceLineItemDto(
	int LineNumber,
	string Description,
	int Quantity,
	decimal UnitPrice,
	decimal LineTotal);
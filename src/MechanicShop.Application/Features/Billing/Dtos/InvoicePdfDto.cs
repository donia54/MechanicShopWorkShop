namespace MechanicShop.Application.Features.Billing.Dtos;

public sealed record InvoicePdfDto(
	string FileName,
	byte[] Content,
	string ContentType);
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Commands.MarkInvoiceAsPaid;

public sealed class MarkInvoiceAsPaidCommandHandler : IRequestHandler<MarkInvoiceAsPaidCommand, Result<Updated>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<MarkInvoiceAsPaidCommandHandler> _logger;
	private readonly TimeProvider _timeProvider;

	public MarkInvoiceAsPaidCommandHandler(
		IAppDbContext dbContext,
		ILogger<MarkInvoiceAsPaidCommandHandler> logger,
		TimeProvider timeProvider)
	{
		_dbContext = dbContext;
		_logger = logger;
		_timeProvider = timeProvider;
	}

	public async Task<Result<Updated>> Handle(MarkInvoiceAsPaidCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Mark invoice as paid started. InvoiceId: {InvoiceId}", request.InvoiceId);

		var invoiceResult = await GetInvoiceAsync(request.InvoiceId, cancellationToken);
		if (invoiceResult.IsError)
		{
			return invoiceResult.Errors;
		}

		var invoice = invoiceResult.Value;

		var statusValidationResult = ValidateInvoiceStatus(invoice);
		if (statusValidationResult.IsError)
		{
			_logger.LogInformation(
				"Mark invoice as paid validation failed. InvoiceId: {InvoiceId}, Status: {Status}, Total: {Total}, LineItemCount: {LineItemCount}",
				invoice.Id,
				invoice.Status,
				invoice.Total,
				invoice.LineItems.Count);
			return statusValidationResult.Errors;
		}

		var markAsPaidResult = invoice.MarkAsPaid(_timeProvider);
		if (markAsPaidResult.IsError)
		{
			_logger.LogInformation("Mark invoice as paid domain transition failed. InvoiceId: {InvoiceId}", request.InvoiceId);
			return markAsPaidResult.Errors;
		}

		await _dbContext.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("Mark invoice as paid succeeded. InvoiceId: {InvoiceId}", request.InvoiceId);

		return Result.Updated;
	}

	private async Task<Result<Invoice>> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Fetching invoice for payment update. InvoiceId: {InvoiceId}", invoiceId);

		var invoice = await _dbContext.Invoices
			.FirstOrDefaultAsync(existingInvoice => existingInvoice.Id == invoiceId, cancellationToken);

		if (invoice is null)
		{
			_logger.LogWarning("Fetching invoice failed. Invoice not found: {InvoiceId}", invoiceId);
			return ApplicationErrors.Invoice.NotFound(invoiceId);
		}

		return invoice;
	}

	private static Result<Success> ValidateInvoiceStatus(Invoice invoice)
	{
		if (invoice.Status == InvoiceStatus.Paid)
		{
			return ApplicationErrors.Invoice.AlreadyPaid;
		}

		if (invoice.LineItems.Count == 0)
		{
			return Error.Validation(
				code: "ApplicationErrors.Invoice.InvalidLineItems",
				description: "Invoice must contain at least one line item before marking as paid.");
		}

		if (invoice.Total <= 0)
		{
			return Error.Validation(
				code: "ApplicationErrors.Invoice.InvalidTotal",
				description: "Invoice total must be greater than zero before marking as paid.");
		}

		return Result.success;
	}
}
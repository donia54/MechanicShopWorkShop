using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;

public sealed class GetInvoicePdfQueryHandler : IRequestHandler<GetInvoicePdfQuery, Result<InvoicePdfDto>>
{
	private readonly IAppDbContext _dbContext;
	private readonly IInvoicePdfGenerator _invoicePdfGenerator;
	private readonly ILogger<GetInvoicePdfQueryHandler> _logger;

	public GetInvoicePdfQueryHandler(
		IAppDbContext dbContext,
		IInvoicePdfGenerator invoicePdfGenerator,
		ILogger<GetInvoicePdfQueryHandler> logger)
	{
		_dbContext = dbContext;
		_invoicePdfGenerator = invoicePdfGenerator;
		_logger = logger;
	}

	public async Task<Result<InvoicePdfDto>> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Generating invoice PDF. InvoiceId: {InvoiceId}", request.InvoiceId);

		var invoice = await _dbContext.Invoices
			.AsNoTracking()
			.Include(existingInvoice => existingInvoice.LineItems)
			.FirstOrDefaultAsync(existingInvoice => existingInvoice.Id == request.InvoiceId, cancellationToken);

		if (invoice is null)
		{
			_logger.LogWarning("Invoice PDF generation failed. Invoice not found: {InvoiceId}", request.InvoiceId);
			return ApplicationErrors.Invoice.NotFound(request.InvoiceId);
		}

		var pdfContent = await _invoicePdfGenerator.GenerateAsync(invoice, cancellationToken);
		var pdfDto = new InvoicePdfDto(
			$"invoice-{invoice.Id}.pdf",
			pdfContent,
			"application/pdf");

		_logger.LogInformation("Invoice PDF generated successfully. InvoiceId: {InvoiceId}", request.InvoiceId);

		return pdfDto;
	}
}
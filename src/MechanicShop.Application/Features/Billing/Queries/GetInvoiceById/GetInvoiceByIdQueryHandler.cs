using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

public sealed class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
	private readonly IAppDbContext _dbContext;
	private readonly ILogger<GetInvoiceByIdQueryHandler> _logger;

	public GetInvoiceByIdQueryHandler(
		IAppDbContext dbContext,
		ILogger<GetInvoiceByIdQueryHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting invoice by id. InvoiceId: {InvoiceId}", request.InvoiceId);

		var invoice = await _dbContext.Invoices
			.AsNoTracking()
			.Include(existingInvoice => existingInvoice.LineItems)
			.FirstOrDefaultAsync(existingInvoice => existingInvoice.Id == request.InvoiceId, cancellationToken);

		if (invoice is null)
		{
			_logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
			return ApplicationErrors.Invoice.NotFound(request.InvoiceId);
		}

		_logger.LogInformation("Invoice retrieved successfully. InvoiceId: {InvoiceId}", request.InvoiceId);
		return invoice.ToDto();
	}
}
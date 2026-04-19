using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Commands.MarkInvoiceAsPaid;

public sealed class MarkInvoiceAsPaidCommandValidator : AbstractValidator<MarkInvoiceAsPaidCommand>
{
	public MarkInvoiceAsPaidCommandValidator()
	{
		RuleFor(x => x.InvoiceId).NotEmpty();
	}
}
using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
	public IssueInvoiceCommandValidator()
	{
		RuleFor(x => x.WorkOrderId).NotEmpty();
	}
}
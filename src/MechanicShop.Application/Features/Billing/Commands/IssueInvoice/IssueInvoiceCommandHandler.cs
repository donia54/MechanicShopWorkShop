using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandHandler : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
{
	private const decimal DefaultTaxAmount = 0m;

	private readonly IAppDbContext _dbContext;
	private readonly ILogger<IssueInvoiceCommandHandler> _logger;

	public IssueInvoiceCommandHandler(
		IAppDbContext dbContext,
		ILogger<IssueInvoiceCommandHandler> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Issuing invoice started. WorkOrderId: {WorkOrderId}", request.WorkOrderId);

		var workOrderResult = await GetWorkOrderAsync(request.WorkOrderId, cancellationToken);
		if (workOrderResult.IsError)
		{
			return workOrderResult.Errors;
		}

		var workOrder = workOrderResult.Value;

		var stateValidationResult = ValidateWorkOrderState(workOrder);
		if (stateValidationResult.IsError)
		{
			_logger.LogInformation("Issue invoice validation failed. WorkOrderId: {WorkOrderId}, CurrentState: {CurrentState}", workOrder.Id, workOrder.State);
			return stateValidationResult.Errors;
		}

		var hasExistingInvoice = await _dbContext.Invoices.AnyAsync(invoice => invoice.WorkOrderId == workOrder.Id, cancellationToken);
		if (hasExistingInvoice)
		{
			_logger.LogInformation("Issue invoice failed. Invoice already exists for WorkOrderId: {WorkOrderId}", workOrder.Id);
			return Error.Conflict(
				code: "ApplicationErrors.Invoice.AlreadyExistsForWorkOrder",
				description: $"An invoice already exists for WorkOrder '{workOrder.Id}'.");
		}

		var invoiceId = Guid.NewGuid();
		var lineItemsResult = BuildLineItems(invoiceId, workOrder.RepairTasks);
		if (lineItemsResult.IsError)
		{
			_logger.LogInformation("Issue invoice failed. Line item validation failed. WorkOrderId: {WorkOrderId}, ErrorCount: {ErrorCount}", workOrder.Id, lineItemsResult.Errors.Count);
			return lineItemsResult.Errors;
		}

		_logger.LogInformation("Creating invoice aggregate. WorkOrderId: {WorkOrderId}, LineItemCount: {LineItemCount}", workOrder.Id, lineItemsResult.Value.Count);

		var createInvoiceResult = CreateInvoiceAggregate(
			workOrder.Id,
			lineItemsResult.Value,
			DefaultTaxAmount);

		if (createInvoiceResult.IsError)
		{
			_logger.LogInformation("Issue invoice failed. Invoice aggregate creation failed. WorkOrderId: {WorkOrderId}, ErrorCount: {ErrorCount}", workOrder.Id, createInvoiceResult.Errors.Count);
			return createInvoiceResult.Errors;
		}

		_dbContext.Invoices.Add(createInvoiceResult.Value);
		await _dbContext.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(
			"Invoice issued successfully. InvoiceId: {InvoiceId}, WorkOrderId: {WorkOrderId}",
			createInvoiceResult.Value.Id,
			workOrder.Id);

		return createInvoiceResult.Value.ToDto();
	}

	private async Task<Result<WorkOrder>> GetWorkOrderAsync(Guid workOrderId, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Fetching workorder for invoicing. WorkOrderId: {WorkOrderId}", workOrderId);

		var workOrder = await _dbContext.WorkOrders
			.Include(order => order.RepairTasks)
				.ThenInclude(task => task.Parts)
			.FirstOrDefaultAsync(order => order.Id == workOrderId, cancellationToken);

		if (workOrder is null)
		{
			_logger.LogWarning("Workorder was not found for invoicing. WorkOrderId: {WorkOrderId}", workOrderId);
			return ApplicationErrors.WorkOrder.NotFound(workOrderId);
		}

		return workOrder;
	}

	private static Result<Success> ValidateWorkOrderState(WorkOrder workOrder)
	{
		if (workOrder.State != WorkOrderState.Completed)
		{
			return ApplicationErrors.Invoice.CannotGenerateUnlessWorkOrderCompleted(workOrder.Id, workOrder.State.ToString());
		}

		return Result.success;
	}

	private static Result<Invoice> CreateInvoiceAggregate(
		Guid workOrderId,
		IReadOnlyList<InvoiceLineItem> lineItems,
		decimal taxAmount)
	{
		if (lineItems.Count == 0)
		{
			return InvoiceErrors.LineItemsRequired;
		}

		var distinctInvoiceIds = lineItems
			.Select(lineItem => lineItem.InvoiceId)
			.Distinct()
			.ToList();

		if (distinctInvoiceIds.Count != 1)
		{
			return Error.Validation(
				code: "ApplicationErrors.Invoice.InconsistentLineItemInvoiceIds",
				description: "All invoice line items must belong to the same invoice aggregate.");
		}

		return Invoice.Create(
			distinctInvoiceIds[0],
			workOrderId,
			lineItems,
			taxAmount);
	}

	private static Result<IReadOnlyList<InvoiceLineItem>> BuildLineItems(Guid invoiceId, IReadOnlyList<Domain.RepairTasks.RepairTask> repairTasks)
	{
		var lineItems = new List<InvoiceLineItem>();
		var errors = new List<Error>();
		var lineNumber = 1;
		var shouldStop = false;

		foreach (var repairTask in repairTasks)
		{
			var laborLineResult = CreateLaborLineItem(invoiceId, lineNumber, repairTask);
			if (laborLineResult.IsError)
			{
				errors.AddRange(laborLineResult.Errors);
			}
			else
			{
				lineItems.Add(laborLineResult.Value);
			}

			var nextLaborLineNumberResult = GetNextLineNumber(lineNumber);
			if (nextLaborLineNumberResult.IsError)
			{
				errors.AddRange(nextLaborLineNumberResult.Errors);
				shouldStop = true;
				break;
			}

			lineNumber = nextLaborLineNumberResult.Value;

			foreach (var part in repairTask.Parts)
			{
				var partLineResult = CreatePartLineItem(invoiceId, lineNumber, repairTask.Name, part);
				if (partLineResult.IsError)
				{
					errors.AddRange(partLineResult.Errors);
				}
				else
				{
					lineItems.Add(partLineResult.Value);
				}

				var nextPartLineNumberResult = GetNextLineNumber(lineNumber);
				if (nextPartLineNumberResult.IsError)
				{
					errors.AddRange(nextPartLineNumberResult.Errors);
					shouldStop = true;
					break;
				}

				lineNumber = nextPartLineNumberResult.Value;
			}

			if (shouldStop)
			{
				break;
			}
		}

		if (lineItems.Count == 0)
		{
			errors.Add(InvoiceErrors.LineItemsRequired);
		}

		if (errors.Count > 0)
		{
			return errors;
		}

		return lineItems;
	}

	private static Result<InvoiceLineItem> CreateLaborLineItem(Guid invoiceId, int lineNumber, Domain.RepairTasks.RepairTask repairTask)
	{
		return InvoiceLineItem.Create(
			invoiceId,
			lineNumber,
			description: $"Labor - {repairTask.Name}",
			quantity: 1,
			unitPrice: repairTask.LaborCost);
	}

	private static Result<InvoiceLineItem> CreatePartLineItem(
		Guid invoiceId,
		int lineNumber,
		string? repairTaskName,
		Domain.RepairTasks.Parts.Part part)
	{
		return InvoiceLineItem.Create(
			invoiceId,
			lineNumber,
			description: $"Part - {part.Name} ({repairTaskName})",
			quantity: part.Quantity,
			unitPrice: part.Cost);
	}

	private static Result<int> GetNextLineNumber(int currentLineNumber)
	{
		try
		{
			return checked(currentLineNumber + 1);
		}
		catch (OverflowException)
		{
			return Error.Failure(
				code: "ApplicationErrors.Invoice.LineNumberOverflow",
				description: "Invoice line number overflow occurred while building line items.");
		}
	}
}
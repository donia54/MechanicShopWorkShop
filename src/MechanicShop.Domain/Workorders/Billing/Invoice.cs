using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.WorkOrders.Billing;

public sealed class Invoice : AuditableEntity
{
    public Guid WorkOrderId { get; }

    public DateTimeOffset IssuedAtUtc { get; }

    public decimal DiscountAmount { get; private set; }

    public decimal TaxAmount { get; }

    public decimal Subtotal => LineItems.Sum(x => x.LineTotal);

    public decimal Total => Subtotal - DiscountAmount + TaxAmount;

    public DateTimeOffset? PaidAt { get; private set; }

    public WorkOrder? WorkOrder { get; private set; }

    private readonly List<InvoiceLineItem> _lineItems = [];

    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems;

    public InvoiceStatus Status { get; private set; }

    public bool IsEditable => Status == InvoiceStatus.Unpaid;

    private Invoice()
    {
    }

    private Invoice(
        Guid id,
        Guid workOrderId,
        DateTimeOffset issuedAt,
        IEnumerable<InvoiceLineItem> lineItems,
        decimal discountAmount,
        decimal taxAmount)
        : base(id)
    {
        WorkOrderId = workOrderId;
        IssuedAtUtc = issuedAt;
        DiscountAmount = discountAmount;
        Status = InvoiceStatus.Unpaid;
        TaxAmount = taxAmount;
        _lineItems.AddRange(lineItems);
    }

    public static Result<Invoice> Create(
        Guid id,
        Guid workOrderId,
        IEnumerable<InvoiceLineItem>? lineItems,
        decimal taxAmount = 0,
        TimeProvider? timeProvider = null)
    {
        if (workOrderId == Guid.Empty)
        {
            return InvoiceErrors.WorkOrderIdRequired;
        }

        if (lineItems is null)
        {
            return InvoiceErrors.LineItemsRequired;
        }

        var items = lineItems.ToList();

        if (items.Count == 0)
        {
            return InvoiceErrors.LineItemsRequired;
        }

        if (items.GroupBy(item => item.LineNumber).Any(group => group.Count() > 1))
        {
            return InvoiceErrors.DuplicateLineItem;
        }

        var issuedAtUtc = timeProvider?.GetUtcNow() ?? DateTimeOffset.UtcNow;

        return new Invoice(
            id,
            workOrderId,
            issuedAtUtc,
            items,
            discountAmount: 0,
            taxAmount: taxAmount);
    }

    public Result<Updated> AddLineItem(InvoiceLineItem item)
    {
        if (!IsEditable)
        {
            return InvoiceErrors.InvoiceLocked;
        }

        if (_lineItems.Any(existing => existing.LineNumber == item.LineNumber))
        {
            return InvoiceErrors.DuplicateLineItem;
        }

        _lineItems.Add(item);
        return Result.Updated;
    }

    public Result<Updated> RemoveLineItem(int lineNumber)
    {
        if (!IsEditable)
        {
            return InvoiceErrors.InvoiceLocked;
        }

        var item = _lineItems.FirstOrDefault(existing => existing.LineNumber == lineNumber);

        if (item is null)
        {
            return InvoiceErrors.LineItemNotFound;
        }

        _lineItems.Remove(item);
        return Result.Updated;
    }

    public Result<Updated> ApplyDiscount(decimal discount)
    {
        if (!IsEditable)
        {
            return InvoiceErrors.InvoiceLocked;
        }

        if (discount < 0)
        {
            return InvoiceErrors.DiscountNegative;
        }

        if (discount > Subtotal)
        {
            return InvoiceErrors.DiscountExceedsSubtotal;
        }

        DiscountAmount = discount;
        return Result.Updated;
    }

    public Result<Updated> MarkAsPaid(TimeProvider timeProvider)
    {
        if (Status != InvoiceStatus.Unpaid)
        {
            return InvoiceErrors.AlreadyPaid;
        }

        Status = InvoiceStatus.Paid;
        PaidAt = timeProvider.GetUtcNow();
        return Result.Updated;
    }
}

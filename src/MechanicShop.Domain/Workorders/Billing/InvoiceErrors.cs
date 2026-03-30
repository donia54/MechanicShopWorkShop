using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.WorkOrders.Billing;

public static class InvoiceErrors
{
    public static Error WorkOrderIdRequired => Error.Validation(
        code: "InvoiceErrors.WorkOrderIdRequired",
        description: "WorkOrder Id is required.");

    public static Error LineItemsRequired => Error.Validation(
        code: "InvoiceErrors.LineItemsRequired",
        description: "At least one line item is required.");

    public static Error InvoiceLocked => Error.Conflict(
        code: "InvoiceErrors.InvoiceLocked",
        description: "Invoice cannot be modified unless it is Unpaid.");

    public static Error DiscountNegative => Error.Conflict(
        code: "InvoiceErrors.DiscountNegative",
        description: "Discount cannot be negative.");

    public static Error DiscountExceedsSubtotal => Error.Conflict(
        code: "InvoiceErrors.DiscountExceedsSubtotal",
        description: "Discount cannot exceed subtotal.");

    public static Error AlreadyPaid => Error.Conflict(
        code: "InvoiceErrors.AlreadyPaid",
        description: "Invoice is already paid.");

    public static Error DuplicateLineItem => Error.Conflict(
        code: "InvoiceErrors.DuplicateLineItem",
        description: "Duplicate line number is not allowed.");

    public static Error LineItemNotFound => Error.Conflict(
        code: "InvoiceErrors.LineItemNotFound",
        description: "Line item was not found.");
}

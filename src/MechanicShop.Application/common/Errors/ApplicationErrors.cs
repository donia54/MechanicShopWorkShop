using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Errors;

public static class ApplicationErrors
{
    public static class WorkOrder
    {
        public static Error NotFound(Guid workOrderId) => Error.NotFound(
            code: "ApplicationErrors.WorkOrder.NotFound",
            description: $"WorkOrder '{workOrderId}' was not found.");

        public static readonly Error MustBeCompletedBeforeInvoiceCreation = Error.Conflict(
            code: "ApplicationErrors.WorkOrder.MustBeCompletedBeforeInvoiceCreation",
            description: "WorkOrder must be completed before invoice creation.");

        public static Error OutsideOperatingHours(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc) => Error.Validation(
            code: "ApplicationErrors.WorkOrder.OutsideOperatingHours",
            description: $"WorkOrder schedule '{startAtUtc:O}' - '{endAtUtc:O}' is outside operating hours.");

        public static Error OverlappingSchedule(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc) => Error.Conflict(
            code: "ApplicationErrors.WorkOrder.OverlappingSchedule",
            description: $"Cannot schedule overlapping work orders for time slot '{startAtUtc:O}' - '{endAtUtc:O}'.");
    }

    public static class Scheduling
    {
        public static Error TechnicianDoubleBooked(Guid laborId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc) => Error.Conflict(
            code: "ApplicationErrors.Scheduling.TechnicianDoubleBooked",
            description: $"Technician '{laborId}' is double-booked for time slot '{startAtUtc:O}' - '{endAtUtc:O}'.");

        public static Error ServiceBayDoubleBooked(string spot, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc) => Error.Conflict(
            code: "ApplicationErrors.Scheduling.ServiceBayDoubleBooked",
            description: $"Service bay '{spot}' is double-booked for time slot '{startAtUtc:O}' - '{endAtUtc:O}'.");

        public static Error VehicleSchedulingConflict(Guid vehicleId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc) => Error.Conflict(
            code: "ApplicationErrors.Scheduling.VehicleSchedulingConflict",
            description: $"Vehicle '{vehicleId}' has a scheduling conflict for time slot '{startAtUtc:O}' - '{endAtUtc:O}'.");

        public static Error OutsideOperatingHours(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc) => Error.Validation(
            code: "ApplicationErrors.Scheduling.OutsideOperatingHours",
            description: $"Requested schedule '{startAtUtc:O}' - '{endAtUtc:O}' is outside operating hours.");
    }

    public static class Customer
    {
        public static Error NotFound(Guid customerId) => Error.NotFound(
            code: "ApplicationErrors.Customer.NotFound",
            description: $"Customer '{customerId}' was not found.");
    }

    public static class Vehicle
    {
        public static Error NotFound(Guid vehicleId) => Error.NotFound(
            code: "ApplicationErrors.Vehicle.NotFound",
            description: $"Vehicle '{vehicleId}' was not found.");
    }

    public static class RepairTask
    {
        public static Error NotFound(Guid repairTaskId) => Error.NotFound(
            code: "ApplicationErrors.RepairTask.NotFound",
            description: $"RepairTask '{repairTaskId}' was not found.");
    }

    public static class Invoice
    {
        public static Error NotFound(Guid invoiceId) => Error.NotFound(
            code: "ApplicationErrors.Invoice.NotFound",
            description: $"Invoice '{invoiceId}' was not found.");

        public static Error CannotGenerateUnlessWorkOrderCompleted(Guid workOrderId, string currentStatus) => Error.Conflict(
            code: "ApplicationErrors.Invoice.CannotGenerateUnlessWorkOrderCompleted",
            description: $"Cannot generate invoice for WorkOrder '{workOrderId}' because current status is '{currentStatus}', not 'Completed'.");

        public static readonly Error GenerationFailed = Error.Failure(
            code: "ApplicationErrors.Invoice.GenerationFailed",
            description: "Failed to generate invoice due to an internal processing error.");
    }

    public static class Auth
    {
        public static readonly Error InvalidRefreshToken = Error.Validation(
            code: "ApplicationErrors.Auth.InvalidRefreshToken",
            description: "Refresh token is invalid.");

        public static readonly Error ExpiredAccessToken = Error.Validation(
            code: "ApplicationErrors.Auth.ExpiredAccessToken",
            description: "Access token has expired.");

        public static Error RefreshTokenExpired(DateTimeOffset expiredAtUtc) => Error.Validation(
            code: "ApplicationErrors.Auth.RefreshTokenExpired",
            description: $"Refresh token expired at '{expiredAtUtc:O}'.");

        public static Error UserNotFound(Guid userId) => Error.NotFound(
            code: "ApplicationErrors.Auth.UserNotFound",
            description: $"User '{userId}' was not found.");

        public static readonly Error TokenGenerationFailure = Error.Failure(
            code: "ApplicationErrors.Auth.TokenGenerationFailure",
            description: "Failed to generate authentication token.");

        public static Error InvalidUserIdClaim(string? claimValue) => Error.Validation(
            code: "ApplicationErrors.Auth.InvalidUserIdClaim",
            description: string.IsNullOrWhiteSpace(claimValue)
                ? "UserId claim is missing or empty."
                : $"UserId claim '{claimValue}' is invalid.");
    }
}

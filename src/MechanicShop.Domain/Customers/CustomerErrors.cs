using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers;

public static class CustomerErrors
{
    public static Error NameRequired =>
        Error.Validation("Customer_Name_Required", "Customer name is required");

    public static Error PhoneNumberRequired =>
        Error.Validation("Customer_Number_Required", "Phone number is required");

    public static Error EmailRequired =>
        Error.Validation("Customer_Email_Required", "Email is required");

    public static Error EmailInvalid =>
      Error.Validation("Customer_Email_Invalid", "Email is invalid");

    public static Error CustomerExists =>
        Error.Conflict("Customer_Email_Exists", "A customer with this email already exists.");

    public static readonly Error InvalidPhoneNumber =
        Error.Validation("Customer_InvalidPhoneNumber", "Phone number must be 7–15 digits and may start with '+'.");

    public static Error VehicleRequired =>
        Error.Validation("Customer_Vehicle_Required", "Vehicle is required.");

    public static Error DuplicateVehicle =>
        Error.Conflict("Customer_Vehicle_Duplicate", "Vehicle already exists for this customer.");

    public static Error VehicleNotFound =>
        Error.Conflict("Customer_Vehicle_NotFound", "Vehicle was not found for this customer.");

    public static readonly Error CannotDeleteCustomerWithWorkOrders =
        Error.Conflict("Customer.CannotDelete", "Customer cannot be deleted due to existing work orders.");
}
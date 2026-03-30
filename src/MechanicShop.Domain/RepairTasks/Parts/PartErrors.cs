using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks.Parts;

public static class PartErrors
{
    public static Error NameRequired => Error.Validation(
        code: "PartErrors.NameRequired",
        description: "Part name is required.");

    public static Error CostInvalid => Error.Validation(
        code: "PartErrors.CostInvalid",
        description: "Part cost must be greater than 0.");

    public static Error QuantityInvalid => Error.Validation(
        code: "PartErrors.QuantityInvalid",
        description: "Part quantity must be greater than 0.");
}

using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks;

public static class RepairTaskErrors
{
	public static Error NameRequired => Error.Validation(
		code: "RepairTaskErrors.NameRequired",
		description: "Repair task name is required.");

	public static Error LaborCostInvalid => Error.Validation(
		code: "RepairTaskErrors.LaborCostInvalid",
		description: "Labor cost must be greater than 0 and less than or equal to 10000.");

	public static Error EstimatedDurationInvalid => Error.Validation(
		code: "RepairTaskErrors.EstimatedDurationInvalid",
		description: "Estimated duration value is invalid.");

	public static Error PartsRequired => Error.Validation(
		code: "RepairTaskErrors.PartsRequired",
		description: "Parts collection is required.");

	public static Error DuplicatePart => Error.Conflict(
		code: "RepairTaskErrors.DuplicatePart",
		description: "Duplicate part is not allowed for the repair task.");

    public static Error PartNotFound => Error.Conflict(
        code: "RepairTaskErrors.PartNotFound",
        description: "Part was not found in the repair task.");

        public static Error PartNameRequired =>
        Error.Validation("RepairTask.Parts.Name.Required", "All parts must have a name.");

    public static Error AtLeastOneRepairTaskIsRequired =>
          Error.Validation(
              code: "RepairTask.Required",
              description: "At least one repair task must be specified.");

    public static Error InUse =>
    Error.Conflict("RepairTask.InUse", "Cannot delete a repair task that is used in work orders.");

    public static Error DuplicateName =>

    Error.Conflict("RepairTaskPart.Duplicate", "A part with the same name already exists in this repair task.");
        
}

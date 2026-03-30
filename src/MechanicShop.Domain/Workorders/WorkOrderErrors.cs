using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Domain.WorkOrders;

public static class WorkOrderErrors
{
	public static Error WorkOrderIdRequired => Error.Validation(
		code: "WorkOrderErrors.WorkOrderIdRequired",
		description: "WorkOrder Id is required");

	public static Error VehicleIdRequired => Error.Validation(
		code: "WorkOrderErrors.VehicleIdRequired",
		description: "Vehicle Id is required");

	public static Error LaborIdRequired => Error.Validation(
		code: "WorkOrderErrors.LaborIdRequired",
		description: "Labor Id is required");

	public static Error RepairTaskRequired => Error.Validation(
		code: "WorkOrderErrors.RepairTaskRequired",
		description: "At least one RepairTask is required");

	public static Error SpotInvalid => Error.Validation(
		code: "WorkOrderErrors.SpotInvalid",
		description: "Invalid Spot value");

	public static Error EndTimeMustBeAfterStartTime => Error.Conflict(
		code: "WorkOrderErrors.EndTimeMustBeAfterStartTime",
		description: "End time must be after start time");

	public static Error WorkOrderNotEditable => Error.Conflict(
		code: "WorkOrderErrors.WorkOrderNotEditable",
		description: "WorkOrder cannot be modified when it is InProgress, Completed, or Cancelled");

	public static Error TimingModificationNotAllowed => Error.Conflict(
		code: "WorkOrderErrors.TimingModificationNotAllowed",
		description: "Cannot modify timing when WorkOrder is not editable");

	public static Error EmptyLaborAssignment => Error.Conflict(
		code: "WorkOrderErrors.EmptyLaborAssignment",
		description: "Cannot assign empty LaborId");

	public static Error DuplicateRepairTask => Error.Conflict(
		code: "WorkOrderErrors.DuplicateRepairTask",
		description: "Cannot add duplicate RepairTask");

	public static Error CannotTransitionScheduledToCompleted => Error.Conflict(
		code: "WorkOrderErrors.CannotTransitionScheduledToCompleted",
		description: "Cannot transition directly from Scheduled to Completed");

	public static Error CannotDeleteInProgress => Error.Conflict(
		code: "WorkOrderErrors.CannotDeleteInProgress",
		description: "Cannot delete WorkOrder when it is InProgress");

	public static Error InvalidStateTransition(WorkOrderState currentState, WorkOrderState nextState, Guid? workOrderId = null)
		=> Error.Conflict(
			code: "WorkOrderErrors.InvalidStateTransition",
			description: workOrderId.HasValue
				? $"Invalid state transition for WorkOrder '{workOrderId.Value}' from '{currentState}' to '{nextState}'."
				: $"Invalid state transition from '{currentState}' to '{nextState}'.");

	public static Error TransitionBeforeScheduledStart(DateTimeOffset scheduledStartAtUtc, Guid? workOrderId = null)
		=> Error.Conflict(
			code: "WorkOrderErrors.TransitionBeforeScheduledStart",
			description: workOrderId.HasValue
				? $"State transition is not allowed for WorkOrder '{workOrderId.Value}' before scheduled start time '{scheduledStartAtUtc:O}'."
				: $"State transition is not allowed before scheduled start time '{scheduledStartAtUtc:O}'.");

	public static Error TechnicianDoubleBooked(Guid laborId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, Guid? workOrderId = null)
		=> Error.Conflict(
			code: "WorkOrderErrors.TechnicianDoubleBooked",
			description: workOrderId.HasValue
				? $"Technician '{laborId}' cannot be double-booked for WorkOrder '{workOrderId.Value}' in the time slot '{startAtUtc:O}' - '{endAtUtc:O}'."
				: $"Technician '{laborId}' cannot be double-booked in the time slot '{startAtUtc:O}' - '{endAtUtc:O}'.");

	public static Error SpotDoubleBooked(Spot spot, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, Guid? workOrderId = null)
		=> Error.Conflict(
			code: "WorkOrderErrors.SpotDoubleBooked",
			description: workOrderId.HasValue
				? $"Service bay '{spot}' cannot be double-booked for WorkOrder '{workOrderId.Value}' in the time slot '{startAtUtc:O}' - '{endAtUtc:O}'."
				: $"Service bay '{spot}' cannot be double-booked in the time slot '{startAtUtc:O}' - '{endAtUtc:O}'.");

	public static Error WorkOrderNotEditableForId(Guid workOrderId)
		=> Error.Conflict(
			code: "WorkOrderErrors.WorkOrderNotEditableForId",
			description: $"WorkOrder '{workOrderId}' cannot be modified when it is InProgress, Completed, or Cancelled.");
}

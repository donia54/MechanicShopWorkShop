using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;


namespace MechanicShop.Domain.WorkOrders;

public sealed class WorkOrder : AuditableEntity
{
	public Guid VehicleId { get; }

	public DateTimeOffset StartAtUtc { get; private set; }

	public DateTimeOffset? EndAtUtc { get; private set; }

	public Guid LaborId { get; private set; }

    public Spot Spot { get; private set; }

	public WorkOrderState State { get; private set; }

    public Employee? Labor { get; private set; }

    public Vehicle? Vehicle { get; private set; }

    public Invoice? Invoice { get; private set; }

	public decimal? Discount { get; private set; }

	public decimal? Tax { get; private set; }

    public bool IsEditable => State == WorkOrderState.Scheduled;

    private readonly List<RepairTask> _repairTasks = [];

    public decimal TotalPartsCost => _repairTasks.Sum(rt => rt.Parts.Sum(p => p.Cost * p.Quantity));
    public decimal TotalLaborCost => _repairTasks.Sum(rt => rt.LaborCost);
    public decimal Total => TotalPartsCost + TotalLaborCost;

    public IReadOnlyList<RepairTask> RepairTasks => _repairTasks.AsReadOnly();


    private WorkOrder()
    { }

    private WorkOrder(
        Guid id,
        Guid vehicleId,
        Guid laborId,
        Spot spot,
        DateTimeOffset startAtUtc,
        DateTimeOffset? endAtUtc,
        WorkOrderState state,
        IEnumerable<RepairTask> repairTasks)
        : base(id)
    {
        VehicleId = vehicleId;
        LaborId = laborId;
        Spot = spot;
        StartAtUtc = startAtUtc;
        EndAtUtc = endAtUtc;
        State = state;
        _repairTasks.AddRange(repairTasks);
    }

    public static Result<WorkOrder> Create(
        Guid id,
        Guid vehicleId,
        Guid laborId,
        Spot spot,
        DateTimeOffset startAtUtc,
        DateTimeOffset? endAtUtc,
        WorkOrderState state = WorkOrderState.Scheduled,
        IEnumerable<RepairTask>? repairTasks = null)
    {
        if (id == Guid.Empty)
        {
            return WorkOrderErrors.WorkOrderIdRequired;
        }

        if (vehicleId == Guid.Empty)
        {
            return WorkOrderErrors.VehicleIdRequired;
        }

        if (laborId == Guid.Empty)
        {
            return WorkOrderErrors.LaborIdRequired;
        }

        if (!Enum.IsDefined(spot))
        {
            return WorkOrderErrors.SpotInvalid;
        }

        if (!Enum.IsDefined(state))
        {
            return WorkOrderErrors.InvalidStateTransition(WorkOrderState.Scheduled, state, id);
        }

        if (endAtUtc.HasValue && endAtUtc.Value <= startAtUtc)
        {
            return WorkOrderErrors.EndTimeMustBeAfterStartTime;
        }

        if (state == WorkOrderState.Completed)
        {
            return WorkOrderErrors.CannotTransitionScheduledToCompleted;
        }

        if (state == WorkOrderState.InProgress && DateTimeOffset.UtcNow < startAtUtc)
        {
            return WorkOrderErrors.TransitionBeforeScheduledStart(startAtUtc, id);
        }

        if (repairTasks is null)
        {
            return WorkOrderErrors.RepairTaskRequired;
        }

        var tasks = repairTasks.ToList();

        if (tasks.Count == 0)
        {
            return WorkOrderErrors.RepairTaskRequired;
        }

        if (tasks.GroupBy(task => task.Id).Any(group => group.Count() > 1))
        {
            return WorkOrderErrors.DuplicateRepairTask;
        }

        return new WorkOrder(id, vehicleId, laborId, spot, startAtUtc, endAtUtc, state, tasks);
    }

    public Result<Updated> AddRepairTask(RepairTask task)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.WorkOrderNotEditableForId(Id);
        }

        if (_repairTasks.Any(existingTask => existingTask.Id == task.Id))
        {
            return WorkOrderErrors.DuplicateRepairTask;
        }

        _repairTasks.Add(task);
        return Result.Updated;
    }

    public Result<Updated> UpdateTiming(DateTimeOffset startAtUtc, DateTimeOffset? endAtUtc)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.TimingModificationNotAllowed;
        }

        if (endAtUtc.HasValue && endAtUtc.Value <= startAtUtc)
        {
            return WorkOrderErrors.EndTimeMustBeAfterStartTime;
        }

        StartAtUtc = startAtUtc;
        EndAtUtc = endAtUtc;
        return Result.Updated;
    }

    public Result<Updated> UpdateLabor(Guid laborId)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.WorkOrderNotEditableForId(Id);
        }

        if (laborId == Guid.Empty)
        {
            return WorkOrderErrors.EmptyLaborAssignment;
        }

        LaborId = laborId;
        return Result.Updated;
    }

    public Result<Updated> UpdateSpot(Spot spot)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.WorkOrderNotEditableForId(Id);
        }

        if (!Enum.IsDefined(spot))
        {
            return WorkOrderErrors.SpotInvalid;
        }

        Spot = spot;
        return Result.Updated;
    }

    public Result<Updated> ClearRepairTasks()
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.WorkOrderNotEditableForId(Id);
        }

        _repairTasks.Clear();
        return Result.Updated;
    }

    public Result<Updated> UpdateState(WorkOrderState newState)
    {
        if (!CanTransitionTo(newState))
        {
            return WorkOrderErrors.InvalidStateTransition(State, newState, Id);
        }

        if (State == WorkOrderState.Scheduled &&
            newState == WorkOrderState.InProgress &&
            DateTimeOffset.UtcNow < StartAtUtc)
        {
            return WorkOrderErrors.TransitionBeforeScheduledStart(StartAtUtc, Id);
        }

        State = newState;
        return Result.Updated;
    }

    public bool CanTransitionTo(WorkOrderState newState)
        => State switch
        {
            WorkOrderState.Scheduled =>
                newState is WorkOrderState.InProgress or WorkOrderState.Cancelled,
            WorkOrderState.InProgress =>
                newState is WorkOrderState.Completed or WorkOrderState.Cancelled,
            WorkOrderState.Completed => false,
            WorkOrderState.Cancelled => false,
            _ => false
        };

    public Result<Updated> Cancel()
    {
        if (State == WorkOrderState.Completed)
        {
            return WorkOrderErrors.InvalidStateTransition(State, WorkOrderState.Cancelled, Id);
        }

        return UpdateState(WorkOrderState.Cancelled);
    }

}

using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Common.Interfaces;

public interface IWorkOrderPolicy
{
    bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration);

    Result<Success> ValidateWorkingHours(DateTimeOffset startAt, DateTimeOffset endAt);

   Result<Success> ValidateNoShow(DateTimeOffset scheduledTime, DateTimeOffset now);

    Task<bool> IsLaborOccupied(Guid laborId, Guid excludedWorkOrderId, DateTimeOffset startAt, DateTimeOffset endAt);

    Task<bool> IsVehicleAlreadyScheduled(Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludedWorkOrderId = null);

    Task<Result<Success>> CheckSpotAvailabilityAsync(Spot spot, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludeWorkOrderId = null, CancellationToken ct = default);

    Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt);

    Task<Result<Success>> ValidateSchedulingAsync(
    Guid laborId,
    Guid vehicleId,
    Spot spot,
    DateTimeOffset startAt,
    DateTimeOffset endAt,
    Guid? excludeWorkOrderId = null,
    CancellationToken ct = default);

   

}
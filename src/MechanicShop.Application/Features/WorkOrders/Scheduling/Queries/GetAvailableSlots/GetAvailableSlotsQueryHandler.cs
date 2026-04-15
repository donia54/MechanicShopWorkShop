using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

using MediatR;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Scheduling.Queries.GetAvailableSlots;

public sealed class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, Result<IReadOnlyList<AvailabilitySlotDto>>>
{
	private const int DayStartHour = 8;
	private const int DayEndHour = 18;
	private static readonly TimeSpan SlotDuration = TimeSpan.FromHours(1);

	private readonly IWorkOrderPolicy _policy;
	private readonly ILogger<GetAvailableSlotsQueryHandler> _logger;

	public GetAvailableSlotsQueryHandler(
		IWorkOrderPolicy policy,
		ILogger<GetAvailableSlotsQueryHandler> logger)
	{
		_policy = policy;
		_logger = logger;
	}

	public async Task<Result<IReadOnlyList<AvailabilitySlotDto>>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting available slots. Date: {Date}, LaborId: {LaborId}", request.Date, request.LaborId);

		var dayUtc = request.Date.ToUniversalTime().Date;
		var slots = new List<AvailabilitySlotDto>();

		for (var hour = DayStartHour; hour < DayEndHour; hour++)
		{
			var startAtUtc = new DateTimeOffset(dayUtc, TimeSpan.Zero).AddHours(hour);
			var endAtUtc = startAtUtc.Add(SlotDuration);

			foreach (var spot in Enum.GetValues<Spot>())
			{
				var isAvailable = await IsSlotAvailableAsync(request.LaborId, spot, startAtUtc, endAtUtc, cancellationToken);

				slots.Add(new AvailabilitySlotDto(
					startAtUtc,
					endAtUtc,
					isAvailable,
					spot.ToString()));
			}
		}

		_logger.LogInformation("Available slots retrieved successfully. Date: {Date}, Count: {Count}", request.Date, slots.Count);
		return slots;
	}

	private async Task<bool> IsSlotAvailableAsync(
		Guid? laborId,
		Spot spot,
		DateTimeOffset startAtUtc,
		DateTimeOffset endAtUtc,
		CancellationToken cancellationToken)
	{
		var workingHoursResult = _policy.ValidateWorkingHours(startAtUtc, endAtUtc);
		if (workingHoursResult.IsError)
		{
			return false;
		}

		var spotAvailabilityResult = await _policy.CheckSpotAvailabilityAsync(
			spot,
			startAtUtc,
			endAtUtc,
			excludeWorkOrderId: null,
			ct: cancellationToken);

		if (spotAvailabilityResult.IsError)
		{
			return false;
		}

		if (!laborId.HasValue)
		{
			return true;
		}

		var isLaborOccupied = await _policy.IsLaborOccupied(
			laborId.Value,
			Guid.Empty,
			startAtUtc,
			endAtUtc);

		return !isLaborOccupied;
	}
}
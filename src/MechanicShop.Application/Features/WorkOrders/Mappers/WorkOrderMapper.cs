using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.RepairTasks.Dtos;
using MechanicShop.Application.Features.WorkOrders.Scheduling.Dtos;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Mappers;

public static class WorkOrderMapper
{
	public static WorkOrderDto ToDto(this WorkOrder workOrder)
	{
		return new WorkOrderDto(
			workOrder.Id,
			workOrder.VehicleId,
			workOrder.LaborId,
			workOrder.Spot.ToString(),
			workOrder.State.ToString(),
			workOrder.StartAtUtc,
			workOrder.EndAtUtc,
			workOrder.Total);
	}

	public static WorkOrderListItemDto ToListItemDto(this WorkOrder workOrder)
	{
		return new WorkOrderListItemDto(
			workOrder.Id,
			workOrder.VehicleId,
			workOrder.LaborId,
			workOrder.Spot.ToString(),
			workOrder.State.ToString(),
			workOrder.StartAtUtc,
			workOrder.EndAtUtc,
			workOrder.Total);
	}

	public static WorkOrderDetailsDto ToDetailsDto(this WorkOrder workOrder)
	{
		return new WorkOrderDetailsDto(
			workOrder.Id,
			workOrder.VehicleId,
			workOrder.Vehicle?.Make ?? string.Empty,
			workOrder.Vehicle?.Model ?? string.Empty,
			workOrder.Vehicle?.Year ?? default,
			workOrder.Vehicle?.LicensePlate ?? string.Empty,
			workOrder.LaborId,
			workOrder.Labor?.FullName ?? string.Empty,
			workOrder.Spot.ToString(),
			workOrder.State.ToString(),
			workOrder.StartAtUtc,
			workOrder.EndAtUtc,
			workOrder.TotalPartsCost,
			workOrder.TotalLaborCost,
			workOrder.Total,
			workOrder.RepairTasks
				.Select(task => new WorkOrderRepairTaskDto(
					task.Id,
					task.Name ?? string.Empty,
					task.LaborCost,
					(int)task.EstimatedDurationInMins,
					task.TotalCost,
					task.Parts
						.Select(part => new WorkOrderPartDto(
							part.Id,
							part.Name,
							part.Cost,
							part.Quantity,
							part.Cost * part.Quantity))
						.ToList()))
				.ToList());
	}

	public static RepairTaskDto ToRepairTaskDto(this RepairTask repairTask)
	{
		return new RepairTaskDto(
			repairTask.Id,
			repairTask.Name ?? string.Empty,
			repairTask.LaborCost,
			(int)repairTask.EstimatedDurationInMins,
			repairTask.Parts
				.Select(part => new PartDto(
					part.Id,
					part.Name,
					part.Cost,
					part.Quantity))
				.ToList());
	}

	public static ScheduleItemDto ToScheduleItemDto(this WorkOrder workOrder)
	{
		return new ScheduleItemDto(
			workOrder.Id,
			workOrder.StartAtUtc,
			workOrder.EndAtUtc,
			workOrder.Labor?.FullName ?? string.Empty,
			workOrder.Vehicle?.VehicleInfo ?? string.Empty,
			workOrder.State.ToString());
	}
}
using MechanicShop.Application.Features.WorkOrders.Dtos;
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
}
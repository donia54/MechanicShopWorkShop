namespace MechanicShop.Application.Features.Dashboard.Dtos;

public sealed record WorkOrderStatsDto(
	int TotalWorkOrders,
	int CompletedWorkOrders,
	int PendingWorkOrders,
	int InProgressWorkOrders,
	int CancelledWorkOrders,
	double AverageCompletionHours,
	decimal TotalRevenue,
	int UniqueVehicles,
	int UniqueCustomers);
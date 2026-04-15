using MechanicShop.Application.Features.WorkOrders.Labor.Dtos;
using MechanicShop.Domain.Employees;

namespace MechanicShop.Application.Features.WorkOrders.Labor.Mapper;

public static class LaborMapper
{
	public static LaborDto ToLaborDto(this Employee employee)
	{
		return new LaborDto(
			employee.Id,
			$"{employee.FirstName} {employee.LastName}");
	}
}
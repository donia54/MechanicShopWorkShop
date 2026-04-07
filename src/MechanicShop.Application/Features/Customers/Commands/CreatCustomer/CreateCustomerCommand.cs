using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.CreatCustomer;

public sealed record CreateCustomerCommand(
	string Name,
	string PhoneNumber,
	string Email,
	List<CreateVehicleInput> Vehicles) : IRequest<Result<CustomerDto>>;

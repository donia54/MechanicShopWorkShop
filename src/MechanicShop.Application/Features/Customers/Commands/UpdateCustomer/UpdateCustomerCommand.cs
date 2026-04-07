using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
	Guid CustomerId,
	string Name,
	string PhoneNumber,
	string Email,
	List<UpdateVehicleInput> Vehicles) : IRequest<Result<Updated>>;
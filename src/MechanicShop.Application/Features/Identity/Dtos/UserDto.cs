namespace MechanicShop.Application.Features.Identity.Dtos;

public sealed record UserDto(
	string Id,
	string Email,
	string Role);
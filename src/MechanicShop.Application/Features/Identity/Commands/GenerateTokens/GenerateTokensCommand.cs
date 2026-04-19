using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.Identity.Commands.GenerateTokens;

public sealed record GenerateTokensCommand(string Email, string Password) : IRequest<Result<TokenResponse>>;
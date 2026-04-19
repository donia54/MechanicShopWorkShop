using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.Identity.Commands.RefreshTokens;

public sealed record RefreshTokensCommand(string RefreshToken) : IRequest<Result<TokenResponse>>;
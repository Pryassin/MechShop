using System.Security.Claims;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

public class TokenProvider : ITokenProvider
{
    public  Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public  ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        throw new NotImplementedException();
    }
}
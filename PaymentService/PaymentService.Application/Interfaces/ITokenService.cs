using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentService.Application.DTOS.Auth;

namespace PaymentService.Application.Interfaces
{
    public interface ITokenService
    {
        Task<TokenDto> GenerateTokensAsync(string userId, IEnumerable<string> roles);
        Task<TokenDto> RefreshTokenAsync(string accessToken, string refreshToken);
        Task RevokeTokenAsync(string userId);
        Task<bool> ValidateAccessTokenAsync(string token);
    }
} 
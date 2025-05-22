using System;
using System.Threading.Tasks;
using IdentityService.Application.Models;

namespace IdentityService.Application.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateTokenAsync(string username, string password);
        Task<TokenResponse> RefreshTokenAsync(string refreshToken);
        Task RevokeTokenAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
    }
} 
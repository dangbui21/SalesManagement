using System;
using System.Threading.Tasks;
using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Interfaces.Auth
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken?> GetByUserIdAsync(string userId);
        Task AddAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task DeleteAsync(Guid id);
    }
} 
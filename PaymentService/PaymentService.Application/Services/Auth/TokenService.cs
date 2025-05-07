using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PaymentService.Application.DTOS.Auth;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces.Auth;

namespace PaymentService.Application.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ITokenValidator _tokenValidator;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtSettings _jwtSettings;

        public TokenService(
            ITokenGenerator tokenGenerator,
            ITokenValidator tokenValidator,
            IRefreshTokenRepository refreshTokenRepository,
            IOptions<JwtSettings> jwtSettings)
        {
            _tokenGenerator = tokenGenerator;
            _tokenValidator = tokenValidator;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<TokenDto> GenerateTokensAsync(string userId, IEnumerable<string> roles)
        {
            // Tạo access token
            var accessToken = _tokenGenerator.GenerateAccessToken(userId, roles);
            
            // Tạo refresh token
            var refreshToken = _tokenGenerator.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            
            // Lưu refresh token vào database
            var existingRefreshToken = await _refreshTokenRepository.GetByUserIdAsync(userId);
            if (existingRefreshToken != null)
            {
                await _refreshTokenRepository.DeleteAsync(existingRefreshToken.Id);
            }
            
            var refreshTokenEntity = RefreshToken.Create(userId, refreshToken, refreshTokenExpiry);
            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            
            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
            };
        }

        public async Task<TokenDto> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            // Xác thực access token (bỏ qua validation về thời gian hết hạn)
            var principal = _tokenValidator.ValidateToken(accessToken, validateLifetime: false);
            if (principal == null)
            {
                throw new InvalidOperationException("Invalid access token");
            }
            
            // Lấy userId từ token
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("User ID not found in token");
            }
            
            // Kiểm tra refresh token có hợp lệ không
            var refreshTokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (refreshTokenEntity == null || !refreshTokenEntity.IsActive || refreshTokenEntity.UserId != userId)
            {
                throw new InvalidOperationException("Invalid refresh token");
            }
            
            // Lấy roles từ token
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            
            // Xóa refresh token cũ
            await _refreshTokenRepository.DeleteAsync(refreshTokenEntity.Id);
            
            // Tạo tokens mới
            return await GenerateTokensAsync(userId, roles);
        }

        public async Task RevokeTokenAsync(string userId)
        {
            var refreshToken = await _refreshTokenRepository.GetByUserIdAsync(userId);
            if (refreshToken != null)
            {
                refreshToken.Revoke();
                await _refreshTokenRepository.UpdateAsync(refreshToken);
            }
        }

        public Task<bool> ValidateAccessTokenAsync(string token)
        {
            try
            {
                var principal = _tokenValidator.ValidateToken(token);
                return Task.FromResult(principal != null);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
} 
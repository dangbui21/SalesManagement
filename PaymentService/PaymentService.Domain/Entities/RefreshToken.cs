using System;

namespace PaymentService.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; }
        public string Token { get; private set; }
        public string UserId { get; private set; }
        public DateTime ExpiryDate { get; private set; }
        public bool IsRevoked { get; private set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        public bool IsActive => !IsRevoked && !IsExpired;

        private RefreshToken() { } // For EF Core

        public static RefreshToken Create(string userId, string token, DateTime expiryDate)
        {
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = token,
                ExpiryDate = expiryDate,
                IsRevoked = false
            };
        }

        public void Revoke()
        {
            IsRevoked = true;
        }
    }
} 
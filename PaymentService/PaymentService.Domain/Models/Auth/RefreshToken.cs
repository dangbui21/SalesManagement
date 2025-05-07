using System.Security.Cryptography;

namespace PaymentService.Domain.Models.Auth
{
    public class RefreshToken
    {
        public string Token { get; private set; } = null!;
        public DateTime ExpiryDate { get; private set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        
    private RefreshToken() { } // For EF Core
    
    public static RefreshToken Create(DateTime expiryDate)
    {
        return new RefreshToken
        {
            Token = GenerateToken(),
                ExpiryDate = expiryDate
        };
    }

    private static string GenerateToken()
    {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
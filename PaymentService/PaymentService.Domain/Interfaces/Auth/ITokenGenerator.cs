using System.Collections.Generic;

namespace PaymentService.Domain.Interfaces.Auth
{
    public interface ITokenGenerator
    {
        string GenerateAccessToken(string userId, IEnumerable<string> roles);
        string GenerateRefreshToken();
    }
}
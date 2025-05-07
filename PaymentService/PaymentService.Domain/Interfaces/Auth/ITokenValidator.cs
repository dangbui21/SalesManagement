using System.Security.Claims;

namespace PaymentService.Domain.Interfaces.Auth
{
    public interface ITokenValidator
    {
        ClaimsPrincipal ValidateToken(string token, bool validateLifetime = true);
    }
}
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOS.Auth;
using PaymentService.Application.Interfaces;

namespace PaymentService.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public AuthController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Trong thực tế cần xác thực người dùng ở đây
            // Bước này giả định người dùng đã được xác thực thành công
            var userId = "test-user-id"; // Trong thực tế lấy từ database
            var roles = new[] { "User" }; // Trong thực tế lấy từ database

            var token = await _tokenService.GenerateTokensAsync(userId, roles);
            return Ok(token);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var token = await _tokenService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
                return Ok(token);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "User ID not found" });
            }

            await _tokenService.RevokeTokenAsync(userId);
            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { message = $"Hello, user {userId}! This is a protected endpoint." });
        }
    }

    // DTO cho login request
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
} 
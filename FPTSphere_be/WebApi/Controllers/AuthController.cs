using BusinessLayer.DTOs;
using BusinessLayer.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// Login với Google OAuth
        [HttpPost("google-login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.IdToken))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Token không hợp lệ"
                    });
                }

                // Process login
                var result = await _authService.LoginWithGoogleAsync(request.IdToken);

                // Return response
                if (!result.Success)
                {
                    _logger.LogWarning($"Login failed: {result.Message}");
                    return Unauthorized(result);
                }

                _logger.LogInformation($"User {result.User?.Email} logged in successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình đăng nhập"
                });
            }
        }

        /// <summary>
        /// Test endpoint để kiểm tra API hoạt động
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                message = "Auth API is working!",
                timestamp = DateTime.Now,
                version = "2.0.0 - Professional Edition"
            });
        }
    }
}

// WebApi/Controllers/TestController.cs
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;

        // ⭐ Inject IEmailService vào constructor
        public TestController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("test-email")]
        [AllowAnonymous]
        public async Task<IActionResult> TestEmail([FromQuery] string toEmail)
        {
            try
            {
                // ⭐ Kiểm tra null trước khi gọi
                if (_emailService == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("EmailService is not configured!"));
                }

                await _emailService.SendEmailAsync(
                    toEmail,
                    "🎉 Test Email từ FPTSphere",
                    @"<html>
                        <body style='font-family: Arial;'>
                            <h1 style='color: #F37021;'>Xin chào!</h1>
                            <p>Đây là email test từ hệ thống <strong>FPTSphere</strong>.</p>
                            <p>Nếu bạn nhận được email này, nghĩa là cấu hình SMTP đã hoạt động! ✅</p>
                        </body>
                    </html>",
                    "Test User"
                );

                return Ok(ApiResponse<object>.SuccessResult(null, $"Email sent to {toEmail}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed: {ex.Message}"));
            }
        }
    }
}
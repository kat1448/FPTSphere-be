using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs;
using DataLayer.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BusinessLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUnitOfWork unitOfWork,
            IGoogleAuthService googleAuthService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _googleAuthService = googleAuthService;
            _configuration = configuration;
        }

        public async Task<LoginResponse> LoginWithGoogleAsync(string idToken)
        {
            try
            {
                // ========== BƯỚC 1: XÁC THỰC GOOGLE TOKEN ==========
                var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(idToken);

                // ========== BƯỚC 2: KIỂM TRA EMAIL TRONG DATABASE ==========
                var user = await _unitOfWork.Users.GetByEmailAsync(googleUser.Email);

                // ========== BƯỚC 3: VALIDATE USER ==========
                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Email không tồn tại trong hệ thống. Vui lòng liên hệ quản trị viên để được cấp quyền truy cập.",
                        Token = null,
                        User = null
                    };
                }

                if (user.IsAuthorized == false)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.",
                        Token = null,
                        User = null
                    };
                }

                // ========== BƯỚC 4: CẬP NHẬT GOOGLE ID ==========
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = googleUser.GoogleId;
                    user.UpdatedAt = DateTime.Now;

                    await _unitOfWork.Users.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync();
                }

                // ========== BƯỚC 5: MANUAL MAPPING (NO AUTOMAPPER!) ==========
                var userInfo = new UserInfo
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    RoleName = user.Role?.RoleName,
                    ClassCode = user.ClassCode
                };

                // ========== BƯỚC 6: TẠO JWT TOKEN ==========
                var token = GenerateJwtToken(userInfo, user.UserId, user.RoleId);

                // ========== BƯỚC 7: TRẢ VỀ RESPONSE ==========
                return new LoginResponse
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Token = token,
                    User = userInfo
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Lỗi đăng nhập: {ex.Message}",
                    Token = null,
                    User = null
                };
            }
        }

        public string GenerateJwtToken(UserInfo userInfo, int userId, int roleId)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
                new Claim(ClaimTypes.Name, userInfo.FullName),
                new Claim(ClaimTypes.Role, userInfo.RoleName ?? "Guest"),
                new Claim("UserId", userId.ToString()),
                new Claim("RoleId", roleId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

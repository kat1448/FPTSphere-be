using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services
{
    public interface IAuthService
    {
        /// Login với Google OAuth
        /// Kiểm tra email trong database và is_authorized
        Task<LoginResponse> LoginWithGoogleAsync(string idToken);

        /// Generate JWT token cho user
        string GenerateJwtToken(UserInfo userInfo, int userId, int roleId);
    }
}

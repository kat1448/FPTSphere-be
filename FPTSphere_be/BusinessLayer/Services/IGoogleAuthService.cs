using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services
{
    public interface IGoogleAuthService
    {
        /// Verify Google ID Token và trả về thông tin user từ Google
        Task<GoogleUserInfo> VerifyGoogleTokenAsync(string idToken);
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 255 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid role ID")]
        public int RoleId { get; set; }

        [StringLength(50, ErrorMessage = "Class code cannot exceed 50 characters")]
        public string? ClassCode { get; set; }
    }

    /// Request to update user information
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 255 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid role ID")]
        public int RoleId { get; set; }

        [StringLength(50, ErrorMessage = "Class code cannot exceed 50 characters")]
        public string? ClassCode { get; set; }

        public bool IsAuthorized { get; set; } = true;
    }

    /// Request to update user role only
    public class UpdateUserRoleRequest
    {
        [Required(ErrorMessage = "Role ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid role ID")]
        public int RoleId { get; set; }
    }

    /// Query parameters for user filtering
    public class UserQueryParameters
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        public int Page { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        public string? Search { get; set; }
        public int? RoleId { get; set; }
        public bool? IsAuthorized { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    /// Full user details response
    public class UserResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? GoogleId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? ClassCode { get; set; }
        public bool IsAuthorized { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// Simplified user response for lists
    public class UserListResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? ClassCode { get; set; }
        public bool IsAuthorized { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// Minimal user info 
    public class UserMinimalResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }

    /// System role response
    public class SystemRoleResponse
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}

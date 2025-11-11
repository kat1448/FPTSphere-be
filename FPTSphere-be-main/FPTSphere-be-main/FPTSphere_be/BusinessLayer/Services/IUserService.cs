using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services
{
    public interface IUserService
    {
        // CRUD OPERATIONS
        Task<PagedResult<UserListResponse>> GetAllUsersAsync(UserQueryParameters parameters);
        Task<UserResponse?> GetUserByIdAsync(int userId);
        Task<UserResponse?> GetUserByEmailAsync(string email);
        Task<UserResponse> CreateUserAsync(CreateUserRequest request);
        Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(int userId);

        // SPECIFIC OPERATIONS
        Task<bool> ToggleAuthorizationAsync(int userId);
        Task<UserResponse> UpdateUserRoleAsync(int userId, UpdateUserRoleRequest request);
        Task<bool> EmailExistsAsync(string email);
        Task<List<UserMinimalResponse>> GetUsersByRoleAsync(int roleId);
        Task<List<UserListResponse>> SearchUsersAsync(string searchTerm);

        // SYSTEM ROLES
        Task<List<SystemRoleResponse>> GetAllRolesAsync();
        Task<SystemRoleResponse?> GetRoleByIdAsync(int roleId);
    }
}
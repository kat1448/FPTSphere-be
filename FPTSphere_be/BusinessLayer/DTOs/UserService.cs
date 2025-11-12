using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.Services;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;

namespace BusinessLayer.DTOs
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        // GET ALL USERS
        public async Task<PagedResult<UserListResponse>> GetAllUsersAsync(UserQueryParameters parameters)
        {
            if (parameters.Page < 1) parameters.Page = 1;
            if (parameters.PageSize < 1) parameters.PageSize = 10;

            var allUsers = (await _unitOfWork.Users.GetAllAsync()).ToList();

            var filteredUsers = allUsers
                .Where(u =>
                    (string.IsNullOrEmpty(parameters.Search) ||
                     u.FullName.Contains(parameters.Search, StringComparison.OrdinalIgnoreCase) ||
                     u.Email.Contains(parameters.Search, StringComparison.OrdinalIgnoreCase)) &&
                    (!parameters.RoleId.HasValue || u.RoleId == parameters.RoleId) &&
                    (!parameters.IsAuthorized.HasValue || u.IsAuthorized == parameters.IsAuthorized))
                .ToList();

            filteredUsers = parameters.SortDescending
                ? filteredUsers.OrderByDescending(u => u.CreatedAt).ToList()
                : filteredUsers.OrderBy(u => u.CreatedAt).ToList();

            var totalRecords = filteredUsers.Count;

            var pagedUsers = filteredUsers
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();

            var mappedUsers = _mapper.Map<List<UserListResponse>>(pagedUsers);

            return new PagedResult<UserListResponse>
            {
                Data = mappedUsers,
                TotalRecords = totalRecords,
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)parameters.PageSize)
            };
        }

        // GET USER BY ID
        public async Task<UserResponse?> GetUserByIdAsync(int userId)
        {
            var allUsers = (await _unitOfWork.Users.GetAllAsync()).ToList();
            var user = allUsers.FirstOrDefault(u => u.UserId == userId);

            if (user == null) return null;

            return _mapper.Map<UserResponse>(user);
        }

        // GET USER BY EMAIL
        public async Task<UserResponse?> GetUserByEmailAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null) return null;

            return _mapper.Map<UserResponse>(user);
        }
        // CREATE USER
        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            if (!await _unitOfWork.SystemRoles.RoleExistsAsync(request.RoleId))
            {
                throw new InvalidOperationException("Role not found");
            }

            var user = _mapper.Map<User>(request);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var createdUser = await _unitOfWork.Users.GetByEmailAsync(user.Email);
            return _mapper.Map<UserResponse>(createdUser!);
        }
        // UPDATE USER
        public async Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            if (!await _unitOfWork.SystemRoles.RoleExistsAsync(request.RoleId))
            {
                throw new InvalidOperationException("Role not found");
            }
            user.FullName = request.FullName;
            user.RoleId = request.RoleId;
            user.ClassCode = request.ClassCode;
            user.IsAuthorized = request.IsAuthorized;
            user.UpdatedAt = DateTime.Now;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var updatedUser = await _unitOfWork.Users.GetByEmailAsync(user.Email);
            return _mapper.Map<UserResponse>(updatedUser!);
        }
        // DELETE USER
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            user.IsAuthorized = false;
            user.UpdatedAt = DateTime.Now;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        // TOGGLE AUTHORIZATION
        public async Task<bool> ToggleAuthorizationAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            user.IsAuthorized = !(user.IsAuthorized ?? true);
            user.UpdatedAt = DateTime.Now;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user.IsAuthorized ?? false;
        }
        // UPDATE USER ROLE
        public async Task<UserResponse> UpdateUserRoleAsync(int userId, UpdateUserRoleRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            if (!await _unitOfWork.SystemRoles.RoleExistsAsync(request.RoleId))
            {
                throw new InvalidOperationException("Role not found");
            }

            user.RoleId = request.RoleId;
            user.UpdatedAt = DateTime.Now;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var updatedUser = await _unitOfWork.Users.GetByEmailAsync(user.Email);
            return _mapper.Map<UserResponse>(updatedUser!);
        }
        // EMAIL EXISTS
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _unitOfWork.Users.EmailExistsAsync(email);
        }

        // GET USERS BY ROLE
        public async Task<List<UserMinimalResponse>> GetUsersByRoleAsync(int roleId)
        {
            var users = await _unitOfWork.Users.GetUsersByRoleAsync(roleId);
            return _mapper.Map<List<UserMinimalResponse>>(users);
        }
        // SEARCH USERS
        public async Task<List<UserListResponse>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<UserListResponse>();
            }

            var users = await _unitOfWork.Users.SearchUsersAsync(searchTerm);
            return _mapper.Map<List<UserListResponse>>(users);
        }
        // GET ALL ROLES
        public async Task<List<SystemRoleResponse>> GetAllRolesAsync()
        {
            var roles = await _unitOfWork.SystemRoles.GetAllActiveRolesAsync();
            return _mapper.Map<List<SystemRoleResponse>>(roles);
        }

        // GET ROLE BY ID
        public async Task<SystemRoleResponse?> GetRoleByIdAsync(int roleId)
        {
            var role = await _unitOfWork.SystemRoles.GetByIdAsync(roleId);
            if (role == null) return null;

            return _mapper.Map<SystemRoleResponse>(role);
        }
    }
}

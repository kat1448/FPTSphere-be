using BusinessLayer.DTOs;
using BusinessLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET ALL USERS
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryParameters parameters)
        {
            try
            {
                var result = await _userService.GetAllUsersAsync(parameters);
                return Ok(ApiResponse<PagedResult<UserListResponse>>.SuccessResult(
                    result,
                    $"Retrieved {result.Data.Count} users successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to retrieve users",
                    new List<string> { ex.Message }));
            }
        }

        // GET USER BY ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"User with ID {id} not found"));
                }

                return Ok(ApiResponse<UserResponse>.SuccessResult(user, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to retrieve user",
                    new List<string> { ex.Message }));
            }
        }


        // GET USER BY EMAIL
        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"User with email {email} not found"));
                }

                return Ok(ApiResponse<UserResponse>.SuccessResult(user, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to retrieve user",
                    new List<string> { ex.Message }));
            }
        }
        // GET USERS BY ROLE
        [HttpGet("role/{roleId}")]
        [Authorize]
        public async Task<IActionResult> GetUsersByRole(int roleId)
        {
            try
            {
                var users = await _userService.GetUsersByRoleAsync(roleId);
                return Ok(ApiResponse<List<UserMinimalResponse>>.SuccessResult(
                    users,
                    $"Retrieved {users.Count} users with role ID {roleId}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to retrieve users by role",
                    new List<string> { ex.Message }));
            }
        }

        // SEARCH USERS
        [HttpGet("search")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<List<UserListResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Search term is required"));
                }

                var users = await _userService.SearchUsersAsync(searchTerm);
                return Ok(ApiResponse<List<UserListResponse>>.SuccessResult(
                    users,
                    $"Found {users.Count} users matching '{searchTerm}'"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to search users",
                    new List<string> { ex.Message }));
            }
        }

        // CREATE USER
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<object>.ErrorResult("Validation failed", errors));
                }

                var user = await _userService.CreateUserAsync(request);

                return CreatedAtAction(
                    nameof(GetUserById),
                    new { id = user.UserId },
                    ApiResponse<UserResponse>.SuccessResult(user, "User created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to create user",
                    new List<string> { ex.Message }));
            }
        }

        // UPDATE USER
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<object>.ErrorResult("Validation failed", errors));
                }

                var user = await _userService.UpdateUserAsync(id, request);
                return Ok(ApiResponse<UserResponse>.SuccessResult(user, "User updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
                }
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to update user",
                    new List<string> { ex.Message }));
            }
        }

        // UPDATE USER ROLE
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<object>.ErrorResult("Validation failed", errors));
                }

                var user = await _userService.UpdateUserRoleAsync(id, request);
                return Ok(ApiResponse<UserResponse>.SuccessResult(user, "User role updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
                }
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to update user role",
                    new List<string> { ex.Message }));
            }
        }

        // TOGGLE AUTHORIZATION
        [HttpPut("{id}/toggle-authorization")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleAuthorization(int id)
        {
            try
            {
                var isAuthorized = await _userService.ToggleAuthorizationAsync(id);
                var message = isAuthorized
                    ? $"User ID {id} has been activated"
                    : $"User ID {id} has been deactivated";

                return Ok(ApiResponse<bool>.SuccessResult(isAuthorized, message));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to toggle authorization",
                    new List<string> { ex.Message }));
            }
        }
        // DELETE USER (SOFT DELETE)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                return Ok(ApiResponse<bool>.SuccessResult(result, $"User ID {id} deactivated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to delete user",
                    new List<string> { ex.Message }));
            }
        }

        // GET ALL ROLES
        [HttpGet("roles")]
        [Authorize]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _userService.GetAllRolesAsync();
                return Ok(ApiResponse<List<SystemRoleResponse>>.SuccessResult(
                    roles,
                    $"Retrieved {roles.Count} roles successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to retrieve roles",
                    new List<string> { ex.Message }));
            }
        }
        // CHECK EMAIL EXISTS
        [HttpGet("check-email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            try
            {
                var exists = await _userService.EmailExistsAsync(email);
                var message = exists
                    ? $"Email '{email}' already exists"
                    : $"Email '{email}' is available";

                return Ok(ApiResponse<bool>.SuccessResult(exists, message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to check email",
                    new List<string> { ex.Message }));
            }
        }

        // GET CURRENT USER
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userEmail = User.FindFirst("email")?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Invalid token"));
                }

                var user = await _userService.GetUserByEmailAsync(userEmail);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("User not found"));
                }

                return Ok(ApiResponse<UserResponse>.SuccessResult(user, "Current user retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Failed to retrieve current user",
                    new List<string> { ex.Message }));
            }
        }
    }
}

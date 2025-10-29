using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs.User;

namespace BusinessLayer.Services.Interfaces
{
    public interface IUserService
    {
        /// <summary>
/// Retrieves all users.
/// </summary>
/// <returns>A collection of UserDto representing all users; an empty collection if no users exist.</returns>
Task<IEnumerable<UserDto>> GetAllAsync();
        /// <summary>
/// Retrieve a user by its identifier.
/// </summary>
/// <param name="id">The user's identifier.</param>
/// <returns>The <see cref="UserDto"/> for the specified user if found, otherwise null.</returns>
Task<UserDto?> GetByIdAsync(int id);
        /// <summary>
/// Creates a new user from the provided data transfer object.
/// </summary>
/// <param name="dto">The information required to create the user.</param>
/// <returns>The created user's DTO.</returns>
Task<UserDto> CreateAsync(CreateUserDto dto);
        /// <summary>
/// Updates an existing user using the provided DTO.
/// </summary>
/// <param name="dto">Data transfer object containing the user's updated properties; must include the user's identifier.</param>
/// <returns>`true` if the user was updated, `false` otherwise.</returns>
Task<bool> UpdateAsync(UpdateUserDto dto);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
/// Retrieves all users.
/// </summary>
/// <returns>An enumerable containing all <see cref="User"/> entities.</returns>
Task<IEnumerable<User>> GetAllAsync();
        /// <summary>
/// Retrieves a user by its unique identifier.
/// </summary>
/// <param name="id">The user's unique identifier.</param>
/// <returns>The <see cref="User"/> with the specified id, or <c>null</c> if no matching user exists.</returns>
Task<User?> GetByIdAsync(int id);
        /// <summary>
/// Creates a new user record in the data store.
/// </summary>
/// <param name="user">The user entity to create; properties may be used to populate the stored record.</param>
/// <returns>The created <see cref="User"/> including any generated fields (for example, assigned identifier).</returns>
Task<User> CreateAsync(User user);
        /// <summary>
/// Updates an existing User record.
/// </summary>
/// <param name="user">The User entity containing updated values; its identifier is used to locate the record to update.</param>
/// <returns><c>true</c> if the user was successfully updated, <c>false</c> otherwise.</returns>
Task<bool> UpdateAsync(User user);
        /// <summary>
/// Deletes the user with the specified identifier.
/// </summary>
/// <param name="id">The identifier of the user to delete.</param>
/// <returns>`true` if a user was deleted, `false` otherwise.</returns>
Task<bool> DeleteAsync(int id);
    }
}
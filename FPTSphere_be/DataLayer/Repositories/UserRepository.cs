using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly EventDbContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="UserRepository"/> using the provided <see cref="EventDbContext"/>.
        /// </summary>
        public UserRepository(EventDbContext context)
        {
            _context = context;
        }

        /// <summary>
            /// Retrieves all User entities from the database.
            /// </summary>
            /// <returns>An IEnumerable&lt;User&gt; containing all users.</returns>
            public async Task<IEnumerable<User>> GetAllAsync() =>
            await _context.Users.ToListAsync();

        /// <summary>
            /// Retrieves the user with the specified UserId.
            /// </summary>
            /// <param name="id">The UserId of the user to retrieve.</param>
            /// <returns>The matching <see cref="User"/>, or <c>null</c> if no user has the specified UserId.</returns>
            public async Task<User?> GetByIdAsync(int id) =>
            await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

        /// <summary>
        /// Adds the provided User to the repository and persists it to the database.
        /// </summary>
        /// <param name="user">The User entity to create.</param>
        /// <returns>The created User with any database-generated fields populated (for example, the UserId).</returns>
        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Updates an existing User entity in the database using the provided user's values.
        /// </summary>
        /// <param name="user">User instance whose UserId identifies the record to update; other properties provide the new values.</param>
        /// <returns>`true` if an existing user was found and updated, `false` if no matching user exists.</returns>
        public async Task<bool> UpdateAsync(User user)
        {
            var existing = await _context.Users.FindAsync(user.UserId);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes the user with the specified id from the data store.
        /// </summary>
        /// <param name="id">The identifier of the user to delete.</param>
        /// <returns>`true` if a user with the given id was found and deleted, `false` otherwise.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Users.FindAsync(id);
            if (existing == null) return false;

            _context.Users.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
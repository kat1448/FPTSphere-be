using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(EventDbContext context) : base(context)
        {
        }

        // Override GetByIdAsync to always load Role navigation property
        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Role)

                // Old code:
                // .FirstOrDefaultAsync(u => u.Email == email);

                // Fixed:
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }
        /// Get users by role
        public async Task<List<User>> GetUsersByRoleAsync(int roleId)
        {
            return await _dbSet
                .Include(u => u.Role)
                .Where(u => u.RoleId == roleId && u.IsAuthorized == true)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        /// Search users by name or email
        public async Task<List<User>> SearchUsersAsync(string searchTerm)
        {
            var lowerSearchTerm = searchTerm.ToLower();

            return await _dbSet
                .Include(u => u.Role)
                .Where(u =>
                    u.FullName.ToLower().Contains(lowerSearchTerm) ||
                    u.Email.ToLower().Contains(lowerSearchTerm))
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        /// Get authorized users only
        public async Task<List<User>> GetAuthorizedUsersAsync()
        {
            return await _dbSet
                .Include(u => u.Role)
                .Where(u => u.IsAuthorized == true)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        /// Override GetAllAsync to always include Role navigation property
        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbSet
                .Include(u => u.Role)
                .ToListAsync();
        }
    }
}

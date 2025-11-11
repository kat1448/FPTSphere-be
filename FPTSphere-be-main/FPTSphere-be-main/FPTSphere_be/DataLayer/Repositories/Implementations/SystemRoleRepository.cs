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
    public class SystemRoleRepository : Repository<SystemRole>, ISystemRoleRepository
    {
        public SystemRoleRepository(EventDbContext context) : base(context)
        {
        }
        /// Get role by name
        public async Task<SystemRole?> GetRoleByNameAsync(string roleName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.RoleName == roleName);
        }

        /// Check if role exists
        public async Task<bool> RoleExistsAsync(int roleId)
        {
            return await _dbSet
                .AnyAsync(r => r.RoleId == roleId);
        }

        /// Get all active roles
        public async Task<List<SystemRole>> GetAllActiveRolesAsync()
        {
            return await _dbSet
                .OrderBy(r => r.RoleId)
                .ToListAsync();
        }
    }
}

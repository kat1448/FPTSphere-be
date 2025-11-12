using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface ISystemRoleRepository : IRepository<SystemRole>
    {
        Task<SystemRole?> GetRoleByNameAsync(string roleName);

        Task<bool> RoleExistsAsync(int roleId);

        Task<List<SystemRole>> GetAllActiveRolesAsync();
    }
}

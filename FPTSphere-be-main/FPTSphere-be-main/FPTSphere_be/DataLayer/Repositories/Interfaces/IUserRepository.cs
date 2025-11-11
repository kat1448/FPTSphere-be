using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByGoogleIdAsync(string googleId);
        Task<bool> EmailExistsAsync(string email);

        Task<List<User>> GetUsersByRoleAsync(int roleId);
        Task<List<User>> SearchUsersAsync(string searchTerm);
        Task<List<User>> GetAuthorizedUsersAsync();

    }
}

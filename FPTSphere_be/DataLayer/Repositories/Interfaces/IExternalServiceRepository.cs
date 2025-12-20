using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface IExternalServiceRepository : IRepository<ExternalService>
    {
        Task<IEnumerable<ExternalService>> GetByEventIdAsync(int eventId);

        Task<ExternalService?> GetByIdWithEventAsync(int serviceId);

        Task<decimal> GetTotalCostByEventIdAsync(int eventId);

        Task<IEnumerable<ExternalService>> GetByProviderNameAsync(string providerName);
    }
}

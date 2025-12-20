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
    public class ExternalServiceRepository : Repository<ExternalService>, IExternalServiceRepository
    {
        public ExternalServiceRepository(EventDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ExternalService>> GetByEventIdAsync(int eventId)
        {
            return await _context.ExternalServices
                .Include(es => es.Event)
                .Where(es => es.EventId == eventId)
                .OrderBy(es => es.ProviderName)
                .ToListAsync();
        }

        public async Task<ExternalService?> GetByIdWithEventAsync(int serviceId)
        {
            return await _context.ExternalServices
                .Include(es => es.Event)
                .FirstOrDefaultAsync(es => es.ServiceId == serviceId);
        }

        public async Task<decimal> GetTotalCostByEventIdAsync(int eventId)
        {
            return await _context.ExternalServices
                .Where(es => es.EventId == eventId && es.Cost.HasValue)
                .SumAsync(es => es.Cost ?? 0);
        }

        public async Task<IEnumerable<ExternalService>> GetByProviderNameAsync(string providerName)
        {
            return await _context.ExternalServices
                .Include(es => es.Event)
                .Where(es => es.ProviderName.Contains(providerName))
                .OrderByDescending(es => es.ServiceId)
                .ToListAsync();
        }
    }
}

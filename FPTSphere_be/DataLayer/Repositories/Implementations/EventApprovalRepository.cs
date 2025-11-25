using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repositories.Implementations
{
    public class EventApprovalRepository : Repository<EventApproval>, IEventApprovalRepository
    {
        public EventApprovalRepository(EventDbContext context) : base(context) { }

        public async Task<List<EventApproval>> GetApprovalsByEventIdAsync(int eventId)
        {
            return await _dbSet
                .Where(a => a.EventId == eventId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<EventApproval?> GetLatestApprovalByEventIdAsync(int eventId)
        {
            return await _dbSet
                .Where(a => a.EventId == eventId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<EventApproval>> GetApprovalsByDirectorIdAsync(int directorId)
        {
            return await _dbSet
                .Where(a => a.DirectorId == directorId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}

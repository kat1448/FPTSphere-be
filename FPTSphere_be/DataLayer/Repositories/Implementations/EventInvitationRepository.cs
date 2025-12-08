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
    public class EventInvitationRepository
        : Repository<EventInvitation>, IEventInvitationRepository
    {
        public EventInvitationRepository(EventDbContext context) : base(context)
        {
        }

        public Task<List<EventInvitation>> GetByEventAsync(int eventId)
        {
            return _dbSet
                .Include(i => i.SentByNavigation)
                .Where(i => i.EventId == eventId)
                .OrderByDescending(i => i.SentAt)
                .ToListAsync();
        }
    }
}

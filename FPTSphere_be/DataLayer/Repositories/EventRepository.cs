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
    public class EventRepository : IEventRepository
    {
        private readonly EventDbContext _context;

        public EventRepository(EventDbContext context)
        {
            _context = context;
        }

        public async Task<Event> CreateAsync(Event entity)
        {
            _context.Events.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<List<Event>> GetAllAsync()
        {
            return await _context.Events
                .Include(e => e.ManagerUser)
                .Include(e => e.ParentEvent)
                .ToListAsync();
        }

        public async Task<Event?> GetByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.ManagerUser)
                .Include(e => e.ParentEvent)
                .FirstOrDefaultAsync(e => e.EventId == id);
        }
    }
}

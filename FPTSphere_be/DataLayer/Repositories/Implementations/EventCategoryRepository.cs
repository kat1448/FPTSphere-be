using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;

namespace DataLayer.Repositories.Implementations
{
    public class EventCategoryRepository : Repository<EventCategory>, IEventCategoryRepository
    {
        public EventCategoryRepository(EventDbContext context) : base(context) { }
    }
}


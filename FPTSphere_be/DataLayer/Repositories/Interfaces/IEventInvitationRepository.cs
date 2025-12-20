using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Repositories.Interfaces
{
    public interface IEventInvitationRepository : IRepository<EventInvitation>
    {
        Task<List<EventInvitation>> GetByEventAsync(int eventId);
    }
}

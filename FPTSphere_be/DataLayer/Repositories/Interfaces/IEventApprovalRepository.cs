using DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repositories.Interfaces
{
    public interface IEventApprovalRepository : IRepository<EventApproval>
    {
        Task<List<EventApproval>> GetApprovalsByEventIdAsync(int eventId);
        Task<EventApproval?> GetLatestApprovalByEventIdAsync(int eventId);
        Task<List<EventApproval>> GetApprovalsByDirectorIdAsync(int directorId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ISystemRoleRepository SystemRoles { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        ILocationRepository Locations { get; }
        IResourceRepository Resources { get; }
        IExternalLocationRepository ExternalLocations { get; }
        IEventStatusRepository EventStatuses { get; }
        IEventRepository Events { get; }
        IExternalServiceRepository ExternalServices { get; }
        IEventResourceRepository EventResources { get; }
        IEventApprovalRepository EventApprovals { get; }
        IEventLogRepository EventLogs { get; }
        IEventInvitationRepository EventInvitations {  get; }

    }
}

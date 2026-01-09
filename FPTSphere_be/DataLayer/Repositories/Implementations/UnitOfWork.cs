using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataLayer.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EventDbContext _context;
        private IDbContextTransaction? _transaction;
        private IUserRepository? _userRepository;
        private ISystemRoleRepository? _systemRoleRepository;
        private ILocationRepository? _locations;
        private IResourceRepository? _resources;
        private IExternalLocationRepository? _externalLocations;
        private IEventStatusRepository? _eventStatuses;
        private IEventRepository? _events;
        private IExternalServiceRepository? _externalServices;
        private IEventResourceRepository? _eventResources;
        private IEventApprovalRepository? _eventApprovals;
        private IEventLogRepository? _eventLogs;
        private IEventInvitationRepository? _eventInvitations;
        private IEventCategoryRepository? _eventCategories;
        private IEventTypeRepository? _eventTypes;

        public UnitOfWork(EventDbContext context)
        {
            _context = context;
        }
        public ILocationRepository Locations => _locations ??= new LocationRepository(_context);
        public IResourceRepository Resources => _resources ??= new ResourceRepository(_context);
        public IExternalLocationRepository ExternalLocations => _externalLocations ??= new ExternalLocationRepository(_context);
        public IEventStatusRepository EventStatuses => _eventStatuses ??= new EventStatusRepository(_context);
        public IEventRepository Events => _events ??= new EventRepository(_context);
        public IExternalServiceRepository ExternalServices => _externalServices ??= new ExternalServiceRepository(_context);
        public IEventResourceRepository EventResources => _eventResources ??= new EventResourceRepository(_context);
        public IEventApprovalRepository EventApprovals => _eventApprovals ??= new EventApprovalRepository(_context);
        public IEventLogRepository EventLogs => _eventLogs ??= new EventLogRepository(_context);
        public IEventInvitationRepository EventInvitations
    => _eventInvitations ??= new EventInvitationRepository(_context);



        // USERS REPOSITORY
        public IUserRepository Users
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new UserRepository(_context);
                }
                return _userRepository;
            }
        }
        // SYSTEM ROLES REPOSITORY (NEW!)
        public ISystemRoleRepository SystemRoles 
        {
            get
            {
                if (_systemRoleRepository == null)
                {
                    _systemRoleRepository = new SystemRoleRepository(_context);
                }
                return _systemRoleRepository;
            }
        }

        private IEventAttendanceRepository? _eventAttendances;
        public IEventAttendanceRepository EventAttendances => _eventAttendances ??= new EventAttendanceRepository(_context);

        public IEventCategoryRepository EventCategories => _eventCategories ??= new EventCategoryRepository(_context);
        public IEventTypeRepository EventTypes => _eventTypes ??= new EventTypeRepository(_context);

        // TRANSACTION MANAGEMENT
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
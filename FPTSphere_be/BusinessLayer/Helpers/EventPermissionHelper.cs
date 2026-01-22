using System;
using System.Threading.Tasks;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;

namespace BusinessLayer.Helpers
{
    /// <summary>
    /// ⭐ Event Permission Helper
    /// Centralized permission checking logic
    /// </summary>
    public class EventPermissionHelper
    {
        private readonly IUnitOfWork _unitOfWork;

        // Role names as constants
        private const string ADMIN_ROLE = "Admin";
        private const string EVENT_MANAGER_ROLE = "Event Manager";
        private const string DIRECTOR_ROLE = "Director";
        // Old code: (no STAFF_ROLE constant)
        // Fixed:
        private const string STAFF_ROLE = "Staff";

        // Status IDs as constants
        private const int DRAFT_STATUS_ID = 1;
        private const int PENDING_STATUS_ID = 2;
        private const int APPROVED_STATUS_ID = 3;
        private const int INPROGRESS_STATUS_ID = 4;
        private const int COMPLETED_STATUS_ID = 5;
        private const int CANCELLED_STATUS_ID = 6;
        private const int REJECTED_STATUS_ID = 7;

        public EventPermissionHelper(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Permission Checking Methods

        /// <summary>
        /// Check if user can modify event (update/delete)
        /// Rules:
        /// - Owner can modify their own events
        /// - Admin can modify any event
        /// - Only DRAFT events can be modified
        /// </summary>
        public async Task<PermissionResult> CanModifyEventAsync(Event ev, int currentUserId)
        {
            // Check if event is in modifiable status
            if (ev.StatusId != DRAFT_STATUS_ID)
            {
                return PermissionResult.Deny(
                    $"Cannot modify events in {GetStatusName(ev.StatusId)} status. Only Draft events can be modified.");
            }

            // Owner can modify
            if (ev.CreatedBy == currentUserId)
                return PermissionResult.Allow();

            // Admin can modify any event
            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (currentUser?.Role?.RoleName == ADMIN_ROLE)
                return PermissionResult.Allow();

            return PermissionResult.Deny("Only the event creator or Admin can modify this event");
        }

        /// <summary>
        /// Check if user can delete event
        /// Rules:
        /// - Owner can delete their own events (if status allows)
        /// - Admin and Event Manager can delete any event (if status allows)
        /// - Cannot delete Completed events or events that have already ended
        /// - Can delete: Draft, Pending, Rejected, Cancelled
        /// - Cannot delete: Approved (if in progress), In Progress, Completed
        /// </summary>
        public async Task<PermissionResult> CanDeleteEventAsync(Event ev, int currentUserId)
        {
            // Cannot delete completed events
            if (ev.StatusId == COMPLETED_STATUS_ID)
            {
                return PermissionResult.Deny("Cannot delete completed events");
            }

            // Cannot delete events that have already ended (unless cancelled/rejected)
            if (ev.EndTime < DateTime.Now && ev.StatusId != CANCELLED_STATUS_ID && ev.StatusId != REJECTED_STATUS_ID)
            {
                return PermissionResult.Deny("Cannot delete events that have already ended");
            }

            // Cannot delete events that are in progress
            if (ev.StatusId == INPROGRESS_STATUS_ID)
            {
                return PermissionResult.Deny("Cannot delete events that are currently in progress");
            }

            // Owner can delete their own events (if status allows)
            if (ev.CreatedBy == currentUserId)
            {
                // Owner can delete: Draft, Pending, Rejected, Cancelled
                if (ev.StatusId == DRAFT_STATUS_ID || 
                    ev.StatusId == PENDING_STATUS_ID || 
                    ev.StatusId == REJECTED_STATUS_ID || 
                    ev.StatusId == CANCELLED_STATUS_ID)
                {
                    return PermissionResult.Allow();
                }
                
                // Owner cannot delete Approved events (must cancel first)
                if (ev.StatusId == APPROVED_STATUS_ID)
                {
                    return PermissionResult.Deny("Cannot delete approved events. Please cancel the event instead.");
                }
            }

            // Admin and Event Manager can delete any event (if status allows)
            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (currentUser == null)
            {
                return PermissionResult.Deny("User not found");
            }

            var roleName = currentUser.Role?.RoleName ?? "";
            
            if (roleName == ADMIN_ROLE || roleName == EVENT_MANAGER_ROLE)
            {
                // Admin/Event Manager can delete: Draft, Pending, Rejected, Cancelled, Approved (if not started)
                if (ev.StatusId == DRAFT_STATUS_ID || 
                    ev.StatusId == PENDING_STATUS_ID || 
                    ev.StatusId == REJECTED_STATUS_ID || 
                    ev.StatusId == CANCELLED_STATUS_ID ||
                    (ev.StatusId == APPROVED_STATUS_ID && ev.StartTime > DateTime.Now))
                {
                    return PermissionResult.Allow();
                }
                
                if (ev.StatusId == APPROVED_STATUS_ID && ev.StartTime <= DateTime.Now)
                {
                    return PermissionResult.Deny("Cannot delete approved events that have started. Please cancel the event instead.");
                }
            }

            return PermissionResult.Deny("You don't have permission to delete this event");
        }

        /// <summary>
        /// Check if user can view event
        /// Rules:
        /// - Approved events: Everyone can view
        /// - Draft/Pending events: Only creator, Admin, Event Manager can view
        /// </summary>
        public async Task<PermissionResult> CanViewEventAsync(Event ev, int currentUserId)
        {
            // Approved events are public
            if (ev.StatusId == APPROVED_STATUS_ID)
                return PermissionResult.Allow();

            // For non-public events, check if user has permission
            if (ev.CreatedBy == currentUserId)
                return PermissionResult.Allow();

            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            var roleName = currentUser?.Role?.RoleName;

            if (roleName == ADMIN_ROLE || roleName == EVENT_MANAGER_ROLE || roleName == DIRECTOR_ROLE)
                return PermissionResult.Allow();

            return PermissionResult.Deny("You don't have permission to view this event");
        }

        /// <summary>
        /// Check if user can approve/reject event
        /// Old code rules:
        /// - Only Director can approve/reject
        /// - Event must be in Pending status
        /// Fixed rules:
        /// - Director can approve any pending event
        /// - Event Manager can approve events created by Staff
        /// - Event must be in Pending status
        /// </summary>
        public async Task<PermissionResult> CanApproveEventAsync(Event ev, int currentUserId)
        {
            // Check status
            if (ev.StatusId != PENDING_STATUS_ID)
            {
                return PermissionResult.Deny(
                    $"Cannot approve events in {GetStatusName(ev.StatusId)} status. Only Pending events can be approved.");
            }

            // Check role
            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            // Old code:
            // if (currentUser?.Role?.RoleName != DIRECTOR_ROLE && currentUser?.Role?.RoleName != ADMIN_ROLE)
            // {
            //     return PermissionResult.Deny("Only Director or Admin can approve events");
            // }
            // return PermissionResult.Allow();
            // Fixed:
            var currentUserRole = currentUser?.Role?.RoleName ?? "";

            // Director can approve any pending event
            if (currentUserRole == DIRECTOR_ROLE)
            {
                return PermissionResult.Allow();
            }

            // Event Manager can approve events created by Staff
            if (currentUserRole == EVENT_MANAGER_ROLE)
            {
                var creator = await _unitOfWork.Users.GetByIdAsync(ev.CreatedBy);
                if (creator?.Role?.RoleName == STAFF_ROLE)
                {
            return PermissionResult.Allow();
                }
            }

            return PermissionResult.Deny("You don't have permission to approve this event");
        }

        /// <summary>
        /// Check if user can submit event for approval
        /// Rules:
        /// - Only creator or Admin can submit
        /// - Event must be in Draft status
        /// </summary>
        public async Task<PermissionResult> CanSubmitForApprovalAsync(Event ev, int currentUserId)
        {
            // Check status
            if (ev.StatusId != DRAFT_STATUS_ID)
            {
                return PermissionResult.Deny("Only Draft events can be submitted for approval");
            }

            // Check permission (creator or admin)
            if (ev.CreatedBy == currentUserId)
                return PermissionResult.Allow();

            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (currentUser?.Role?.RoleName == ADMIN_ROLE)
                return PermissionResult.Allow();

            return PermissionResult.Deny("Only the event creator or Admin can submit for approval");
        }

        /// <summary>
        /// Check if user can cancel event
        /// Rules:
        /// - Only creator, Event Manager, or Admin can cancel
        /// - Cannot cancel already cancelled or completed events
        /// </summary>
        public async Task<PermissionResult> CanCancelEventAsync(Event ev, int currentUserId)
        {
            // Check status
            if (ev.StatusId == CANCELLED_STATUS_ID)
                return PermissionResult.Deny("Event is already cancelled");

            if (ev.EndTime < DateTime.Now)
                return PermissionResult.Deny("Cannot cancel completed events");

            // Check permission
            if (ev.CreatedBy == currentUserId)
                return PermissionResult.Allow();

            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            var roleName = currentUser?.Role?.RoleName;

            if (roleName == ADMIN_ROLE || roleName == EVENT_MANAGER_ROLE)
                return PermissionResult.Allow();

            return PermissionResult.Deny("You don't have permission to cancel this event");
        }

        /// <summary>
        /// Check if user has specific role
        /// </summary>
        public async Task<bool> HasRoleAsync(int userId, string roleName)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return user?.Role?.RoleName == roleName;
        }

        /// <summary>
        /// Check if user is Admin
        /// </summary>
        public async Task<bool> IsAdminAsync(int userId)
        {
            return await HasRoleAsync(userId, ADMIN_ROLE);
        }

        /// <summary>
        /// Check if user is Event Manager
        /// </summary>
        public async Task<bool> IsEventManagerAsync(int userId)
        {
            return await HasRoleAsync(userId, EVENT_MANAGER_ROLE);
        }

        /// <summary>
        /// Check if user is Director
        /// </summary>
        public async Task<bool> IsDirectorAsync(int userId)
        {
            return await HasRoleAsync(userId, DIRECTOR_ROLE);
        }

        /// <summary>
        /// Check if user is Staff
        /// </summary>
        public async Task<bool> IsStaffAsync(int userId)
        {
            return await HasRoleAsync(userId, STAFF_ROLE);
        }

        #endregion

        #region Status Validation Methods

        /// <summary>
        /// Validate if event status allows modification
        /// </summary>
        public PermissionResult ValidateStatusForModification(int statusId)
        {
            if (statusId != DRAFT_STATUS_ID)
            {
                return PermissionResult.Deny(
                    $"Cannot modify events in {GetStatusName(statusId)} status. Only Draft events can be modified.");
            }
            return PermissionResult.Allow();
        }

        /// <summary>
        /// Validate if event can transition to new status
        /// </summary>
        public PermissionResult ValidateStatusTransition(int currentStatusId, int newStatusId)
        {
            // Define allowed transitions
            var allowedTransitions = new[]
            {
                (DRAFT_STATUS_ID, PENDING_STATUS_ID),           // Draft → Pending
                (PENDING_STATUS_ID, APPROVED_STATUS_ID),        // Pending → Approved
                (PENDING_STATUS_ID, REJECTED_STATUS_ID),        // Pending → Rejected
                (DRAFT_STATUS_ID, CANCELLED_STATUS_ID),         // Draft → Cancelled
                (PENDING_STATUS_ID, CANCELLED_STATUS_ID),       // Pending → Cancelled
                (APPROVED_STATUS_ID, CANCELLED_STATUS_ID),      // Approved → Cancelled
                (APPROVED_STATUS_ID, INPROGRESS_STATUS_ID),    // Approved → In Progress
                (INPROGRESS_STATUS_ID, COMPLETED_STATUS_ID),   // In Progress → Completed
                (INPROGRESS_STATUS_ID, CANCELLED_STATUS_ID),   // In Progress → Cancelled
            };

            foreach (var (from, to) in allowedTransitions)
            {
                if (currentStatusId == from && newStatusId == to)
                    return PermissionResult.Allow();
            }

            return PermissionResult.Deny(
                $"Invalid status transition from {GetStatusName(currentStatusId)} to {GetStatusName(newStatusId)}");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get status name by ID
        /// </summary>
        private string GetStatusName(int statusId)
        {
            return statusId switch
            {
                DRAFT_STATUS_ID => "Draft",
                PENDING_STATUS_ID => "Pending Approval",
                APPROVED_STATUS_ID => "Approved",
                INPROGRESS_STATUS_ID => "In Progress",
                COMPLETED_STATUS_ID => "Completed",
                CANCELLED_STATUS_ID => "Cancelled",
                REJECTED_STATUS_ID => "Rejected",
                _ => "Unknown"
            };
        }

        #endregion
    }

    /// <summary>
    /// Permission result object for cleaner permission handling
    /// </summary>
    public class PermissionResult
    {
        public bool IsAllowed { get; private set; }
        public string DenyReason { get; private set; }

        private PermissionResult(bool isAllowed, string denyReason = "")
        {
            IsAllowed = isAllowed;
            DenyReason = denyReason;
        }

        public static PermissionResult Allow() => new PermissionResult(true);

        public static PermissionResult Deny(string reason) => new PermissionResult(false, reason);

        /// <summary>
        /// Throw UnauthorizedAccessException if permission is denied
        /// </summary>
        public void ThrowIfDenied()
        {
            if (!IsAllowed)
                throw new UnauthorizedAccessException(DenyReason);
        }
    }
}
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs.EventApproval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services.Interfaces
{
    public interface IEventApprovalService
    {
        Task<EventDto> SubmitForApprovalAsync(int eventId, SubmitForApprovalDto dto, int currentUserId);
        Task<EventDto> ApproveEventAsync(int eventId, ApproveEventDto dto, int directorId);
        Task<EventDto> RejectEventAsync(int eventId, RejectEventDto dto, int directorId);
        Task<List<PendingApprovalDto>> GetPendingApprovalsAsync();
        Task<List<EventApprovalDto>> GetApprovalHistoryAsync(int eventId);
    }
}

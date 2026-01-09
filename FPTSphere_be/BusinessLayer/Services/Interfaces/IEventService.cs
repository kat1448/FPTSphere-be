using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs.EventApproval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs;
using BusinessLayer.Helpers;

namespace BusinessLayer.Services.Interfaces
{
    public interface IEventService
    {
        Task<PagedResult<EventDto>> GetEventsAsync(int page, int pageSize, EventFilterDto? filter, string sortBy, bool sortDescending);
        Task<EventDto?> GetEventByIdAsync(int id);
        // Old code:
        // Task<EventDto> CreateAsync(CreateEventDto dto, int currentUserId);
        // Fixed:
        Task<EventDto> CreateAsync(CreateEventDto dto, int currentUserId, string? userRole = null);
        Task<EventDto?> UpdateAsync(int id, UpdateEventDto dto, int currentUserId);
        Task<bool> DeleteAsync(int id, int currentUserId);

        //SUb-event
        Task<List<SubEventDto>> GetSubEventsAsync(int parentEventId);
        Task<SubEventDto> CreateSubEventAsync(int parentEventId, CreateSubEventDto dto, int currentUserId);
        Task<SubEventDto?> UpdateSubEventAsync(int subEventId, UpdateSubEventDto dto, int currentUserId);
        Task<bool> DeleteSubEventAsync(int subEventId, int currentUserId);

        Task<List<PublicEventDto>> GetPublicEventsAsync();

        Task<PublicEventDto?> GetPublicEventByIdAsync(int id);
        Task<PublicEventDto?> GetPublicEventWithSubEventsAsync(int id);
        Task<EventStatistics> GetMyEventStatisticsAsync(int currentUserId);
        // ⭐ PHÊ DUYỆT
        Task<EventDto?> SubmitForApprovalAsync(int eventId, int currentUserId);
        Task<List<EventInvitationDto>> SendInvitationsAsync(int eventId, int senderUserId, SendEventInvitationsDto dto);
        
        // Event approval
        Task<List<PendingApprovalDto>> GetPendingApprovalsAsync();
        Task<EventDto> ApproveEventAsync(int eventId, EventDecisionDto dto, int directorId);
        Task<EventDto> RejectEventAsync(int eventId, EventDecisionDto dto, int directorId);
        Task<List<EventApprovalDto>> GetApprovalHistoryAsync(int eventId);
        // Đăng ký sự kiện cho user
        Task<List<EventAttendanceDto>> GetRegisteredEventsAsync(int userId);
        Task<EventAttendanceDto> RegisterEventAsync(int eventId, int userId);
        Task<EventAttendanceDto> CheckinEventAsync(int eventId, int userId);
        Task<EventAttendanceDto> CheckoutEventAsync(int eventId, int userId);
        Task<List<RegisteredEventFullDto>> GetRegisteredEventsFullAsync(int userId);
        Task UnregisterEventAsync(int eventId, int userId);
    }
}

using AutoMapper;
using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs.EventApproval;
using BusinessLayer.Helpers;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Services.Implementations
{
        public class EventService : IEventService
        {
          
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly EventValidationHelper _validationHelper;
        private readonly EventPermissionHelper _permissionHelper;
        private readonly EventFilterHelper _filterHelper;
        private readonly IEmailService _emailService;
        private const int DRAFT_STATUS_ID = 1;
        private const int PENDING_STATUS_ID = 2;
        private const int APPROVED_STATUS_ID = 3;
        private const int INPROGRESS_STATUS_ID = 4;
        private const int COMPLETED_STATUS_ID = 5;
        private const int CANCELLED_STATUS_ID = 6;
        private const int REJECTED_STATUS_ID = 7;

        public EventService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            EventValidationHelper validationHelper,
            EventPermissionHelper permissionHelper,
            IEmailService emailService,
            EventFilterHelper filterHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validationHelper = validationHelper;
            _permissionHelper = permissionHelper;
            _filterHelper = filterHelper;
            _emailService = emailService;
        }

        #region Auto status update

        /// <summary>
        /// Tự động cập nhật trạng thái dựa trên StartTime / EndTime.
        /// Chỉ can thiệp các status: Approved / In Progress / Completed.
        /// Trả về true nếu StatusId thay đổi.
        /// </summary>
        private bool AutoUpdateStatus(Event ev)
        {
            if (ev == null || ev.IsDeleted == true)
                return false;

            // Không tự động động chạm các trạng thái đặc biệt
            if (ev.StatusId == DRAFT_STATUS_ID ||
                ev.StatusId == PENDING_STATUS_ID ||
                ev.StatusId == CANCELLED_STATUS_ID ||
                ev.StatusId == REJECTED_STATUS_ID)
            {
                return false;
            }

            var now = DateTime.Now;
            var oldStatus = ev.StatusId;
            var newStatus = oldStatus;

            if (now < ev.StartTime)
            {
                // Sắp diễn ra (đã được duyệt)
                newStatus = APPROVED_STATUS_ID;
            }
            else if (ev.StartTime <= now && now <= ev.EndTime)
            {
                // Đang diễn ra
                newStatus = INPROGRESS_STATUS_ID;
            }
            else if (now > ev.EndTime)
            {
                // Đã kết thúc
                newStatus = COMPLETED_STATUS_ID;
            }

            if (newStatus != oldStatus)
            {
                ev.StatusId = newStatus;
                ev.UpdatedAt = DateTime.Now;
                return true;
            }

            return false;
        }

          public async Task<List<EventAttendanceDto>> GetRegisteredEventsAsync(int userId)
            {
                var attendances = await _unitOfWork.EventAttendances.GetByUserAsync(userId);
                return _mapper.Map<List<EventAttendanceDto>>(attendances);
            }

        public async Task<List<RegisteredEventFullDto>> GetRegisteredEventsFullAsync(int userId)
        {
            var attendances = await _unitOfWork.EventAttendances.GetByUserWithEventFullAsync(userId);
            var result = new List<RegisteredEventFullDto>();
            foreach (var att in attendances)
            {
                var eventDto = _mapper.Map<EventDto>(att.Event); // Event đã có đầy đủ các quan hệ
                result.Add(new RegisteredEventFullDto
                {
                    AttendanceId = att.AttendanceId,
                    EventId = att.EventId,
                    CheckinAt = att.CheckinAt,
                    CheckoutAt = att.CheckoutAt,
                    Method = att.Method,
                    Event = eventDto
                });
            }
            return result;
        }

        private async Task AutoUpdateAndSaveIfNeededAsync(IEnumerable<Event> events)
        {
            bool changed = false;
            foreach (var ev in events)
            {
                if (AutoUpdateStatus(ev))
                    changed = true;
            }

            if (changed)
                await _unitOfWork.SaveChangesAsync();
        }

        private async Task AutoUpdateAndSaveIfNeededAsync(Event ev)
        {
            if (AutoUpdateStatus(ev))
                await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region CRUD main events

        public async Task<PagedResult<EventDto>> GetEventsAsync(
            int page, int pageSize, EventFilterDto? filter, string sortBy, bool sortDescending)
        {
            (page, pageSize) = _filterHelper.NormalizePagination(page, pageSize);

            var allEvents = await _unitOfWork.Events.GetAllWithDetailsAsync();

            // Auto-update trạng thái
            await AutoUpdateAndSaveIfNeededAsync(allEvents);

            var filtered = _filterHelper.ApplyFilters(allEvents, filter);
            var sorted = _filterHelper.ApplySorting(filtered, sortBy, sortDescending);

            var total = sorted.Count();
            var items = _filterHelper.ApplyPagination(sorted, page, pageSize).ToList();

            return new PagedResult<EventDto>
            {
                Data = _mapper.Map<List<EventDto>>(items),
                TotalRecords = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = _filterHelper.CalculateTotalPages(total, pageSize)
            };
        }

        public async Task<EventDto?> GetEventByIdAsync(int id)
        {
            var ev = await _unitOfWork.Events.GetByIdWithDetailsAsync(id);
            if (ev == null || ev.IsDeleted == true)
                return null;

            await AutoUpdateAndSaveIfNeededAsync(ev);

            return _mapper.Map<EventDto>(ev);
        }

        public async Task<EventDto> CreateAsync(CreateEventDto dto, int currentUserId)
        {
            // ⭐ Main event: không có parent, không cần parentEventId / currentEventId
            var validation = await _validationHelper.ValidateEventDataAsync(
                eventName: dto.EventName,
                startTime: dto.StartTime,
                endTime: dto.EndTime,
                locationId: dto.LocationId,
                externalLocationId: dto.ExternalLocationId,
                expectedAttendees: dto.ExpectedAttendees
            );

            if (!validation.IsSuccess)
                throw new InvalidOperationException(validation.ErrorMessage);

            var ev = _mapper.Map<Event>(dto);
            ev.CreatedBy = currentUserId;
            ev.StatusId = DRAFT_STATUS_ID;
            ev.CreatedAt = DateTime.Now;
            ev.IsDeleted = false;

            await _unitOfWork.Events.AddAsync(ev);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Events.GetByIdWithDetailsAsync(ev.EventId);
            return _mapper.Map<EventDto>(created);
        }

        public async Task<EventDto?> UpdateAsync(int id, UpdateEventDto dto, int currentUserId)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(id);
            if (ev == null) return null;

            var permission = await _permissionHelper.CanModifyEventAsync(ev, currentUserId);
            permission.ThrowIfDenied();

            // ⭐ Main event update: truyền currentEventId để tránh tự conflict chính nó
            var validation = await _validationHelper.ValidateEventDataAsync(
                eventName: dto.EventName,
                startTime: dto.StartTime,
                endTime: dto.EndTime,
                locationId: dto.LocationId,
                externalLocationId: dto.ExternalLocationId,
                expectedAttendees: dto.ExpectedAttendees,
                parentStartTime: null,
                parentEndTime: null,
                parentEventId: null,
                currentEventId: ev.EventId
            );

            if (!validation.IsSuccess)
                throw new InvalidOperationException(validation.ErrorMessage);

            _mapper.Map(dto, ev);
            ev.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(ev);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Events.GetByIdWithDetailsAsync(ev.EventId);

            await AutoUpdateAndSaveIfNeededAsync(updated);

            return _mapper.Map<EventDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id, int currentUserId)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(id);
            if (ev == null || ev.IsDeleted == true) return false;

            var permission = await _permissionHelper.CanDeleteEventAsync(ev, currentUserId);
            permission.ThrowIfDenied();

            ev.IsDeleted = true;
            ev.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(ev);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Sub events

        public async Task<List<SubEventDto>> GetSubEventsAsync(int parentEventId)
        {
            var parent = await _unitOfWork.Events.GetByIdAsync(parentEventId);
            if (parent == null)
                throw new InvalidOperationException("Parent event not found");

            var subEvents = await _unitOfWork.Events.GetSubEventsByParentIdAsync(parentEventId);

            await AutoUpdateAndSaveIfNeededAsync(subEvents);

            return _mapper.Map<List<SubEventDto>>(subEvents);
        }

        public async Task<SubEventDto> CreateSubEventAsync(
            int parentEventId, CreateSubEventDto dto, int currentUserId)
        {
            var parent = await _unitOfWork.Events.GetByIdWithDetailsAsync(parentEventId);
            if (parent == null)
                throw new InvalidOperationException("Parent event not found");

            if (parent.ParentEventId != null)
                throw new InvalidOperationException("Cannot create sub-event under another sub-event");

            // ⭐ Sub-event tạo mới:
            //  - Phải nằm trong khung giờ parent (parent.StartTime / EndTime)
            //  - Check trùng phòng nhưng BỎ QUA chính parentEvent (parentEventId = parentEventId)
            var validation = await _validationHelper.ValidateEventDataAsync(
                eventName: dto.EventName,
                startTime: dto.StartTime,
                endTime: dto.EndTime,
                locationId: dto.LocationId,
                externalLocationId: dto.ExternalLocationId,
                expectedAttendees: null,
                parentStartTime: parent.StartTime,
                parentEndTime: parent.EndTime,
                parentEventId: parentEventId,
                currentEventId: null
            );

            if (!validation.IsSuccess)
                throw new InvalidOperationException(validation.ErrorMessage);

            var subEvent = _mapper.Map<Event>(dto);
            subEvent.ParentEventId = parentEventId;
            subEvent.CreatedBy = currentUserId;
            subEvent.StatusId = DRAFT_STATUS_ID;
            subEvent.CreatedAt = DateTime.Now;
            subEvent.IsDeleted = false;

            await _unitOfWork.Events.AddAsync(subEvent);
            await _unitOfWork.SaveChangesAsync();

            var result = await _unitOfWork.Events.GetByIdWithDetailsAsync(subEvent.EventId);

            await AutoUpdateAndSaveIfNeededAsync(result);

            return _mapper.Map<SubEventDto>(result);
        }

        public async Task<SubEventDto?> UpdateSubEventAsync(
            int subEventId, UpdateSubEventDto dto, int currentUserId)
        {
            var subEvent = await _unitOfWork.Events.GetByIdWithDetailsAsync(subEventId);
            if (subEvent == null || subEvent.ParentEventId == null) return null;

            var parent = await _unitOfWork.Events.GetByIdAsync(subEvent.ParentEventId.Value);
            if (parent == null)
                throw new InvalidOperationException("Parent event not found");

            var permission = await _permissionHelper.CanModifyEventAsync(subEvent, currentUserId);
            permission.ThrowIfDenied();

            // ⭐ Sub-event update:
            //  - Phải nằm trong khung giờ parent
            //  - Bỏ qua parentEvent khi check phòng
            //  - Bỏ qua chính subEvent khi update
            var validation = await _validationHelper.ValidateEventDataAsync(
                eventName: dto.EventName,
                startTime: dto.StartTime,
                endTime: dto.EndTime,
                locationId: dto.LocationId,
                externalLocationId: dto.ExternalLocationId,
                expectedAttendees: null,
                parentStartTime: parent.StartTime,
                parentEndTime: parent.EndTime,
                parentEventId: subEvent.ParentEventId,
                currentEventId: subEvent.EventId
            );

            if (!validation.IsSuccess)
                throw new InvalidOperationException(validation.ErrorMessage);

            _mapper.Map(dto, subEvent);
            subEvent.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(subEvent);
            await _unitOfWork.SaveChangesAsync();

            var result = await _unitOfWork.Events.GetByIdWithDetailsAsync(subEvent.EventId);

            await AutoUpdateAndSaveIfNeededAsync(result);

            return _mapper.Map<SubEventDto>(result);
        }

        public async Task<bool> DeleteSubEventAsync(int subEventId, int currentUserId)
        {
            var subEvent = await _unitOfWork.Events.GetByIdAsync(subEventId);
            if (subEvent == null || subEvent.IsDeleted == true || subEvent.ParentEventId == null)
                return false;

            var permission = await _permissionHelper.CanDeleteEventAsync(subEvent, currentUserId);
            permission.ThrowIfDenied();

            subEvent.IsDeleted = true;
            subEvent.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(subEvent);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Public events

        public async Task<List<PublicEventDto>> GetPublicEventsAsync()
        {
            var allEvents = await _unitOfWork.Events.GetAllWithDetailsAsync();

            await AutoUpdateAndSaveIfNeededAsync(allEvents);

            var publicEvents = _filterHelper.FilterPublicEvents(allEvents);
            var sorted = _filterHelper.SortByStartTime(publicEvents);

            return _mapper.Map<List<PublicEventDto>>(sorted.ToList());
        }

        public async Task<PublicEventDto?> GetPublicEventByIdAsync(int id)
        {
            var ev = await _unitOfWork.Events.GetByIdWithDetailsAsync(id);
            if (ev == null)
                return null;

            await AutoUpdateAndSaveIfNeededAsync(ev);

            if (!IsPublicEvent(ev))
                return null;

            return _mapper.Map<PublicEventDto>(ev);
        }

        public async Task<PublicEventDto?> GetPublicEventWithSubEventsAsync(int id)
        {
            var ev = await _unitOfWork.Events.GetByIdWithSubEventsAsync(id);
            if (ev == null)
                return null;

            await AutoUpdateAndSaveIfNeededAsync(ev);

            if (!IsPublicEvent(ev))
                return null;

            var result = _mapper.Map<PublicEventDto>(ev);

            if (ev.InverseParentEvent != null)
            {
                var approvedSubEvents = ev.InverseParentEvent
                    .Where(se => se.IsDeleted != true && se.StatusId == APPROVED_STATUS_ID)
                    .OrderBy(se => se.StartTime);

                result.SubEvents = _mapper.Map<List<PublicSubEventDto>>(approvedSubEvents.ToList());
            }

            return result;
        }

        private bool IsPublicEvent(Event? ev)
        {
            if (ev == null ||
                ev.ParentEventId != null ||
                ev.IsDeleted == true ||
                ev.StatusId != APPROVED_STATUS_ID)
                return false;

            return ev.EndTime >= DateTime.Now;
        }
        #endregion

        #region Statistics (EM dashboard)

        public async Task<EventStatistics> GetMyEventStatisticsAsync(int currentUserId)
        {
            var allEvents = await _unitOfWork.Events.GetAllWithDetailsAsync();

            await AutoUpdateAndSaveIfNeededAsync(allEvents);

            var myEvents = allEvents
                .Where(e => e.CreatedBy == currentUserId &&
                            e.IsDeleted != true &&
                            e.ParentEventId == null);

            var stats = _filterHelper.GetStatistics(myEvents);
            return stats;
        }

        #endregion

        #region Approval workflow

        public async Task<EventDto?> SubmitForApprovalAsync(int eventId, int currentUserId)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (ev == null || ev.IsDeleted == true) return null;

            var permission = await _permissionHelper.CanSubmitForApprovalAsync(ev, currentUserId);
            permission.ThrowIfDenied();

            var statusCheck = _permissionHelper.ValidateStatusTransition(ev.StatusId, PENDING_STATUS_ID);
            statusCheck.ThrowIfDenied();

            ev.StatusId = PENDING_STATUS_ID;
            ev.UpdatedAt = DateTime.Now;

            await _unitOfWork.Events.UpdateAsync(ev);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Events.GetByIdWithDetailsAsync(ev.EventId);
            return _mapper.Map<EventDto>(updated);
        }

        
        public async Task<List<EventInvitationDto>> SendInvitationsAsync(
            int eventId, int senderUserId, SendEventInvitationsDto dto)
        {
            var ev = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (ev == null || ev.IsDeleted == true)
                throw new InvalidOperationException("Event not found");

            if (ev.StatusId != APPROVED_STATUS_ID)
                throw new InvalidOperationException("Event must be approved before sending invitations.");

            var sender = await _unitOfWork.Users.GetByIdAsync(senderUserId);
            if (sender == null)
                throw new InvalidOperationException("Sender not found");

            var allUsers = await _unitOfWork.Users.GetAllAsync();
            var resultInvitations = new List<EventInvitationDto>();

            // CLASS INVITATIONS
            if (dto.ClassCodes != null && dto.ClassCodes.Count > 0)
            {
                foreach (var classCode in dto.ClassCodes
                             .Where(x => !string.IsNullOrWhiteSpace(x))
                             .Select(x => x.Trim())
                             .Distinct())
                {
                    var invitation = new EventInvitation
                    {
                        EventId = eventId,
                        ClassCode = classCode,
                        SentBy = senderUserId,
                        SentAt = DateTime.Now
                    };

                    await _unitOfWork.EventInvitations.AddAsync(invitation);

                    var students = allUsers.Where(u => u.ClassCode == classCode).ToList();

                    foreach (var user in students)
                    {
                        var subject = dto.IsMandatory
                            ? $"[BẮT BUỘC] Tham gia sự kiện: {ev.EventName}"
                            : $"Mời tham gia sự kiện: {ev.EventName}";

                        var body =
                            $"Chào {user.FullName},\n\n" +
                            $"Bạn được {(dto.IsMandatory ? "YÊU CẦU" : "mời")} tham gia sự kiện:\n" +
                            $"- {ev.EventName}\n" +
                            $"- {ev.StartTime:g} - {ev.EndTime:g}\n\n" +
                            $"Chi tiết: https://yourdomain.com/events/{ev.EventId}\n\n";

                        await _emailService.SendEmailAsync(user.Email, subject, body, user.FullName);
                    }

                    var dtoMapped = _mapper.Map<EventInvitationDto>(invitation);
                    dtoMapped.IsMandatory = dto.IsMandatory;
                    resultInvitations.Add(dtoMapped);
                }
            }

            // INTERNAL USER INVITES
            if (dto.InternalUserIds != null && dto.InternalUserIds.Count > 0)
            {
                var targetUsers = allUsers
                    .Where(u => dto.InternalUserIds.Contains(u.UserId))
                    .ToList();

                foreach (var user in targetUsers)
                {
                    var subject = dto.IsMandatory
                        ? $"[BẮT BUỘC] Tham gia sự kiện: {ev.EventName}"
                        : $"Mời tham gia sự kiện: {ev.EventName}";

                    await _emailService.SendEmailAsync(
                        user.Email,
                        subject,
                        $"Bạn được mời tham gia {ev.EventName}",
                        user.FullName);
                }
            }

            // EXTERNAL EMAIL INVITES
            if (dto.ExternalInvites != null && dto.ExternalInvites.Count > 0)
            {
                foreach (var ext in dto.ExternalInvites)
                {
                    var subject = dto.IsMandatory
                        ? $"[BẮT BUỘC] Tham gia sự kiện: {ev.EventName}"
                        : $"Mời tham gia sự kiện: {ev.EventName}";

                    await _emailService.SendEmailAsync(
                        ext.Email,
                        subject,
                        $"Chào {ext.FullName ?? "Quý khách"},\nBạn được mời tham gia {ev.EventName}");
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return resultInvitations;
        }
        #endregion

        #region Director aprove/reject

        // Get pending approval
        public async Task<List<PendingApprovalDto>> GetPendingApprovalsAsync()
        {
            var allEvents = await _unitOfWork.Events.GetAllWithDetailsAsync();
            var pendingEvents = allEvents
                .Where(e => e.StatusId == PENDING_STATUS_ID && e.IsDeleted != true)
                .OrderBy(e => e.CreatedAt)
                .ToList();

            var result = new List<PendingApprovalDto>();

            foreach (var evt in pendingEvents)
            {
                var subEventsCount = allEvents.Count(e => e.ParentEventId == evt.EventId);

                var logs = await _unitOfWork.EventLogs.GetLogsByEventIdAsync(evt.EventId);
                var submissionLog = logs
                    .Where(l => l.Action == "SubmittedForApproval")
                    .OrderByDescending(l => l.CreatedAt)
                    .FirstOrDefault();

                result.Add(new PendingApprovalDto
                {
                    EventId = evt.EventId,
                    EventName = evt.EventName,
                    StartTime = evt.StartTime,
                    EndTime = evt.EndTime,
                    Budget = evt.EstimatedCost,
                    ExpectedAttendees = evt.ExpectedAttendees,
                    CreatedByName = evt.CreatedByNavigation?.FullName ?? "Unknown",
                    SubmittedDate = evt.UpdatedAt ?? evt.CreatedAt ?? DateTime.Now,
                    SubEventsCount = subEventsCount,
                    SubmitterNote = submissionLog?.Details
                });
            }

            return result;
        }

        // Approve event
        public async Task<EventDto> ApproveEventAsync(int eventId, EventDecisionDto dto, int directorId)
        {
            var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (evt == null)
                throw new InvalidOperationException("Event not found");

            var permission = await _permissionHelper.CanApproveEventAsync(evt, directorId);
            permission.ThrowIfDenied();

            evt.StatusId = APPROVED_STATUS_ID;
            evt.UpdatedAt = DateTime.Now;
            await _unitOfWork.Events.UpdateAsync(evt);

            var approval = new EventApproval
            {
                EventId = eventId,
                DirectorId = directorId,
                ApprovalStatus = "Approved",
                Comment = dto.Comment,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.EventApprovals.AddAsync(approval);

            await LogEventActionAsync(eventId, directorId, "Approved", dto.Comment);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventDto>(await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId));
        }

        // Reject event
        public async Task<EventDto> RejectEventAsync(int eventId, EventDecisionDto dto, int directorId)
        {
            var evt = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (evt == null)
                throw new InvalidOperationException("Event not found");

            var permission = await _permissionHelper.CanApproveEventAsync(evt, directorId);
            permission.ThrowIfDenied();

            evt.StatusId = REJECTED_STATUS_ID;
            evt.UpdatedAt = DateTime.Now;
            await _unitOfWork.Events.UpdateAsync(evt);

            var approval = new EventApproval
            {
                EventId = eventId,
                DirectorId = directorId,
                ApprovalStatus = "Rejected",
                Comment = dto.Comment,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.EventApprovals.AddAsync(approval);

            await LogEventActionAsync(eventId, directorId, "Rejected", dto.Comment);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventDto>(await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId));
        }

        // Get approval history for a specific event
        public async Task<List<EventApprovalDto>> GetApprovalHistoryAsync(int eventId)
        {
            var approvals = await _unitOfWork.EventApprovals.GetApprovalsByEventIdAsync(eventId);
            var result = new List<EventApprovalDto>();

            foreach (var approval in approvals)
            {
                var evt = await _unitOfWork.Events.GetByIdAsync(approval.EventId);
                var director = await _unitOfWork.Users.GetByIdAsync(approval.DirectorId);

                var dto = _mapper.Map<EventApprovalDto>(approval);
                dto.EventName = evt?.EventName ?? "Unknown Event";
                dto.DirectorName = director?.FullName ?? "Unknown Director";

                result.Add(dto);
            }

            return result;
        }

        // Log event
        private async Task LogEventActionAsync(int eventId, int userId, string action, string? details)
        {
            var log = new EventLog
            {
                EventId = eventId,
                UserId = userId,
                Action = action,
                Details = details,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.EventLogs.AddAsync(log);
        }
        #endregion
        // Đăng ký sự kiện cho user
        public async Task<EventAttendanceDto> RegisterEventAsync(int eventId, int userId)
        {
            var eventObj = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventObj == null || eventObj.IsDeleted == true)
                throw new InvalidOperationException("Event not found or deleted");

            var userObj = await _unitOfWork.Users.GetByIdAsync(userId);
            if (userObj == null)
                throw new InvalidOperationException("User not found");
        
            // Kiểm tra đã đăng ký chưa (trực tiếp trên DB)
            var existed = await _unitOfWork.EventAttendances.ExistsAsync(x => x.EventId == eventId && x.UserId == userId);
            if (existed)
                throw new InvalidOperationException("User already registered for this event");

            var attendance = new EventAttendance
            {
                EventId = eventId,
                UserId = userId,
                CheckinAt = null,
                CheckoutAt = null,
                Method = "manual"
            };
            await _unitOfWork.EventAttendances.AddAsync(attendance);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EventAttendanceDto>(attendance);
        }

        public async Task<EventAttendanceDto> CheckinEventAsync(int eventId, int userId)
        {
            var attendance = await _unitOfWork.EventAttendances.FindAsync(x => x.EventId == eventId && x.UserId == userId);
            if (attendance == null)
                throw new InvalidOperationException("User chưa đăng ký sự kiện này");
            if (attendance.CheckinAt != null)
                throw new InvalidOperationException("User đã checkin sự kiện này");
            attendance.CheckinAt = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<EventAttendanceDto>(attendance);
        }

        public async Task<EventAttendanceDto> CheckoutEventAsync(int eventId, int userId)
        {
            var attendance = await _unitOfWork.EventAttendances.FindAsync(x => x.EventId == eventId && x.UserId == userId);
            if (attendance == null)
                throw new InvalidOperationException("User chưa đăng ký sự kiện này");
            if (attendance.CheckinAt == null)
                throw new InvalidOperationException("User chưa checkin, không thể checkout");
            if (attendance.CheckoutAt != null)
                throw new InvalidOperationException("User đã checkout sự kiện này");
            attendance.CheckoutAt = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<EventAttendanceDto>(attendance);
        }

        public async Task UnregisterEventAsync(int eventId, int userId)
        {
            var attendance = await _unitOfWork.EventAttendances.FindAsync(x => x.EventId == eventId && x.UserId == userId);
            if (attendance == null)
                throw new InvalidOperationException("Bạn chưa đăng ký sự kiện này");
            if (attendance.CheckinAt != null)
                throw new InvalidOperationException("Bạn đã checkin, không thể hủy đăng ký");
            await _unitOfWork.EventAttendances.DeleteAsync(attendance);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
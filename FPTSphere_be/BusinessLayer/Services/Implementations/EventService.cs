using AutoMapper;
using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs.EventApproval;
using BusinessLayer.Helpers;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using DataLayer.Repositories.Implementations;
using DataLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using Microsoft.Extensions.Options;
using OfficeOpenXml;


namespace BusinessLayer.Services.Implementations
{
        public class EventService : IEventService
        {
          
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly EventValidationHelper _validationHelper;
        private readonly EventPermissionHelper _permissionHelper;
        private readonly EventFilterHelper _filterHelper;
        private readonly IEventTaskService _eventTaskService;
        private readonly IEmailService _emailService;
        private readonly IEventTaskRepository _eventTaskRepository;
        private readonly IFileService _fileService;
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
            IEventTaskService eventTaskService,
            IEventTaskRepository eventTaskRepository,
            IEmailService emailService,
            EventFilterHelper filterHelper,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validationHelper = validationHelper;
            _permissionHelper = permissionHelper;
            _filterHelper = filterHelper;
            _emailService = emailService;
            _eventTaskService = eventTaskService;
            _eventTaskRepository = eventTaskRepository;
            _fileService = fileService;
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

        // Old code:
        // public async Task<EventDto> CreateAsync(CreateEventDto dto, int currentUserId)
        // Fixed:
        public async Task<EventDto> CreateAsync(CreateEventDto dto, int currentUserId, string? userRole = null)
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

            // Determine initial status based on user role
            // If userRole is not provided, get it from database
            if (string.IsNullOrEmpty(userRole))
            {
                var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                userRole = user?.Role?.RoleName ?? "";
            }

            // Fixed: New logic for event creation
            // - Manager creates event with PENDING status (needs Director approval)
            // - Director creates event with APPROVED status (auto-approved)
            // - Admin creates event with APPROVED status (auto-approved)
            int initialStatusId = DRAFT_STATUS_ID;
            if (userRole == "Event Manager")
            {
                initialStatusId = PENDING_STATUS_ID;
            }
            else if (userRole == "Director" || userRole == "Admin")
            {
                initialStatusId = APPROVED_STATUS_ID;
            }

            // Upload banner file to Cloudinary if provided
            string? bannerUrl = null;
            if (dto.BannerUrl != null && dto.BannerUrl.Length > 0)
            {
                try
                {
                    var uploadResult = await _fileService.UploadFileAsync(
                        dto.BannerUrl,
                        folder: "events/banners",
                        transformation: "w_1200,h_600,c_fill,q_auto,f_auto"
                    );
                    bannerUrl = uploadResult.SecureUrl;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to upload banner file: {ex.Message}");
                }
            }

            var ev = _mapper.Map<Event>(dto);
            // Set BannerUrl from uploaded file URL
            ev.BannerUrl = bannerUrl;
            ev.CreatedBy = currentUserId;
            ev.StatusId = initialStatusId;
            ev.CreatedAt = DateTime.Now;
            ev.IsDeleted = false;
            ev.ParentEventId = null;

            // Validate that status exists
            var statusExists = await _unitOfWork.EventStatuses.GetByIdAsync(initialStatusId);
            if (statusExists == null)
                throw new InvalidOperationException($"Event status with ID {initialStatusId} does not exist");

            // Validate that user exists
            var userExists = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (userExists == null)
                throw new InvalidOperationException($"User with ID {currentUserId} does not exist");

            // Validate location exists if provided
            if (ev.LocationId.HasValue)
            {
                var location = await _unitOfWork.Locations.GetByIdAsync(ev.LocationId.Value);
                if (location == null)
                    throw new InvalidOperationException($"Location with ID {ev.LocationId.Value} does not exist");
            }

            if (ev.ExternalLocationId.HasValue)
            {
                var extLocation = await _unitOfWork.ExternalLocations.GetByIdAsync(ev.ExternalLocationId.Value);
                if (extLocation == null)
                    throw new InvalidOperationException($"External location with ID {ev.ExternalLocationId.Value} does not exist");
            }

            // Validate category exists if provided
            if (ev.CategoryId.HasValue)
            {
                var category = await _unitOfWork.EventCategories.GetByIdAsync(ev.CategoryId.Value);
                if (category == null)
                    throw new InvalidOperationException($"Event category with ID {ev.CategoryId.Value} does not exist");
            }

            // Validate type exists if provided
            if (ev.TypeId.HasValue)
            {
                var type = await _unitOfWork.EventTypes.GetByIdAsync(ev.TypeId.Value);
                if (type == null)
                    throw new InvalidOperationException($"Event type with ID {ev.TypeId.Value} does not exist");
            }

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

            // Save existing BannerUrl before mapping
            var existingBannerUrl = ev.BannerUrl;

            // Upload banner file to Cloudinary if provided
            // Only update BannerUrl if a new file is uploaded
            string? newBannerUrl = null;
            if (dto.BannerUrl != null && dto.BannerUrl.Length > 0)
            {
                try
                {
                    var uploadResult = await _fileService.UploadFileAsync(
                        dto.BannerUrl,
                        folder: "events/banners",
                        transformation: "w_1200,h_600,c_fill,q_auto,f_auto"
                    );
                    newBannerUrl = uploadResult.SecureUrl;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to upload banner file: {ex.Message}");
                }
            }

            // Map other fields from DTO (BannerUrl will be handled separately)
            _mapper.Map(dto, ev);
            
            // Set BannerUrl: use new URL if file was uploaded, otherwise keep existing
            ev.BannerUrl = newBannerUrl ?? existingBannerUrl;
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
            int parentEventId, CreateSubEventDto dto, int currentUserId, string? userRole = null)
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

            // Determine initial status based on user role
            // If userRole is not provided, get it from database
            if (string.IsNullOrEmpty(userRole))
            {
                var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                userRole = user?.Role?.RoleName ?? "";
            }

            // Fixed: New logic for sub-event creation
            // - Staff creates sub-event with PENDING status (needs Manager approval)
            // - Manager creates sub-event with APPROVED status (auto-approved)
            // - Director creates sub-event with APPROVED status (auto-approved)
            // - Admin creates sub-event with APPROVED status (auto-approved)
            int initialStatusId = DRAFT_STATUS_ID;
            if (userRole == "Staff")
            {
                initialStatusId = PENDING_STATUS_ID;
            }
            else if (userRole == "Event Manager" || userRole == "Director" || userRole == "Admin")
            {
                initialStatusId = APPROVED_STATUS_ID;
            }

            // Upload banner file to Cloudinary if provided
            string? bannerUrl = null;
            if (dto.BannerUrl != null && dto.BannerUrl.Length > 0)
            {
                try
                {
                    var uploadResult = await _fileService.UploadFileAsync(
                        dto.BannerUrl,
                        folder: "sub-events/banners",
                        transformation: "w_1200,h_600,c_fill,q_auto,f_auto"
                    );
                    bannerUrl = uploadResult.SecureUrl ?? uploadResult.Url;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to upload banner file: {ex.Message}");
                }
            }

            var subEvent = _mapper.Map<Event>(dto);
            // Set BannerUrl from uploaded file URL
            subEvent.BannerUrl = bannerUrl;
            subEvent.ParentEventId = parentEventId;
            subEvent.CreatedBy = currentUserId;
            subEvent.StatusId = initialStatusId;
            subEvent.CreatedAt = DateTime.Now;
            subEvent.IsDeleted = false;

            // Auto-fill CategoryId and TypeId from parent event if not provided
            if (!subEvent.CategoryId.HasValue && parent.CategoryId.HasValue)
            {
                subEvent.CategoryId = parent.CategoryId;
            }

            if (!subEvent.TypeId.HasValue && parent.TypeId.HasValue)
            {
                subEvent.TypeId = parent.TypeId;
            }

            // Auto-fill LocationId and ExternalLocationId from parent event if not provided
            // Sub-event có thể có location riêng hoặc inherit từ parent
            if (!subEvent.LocationId.HasValue && !subEvent.ExternalLocationId.HasValue)
            {
                // Nếu sub-event không có location nào, lấy từ parent
                if (parent.LocationId.HasValue)
                {
                    subEvent.LocationId = parent.LocationId;
                }
                else if (parent.ExternalLocationId.HasValue)
                {
                    subEvent.ExternalLocationId = parent.ExternalLocationId;
                }
            }

            // Validate category exists if provided
            if (subEvent.CategoryId.HasValue)
            {
                var category = await _unitOfWork.EventCategories.GetByIdAsync(subEvent.CategoryId.Value);
                if (category == null)
                    throw new InvalidOperationException($"Event category with ID {subEvent.CategoryId.Value} does not exist");
            }

            // Validate type exists if provided
            if (subEvent.TypeId.HasValue)
            {
                var type = await _unitOfWork.EventTypes.GetByIdAsync(subEvent.TypeId.Value);
                if (type == null)
                    throw new InvalidOperationException($"Event type with ID {subEvent.TypeId.Value} does not exist");
            }

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

            // Validate category exists if provided
            if (subEvent.CategoryId.HasValue)
            {
                var category = await _unitOfWork.EventCategories.GetByIdAsync(subEvent.CategoryId.Value);
                if (category == null)
                    throw new InvalidOperationException($"Event category with ID {subEvent.CategoryId.Value} does not exist");
            }

            // Validate type exists if provided
            if (subEvent.TypeId.HasValue)
            {
                var type = await _unitOfWork.EventTypes.GetByIdAsync(subEvent.TypeId.Value);
                if (type == null)
                    throw new InvalidOperationException($"Event type with ID {subEvent.TypeId.Value} does not exist");
            }

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


        public async Task<EventDto> ApproveEventAsync(int eventId, EventDecisionDto dto, int directorId)
        {
            var evt = await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId);
            if (evt == null || evt.IsDeleted == true)
                throw new InvalidOperationException("Event not found");

            // Check permission first (this will throw UnauthorizedAccessException with clear message)
            var permission = await _permissionHelper.CanApproveEventAsync(evt, directorId);
            if (!permission.IsAllowed)
            {
                throw new UnauthorizedAccessException(permission.DenyReason);
            }

            // Additional status check with clear message
            if (evt.StatusId != PENDING_STATUS_ID)
            {
                var statusName = evt.StatusId switch
                {
                    1 => "Draft",
                    2 => "Pending Approval",
                    3 => "Approved",
                    4 => "In Progress",
                    5 => "Completed",
                    6 => "Cancelled",
                    7 => "Rejected",
                    _ => "Unknown"
                };
                throw new InvalidOperationException($"Cannot approve event with status '{statusName}'. Only events with 'Pending Approval' status can be approved.");
            }

            // Validate status transition
            var statusCheck = _permissionHelper.ValidateStatusTransition(evt.StatusId, APPROVED_STATUS_ID);
            if (!statusCheck.IsAllowed)
            {
                throw new InvalidOperationException(statusCheck.DenyReason);
            }

            evt.StatusId = APPROVED_STATUS_ID;
            evt.UpdatedAt = DateTime.Now;
            await _unitOfWork.Events.UpdateAsync(evt);

            var approval = new EventApproval
            {
                EventId = eventId,
                DirectorId = directorId,
                ApprovalStatus = "Approved",
                Comment = dto?.Comment,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.EventApprovals.AddAsync(approval);

            await LogEventActionAsync(eventId, directorId, "Approved", dto?.Comment);

            var tasks = await _eventTaskRepository.Query()
                .Where(t => t.EventId == eventId)
                .ToListAsync();

            foreach (var task in tasks)
            {
                var st = (task.Status ?? "").Trim();

                if (st.Equals("PendingApproval", StringComparison.OrdinalIgnoreCase) ||
                    st.Equals("Draft", StringComparison.OrdinalIgnoreCase) ||
                    st.Equals("Planned", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrWhiteSpace(st))
                {
                    task.Status = "Todo";
                    await _eventTaskRepository.UpdateAsync(task);

                    var staff = await _unitOfWork.Users.GetByIdAsync(task.AssignedTo);
                    if (staff != null && !string.IsNullOrWhiteSpace(staff.Email))
                    {
                        try
                        {
                            var subject = $"New Task Assigned: {evt.EventName}";
                            var body =
                                $"Hello {staff.FullName},\n\n" +
                                $"The event \"{evt.EventName}\" has been approved. Your task is now active.\n\n" +
                                $"Task: {task.Title}\n" +
                                $"Status: {task.Status}\n\n" +
                                $"Please log in to view details and update progress.\n\n" +
                                $"FPTU Event System";

                            await _emailService.SendEmailAsync(staff.Email, subject, body, staff.FullName);
                        }
                        catch (Exception emailEx)
                        {
                            // Log email error but don't fail the approval
                            // Email sending failure should not prevent event approval
                            await LogEventActionAsync(eventId, directorId, "Approved (Email notification failed)", 
                                $"Approval successful but email notification to {staff.FullName} failed: {emailEx.Message}");
                        }
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId);
            return _mapper.Map<EventDto>(updated);
        }

        public async Task<EventDto> RejectEventAsync(int eventId, EventDecisionDto dto, int directorId)
        {
            var evt = await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId);
            if (evt == null || evt.IsDeleted == true)
                throw new InvalidOperationException("Event not found");

            // Check permission first (this will throw UnauthorizedAccessException with clear message)
            var permission = await _permissionHelper.CanApproveEventAsync(evt, directorId);
            if (!permission.IsAllowed)
            {
                throw new UnauthorizedAccessException(permission.DenyReason);
            }

            // Additional status check with clear message
            if (evt.StatusId != PENDING_STATUS_ID)
            {
                var statusName = evt.StatusId switch
                {
                    1 => "Draft",
                    2 => "Pending Approval",
                    3 => "Approved",
                    4 => "In Progress",
                    5 => "Completed",
                    6 => "Cancelled",
                    7 => "Rejected",
                    _ => "Unknown"
                };
                throw new InvalidOperationException($"Cannot reject event with status '{statusName}'. Only events with 'Pending Approval' status can be rejected.");
            }

            // Validate status transition
            var statusCheck = _permissionHelper.ValidateStatusTransition(evt.StatusId, REJECTED_STATUS_ID);
            if (!statusCheck.IsAllowed)
            {
                throw new InvalidOperationException(statusCheck.DenyReason);
            }

            evt.StatusId = REJECTED_STATUS_ID;
            evt.UpdatedAt = DateTime.Now;
            await _unitOfWork.Events.UpdateAsync(evt);

            var approval = new EventApproval
            {
                EventId = eventId,
                DirectorId = directorId,
                ApprovalStatus = "Rejected",
                Comment = dto?.Comment,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.EventApprovals.AddAsync(approval);

            await LogEventActionAsync(eventId, directorId, "Rejected", dto?.Comment);

            var tasks = await _eventTaskRepository.Query()
                .Where(t => t.EventId == eventId)
                .ToListAsync();

            foreach (var task in tasks)
            {
                var st = (task.Status ?? "").Trim();
                if (st.Equals("PendingApproval", StringComparison.OrdinalIgnoreCase))
                {
                    task.Status = "Draft";
                    await _eventTaskRepository.UpdateAsync(task);
                }
            }

            // Send rejection email to creator (non-blocking - don't fail if email fails)
            var creator = await _unitOfWork.Users.GetByIdAsync(evt.CreatedBy);
            if (creator != null && !string.IsNullOrWhiteSpace(creator.Email))
            {
                try
                {
                    var subject = $"Event Rejected: {evt.EventName}";
                    var body =
                        $"Hello {creator.FullName},\n\n" +
                        $"Your event \"{evt.EventName}\" has been rejected.\n\n" +
                        $"Reason: {dto?.Comment ?? "No reason provided"}\n\n" +
                        $"Please revise and submit again.\n\n" +
                        $"FPTU Event System";

                    await _emailService.SendEmailAsync(creator.Email, subject, body, creator.FullName);
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the rejection
                    // Email sending failure should not prevent event rejection
                    await LogEventActionAsync(eventId, directorId, "Rejected (Email notification failed)", 
                        $"Rejection successful but email notification failed: {emailEx.Message}");
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId);
            return _mapper.Map<EventDto>(updated);
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

        #region Sub-event Email and QR Code

        /// <summary>
        /// Generate QR code for sub-event registration form (Google Form URL)
        /// </summary>
        public async Task<GenerateQRCodeResponseDto> GenerateQRCodeForSubEventAsync(int subEventId, GenerateQRCodeDto dto)
        {
            var subEvent = await _unitOfWork.Events.GetByIdAsync(subEventId);
            if (subEvent == null || subEvent.IsDeleted == true)
                throw new InvalidOperationException("Sub-event not found");

            if (subEvent.ParentEventId == null)
                throw new InvalidOperationException("This is not a sub-event");

            // Use Google Form URL provided by frontend
            var googleFormUrl = dto.GoogleFormUrl;

            // Generate QR code
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(googleFormUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            // Convert to base64
            var qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);

            return new GenerateQRCodeResponseDto
            {
                SubEventId = subEventId,
                GoogleFormUrl = googleFormUrl,
                QrCodeBase64 = $"data:image/png;base64,{qrCodeBase64}",
                QrCodeUrl = googleFormUrl
            };
        }

        /// <summary>
        /// Send email to sub-event attendees
        /// Updated: Removed QR code and recipientType, added image upload and Excel import
        /// </summary>
        public async Task<SendSubEventEmailResponseDto> SendEmailToSubEventAttendeesAsync(
            int subEventId, SendSubEventEmailDto dto, int currentUserId, IFileService fileService)
        {
            var subEvent = await _unitOfWork.Events.GetByIdWithDetailsAsync(subEventId);
            if (subEvent == null || subEvent.IsDeleted == true)
                throw new InvalidOperationException("Sub-event not found");

            if (subEvent.ParentEventId == null)
                throw new InvalidOperationException("This is not a sub-event");

            // Check permission
            var permission = await _permissionHelper.CanModifyEventAsync(subEvent, currentUserId);
            permission.ThrowIfDenied();

            // Upload image file if provided
            string? imageUrl = null;
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                try
                {
                    var uploadResult = await fileService.UploadFileAsync(
                        dto.ImageFile,
                        folder: "email-images",
                        transformation: "w_1200,h_600,c_fill,q_auto,f_auto"
                    );
                    imageUrl = uploadResult.SecureUrl ?? uploadResult.Url;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to upload image file: {ex.Message}");
                }
            }

            // Extract emails from Excel file or use custom email list
            List<string> recipientEmails = new List<string>();

            if (dto.ExcelFile != null && dto.ExcelFile.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = System.IO.Path.GetExtension(dto.ExcelFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new InvalidOperationException("Only Excel files (.xlsx, .xls) are allowed");
                }

                try
                {
                    // Read Excel file and extract Email column
                    using var stream = new MemoryStream();
                    await dto.ExcelFile.CopyToAsync(stream);
                    stream.Position = 0;

                    using var package = new OfficeOpenXml.ExcelPackage(stream);
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        throw new InvalidOperationException("Excel file does not contain any worksheets.");
                    }

                    // Find Email column with flexible matching
                    // Support various column names: Email, email, EMAIL, gmail, e-mail, email address, etc.
                    int emailColumnIndex = -1;
                    int partialMatchColumnIndex = -1;
                    var emailKeywords = new[] { "email", "gmail", "e-mail", "e mail", "email address", "mail", "correo" };
                    
                    // First pass: Look for exact match "email" (highest priority)
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(headerValue))
                            continue;

                        var headerLower = headerValue.ToLower();
                        
                        // Exact match has highest priority
                        if (headerLower == "email")
                        {
                            emailColumnIndex = col;
                            break;
                        }
                    }

                    // Second pass: If no exact match, look for partial matches
                    if (emailColumnIndex == -1)
                    {
                        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                        {
                            var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                            if (string.IsNullOrWhiteSpace(headerValue))
                                continue;

                            var headerLower = headerValue.ToLower();
                            
                            // Check for partial match with email keywords
                            foreach (var keyword in emailKeywords)
                            {
                                if (headerLower.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                                {
                                    partialMatchColumnIndex = col;
                                    break;
                                }
                            }
                            
                            if (partialMatchColumnIndex != -1)
                                break;
                        }
                        
                        emailColumnIndex = partialMatchColumnIndex;
                    }

                    if (emailColumnIndex == -1)
                    {
                        throw new InvalidOperationException(
                            "Email column not found in Excel file. " +
                            "Please ensure the first row contains a column with name containing 'Email', 'Gmail', 'E-mail', 'Mail', or similar keywords.");
                    }

                    // Extract emails from Email column (skip header row)
                    var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var emailCell = worksheet.Cells[row, emailColumnIndex].Value?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(emailCell) && emailCell.Contains("@"))
                        {
                            emails.Add(emailCell);
                        }
                    }

                    recipientEmails = emails.ToList();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error processing Excel file: {ex.Message}");
                }
            }
            else if (dto.CustomEmailList != null && dto.CustomEmailList.Count > 0)
            {
                // Use custom email list
                recipientEmails = dto.CustomEmailList
                    .Where(email => !string.IsNullOrWhiteSpace(email) && email.Contains("@"))
                    .Select(email => email.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            if (recipientEmails.Count == 0)
            {
                throw new InvalidOperationException("No valid email addresses found. Please provide either an Excel file with Email column or a custom email list.");
            }

            // Get sender information (person who is sending the email)
            var sender = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (sender == null)
                throw new InvalidOperationException("Sender not found");

            var senderName = sender.FullName ?? "Event Manager";

            // Send emails
            int successCount = 0;
            int failureCount = 0;
            List<string> failedEmails = new List<string>();

            // Get all users by emails to avoid multiple queries
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            var userDict = allUsers
                .Where(u => !string.IsNullOrWhiteSpace(u.Email) && recipientEmails.Contains(u.Email))
                .ToDictionary(u => u.Email!, u => u);

            foreach (var email in recipientEmails)
            {
                try
                {
                    // Get user by email
                    var user = userDict.ContainsKey(email) ? userDict[email] : null;
                    
                    var recipientName = user?.FullName ?? email;
                    var firstName = user?.FullName?.Split(' ').FirstOrDefault() ?? "";
                    var lastName = user?.FullName?.Split(' ').Skip(1).FirstOrDefault() ?? "";

                    // Process email template with user-specific variables
                    var processedBody = ProcessEmailTemplate(dto.Body, subEvent, imageUrl, firstName, lastName, senderName);
                    var processedSubject = ProcessEmailTemplate(dto.Subject, subEvent, imageUrl, firstName, lastName, senderName);

                    await _emailService.SendEmailAsync(email, processedSubject, processedBody, recipientName);
                    successCount++;
                }
                catch (Exception ex)
                {
                    failureCount++;
                    failedEmails.Add(email);
                    // Log error but continue sending to other recipients
                    Console.WriteLine($"Failed to send email to {email}: {ex.Message}");
                }
            }

            return new SendSubEventEmailResponseDto
            {
                SubEventId = subEventId,
                TotalRecipients = recipientEmails.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                FailedEmails = failedEmails.Count > 0 ? failedEmails : null,
                ImportedEmails = recipientEmails,
                ImageUrl = imageUrl
            };
        }

        /// <summary>
        /// Process email template and replace variables
        /// Updated: Removed QR code variables, added image URL support
        /// </summary>
        private string ProcessEmailTemplate(string template, Event subEvent, string? imageUrl, string firstName = "", string lastName = "", string senderName = "")
        {
            var result = template;

            // Replace user-specific variables
            result = result.Replace("{FirstName}", firstName);
            result = result.Replace("{LastName}", lastName);
            
            // Replace sender name (person who is sending the email)
            result = result.Replace("{SenderName}", senderName);
            
            // Replace event-specific variables
            result = result.Replace("{EventName}", subEvent.EventName ?? "");
            result = result.Replace("{Date}", subEvent.StartTime.ToString("dd/MM/yyyy"));
            result = result.Replace("{Time}", subEvent.StartTime.ToString("HH:mm"));
            
            // Location
            string location = "";
            if (subEvent.Location != null)
                location = subEvent.Location.Name ?? "";
            else if (subEvent.ExternalLocation != null)
                location = subEvent.ExternalLocation.Name ?? "";
            result = result.Replace("{Location}", location);

            // Image - replace with image tag if URL is provided
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                var imageTag = $"<img src=\"{imageUrl}\" alt=\"Event Image\" style=\"max-width: 100%; height: auto;\" />";
                result = result.Replace("{Image}", imageTag);
            }
            else
            {
                result = result.Replace("{Image}", "");
            }

            return result;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Event;
using BusinessLayer.Services.Interfaces;
using DataLayer.Constants;
using DataLayer.Repositories.Interfaces;

namespace BusinessLayer.Services.Implementations
{
    /// <summary>
    /// Service lấy danh sách sự kiện đang book một location trong khoảng thời gian, dùng để hiển thị thông tin booking trong UI chọn phòng.
    /// </summary>
    public class LocationBookingService : ILocationBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LocationBookingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<EventDto>> GetLocationBookingsAsync(
            int locationId,
            DateTime startTime,
            DateTime endTime,
            int? ignoreEventId = null,
            int? ignoreParentEventId = null)
        {
            if (endTime <= startTime)
                throw new ArgumentException("End time must be after start time");

            // Chỉ các status này mới block phòng
            var blockingStatusIds = new[]
            {
                EventStatusIds.PendingApproval, // 2
                EventStatusIds.Approved,        // 3
                EventStatusIds.InProgress       // 4
            };

            var conflictingEvents = await _unitOfWork.Events.FindAllAsync(e =>
                e.LocationId == locationId &&
                e.IsDeleted != true &&
                blockingStatusIds.Contains(e.StatusId) &&
                (!ignoreEventId.HasValue || e.EventId != ignoreEventId.Value) &&
                (!ignoreParentEventId.HasValue || e.EventId != ignoreParentEventId.Value) &&
                e.StartTime < endTime && e.EndTime > startTime
            );

            return _mapper.Map<List<EventDto>>(conflictingEvents.OrderBy(e => e.StartTime).ToList());
        }
    }
}



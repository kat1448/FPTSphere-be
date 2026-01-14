using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Event;

namespace BusinessLayer.Services.Interfaces
{
    /// <summary>
    /// Service trả về danh sách sự kiện đang sử dụng một địa điểm trong khoảng thời gian nhất định. Dùng để hiển thị thông tin \"phòng đã được book bởi sự kiện...\" trên UI.
    /// </summary>
    public interface ILocationBookingService
    {
        Task<List<EventDto>> GetLocationBookingsAsync(
            int locationId,
            DateTime startTime,
            DateTime endTime,
            int? ignoreEventId = null,
            int? ignoreParentEventId = null);
    }
}



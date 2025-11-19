using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services.Interfaces
{
    public interface IEventService
    {
        Task<PagedResult<EventDto>> GetEventsAsync(int page, int pageSize, EventFilterDto? filter, string sortBy, bool sortDescending);
        Task<EventDto?> GetEventByIdAsync(int id);
        Task<EventDto> CreateAsync(CreateEventDto dto, int currentUserId);
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


    }
}

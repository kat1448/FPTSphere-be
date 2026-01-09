using BusinessLayer.DTOs.EventType;

namespace BusinessLayer.Services.Interfaces
{
    public interface IEventTypeService
    {
        Task<List<EventTypeDto>> GetAllAsync();
        Task<EventTypeDto?> GetByIdAsync(int id);
    }
}


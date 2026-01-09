using BusinessLayer.DTOs.EventCategory;

namespace BusinessLayer.Services.Interfaces
{
    public interface IEventCategoryService
    {
        Task<List<EventCategoryDto>> GetAllAsync();
        Task<EventCategoryDto?> GetByIdAsync(int id);
    }
}


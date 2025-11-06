using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs.Event;

namespace BusinessLayer.Services.Interfaces
{
    public interface IEventService
    {
        Task<EventDto> CreateAsync(CreateEventDto dto);
        Task<List<EventDto>> GetAllAsync();
        Task<EventDto?> GetByIdAsync(int id);
    }
}

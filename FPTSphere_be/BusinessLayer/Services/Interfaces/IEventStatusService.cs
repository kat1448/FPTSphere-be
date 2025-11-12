using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs.EventStatus;

namespace BusinessLayer.Services.Interfaces
{
    public interface IEventStatusService
    {
        Task<List<EventStatusDto>> GetAllAsync();
        Task<EventStatusDto?> GetByIdAsync(int id);
        Task<EventStatusDto?> GetByNameAsync(string name);
        Task<EventStatusDto> CreateAsync(CreateEventStatusDto dto);
        Task<EventStatusDto?> UpdateAsync(int id, UpdateEventStatusDto dto);
        Task<(bool success, string message)> DeleteAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}

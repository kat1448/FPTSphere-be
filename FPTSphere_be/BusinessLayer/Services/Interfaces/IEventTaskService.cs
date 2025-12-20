using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.DTOs.EventTask;
using System.Security.Claims;

namespace BusinessLayer.Services.Interfaces
{
    public interface IEventTaskService
    {
        Task<List<EventTaskDto>> GetTasksAssignedToUserAsync(int userId);
        Task<EventTaskDto> UpdateTaskStatusAsync(int taskId, string status, ClaimsPrincipal user);
        Task<EventTaskDto> UpdateTaskReportAsync(int taskId, string report, ClaimsPrincipal user);
        Task<List<EventTaskWithEventDto>> GetTasksWithEventAssignedToUserAsync(int userId);
    }
}

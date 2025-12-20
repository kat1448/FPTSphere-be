using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs.ExternalService;
using BusinessLayer.DTOs.ExternalServices;

namespace BusinessLayer.Services.Interfaces
{
    public interface IExternalServiceService
    {
        Task<IEnumerable<ExternalServiceDto>> GetByEventIdAsync(int eventId);
        Task<ExternalServiceDto?> GetByIdAsync(int serviceId);
        Task<ExternalServiceDto> CreateAsync(int eventId, CreateExternalServiceDto dto);
        Task<ExternalServiceDto> UpdateAsync(int serviceId, UpdateExternalServiceDto dto);
        Task<bool> DeleteAsync(int serviceId);
        Task<EventCostBreakdownDto> GetEventCostBreakdownAsync(int eventId);
        Task<IEnumerable<ExternalServiceDto>> SearchByProviderAsync(string providerName);
    }
}

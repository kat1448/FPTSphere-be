using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.ExternalService;
using BusinessLayer.DTOs.ExternalServices;
using BusinessLayer.Services.Interfaces;
using DataLayer.Models;
using DataLayer.Repositories.Interfaces;

namespace BusinessLayer.Services.Implementations
{
    public class ExternalServiceService : IExternalServiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ExternalServiceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ExternalServiceDto>> GetByEventIdAsync(int eventId)
        {
            // ✅ FIX: Use Expression instead of int
            var eventExists = await _unitOfWork.Events.ExistsAsync(e => e.EventId == eventId);

            if (!eventExists)
                throw new KeyNotFoundException($"Event with ID {eventId} not found");

            var services = await _unitOfWork.ExternalServices.GetByEventIdAsync(eventId);

            return _mapper.Map<IEnumerable<ExternalServiceDto>>(services);
        }

        public async Task<ExternalServiceDto?> GetByIdAsync(int serviceId)
        {
            var service = await _unitOfWork.ExternalServices.GetByIdWithEventAsync(serviceId);

            return service == null ? null : _mapper.Map<ExternalServiceDto>(service);
        }

        public async Task<ExternalServiceDto> CreateAsync(int eventId, CreateExternalServiceDto dto)
        {
            // ✅ FIX: Use Expression instead of int
            var eventExists = await _unitOfWork.Events.ExistsAsync(e => e.EventId == eventId);

            if (!eventExists)
                throw new KeyNotFoundException($"Event with ID {eventId} not found");

            // Map and create
            var service = _mapper.Map<ExternalService>(dto);
            service.EventId = eventId;

            await _unitOfWork.ExternalServices.AddAsync(service);
            await _unitOfWork.SaveChangesAsync();

            // Reload with event info
            var created = await _unitOfWork.ExternalServices.GetByIdWithEventAsync(service.ServiceId);
            return _mapper.Map<ExternalServiceDto>(created!);
        }

        public async Task<ExternalServiceDto> UpdateAsync(int serviceId, UpdateExternalServiceDto dto)
        {
            var service = await _unitOfWork.ExternalServices.GetByIdAsync(serviceId);

            if (service == null)
                throw new KeyNotFoundException($"External service with ID {serviceId} not found");

            // Map updates (AutoMapper will only map non-null values)
            _mapper.Map(dto, service);

            await _unitOfWork.ExternalServices.UpdateAsync(service);
            await _unitOfWork.SaveChangesAsync();

            // Reload with event info
            var updated = await _unitOfWork.ExternalServices.GetByIdWithEventAsync(serviceId);
            return _mapper.Map<ExternalServiceDto>(updated!);
        }

        public async Task<bool> DeleteAsync(int serviceId)
        {
            // ✅ FIX: Use Expression to check exists
            var exists = await _unitOfWork.ExternalServices.ExistsAsync(es => es.ServiceId == serviceId);

            if (!exists)
                throw new KeyNotFoundException($"External service with ID {serviceId} not found");

            // ✅ FIX: Get entity first, then delete
            var service = await _unitOfWork.ExternalServices.GetByIdAsync(serviceId);
            if (service != null)
            {
                await _unitOfWork.ExternalServices.DeleteAsync(service);
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        public async Task<EventCostBreakdownDto> GetEventCostBreakdownAsync(int eventId)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdWithDetailsAsync(eventId);

            if (eventEntity == null)
                throw new KeyNotFoundException($"Event with ID {eventId} not found");

            // Get external services
            var services = await _unitOfWork.ExternalServices.GetByEventIdAsync(eventId);
            var externalServicesCost = services.Where(s => s.Cost.HasValue).Sum(s => s.Cost ?? 0);

            // Calculate total
            var externalLocationCost = eventEntity.ExternalLocation?.Cost;
            var totalCost = (eventEntity.EstimatedCost ?? 0) +
                           (externalLocationCost ?? 0) +
                           externalServicesCost;

            return new EventCostBreakdownDto
            {
                EventId = eventEntity.EventId,
                EventName = eventEntity.EventName,
                EstimatedCost = eventEntity.EstimatedCost ?? 0,
                ExternalLocationCost = externalLocationCost,
                ExternalServicesCost = externalServicesCost,
                TotalCost = totalCost,
                Services = services.Where(s => s.Cost.HasValue).Select(s => new ServiceCostItemDto
                {
                    ProviderName = s.ProviderName,
                    ResourceType = s.ResourceType,
                    Cost = s.Cost ?? 0
                }).ToList()
            };
        }

        public async Task<IEnumerable<ExternalServiceDto>> SearchByProviderAsync(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name cannot be empty");

            var services = await _unitOfWork.ExternalServices.GetByProviderNameAsync(providerName);

            return _mapper.Map<IEnumerable<ExternalServiceDto>>(services);
        }
    }

}

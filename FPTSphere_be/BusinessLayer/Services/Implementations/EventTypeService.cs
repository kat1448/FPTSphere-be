using AutoMapper;
using BusinessLayer.DTOs.EventType;
using BusinessLayer.Services.Interfaces;
using DataLayer.Repositories.Interfaces;

namespace BusinessLayer.Services.Implementations
{
    public class EventTypeService : IEventTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EventTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<EventTypeDto>> GetAllAsync()
        {
            var types = await _unitOfWork.EventTypes.GetAllAsync();
            return _mapper.Map<List<EventTypeDto>>(types.OrderBy(t => t.TypeId).ToList());
        }

        public async Task<EventTypeDto?> GetByIdAsync(int id)
        {
            var type = await _unitOfWork.EventTypes.GetByIdAsync(id);
            return type == null ? null : _mapper.Map<EventTypeDto>(type);
        }
    }
}


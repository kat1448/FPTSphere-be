using AutoMapper;
using BusinessLayer.DTOs.EventCategory;
using BusinessLayer.Services.Interfaces;
using DataLayer.Repositories.Interfaces;

namespace BusinessLayer.Services.Implementations
{
    public class EventCategoryService : IEventCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EventCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<EventCategoryDto>> GetAllAsync()
        {
            var categories = await _unitOfWork.EventCategories.GetAllAsync();
            return _mapper.Map<List<EventCategoryDto>>(categories.OrderBy(c => c.CategoryId).ToList());
        }

        public async Task<EventCategoryDto?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.EventCategories.GetByIdAsync(id);
            return category == null ? null : _mapper.Map<EventCategoryDto>(category);
        }
    }
}


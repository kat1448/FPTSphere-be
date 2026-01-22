using AutoMapper;
using BusinessLayer.DTOs.EventCategory;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class EventCategoryMappingProfile : Profile
    {
        public EventCategoryMappingProfile()
        {
            CreateMap<EventCategory, EventCategoryDto>();
        }
    }
}


using AutoMapper;
using BusinessLayer.DTOs.EventType;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class EventTypeMappingProfile : Profile
    {
        public EventTypeMappingProfile()
        {
            CreateMap<EventType, EventTypeDto>();
        }
    }
}


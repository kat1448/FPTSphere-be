using AutoMapper;
using BusinessLayer.DTOs.EventTask;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class EventTaskMappingProfile : Profile
    {
        public EventTaskMappingProfile()
        {
            CreateMap<EventTask, EventTaskDto>()
                .ForMember(d => d.AssignedToName, opt => opt.MapFrom(s => s.AssignedToNavigation != null ? s.AssignedToNavigation.FullName : null))
                .ForMember(d => d.AssignByName, opt => opt.MapFrom(s => s.AssignByNavigation != null ? s.AssignByNavigation.FullName : null));

            CreateMap<EventTask, EventTaskWithEventDto>();

            CreateMap<CreateEventTaskDto, EventTask>();
        }
    }
}

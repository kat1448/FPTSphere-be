using AutoMapper;
using BusinessLayer.DTOs.EventTask;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class EventTaskMappingProfile : Profile
    {
        public EventTaskMappingProfile()
        {
            CreateMap<EventTask, EventTaskDto>();
            CreateMap<EventTask, EventTaskWithEventDto>();
        }
    }
}

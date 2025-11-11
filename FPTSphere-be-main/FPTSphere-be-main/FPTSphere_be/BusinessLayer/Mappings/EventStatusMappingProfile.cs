using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.EventStatus;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class EventStatusMappingProfile : Profile
    {
        public EventStatusMappingProfile()
        {
            CreateMap<EventStatus, EventStatusDto>();

            CreateMap<CreateEventStatusDto, EventStatus>()
                .ForMember(d => d.StatusId, opt => opt.Ignore())
                .ForMember(d => d.Events, opt => opt.Ignore());

            CreateMap<UpdateEventStatusDto, EventStatus>()
                .ForMember(d => d.StatusId, opt => opt.Ignore())
                .ForMember(d => d.Events, opt => opt.Ignore());
        }
    }
}

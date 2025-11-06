using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.Event;
using BusinessLayer.DTOs.User;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
            CreateMap<Event, EventDto>()
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.ManagerUser.FullName))
                .ForMember(dest => dest.ParentEventName, opt => opt.MapFrom(src => src.ParentEvent != null ? src.ParentEvent.EventName : null));

            CreateMap<CreateEventDto, Event>();
        }
    }
}

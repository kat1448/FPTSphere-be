using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.EventResource;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class EventResourceMappingProfile : Profile
    {
        public EventResourceMappingProfile()
        {
            // Entity to DTO
            CreateMap<EventResource, EventResourceResponseDto>()
                .ForMember(dest => dest.EventName,
                    opt => opt.MapFrom(src => src.Event != null ? src.Event.EventName : string.Empty))
                .ForMember(dest => dest.ResourceName,
                    opt => opt.MapFrom(src => src.Resource != null ? src.Resource.Name : string.Empty))
                .ForMember(dest => dest.ResourceType,
                    opt => opt.MapFrom(src => src.Resource != null ? src.Resource.Type : null))
                .ForMember(dest => dest.AvailableQuantity,
                    opt => opt.MapFrom(src => src.Resource != null ? src.Resource.Quantity : 0))
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.Resource != null && (src.Resource.IsActive ?? false)));

            // DTO to Entity - Not needed anymore since we create manually in service
            CreateMap<AssignResourceToEventDto, EventResource>()
                .ForMember(dest => dest.EventId, opt => opt.Ignore())
                .ForMember(dest => dest.EventResourceId, opt => opt.Ignore())
                .ForMember(dest => dest.Event, opt => opt.Ignore())
                .ForMember(dest => dest.Resource, opt => opt.Ignore());

            CreateMap<UpdateEventResourceDto, EventResource>()
                .ForMember(dest => dest.EventResourceId, opt => opt.Ignore())
                .ForMember(dest => dest.EventId, opt => opt.Ignore())
                .ForMember(dest => dest.ResourceId, opt => opt.Ignore())
                .ForMember(dest => dest.Event, opt => opt.Ignore())
                .ForMember(dest => dest.Resource, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

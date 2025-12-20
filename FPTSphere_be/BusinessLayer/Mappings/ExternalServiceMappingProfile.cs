using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.ExternalService;
using BusinessLayer.DTOs.ExternalServices;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class ExternalServiceMappingProfile : Profile
    {
        public ExternalServiceMappingProfile()
        {
            // Entity to DTO mappings
            CreateMap<ExternalService, ExternalServiceDto>()
                .ForMember(dest => dest.EventName,
                    opt => opt.MapFrom(src => src.Event != null ? src.Event.EventName : string.Empty));

            CreateMap<ExternalService, ExternalServiceListDto>();

            // DTO to Entity mappings
            CreateMap<CreateExternalServiceDto, ExternalService>()
                .ForMember(dest => dest.ServiceId, opt => opt.Ignore())
                .ForMember(dest => dest.EventId, opt => opt.Ignore())
                .ForMember(dest => dest.Event, opt => opt.Ignore());

            CreateMap<UpdateExternalServiceDto, ExternalService>()
                .ForMember(dest => dest.ServiceId, opt => opt.Ignore())
                .ForMember(dest => dest.EventId, opt => opt.Ignore())
                .ForMember(dest => dest.Event, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.ExternalLocation;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class ExternalLocationMappingProfile : Profile
    {
        public ExternalLocationMappingProfile()
        {
            CreateMap<ExternalLocation, ExternalLocationDto>();

            CreateMap<CreateExternalLocationDto, ExternalLocation>()
                .ForMember(d => d.ExternalLocationId, opt => opt.Ignore())
                .ForMember(d => d.Events, opt => opt.Ignore());

            CreateMap<UpdateExternalLocationDto, ExternalLocation>()
                .ForMember(d => d.ExternalLocationId, opt => opt.Ignore())
                .ForMember(d => d.Events, opt => opt.Ignore());
        }
    }

}

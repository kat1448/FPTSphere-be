using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.Resource;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class ResourceMappingProfile : Profile
    {
        public ResourceMappingProfile()
        {
            CreateMap<Resource, ResourceDto>();

            CreateMap<CreateResourceDto, Resource>()
                .ForMember(d => d.ResourceId, opt => opt.Ignore())
                .ForMember(d => d.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(d => d.EventResources, opt => opt.Ignore());

            CreateMap<UpdateResourceDto, Resource>()
                .ForMember(d => d.ResourceId, opt => opt.Ignore())
                .ForMember(d => d.IsActive, opt => opt.Ignore())
                .ForMember(d => d.EventResources, opt => opt.Ignore());
        }
    }

}

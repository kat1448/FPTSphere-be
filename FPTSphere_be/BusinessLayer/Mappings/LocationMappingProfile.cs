using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.Location;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class LocationMappingProfile : Profile
    {
        public LocationMappingProfile()
        {
            CreateMap<Location, LocationDto>();

            CreateMap<CreateLocationDto, Location>()
                .ForMember(d => d.LocationId, opt => opt.Ignore())
                .ForMember(d => d.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(d => d.Events, opt => opt.Ignore());

            CreateMap<UpdateLocationDto, Location>()
                .ForMember(d => d.LocationId, opt => opt.Ignore())
                .ForMember(d => d.IsActive, opt => opt.Ignore())
                .ForMember(d => d.Events, opt => opt.Ignore());
        }
    }
}

using System;
using System.Linq;
using AutoMapper;
using BusinessLayer.DTOs.Event;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class PublicEventMappingProfile : Profile
    {
        public PublicEventMappingProfile()
        {
            // Main Event to PublicEventDto
            CreateMap<Event, PublicEventDto>()
                .ForMember(d => d.LocationName, opt => opt.MapFrom(s =>
                    s.Location != null ? s.Location.Name :
                    s.ExternalLocation != null ? s.ExternalLocation.Name :
                    "TBA"))
                .ForMember(d => d.LocationAddress, opt => opt.MapFrom(s =>
                    s.Location != null ? s.Location.RoomNumber :
                    s.ExternalLocation != null ? s.ExternalLocation.Address :
                    null))
                .ForMember(d => d.StatusName, opt => opt.MapFrom(s =>
                    s.Status != null ? s.Status.StatusName : "Unknown"))
                .ForMember(d => d.IsOngoing, opt => opt.MapFrom(s =>
                    s.StartTime <= DateTime.Now && s.EndTime >= DateTime.Now))
                .ForMember(d => d.IsUpcoming, opt => opt.MapFrom(s =>
                    s.StartTime > DateTime.Now))
                .ForMember(d => d.SubEvents, opt => opt.Ignore()); // Handled separately

            // Sub-Event to PublicSubEventDto
            CreateMap<Event, PublicSubEventDto>()
                .ForMember(d => d.LocationName, opt => opt.MapFrom(s =>
                    s.Location != null ? s.Location.Name :
                    s.ExternalLocation != null ? s.ExternalLocation.Name :
                    "TBA"))
                .ForMember(d => d.Building, opt => opt.MapFrom(s =>
    s.Location != null ? s.Location.Building : null))
.ForMember(d => d.RoomNumber, opt => opt.MapFrom(s =>
    s.Location != null ? s.Location.RoomNumber : null));
        }
    }
}
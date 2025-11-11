using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.Event;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class EventMappingProfile : Profile
    {
        public EventMappingProfile()
        {
            CreateMap<Event, EventDto>()
                .ForMember(d => d.Creator, opt => opt.MapFrom(s => s.CreatedByNavigation))
                .ForMember(d => d.Location, opt => opt.MapFrom(s => s.Location))
                .ForMember(d => d.ExternalLocation, opt => opt.MapFrom(s => s.ExternalLocation))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status));

            CreateMap<CreateEventDto, Event>()
                .ForMember(d => d.EventId, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.StatusId, opt => opt.Ignore())
                .ForMember(d => d.ParentEventId, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
                .ForMember(d => d.IsDeleted, opt => opt.Ignore())
                .ForMember(d => d.CreatedByNavigation, opt => opt.Ignore())
                .ForMember(d => d.Status, opt => opt.Ignore())
                .ForMember(d => d.Location, opt => opt.Ignore())
                .ForMember(d => d.ExternalLocation, opt => opt.Ignore())
                .ForMember(d => d.ParentEvent, opt => opt.Ignore())
                .ForMember(d => d.InverseParentEvent, opt => opt.Ignore())
                .ForMember(d => d.Template, opt => opt.Ignore())
                .ForMember(d => d.AttendanceTokens, opt => opt.Ignore())
                .ForMember(d => d.EventAiresults, opt => opt.Ignore())
                .ForMember(d => d.EventApprovals, opt => opt.Ignore())
                .ForMember(d => d.EventAttendances, opt => opt.Ignore())
                .ForMember(d => d.EventInvitations, opt => opt.Ignore())
                .ForMember(d => d.EventLogs, opt => opt.Ignore())
                .ForMember(d => d.EventResources, opt => opt.Ignore())
                .ForMember(d => d.EventTasks, opt => opt.Ignore())
                .ForMember(d => d.ExternalServices, opt => opt.Ignore())
                .ForMember(d => d.StudentFeedbackHeaders, opt => opt.Ignore());

            CreateMap<UpdateEventDto, Event>()
                .ForMember(d => d.EventId, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.StatusId, opt => opt.Ignore())
                .ForMember(d => d.ParentEventId, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
                .ForMember(d => d.IsDeleted, opt => opt.Ignore())
                .ForMember(d => d.CreatedByNavigation, opt => opt.Ignore())
                .ForMember(d => d.Status, opt => opt.Ignore())
                .ForMember(d => d.Location, opt => opt.Ignore())
                .ForMember(d => d.ExternalLocation, opt => opt.Ignore())
                .ForMember(d => d.ParentEvent, opt => opt.Ignore())
                .ForMember(d => d.InverseParentEvent, opt => opt.Ignore())
                .ForMember(d => d.Template, opt => opt.Ignore())
                .ForMember(d => d.AttendanceTokens, opt => opt.Ignore())
                .ForMember(d => d.EventAiresults, opt => opt.Ignore())
                .ForMember(d => d.EventApprovals, opt => opt.Ignore())
                .ForMember(d => d.EventAttendances, opt => opt.Ignore())
                .ForMember(d => d.EventInvitations, opt => opt.Ignore())
                .ForMember(d => d.EventLogs, opt => opt.Ignore())
                .ForMember(d => d.EventResources, opt => opt.Ignore())
                .ForMember(d => d.EventTasks, opt => opt.Ignore())
                .ForMember(d => d.ExternalServices, opt => opt.Ignore())
                .ForMember(d => d.StudentFeedbackHeaders, opt => opt.Ignore());
        }
    }

}

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
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))
                .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.CategoryName : null))
                .ForMember(d => d.TypeName, opt => opt.MapFrom(s => s.Type != null ? s.Type.TypeName : null));

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

            CreateMap<Event, SubEventDto>()
               // Location: Ưu tiên location của sub-event, nếu không có thì lấy từ parent
               .ForMember(d => d.LocationName, opt => opt.MapFrom(s => 
                   s.Location != null ? s.Location.Name : 
                   (s.ParentEvent != null && s.ParentEvent.Location != null ? s.ParentEvent.Location.Name : null)))
               .ForMember(d => d.ExternalLocationName, opt => opt.MapFrom(s => 
                   s.ExternalLocation != null ? s.ExternalLocation.Name : 
                   (s.ParentEvent != null && s.ParentEvent.ExternalLocation != null ? s.ParentEvent.ExternalLocation.Name : null)))
               // LocationId và ExternalLocationId: Ưu tiên của sub-event, nếu không có thì lấy từ parent
               .ForMember(d => d.LocationId, opt => opt.MapFrom(s => 
                   s.LocationId.HasValue ? s.LocationId : 
                   (s.ParentEvent != null && s.ParentEvent.LocationId.HasValue ? s.ParentEvent.LocationId : null)))
               .ForMember(d => d.ExternalLocationId, opt => opt.MapFrom(s => 
                   s.ExternalLocationId.HasValue ? s.ExternalLocationId : 
                   (s.ParentEvent != null && s.ParentEvent.ExternalLocationId.HasValue ? s.ParentEvent.ExternalLocationId : null)))
               .ForMember(d => d.ParentEventId, opt => opt.MapFrom(s => s.ParentEventId.HasValue ? s.ParentEventId.Value : 0))
               .ForMember(d => d.ParentEventName, opt => opt.MapFrom(s => s.ParentEvent != null ? s.ParentEvent.EventName : string.Empty))
               .ForMember(d => d.StatusName, opt => opt.MapFrom(s => s.Status != null ? s.Status.StatusName : string.Empty))
               .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.CategoryName : null))
               .ForMember(d => d.TypeName, opt => opt.MapFrom(s => s.Type != null ? s.Type.TypeName : null))
               .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt.HasValue ? s.CreatedAt.Value : DateTime.Now))
               .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(s => s.UpdatedAt));

            CreateMap<CreateSubEventDto, Event>()
                .ForMember(d => d.EventId, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.StatusId, opt => opt.Ignore())
                .ForMember(d => d.ParentEventId, opt => opt.Ignore())
                .ForMember(d => d.ExpectedAttendees, opt => opt.Ignore())
                .ForMember(d => d.EstimatedCost, opt => opt.Ignore())
                .ForMember(d => d.TemplateId, opt => opt.Ignore())
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
                .ForMember(d => d.StudentFeedbackHeaders, opt => opt.Ignore())
                .ForMember(d => d.Category, opt => opt.Ignore())
                .ForMember(d => d.Type, opt => opt.Ignore());

            CreateMap<UpdateSubEventDto, Event>()
                .ForMember(d => d.EventId, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.StatusId, opt => opt.Ignore())
                .ForMember(d => d.ParentEventId, opt => opt.Ignore())
                .ForMember(d => d.ExpectedAttendees, opt => opt.Ignore())
                .ForMember(d => d.EstimatedCost, opt => opt.Ignore())
                .ForMember(d => d.TemplateId, opt => opt.Ignore())
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
                .ForMember(d => d.StudentFeedbackHeaders, opt => opt.Ignore())
                .ForMember(d => d.Category, opt => opt.Ignore())
                .ForMember(d => d.Type, opt => opt.Ignore());

            // Mapping cho EventAttendance
            CreateMap<EventAttendance, EventAttendanceDto>();
        }
    }

}

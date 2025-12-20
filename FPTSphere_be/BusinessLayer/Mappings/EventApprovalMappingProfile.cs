using AutoMapper;
using BusinessLayer.DTOs.EventApproval;
using DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Mappings
{
    public class EventApprovalMappingProfile : Profile
    {
        public EventApprovalMappingProfile()
        {
            CreateMap<EventApproval, EventApprovalDto>()
                .ForMember(d => d.EventName, opt => opt.Ignore())
                .ForMember(d => d.DirectorName, opt => opt.Ignore());
        }
    }
}

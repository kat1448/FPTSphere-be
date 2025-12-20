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
    public class EventInvitationProfile : Profile
    {
        public EventInvitationProfile()
        {
            // Entity -> DTO
            CreateMap<EventInvitation, EventInvitationDto>()
                .ForMember(dest => dest.SentByName,
                    opt => opt.MapFrom(src => src.SentByNavigation.FullName))
                .ForMember(dest => dest.SentByEmail,
                    opt => opt.MapFrom(src => src.SentByNavigation.Email))
                // IsMandatory hiện chưa lưu trong DB, nên mặc định false
                .ForMember(dest => dest.IsMandatory, opt => opt.Ignore());

            // Nếu sau này bạn muốn map từ DTO tạo mới sang entity:
            // CreateMap<CreateEventInvitationDto, EventInvitation>();
        }
    }
}

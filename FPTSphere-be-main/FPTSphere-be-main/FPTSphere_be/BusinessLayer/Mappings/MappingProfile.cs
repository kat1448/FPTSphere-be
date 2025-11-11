using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // ========================================
            // USER ENTITY → RESPONSE DTOs
            // ========================================

            // User → UserResponse (Full details)
            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.RoleName,
                    opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.IsAuthorized,
                    opt => opt.MapFrom(src => src.IsAuthorized ?? true))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.CreatedAt ?? DateTime.Now));

            // User → UserListResponse (For lists)
            CreateMap<User, UserListResponse>()
                .ForMember(dest => dest.RoleName,
                    opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.IsAuthorized,
                    opt => opt.MapFrom(src => src.IsAuthorized ?? true))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.CreatedAt ?? DateTime.Now));

            // User → UserMinimalResponse (Minimal info)
            CreateMap<User, UserMinimalResponse>()
                .ForMember(dest => dest.RoleName,
                    opt => opt.MapFrom(src => src.Role.RoleName));

            // ========================================
            // REQUEST DTOs → USER ENTITY
            // ========================================

            // CreateUserRequest → User
            CreateMap<CreateUserRequest, User>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.GoogleId, opt => opt.Ignore())
                .ForMember(dest => dest.IsAuthorized, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Role, opt => opt.Ignore());

            // UpdateUserRequest → User (for updates)
            CreateMap<UpdateUserRequest, User>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.GoogleId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Role, opt => opt.Ignore());

            // ========================================
            // SYSTEM ROLE MAPPINGS
            // ========================================

            // SystemRole → SystemRoleResponse
            CreateMap<SystemRole, SystemRoleResponse>();
        }
    }
}

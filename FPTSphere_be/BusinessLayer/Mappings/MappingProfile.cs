using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.User;
using DataLayer.Models;

namespace BusinessLayer.Mappings
{
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Configures AutoMapper mappings for user domain models and DTOs.
        /// </summary>
        /// <remarks>
        /// Defines a bidirectional mapping between <c>User</c> and <c>UserDto</c>, and maps <c>CreateUserDto</c> and <c>UpdateUserDto</c> to <c>User</c>.
        /// </remarks>
        public MappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services
{
    public class UserService : IUserService
    {
        private readonly EventDbContext _context;
        private readonly IMapper _mapper;

        public UserService(EventDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //Create user
        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            user.IsAuthorized = true;
            user.DepartmentMajor = "User";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        //Read all users
        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _context.Users.ToListAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        //Read user by id
        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        //Update user
        public async Task<bool> UpdateAsync(UpdateUserDto dto)
        {
            var existingUser = await _context.Users.FindAsync(dto.UserId);
            if (existingUser == null) return false;

            _mapper.Map(dto, existingUser);

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

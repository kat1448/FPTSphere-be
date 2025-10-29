using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.User;
using BusinessLayer.Services.Interfaces;
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

        /// <summary>
        /// Initializes a new instance of <see cref="UserService"/> with the provided database context and object mapper.
        /// </summary>
        public UserService(EventDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new user from the provided DTO and persists it to the database.
        /// </summary>
        /// <param name="dto">The data used to create the user.</param>
        /// <returns>The persisted user's DTO, reflecting any database-assigned values such as the new Id.</returns>
        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            user.IsAuthorized = true;
            user.DepartmentMajor = "User";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>An IEnumerable&lt;UserDto&gt; containing all users.</returns>
        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _context.Users.ToListAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        /// <summary>
        /// Gets a user by its primary key and returns its DTO representation.
        /// </summary>
        /// <param name="id">The user's primary key.</param>
        /// <returns>The mapped <see cref="UserDto"/> if a user with the specified id exists, or <c>null</c> otherwise.</returns>
        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Updates an existing user's data with values from the provided DTO.
        /// </summary>
        /// <param name="dto">DTO containing the user ID and fields to update.</param>
        /// <returns>`true` if the user was found and updated, `false` if no user with the specified ID exists.</returns>
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
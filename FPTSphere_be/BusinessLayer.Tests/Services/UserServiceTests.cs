using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.DTOs.User;
using BusinessLayer.Mappings;
using BusinessLayer.Services;
using DataLayer.Data;
using DataLayer.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BusinessLayer.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly EventDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EventDbContext(options);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _userService = new UserService(_context, _mapper);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateAsync_WithValidDto_ShouldCreateUser()
        {
            var dto = new CreateUserDto
            {
                FullName = "John Doe",
                Email = "john.doe@example.com",
                DepartmentMajor = "Computer Science"
            };

            var result = await _userService.CreateAsync(dto);

            result.Should().NotBeNull();
            result.UserId.Should().BeGreaterThan(0);
            result.FullName.Should().Be("John Doe");
            result.DepartmentMajor.Should().Be("User");
        }

        [Fact]
        public async Task CreateAsync_ShouldSetIsAuthorizedToTrue()
        {
            var dto = new CreateUserDto
            {
                FullName = "Jane Smith",
                Email = "jane.smith@example.com",
                DepartmentMajor = "Engineering"
            };

            var result = await _userService.CreateAsync(dto);

            var userInDb = await _context.Users.FindAsync(result.UserId);
            userInDb.Should().NotBeNull();
            userInDb!.IsAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_WithNoUsers_ShouldReturnEmptyList()
        {
            var result = await _userService.GetAllAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleUsers_ShouldReturnAllUsers()
        {
            var users = new List<User>
            {
                new User { FullName = "User 1", Email = "user1@test.com", IsAuthorized = true },
                new User { FullName = "User 2", Email = "user2@test.com", IsAuthorized = false }
            };
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            var result = await _userService.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnUser()
        {
            var user = new User
            {
                FullName = "Test User",
                Email = "test@test.com",
                IsAuthorized = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var result = await _userService.GetByIdAsync(user.UserId);

            result.Should().NotBeNull();
            result!.FullName.Should().Be("Test User");
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            var result = await _userService.GetByIdAsync(999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_WithExistingUser_ShouldUpdateUser()
        {
            var user = new User
            {
                FullName = "Original",
                Email = "original@test.com",
                IsAuthorized = false
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateUserDto
            {
                UserId = user.UserId,
                FullName = "Updated",
                Email = "updated@test.com"
            };

            var result = await _userService.UpdateAsync(updateDto);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingUser_ShouldReturnFalse()
        {
            var updateDto = new UpdateUserDto
            {
                UserId = 999,
                FullName = "Non Existing"
            };

            var result = await _userService.UpdateAsync(updateDto);

            result.Should().BeFalse();
        }
    }
}
using AutoMapper;
using BusinessLayer.DTOs.User;
using BusinessLayer.Mappings;
using BusinessLayer.Services;
using DataLayer.Data;
using DataLayer.Models;
using FluentAssertions;
using FPTSphere_be.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FPTSphere_be.Tests.BusinessLayer.Services
{
    public class UserServiceTests
    {
        private readonly IMapper _mapper;

        public UserServiceTests()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateUserWithCorrectProperties()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);
            
            var createDto = new CreateUserDto
            {
                FullName = "John Doe",
                Email = "john@example.com",
                DepartmentMajor = "Engineering"
            };

            // Act
            var result = await service.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().BeGreaterThan(0);
            result.FullName.Should().Be("John Doe");
            result.Email.Should().Be("john@example.com");
        }

        [Fact]
        public async Task CreateAsync_ShouldSetIsAuthorizedToTrue()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);
            
            var createDto = new CreateUserDto
            {
                FullName = "Jane Smith",
                Email = "jane@example.com",
                DepartmentMajor = "Marketing"
            };

            // Act
            var result = await service.CreateAsync(createDto);

            // Assert
            var userInDb = await context.Users.FindAsync(result.UserId);
            userInDb.Should().NotBeNull();
            userInDb!.IsAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_ShouldSetDepartmentMajorToUser()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);
            
            var createDto = new CreateUserDto
            {
                FullName = "Test User",
                Email = "test@example.com",
                DepartmentMajor = "Science"
            };

            // Act
            var result = await service.CreateAsync(createDto);

            // Assert
            var userInDb = await context.Users.FindAsync(result.UserId);
            userInDb.Should().NotBeNull();
            userInDb!.DepartmentMajor.Should().Be("User");
        }

        [Fact]
        public async Task CreateAsync_ShouldPersistUserToDatabase()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);
            
            var createDto = new CreateUserDto
            {
                FullName = "Persisted User",
                Email = "persisted@example.com",
                DepartmentMajor = "Business"
            };

            // Act
            var result = await service.CreateAsync(createDto);

            // Assert
            var usersInDb = await context.Users.ToListAsync();
            usersInDb.Should().HaveCount(1);
            usersInDb[0].Email.Should().Be("persisted@example.com");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);
            
            var users = new List<User>
            {
                new User { UserId = 1, FullName = "User 1", Email = "user1@test.com", GoogleId = "g1", UserType = "Student", IsAuthorized = true },
                new User { UserId = 2, FullName = "User 2", Email = "user2@test.com", GoogleId = "g2", UserType = "Teacher", IsAuthorized = true },
                new User { UserId = 3, FullName = "User 3", Email = "user3@test.com", GoogleId = "g3", UserType = "Admin", IsAuthorized = false }
            };
            
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(u => u.FullName == "User 1");
            result.Should().Contain(u => u.FullName == "User 2");
            result.Should().Contain(u => u.FullName == "User 3");
        }

        [Fact]
        public async Task GetAllAsync_WhenNoUsers_ShouldReturnEmptyList()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserExists_ShouldReturnUserDto()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);
            
            var user = new User
            {
                UserId = 10,
                FullName = "Specific User",
                Email = "specific@test.com",
                GoogleId = "specific123",
                UserType = "Student",
                IsAuthorized = true,
                DepartmentMajor = "Physics"
            };
            
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByIdAsync(10);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(10);
            result.FullName.Should().Be("Specific User");
            result.Email.Should().Be("specific@test.com");
            result.DepartmentMajor.Should().Be("Physics");
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_WhenUserExists_ShouldUpdateAndReturnTrue()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);
            
            var user = new User
            {
                UserId = 20,
                FullName = "Original Name",
                Email = "original@test.com",
                GoogleId = "orig123",
                UserType = "Student",
                IsAuthorized = true,
                DepartmentMajor = "Math"
            };
            
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var updateDto = new UpdateUserDto
            {
                UserId = 20,
                FullName = "Updated Name",
                Email = "updated@test.com",
                Department = "Science",
                IsAuthorized = false
            };

            // Act
            var result = await service.UpdateAsync(updateDto);

            // Assert
            result.Should().BeTrue();
            
            var updatedUser = await context.Users.FindAsync(20);
            updatedUser.Should().NotBeNull();
            updatedUser!.FullName.Should().Be("Updated Name");
            updatedUser.Email.Should().Be("updated@test.com");
            updatedUser.IsAuthorized.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_WhenUserDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);
            
            var updateDto = new UpdateUserDto
            {
                UserId = 999,
                FullName = "Non Existent",
                Email = "none@test.com"
            };

            // Act
            var result = await service.UpdateAsync(updateDto);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldNotCreateNewUser()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);
            
            var updateDto = new UpdateUserDto
            {
                UserId = 888,
                FullName = "Should Not Create",
                Email = "nocreate@test.com"
            };

            var initialCount = await context.Users.CountAsync();

            // Act
            await service.UpdateAsync(updateDto);

            // Assert
            var finalCount = await context.Users.CountAsync();
            finalCount.Should().Be(initialCount);
        }

        [Fact]
        public async Task CreateAsync_WithEmptyStrings_ShouldHandleGracefully()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);
            
            var createDto = new CreateUserDto
            {
                FullName = "",
                Email = "",
                DepartmentMajor = ""
            };

            // Act
            var result = await service.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnUserDtosWithCorrectMapping()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new UserService(context, _mapper);
            
            var user = new User
            {
                UserId = 100,
                FullName = "Mapped User",
                Email = "mapped@test.com",
                GoogleId = "mapped123",
                UserType = "Teacher",
                IsAuthorized = true,
                DepartmentMajor = "History",
                ClassCode = "HIST101"
            };
            
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            var userDto = result.First();
            userDto.UserId.Should().Be(100);
            userDto.FullName.Should().Be("Mapped User");
            userDto.Email.Should().Be("mapped@test.com");
            userDto.DepartmentMajor.Should().Be("History");
        }
    }
}
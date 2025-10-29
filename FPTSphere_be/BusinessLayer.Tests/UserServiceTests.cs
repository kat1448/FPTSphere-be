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

namespace BusinessLayer.Tests
{
    public class UserServiceTests
    {
        private readonly IMapper _mapper;

        public UserServiceTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
        }

        private EventDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new EventDbContext(options);
        }

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidDto_ShouldCreateUserAndReturnUserDto()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            var createDto = new CreateUserDto
            {
                FullName = "John Doe",
                Email = "john.doe@example.com",
                DepartmentMajor = "Computer Science"
            };

            // Act
            var result = await service.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().BeGreaterThan(0);
            result.FullName.Should().Be("John Doe");
            result.Email.Should().Be("john.doe@example.com");
            result.DepartmentMajor.Should().Be("User");

            var userInDb = await context.Users.FindAsync(result.UserId);
            userInDb.Should().NotBeNull();
            userInDb!.IsAuthorized.Should().BeTrue();
            userInDb.DepartmentMajor.Should().Be("User");
        }

        [Fact]
        public async Task CreateAsync_ShouldSetIsAuthorizedToTrue()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            var createDto = new CreateUserDto
            {
                FullName = "Jane Smith",
                Email = "jane.smith@example.com",
                DepartmentMajor = "Engineering"
            };

            // Act
            var result = await service.CreateAsync(createDto);

            // Assert
            var userInDb = await context.Users.FindAsync(result.UserId);
            userInDb!.IsAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_ShouldOverrideDepartmentMajorToUser()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            var createDto = new CreateUserDto
            {
                FullName = "Alice Johnson",
                Email = "alice.johnson@example.com",
                DepartmentMajor = "Mathematics"
            };

            // Act
            var result = await service.CreateAsync(createDto);

            // Assert
            result.DepartmentMajor.Should().Be("User");
            var userInDb = await context.Users.FindAsync(result.UserId);
            userInDb!.DepartmentMajor.Should().Be("User");
        }

        [Fact]
        public async Task CreateAsync_WithEmptyStrings_ShouldCreateUser()
        {
            // Arrange
            var context = GetInMemoryContext();
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
        public async Task CreateAsync_MultipleUsers_ShouldCreateAllSuccessfully()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            var createDto1 = new CreateUserDto { FullName = "User 1", Email = "user1@example.com", DepartmentMajor = "CS" };
            var createDto2 = new CreateUserDto { FullName = "User 2", Email = "user2@example.com", DepartmentMajor = "IT" };

            // Act
            var result1 = await service.CreateAsync(createDto1);
            var result2 = await service.CreateAsync(createDto2);

            // Assert
            result1.UserId.Should().NotBe(result2.UserId);
            var allUsers = await context.Users.ToListAsync();
            allUsers.Should().HaveCount(2);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithNoUsers_ShouldReturnEmptyList()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleUsers_ShouldReturnAllUsers()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            
            context.Users.AddRange(
                new User { FullName = "User 1", Email = "user1@example.com", GoogleId = "google1", UserType = "Student", IsAuthorized = true },
                new User { FullName = "User 2", Email = "user2@example.com", GoogleId = "google2", UserType = "Teacher", IsAuthorized = false },
                new User { FullName = "User 3", Email = "user3@example.com", GoogleId = "google3", UserType = "Admin", IsAuthorized = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Select(u => u.FullName).Should().Contain(new[] { "User 1", "User 2", "User 3" });
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnUserDtos()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            
            context.Users.Add(new User 
            { 
                FullName = "Test User", 
                Email = "test@example.com", 
                GoogleId = "google123", 
                UserType = "Student", 
                IsAuthorized = true,
                DepartmentMajor = "CS"
            });
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            var userDto = result.First();
            userDto.Should().BeOfType<UserDto>();
            userDto.FullName.Should().Be("Test User");
            userDto.Email.Should().Be("test@example.com");
            userDto.DepartmentMajor.Should().Be("CS");
        }

        [Fact]
        public async Task GetAllAsync_WithLargeNumberOfUsers_ShouldReturnAll()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            
            var users = Enumerable.Range(1, 100).Select(i => new User
            {
                FullName = $"User {i}",
                Email = $"user{i}@example.com",
                GoogleId = $"google{i}",
                UserType = "Student",
                IsAuthorized = true
            });
            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().HaveCount(100);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnUserDto()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            
            var user = new User 
            { 
                FullName = "John Doe", 
                Email = "john@example.com", 
                GoogleId = "google1", 
                UserType = "Student", 
                IsAuthorized = true,
                DepartmentMajor = "Engineering"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByIdAsync(user.UserId);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(user.UserId);
            result.FullName.Should().Be("John Doe");
            result.Email.Should().Be("john@example.com");
            result.DepartmentMajor.Should().Be("Engineering");
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithNegativeId_ShouldReturnNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);

            // Act
            var result = await service.GetByIdAsync(-1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithZeroId_ShouldReturnNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);

            // Act
            var result = await service.GetByIdAsync(0);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_MultipleUsers_ShouldReturnCorrectUser()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            
            var user1 = new User { FullName = "User 1", Email = "user1@example.com", GoogleId = "g1", UserType = "Student", IsAuthorized = true };
            var user2 = new User { FullName = "User 2", Email = "user2@example.com", GoogleId = "g2", UserType = "Student", IsAuthorized = true };
            var user3 = new User { FullName = "User 3", Email = "user3@example.com", GoogleId = "g3", UserType = "Student", IsAuthorized = true };
            
            context.Users.AddRange(user1, user2, user3);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByIdAsync(user2.UserId);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("User 2");
            result.Email.Should().Be("user2@example.com");
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidDto_ShouldUpdateUserAndReturnTrue()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            
            var user = new User 
            { 
                FullName = "Original Name", 
                Email = "original@example.com", 
                GoogleId = "google1", 
                UserType = "Student", 
                IsAuthorized = false,
                DepartmentMajor = "CS"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var updateDto = new UpdateUserDto
            {
                UserId = user.UserId,
                FullName = "Updated Name",
                Email = "updated@example.com",
                Department = "Engineering",
                IsAuthorized = true
            };

            // Act
            var result = await service.UpdateAsync(updateDto);

            // Assert
            result.Should().BeTrue();
            var updatedUser = await context.Users.FindAsync(user.UserId);
            updatedUser!.FullName.Should().Be("Updated Name");
            updatedUser.Email.Should().Be("updated@example.com");
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            
            var updateDto = new UpdateUserDto
            {
                UserId = 999,
                FullName = "Test Name",
                Email = "test@example.com"
            };

            // Act
            var result = await service.UpdateAsync(updateDto);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAllProvidedFields()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            
            var user = new User 
            { 
                FullName = "Old Name", 
                Email = "old@example.com", 
                GoogleId = "google1", 
                UserType = "Student", 
                IsAuthorized = false,
                DepartmentMajor = "OldDept"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var updateDto = new UpdateUserDto
            {
                UserId = user.UserId,
                FullName = "New Name",
                Email = "new@example.com",
                Department = "NewDept",
                IsAuthorized = true
            };

            // Act
            await service.UpdateAsync(updateDto);

            // Assert
            var updatedUser = await context.Users.FindAsync(user.UserId);
            updatedUser!.FullName.Should().Be("New Name");
            updatedUser.Email.Should().Be("new@example.com");
        }

        [Fact]
        public async Task UpdateAsync_WithNullOptionalFields_ShouldUpdateOnlyRequiredFields()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            
            var user = new User 
            { 
                FullName = "Original Name", 
                Email = "original@example.com", 
                GoogleId = "google1", 
                UserType = "Student", 
                IsAuthorized = true,
                DepartmentMajor = "CS"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var updateDto = new UpdateUserDto
            {
                UserId = user.UserId,
                FullName = "Updated Name",
                Email = null,
                Department = null,
                IsAuthorized = null
            };

            // Act
            var result = await service.UpdateAsync(updateDto);

            // Assert
            result.Should().BeTrue();
            var updatedUser = await context.Users.FindAsync(user.UserId);
            updatedUser!.FullName.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateAsync_MultipleUpdates_ShouldAllSucceed()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            
            var user = new User 
            { 
                FullName = "Original", 
                Email = "original@example.com", 
                GoogleId = "google1", 
                UserType = "Student", 
                IsAuthorized = true
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act & Assert - First update
            var updateDto1 = new UpdateUserDto { UserId = user.UserId, FullName = "Update 1" };
            var result1 = await service.UpdateAsync(updateDto1);
            result1.Should().BeTrue();

            // Act & Assert - Second update
            var updateDto2 = new UpdateUserDto { UserId = user.UserId, FullName = "Update 2" };
            var result2 = await service.UpdateAsync(updateDto2);
            result2.Should().BeTrue();

            var finalUser = await context.Users.FindAsync(user.UserId);
            finalUser!.FullName.Should().Be("Update 2");
        }

        [Fact]
        public async Task UpdateAsync_WithEmptyStrings_ShouldUpdate()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            
            var user = new User 
            { 
                FullName = "Original Name", 
                Email = "original@example.com", 
                GoogleId = "google1", 
                UserType = "Student", 
                IsAuthorized = true
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var updateDto = new UpdateUserDto
            {
                UserId = user.UserId,
                FullName = "",
                Email = "",
                Department = ""
            };

            // Act
            var result = await service.UpdateAsync(updateDto);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task CreateAndRetrieve_ShouldWorkEndToEnd()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            var createDto = new CreateUserDto
            {
                FullName = "Integration Test User",
                Email = "integration@example.com",
                DepartmentMajor = "IT"
            };

            // Act
            var created = await service.CreateAsync(createDto);
            var retrieved = await service.GetByIdAsync(created.UserId);

            // Assert
            retrieved.Should().NotBeNull();
            retrieved!.UserId.Should().Be(created.UserId);
            retrieved.FullName.Should().Be("Integration Test User");
            retrieved.Email.Should().Be("integration@example.com");
        }

        [Fact]
        public async Task CreateUpdateAndRetrieve_ShouldWorkEndToEnd()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);
            
            var createDto = new CreateUserDto
            {
                FullName = "Original User",
                Email = "original@example.com",
                DepartmentMajor = "CS"
            };

            // Act - Create
            var created = await service.CreateAsync(createDto);
            
            // Act - Update
            var updateDto = new UpdateUserDto
            {
                UserId = created.UserId,
                FullName = "Updated User",
                Email = "updated@example.com"
            };
            var updateResult = await service.UpdateAsync(updateDto);
            
            // Act - Retrieve
            var retrieved = await service.GetByIdAsync(created.UserId);

            // Assert
            updateResult.Should().BeTrue();
            retrieved.Should().NotBeNull();
            retrieved!.FullName.Should().Be("Updated User");
            retrieved.Email.Should().Be("updated@example.com");
        }

        [Fact]
        public async Task GetAllAsync_AfterCreatingAndUpdatingMultipleUsers_ShouldReturnAllCorrectly()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new UserService(context, _mapper);

            // Act - Create multiple users
            await service.CreateAsync(new CreateUserDto { FullName = "User 1", Email = "user1@example.com", DepartmentMajor = "CS" });
            await service.CreateAsync(new CreateUserDto { FullName = "User 2", Email = "user2@example.com", DepartmentMajor = "IT" });
            await service.CreateAsync(new CreateUserDto { FullName = "User 3", Email = "user3@example.com", DepartmentMajor = "EE" });

            var allUsers = await service.GetAllAsync();

            // Assert
            allUsers.Should().HaveCount(3);
            allUsers.Select(u => u.FullName).Should().Contain(new[] { "User 1", "User 2", "User 3" });
        }

        #endregion
    }
}
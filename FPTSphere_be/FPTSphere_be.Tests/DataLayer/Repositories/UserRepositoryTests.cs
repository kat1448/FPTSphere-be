using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories;
using FluentAssertions;
using FPTSphere_be.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FPTSphere_be.Tests.DataLayer.Repositories
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repository = new UserRepository(context);
            
            var users = new List<User>
            {
                new User { UserId = 1, FullName = "User 1", Email = "user1@test.com", GoogleId = "g1", UserType = "Student", IsAuthorized = true },
                new User { UserId = 2, FullName = "User 2", Email = "user2@test.com", GoogleId = "g2", UserType = "Teacher", IsAuthorized = true }
            };
            
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(u => u.FullName == "User 1");
            result.Should().Contain(u => u.FullName == "User 2");
        }

        [Fact]
        public async Task GetAllAsync_WhenNoUsers_ShouldReturnEmptyList()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repository = new UserRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repository = new UserRepository(context);
            
            var user = new User 
            { 
                UserId = 10, 
                FullName = "Test User", 
                Email = "test@test.com", 
                GoogleId = "google123", 
                UserType = "Student", 
                IsAuthorized = true 
            };
            
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(10);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(10);
            result.FullName.Should().Be("Test User");
            result.Email.Should().Be("test@test.com");
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repository = new UserRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_ShouldAddUserToDatabase()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repository = new UserRepository(context);
            
            var newUser = new User
            {
                FullName = "New User",
                Email = "new@test.com",
                GoogleId = "newgoogle",
                UserType = "Student",
                IsAuthorized = true
            };

            // Act
            var result = await repository.CreateAsync(newUser);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("New User");
            result.UserId.Should().BeGreaterThan(0);
            
            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Email == "new@test.com");
            userInDb.Should().NotBeNull();
            userInDb!.FullName.Should().Be("New User");
        }

        [Fact]
        public async Task CreateAsync_ShouldGenerateUserId()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repository = new UserRepository(context);
            
            var newUser = new User
            {
                FullName = "Auto ID User",
                Email = "autoid@test.com",
                GoogleId = "autogoogle",
                UserType = "Teacher",
                IsAuthorized = false
            };

            // Act
            var result = await repository.CreateAsync(newUser);

            // Assert
            result.UserId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAsync_WhenUserExists_ShouldUpdateUser()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repository = new UserRepository(context);
            
            var user = new User
            {
                UserId = 20,
                FullName = "Original Name",
                Email = "original@test.com",
                GoogleId = "origgoogle",
                UserType = "Student",
                IsAuthorized = true
            };
            
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var updatedUser = new User
            {
                UserId = 20,
                FullName = "Updated Name",
                Email = "updated@test.com",
                GoogleId = "origgoogle",
                UserType = "Teacher",
                IsAuthorized = false
            };
            
            var result = await repository.UpdateAsync(updatedUser);

            // Assert
            result.Should().BeTrue();
            
            var userInDb = await context.Users.FindAsync(20);
            userInDb.Should().NotBeNull();
            userInDb!.FullName.Should().Be("Updated Name");
            userInDb.Email.Should().Be("updated@test.com");
            userInDb.UserType.Should().Be("Teacher");
            userInDb.IsAuthorized.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_WhenUserDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repository = new UserRepository(context);
            
            var nonExistentUser = new User
            {
                UserId = 999,
                FullName = "Non Existent",
                Email = "none@test.com",
                GoogleId = "nonegoogle",
                UserType = "Student",
                IsAuthorized = true
            };

            // Act
            var result = await repository.UpdateAsync(nonExistentUser);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WhenUserExists_ShouldDeleteUser()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repository = new UserRepository(context);
            
            var user = new User
            {
                UserId = 30,
                FullName = "To Delete",
                Email = "delete@test.com",
                GoogleId = "deletegoogle",
                UserType = "Student",
                IsAuthorized = true
            };
            
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.DeleteAsync(30);

            // Assert
            result.Should().BeTrue();
            
            var userInDb = await context.Users.FindAsync(30);
            userInDb.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WhenUserDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repository = new UserRepository(context);

            // Act
            var result = await repository.DeleteAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAsync_WithCompleteUserData_ShouldPreserveAllProperties()
        {
            // Arrange
            var context = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var repository = new UserRepository(context);
            
            var newUser = new User
            {
                FullName = "Complete User",
                Email = "complete@test.com",
                GoogleId = "completegoogle",
                UserType = "Admin",
                IsAuthorized = true,
                DepartmentMajor = "Computer Science",
                ClassCode = "CS101"
            };

            // Act
            var result = await repository.CreateAsync(newUser);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("Complete User");
            result.Email.Should().Be("complete@test.com");
            result.GoogleId.Should().Be("completegoogle");
            result.UserType.Should().Be("Admin");
            result.IsAuthorized.Should().BeTrue();
            result.DepartmentMajor.Should().Be("Computer Science");
            result.ClassCode.Should().Be("CS101");
        }
    }
}
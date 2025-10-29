using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DataLayer.Tests
{
    public class UserRepositoryTests
    {
        private EventDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new EventDbContext(options);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithNoUsers_ShouldReturnEmptyList()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleUsers_ShouldReturnAllUsers()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            context.Users.AddRange(
                new User { FullName = "User 1", Email = "user1@test.com", GoogleId = "g1", UserType = "Student", IsAuthorized = true },
                new User { FullName = "User 2", Email = "user2@test.com", GoogleId = "g2", UserType = "Teacher", IsAuthorized = false },
                new User { FullName = "User 3", Email = "user3@test.com", GoogleId = "g3", UserType = "Admin", IsAuthorized = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Select(u => u.FullName).Should().Contain(new[] { "User 1", "User 2", "User 3" });
        }

        [Fact]
        public async Task GetAllAsync_WithLargeDataset_ShouldReturnAll()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var users = Enumerable.Range(1, 50).Select(i => new User
            {
                FullName = $"User {i}",
                Email = $"user{i}@test.com",
                GoogleId = $"google{i}",
                UserType = "Student",
                IsAuthorized = i % 2 == 0
            });
            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(50);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user = new User 
            { 
                FullName = "John Doe", 
                Email = "john@test.com", 
                GoogleId = "google123", 
                UserType = "Student", 
                IsAuthorized = true 
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(user.UserId);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(user.UserId);
            result.FullName.Should().Be("John Doe");
            result.Email.Should().Be("john@test.com");
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithNegativeId_ShouldReturnNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);

            // Act
            var result = await repository.GetByIdAsync(-1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithZeroId_ShouldReturnNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);

            // Act
            var result = await repository.GetByIdAsync(0);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithMultipleUsers_ShouldReturnCorrectUser()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user1 = new User { FullName = "User 1", Email = "u1@test.com", GoogleId = "g1", UserType = "Student", IsAuthorized = true };
            var user2 = new User { FullName = "User 2", Email = "u2@test.com", GoogleId = "g2", UserType = "Student", IsAuthorized = true };
            var user3 = new User { FullName = "User 3", Email = "u3@test.com", GoogleId = "g3", UserType = "Student", IsAuthorized = true };
            
            context.Users.AddRange(user1, user2, user3);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(user2.UserId);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("User 2");
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidUser_ShouldCreateAndReturnUser()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user = new User
            {
                FullName = "New User",
                Email = "new@test.com",
                GoogleId = "newgoogle",
                UserType = "Student",
                IsAuthorized = true,
                DepartmentMajor = "CS"
            };

            // Act
            var result = await repository.CreateAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().BeGreaterThan(0);
            result.FullName.Should().Be("New User");
            result.Email.Should().Be("new@test.com");
            
            var userInDb = await context.Users.FindAsync(result.UserId);
            userInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_ShouldPersistToDatabase()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user = new User
            {
                FullName = "Test User",
                Email = "test@test.com",
                GoogleId = "testgoogle",
                UserType = "Teacher",
                IsAuthorized = false
            };

            // Act
            await repository.CreateAsync(user);

            // Assert
            var allUsers = await context.Users.ToListAsync();
            allUsers.Should().HaveCount(1);
            allUsers[0].FullName.Should().Be("Test User");
        }

        [Fact]
        public async Task CreateAsync_MultipleUsers_ShouldCreateAll()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user1 = new User { FullName = "User 1", Email = "u1@test.com", GoogleId = "g1", UserType = "Student", IsAuthorized = true };
            var user2 = new User { FullName = "User 2", Email = "u2@test.com", GoogleId = "g2", UserType = "Student", IsAuthorized = true };

            // Act
            await repository.CreateAsync(user1);
            await repository.CreateAsync(user2);

            // Assert
            var allUsers = await context.Users.ToListAsync();
            allUsers.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateAsync_WithMinimalData_ShouldSucceed()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user = new User
            {
                FullName = "Minimal User",
                Email = "minimal@test.com",
                GoogleId = "mingoogle",
                UserType = "Student",
                IsAuthorized = true
            };

            // Act
            var result = await repository.CreateAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().BeGreaterThan(0);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidUser_ShouldUpdateAndReturnTrue()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user = new User 
            { 
                FullName = "Original Name", 
                Email = "original@test.com", 
                GoogleId = "google1", 
                UserType = "Student", 
                IsAuthorized = false 
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            user.FullName = "Updated Name";
            user.Email = "updated@test.com";
            user.IsAuthorized = true;

            // Act
            var result = await repository.UpdateAsync(user);

            // Assert
            result.Should().BeTrue();
            var updatedUser = await context.Users.FindAsync(user.UserId);
            updatedUser!.FullName.Should().Be("Updated Name");
            updatedUser.Email.Should().Be("updated@test.com");
            updatedUser.IsAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentUser_ShouldReturnFalse()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user = new User
            {
                UserId = 999,
                FullName = "Non Existent",
                Email = "nonexistent@test.com",
                GoogleId = "google999",
                UserType = "Student",
                IsAuthorized = true
            };

            // Act
            var result = await repository.UpdateAsync(user);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAllProperties()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user = new User 
            { 
                FullName = "Old Name", 
                Email = "old@test.com", 
                GoogleId = "google1", 
                UserType = "Student", 
                IsAuthorized = false,
                DepartmentMajor = "OldDept",
                ClassCode = "OLD101"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            user.FullName = "New Name";
            user.Email = "new@test.com";
            user.UserType = "Teacher";
            user.IsAuthorized = true;
            user.DepartmentMajor = "NewDept";
            user.ClassCode = "NEW201";

            // Act
            await repository.UpdateAsync(user);

            // Assert
            var updatedUser = await context.Users.FindAsync(user.UserId);
            updatedUser!.FullName.Should().Be("New Name");
            updatedUser.Email.Should().Be("new@test.com");
            updatedUser.UserType.Should().Be("Teacher");
            updatedUser.IsAuthorized.Should().BeTrue();
            updatedUser.DepartmentMajor.Should().Be("NewDept");
            updatedUser.ClassCode.Should().Be("NEW201");
        }

        [Fact]
        public async Task UpdateAsync_MultipleUpdates_ShouldAllSucceed()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user = new User 
            { 
                FullName = "Original", 
                Email = "original@test.com", 
                GoogleId = "google1", 
                UserType = "Student", 
                IsAuthorized = true 
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // First update
            user.FullName = "Update 1";
            var result1 = await repository.UpdateAsync(user);

            // Second update
            user.FullName = "Update 2";
            var result2 = await repository.UpdateAsync(user);

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            
            var finalUser = await context.Users.FindAsync(user.UserId);
            finalUser!.FullName.Should().Be("Update 2");
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user = new User 
            { 
                FullName = "To Delete", 
                Email = "delete@test.com", 
                GoogleId = "google1", 
                UserType = "Student", 
                IsAuthorized = true 
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.DeleteAsync(user.UserId);

            // Assert
            result.Should().BeTrue();
            var deletedUser = await context.Users.FindAsync(user.UserId);
            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);

            // Act
            var result = await repository.DeleteAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WithNegativeId_ShouldReturnFalse()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);

            // Act
            var result = await repository.DeleteAsync(-1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveFromDatabase()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user = new User 
            { 
                FullName = "Remove Me", 
                Email = "remove@test.com", 
                GoogleId = "google1", 
                UserType = "Student", 
                IsAuthorized = true 
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var initialCount = await context.Users.CountAsync();

            // Act
            await repository.DeleteAsync(user.UserId);

            // Assert
            var finalCount = await context.Users.CountAsync();
            finalCount.Should().Be(initialCount - 1);
        }

        [Fact]
        public async Task DeleteAsync_MultipleDeletes_ShouldWorkCorrectly()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user1 = new User { FullName = "User 1", Email = "u1@test.com", GoogleId = "g1", UserType = "Student", IsAuthorized = true };
            var user2 = new User { FullName = "User 2", Email = "u2@test.com", GoogleId = "g2", UserType = "Student", IsAuthorized = true };
            var user3 = new User { FullName = "User 3", Email = "u3@test.com", GoogleId = "g3", UserType = "Student", IsAuthorized = true };
            
            context.Users.AddRange(user1, user2, user3);
            await context.SaveChangesAsync();

            // Act
            await repository.DeleteAsync(user1.UserId);
            await repository.DeleteAsync(user3.UserId);

            // Assert
            var remainingUsers = await context.Users.ToListAsync();
            remainingUsers.Should().HaveCount(1);
            remainingUsers[0].FullName.Should().Be("User 2");
        }

        #endregion

        #region Integration and Edge Case Tests

        [Fact]
        public async Task CreateAndRetrieve_ShouldWorkEndToEnd()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user = new User
            {
                FullName = "Integration Test",
                Email = "integration@test.com",
                GoogleId = "intgoogle",
                UserType = "Student",
                IsAuthorized = true
            };

            // Act
            var created = await repository.CreateAsync(user);
            var retrieved = await repository.GetByIdAsync(created.UserId);

            // Assert
            retrieved.Should().NotBeNull();
            retrieved!.UserId.Should().Be(created.UserId);
            retrieved.FullName.Should().Be("Integration Test");
        }

        [Fact]
        public async Task CreateUpdateAndDelete_ShouldWorkEndToEnd()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);
            
            var user = new User
            {
                FullName = "Full Cycle User",
                Email = "cycle@test.com",
                GoogleId = "cyclegoogle",
                UserType = "Student",
                IsAuthorized = true
            };

            // Act - Create
            var created = await repository.CreateAsync(user);
            created.FullName = "Updated Cycle User";
            
            // Act - Update
            var updateResult = await repository.UpdateAsync(created);
            
            // Act - Delete
            var deleteResult = await repository.DeleteAsync(created.UserId);

            // Assert
            updateResult.Should().BeTrue();
            deleteResult.Should().BeTrue();
            var deletedUser = await repository.GetByIdAsync(created.UserId);
            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_AfterVariousOperations_ShouldReturnCorrectCount()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repository = new UserRepository(context);

            // Create 5 users
            for (int i = 1; i <= 5; i++)
            {
                await repository.CreateAsync(new User
                {
                    FullName = $"User {i}",
                    Email = $"user{i}@test.com",
                    GoogleId = $"google{i}",
                    UserType = "Student",
                    IsAuthorized = true
                });
            }

            // Delete 2 users
            var allUsers = await repository.GetAllAsync();
            await repository.DeleteAsync(allUsers.First().UserId);
            await repository.DeleteAsync(allUsers.Last().UserId);

            // Act
            var finalUsers = await repository.GetAllAsync();

            // Assert
            finalUsers.Should().HaveCount(3);
        }

        #endregion
    }
}
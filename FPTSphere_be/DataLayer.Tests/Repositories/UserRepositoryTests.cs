using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.Models;
using DataLayer.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DataLayer.Tests.Repositories
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly EventDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new EventDbContext(options);
            _repository = new UserRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllAsync_WithNoUsers_ShouldReturnEmptyList()
        {
            var result = await _repository.GetAllAsync();
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleUsers_ShouldReturnAllUsers()
        {
            var users = new[]
            {
                new User { FullName = "User 1", Email = "user1@test.com", IsAuthorized = true },
                new User { FullName = "User 2", Email = "user2@test.com", IsAuthorized = false }
            };
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnUser()
        {
            var user = new User { FullName = "Test", Email = "test@test.com", IsAuthorized = true };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(user.UserId);

            result.Should().NotBeNull();
            result!.FullName.Should().Be("Test");
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            var result = await _repository.GetByIdAsync(999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_WithValidUser_ShouldCreateUser()
        {
            var user = new User
            {
                FullName = "New User",
                Email = "new@test.com",
                IsAuthorized = true
            };

            var result = await _repository.CreateAsync(user);

            result.Should().NotBeNull();
            result.UserId.Should().BeGreaterThan(0);
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

            user.FullName = "Updated";
            var result = await _repository.UpdateAsync(user);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingUser_ShouldReturnFalse()
        {
            var user = new User { UserId = 999, FullName = "None", Email = "none@test.com", IsAuthorized = true };

            var result = await _repository.UpdateAsync(user);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_ShouldDeleteUser()
        {
            var user = new User { FullName = "Delete", Email = "delete@test.com", IsAuthorized = true };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            var userId = user.UserId;

            var result = await _repository.DeleteAsync(userId);

            result.Should().BeTrue();
            var deleted = await _context.Users.FindAsync(userId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
        {
            var result = await _repository.DeleteAsync(999);
            result.Should().BeFalse();
        }
    }
}
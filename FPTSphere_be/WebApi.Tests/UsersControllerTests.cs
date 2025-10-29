using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.DTOs.User;
using BusinessLayer.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;
using Xunit;

namespace WebApi.Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UsersController(_mockUserService.Object);
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_WithUsers_ShouldReturnOkWithUserList()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { UserId = 1, FullName = "User 1", Email = "user1@test.com", DepartmentMajor = "CS" },
                new UserDto { UserId = 2, FullName = "User 2", Email = "user2@test.com", DepartmentMajor = "IT" }
            };
            _mockUserService.Setup(s => s.GetAllAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
            returnedUsers.Should().HaveCount(2);
            returnedUsers.First().FullName.Should().Be("User 1");
        }

        [Fact]
        public async Task GetAll_WithNoUsers_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<UserDto>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
            returnedUsers.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAll_ShouldCallServiceOnce()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<UserDto>());

            // Act
            await _controller.GetAll();

            // Assert
            _mockUserService.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_WithLargeDataset_ShouldReturnAllUsers()
        {
            // Arrange
            var users = Enumerable.Range(1, 100).Select(i => new UserDto
            {
                UserId = i,
                FullName = $"User {i}",
                Email = $"user{i}@test.com",
                DepartmentMajor = "CS"
            }).ToList();
            _mockUserService.Setup(s => s.GetAllAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
            returnedUsers.Should().HaveCount(100);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WithValidId_ShouldReturnOkWithUser()
        {
            // Arrange
            var user = new UserDto { UserId = 1, FullName = "John Doe", Email = "john@test.com", DepartmentMajor = "CS" };
            _mockUserService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUser = okResult.Value.Should().BeOfType<UserDto>().Subject;
            returnedUser.UserId.Should().Be(1);
            returnedUser.FullName.Should().Be("John Doe");
        }

        [Fact]
        public async Task GetById_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((UserDto?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetById_WithNegativeId_ShouldReturnNotFound()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetByIdAsync(-1)).ReturnsAsync((UserDto?)null);

            // Act
            var result = await _controller.GetById(-1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetById_WithZeroId_ShouldReturnNotFound()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetByIdAsync(0)).ReturnsAsync((UserDto?)null);

            // Act
            var result = await _controller.GetById(0);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetById_ShouldCallServiceWithCorrectId()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((UserDto?)null);

            // Act
            await _controller.GetById(42);

            // Assert
            _mockUserService.Verify(s => s.GetByIdAsync(42), Times.Once);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_WithValidDto_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                FullName = "New User",
                Email = "new@test.com",
                DepartmentMajor = "CS"
            };
            var createdUser = new UserDto
            {
                UserId = 1,
                FullName = "New User",
                Email = "new@test.com",
                DepartmentMajor = "User"
            };
            _mockUserService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdUser);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(UsersController.GetById));
            createdResult.RouteValues!["id"].Should().Be(1);
            
            var returnedUser = createdResult.Value.Should().BeOfType<UserDto>().Subject;
            returnedUser.UserId.Should().Be(1);
            returnedUser.FullName.Should().Be("New User");
        }

        [Fact]
        public async Task Create_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var createDto = new CreateUserDto();
            _controller.ModelState.AddModelError("FullName", "Required");

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Create_ShouldCallServiceOnce()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                FullName = "Test User",
                Email = "test@test.com",
                DepartmentMajor = "IT"
            };
            var createdUser = new UserDto { UserId = 1, FullName = "Test User" };
            _mockUserService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdUser);

            // Act
            await _controller.Create(createDto);

            // Assert
            _mockUserService.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_WithEmptyStrings_ShouldCallService()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                FullName = "",
                Email = "",
                DepartmentMajor = ""
            };
            var createdUser = new UserDto { UserId = 1 };
            _mockUserService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdUser);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            _mockUserService.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldReturnCorrectLocationHeader()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                FullName = "Location Test",
                Email = "location@test.com",
                DepartmentMajor = "CS"
            };
            var createdUser = new UserDto { UserId = 42, FullName = "Location Test" };
            _mockUserService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdUser);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.RouteValues!["id"].Should().Be(42);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidDto_ShouldReturnNoContent()
        {
            // Arrange
            var updateDto = new UpdateUserDto
            {
                UserId = 1,
                FullName = "Updated User",
                Email = "updated@test.com"
            };
            _mockUserService.Setup(s => s.UpdateAsync(updateDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.Update(1, updateDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Update_WithNonExistentUser_ShouldReturnNotFound()
        {
            // Arrange
            var updateDto = new UpdateUserDto
            {
                UserId = 999,
                FullName = "Non Existent",
                Email = "nonexistent@test.com"
            };
            _mockUserService.Setup(s => s.UpdateAsync(updateDto)).ReturnsAsync(false);

            // Act
            var result = await _controller.Update(999, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Update_WithMismatchedIds_ShouldReturnBadRequest()
        {
            // Arrange
            var updateDto = new UpdateUserDto
            {
                UserId = 1,
                FullName = "Test User",
                Email = "test@test.com"
            };

            // Act
            var result = await _controller.Update(2, updateDto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("ID mismatch");
        }

        [Fact]
        public async Task Update_ShouldCallServiceWithCorrectDto()
        {
            // Arrange
            var updateDto = new UpdateUserDto
            {
                UserId = 5,
                FullName = "Service Test",
                Email = "service@test.com"
            };
            _mockUserService.Setup(s => s.UpdateAsync(updateDto)).ReturnsAsync(true);

            // Act
            await _controller.Update(5, updateDto);

            // Assert
            _mockUserService.Verify(s => s.UpdateAsync(updateDto), Times.Once);
        }

        [Fact]
        public async Task Update_WithNegativeId_AndMismatch_ShouldReturnBadRequest()
        {
            // Arrange
            var updateDto = new UpdateUserDto
            {
                UserId = 1,
                FullName = "Test",
                Email = "test@test.com"
            };

            // Act
            var result = await _controller.Update(-1, updateDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_WithZeroId_AndMismatch_ShouldReturnBadRequest()
        {
            // Arrange
            var updateDto = new UpdateUserDto
            {
                UserId = 1,
                FullName = "Test",
                Email = "test@test.com"
            };

            // Act
            var result = await _controller.Update(0, updateDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_WhenServiceReturnsFalse_ShouldNotCallServiceMultipleTimes()
        {
            // Arrange
            var updateDto = new UpdateUserDto
            {
                UserId = 10,
                FullName = "Single Call Test",
                Email = "single@test.com"
            };
            _mockUserService.Setup(s => s.UpdateAsync(updateDto)).ReturnsAsync(false);

            // Act
            await _controller.Update(10, updateDto);

            // Assert
            _mockUserService.Verify(s => s.UpdateAsync(updateDto), Times.Once);
        }

        #endregion

        #region Integration and Edge Case Tests

        [Fact]
        public async Task Controller_WithNullService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UsersController(null!));
        }

        [Fact]
        public async Task GetAll_WhenServiceThrows_ShouldPropagateException()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetAllAsync()).ThrowsAsync(new InvalidOperationException("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetAll());
        }

        [Fact]
        public async Task GetById_WhenServiceThrows_ShouldPropagateException()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ThrowsAsync(new InvalidOperationException("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetById(1));
        }

        [Fact]
        public async Task Create_WhenServiceThrows_ShouldPropagateException()
        {
            // Arrange
            var createDto = new CreateUserDto { FullName = "Test", Email = "test@test.com", DepartmentMajor = "CS" };
            _mockUserService.Setup(s => s.CreateAsync(It.IsAny<CreateUserDto>())).ThrowsAsync(new InvalidOperationException("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(createDto));
        }

        [Fact]
        public async Task Update_WhenServiceThrows_ShouldPropagateException()
        {
            // Arrange
            var updateDto = new UpdateUserDto { UserId = 1, FullName = "Test", Email = "test@test.com" };
            _mockUserService.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserDto>())).ThrowsAsync(new InvalidOperationException("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Update(1, updateDto));
        }

        [Fact]
        public async Task Create_MultipleSequentialCalls_ShouldWorkCorrectly()
        {
            // Arrange
            var createDto1 = new CreateUserDto { FullName = "User 1", Email = "user1@test.com", DepartmentMajor = "CS" };
            var createDto2 = new CreateUserDto { FullName = "User 2", Email = "user2@test.com", DepartmentMajor = "IT" };
            
            var createdUser1 = new UserDto { UserId = 1, FullName = "User 1" };
            var createdUser2 = new UserDto { UserId = 2, FullName = "User 2" };
            
            _mockUserService.Setup(s => s.CreateAsync(createDto1)).ReturnsAsync(createdUser1);
            _mockUserService.Setup(s => s.CreateAsync(createDto2)).ReturnsAsync(createdUser2);

            // Act
            var result1 = await _controller.Create(createDto1);
            var result2 = await _controller.Create(createDto2);

            // Assert
            result1.Should().BeOfType<CreatedAtActionResult>();
            result2.Should().BeOfType<CreatedAtActionResult>();
            _mockUserService.Verify(s => s.CreateAsync(It.IsAny<CreateUserDto>()), Times.Exactly(2));
        }

        #endregion
    }
}
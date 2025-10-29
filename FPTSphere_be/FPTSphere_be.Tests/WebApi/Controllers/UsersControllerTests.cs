using BusinessLayer.DTOs.User;
using BusinessLayer.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;
using Xunit;

namespace FPTSphere_be.Tests.WebApi.Controllers
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

        [Fact]
        public async Task GetAll_ShouldReturnOkWithUsers()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { UserId = 1, FullName = "User 1", Email = "user1@test.com" },
                new UserDto { UserId = 2, FullName = "User 2", Email = "user2@test.com" }
            };
            
            _mockUserService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(users);
            _mockUserService.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_WhenNoUsers_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            var emptyList = new List<UserDto>();
            _mockUserService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedList = okResult!.Value as IEnumerable<UserDto>;
            returnedList.Should().BeEmpty();
        }

        [Fact]
        public async Task GetById_WhenUserExists_ShouldReturnOkWithUser()
        {
            // Arrange
            var userId = 10;
            var user = new UserDto 
            { 
                UserId = userId, 
                FullName = "Test User", 
                Email = "test@example.com",
                DepartmentMajor = "Engineering"
            };
            
            _mockUserService.Setup(s => s.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetById(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(user);
            _mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenUserDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var userId = 999;
            _mockUserService.Setup(s => s.GetByIdAsync(userId))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _controller.GetById(userId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Create_WithValidDto_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                FullName = "New User",
                Email = "new@example.com",
                DepartmentMajor = "Business"
            };
            
            var createdUser = new UserDto
            {
                UserId = 5,
                FullName = "New User",
                Email = "new@example.com",
                DepartmentMajor = "Business"
            };
            
            _mockUserService.Setup(s => s.CreateAsync(createDto))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(_controller.GetById));
            createdResult.RouteValues!["id"].Should().Be(5);
            createdResult.Value.Should().BeEquivalentTo(createdUser);
            _mockUserService.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                FullName = "Invalid User",
                Email = "invalid@example.com",
                DepartmentMajor = "Test"
            };
            
            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            _mockUserService.Verify(s => s.CreateAsync(It.IsAny<CreateUserDto>()), Times.Never);
        }

        [Fact]
        public async Task Update_WithMatchingId_WhenUserExists_ShouldReturnNoContent()
        {
            // Arrange
            var userId = 15;
            var updateDto = new UpdateUserDto
            {
                UserId = userId,
                FullName = "Updated User",
                Email = "updated@example.com",
                Department = "Marketing",
                IsAuthorized = true
            };
            
            _mockUserService.Setup(s => s.UpdateAsync(updateDto))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Update(userId, updateDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockUserService.Verify(s => s.UpdateAsync(updateDto), Times.Once);
        }

        [Fact]
        public async Task Update_WithMismatchedId_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = 20;
            var updateDto = new UpdateUserDto
            {
                UserId = 25, // Different from userId parameter
                FullName = "Mismatched User",
                Email = "mismatch@example.com"
            };

            // Act
            var result = await _controller.Update(userId, updateDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("ID mismatch");
            _mockUserService.Verify(s => s.UpdateAsync(It.IsAny<UpdateUserDto>()), Times.Never);
        }

        [Fact]
        public async Task Update_WhenUserDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var userId = 999;
            var updateDto = new UpdateUserDto
            {
                UserId = userId,
                FullName = "Non Existent User",
                Email = "nonexistent@example.com"
            };
            
            _mockUserService.Setup(s => s.UpdateAsync(updateDto))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Update(userId, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockUserService.Verify(s => s.UpdateAsync(updateDto), Times.Once);
        }

        [Fact]
        public async Task GetById_WithNegativeId_ShouldCallService()
        {
            // Arrange
            var negativeId = -1;
            _mockUserService.Setup(s => s.GetByIdAsync(negativeId))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _controller.GetById(negativeId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockUserService.Verify(s => s.GetByIdAsync(negativeId), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldPassDtoToService()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                FullName = "Pass Through User",
                Email = "passthrough@example.com",
                DepartmentMajor = "Testing"
            };
            
            var createdUser = new UserDto { UserId = 100, FullName = "Pass Through User" };
            _mockUserService.Setup(s => s.CreateAsync(createDto))
                .ReturnsAsync(createdUser);

            // Act
            await _controller.Create(createDto);

            // Assert
            _mockUserService.Verify(s => s.CreateAsync(It.Is<CreateUserDto>(
                dto => dto.FullName == "Pass Through User" &&
                       dto.Email == "passthrough@example.com" &&
                       dto.DepartmentMajor == "Testing"
            )), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldPassDtoToService()
        {
            // Arrange
            var userId = 50;
            var updateDto = new UpdateUserDto
            {
                UserId = userId,
                FullName = "Service Update",
                Email = "serviceupdate@example.com",
                Department = "Research",
                IsAuthorized = false
            };
            
            _mockUserService.Setup(s => s.UpdateAsync(updateDto))
                .ReturnsAsync(true);

            // Act
            await _controller.Update(userId, updateDto);

            // Assert
            _mockUserService.Verify(s => s.UpdateAsync(It.Is<UpdateUserDto>(
                dto => dto.UserId == userId &&
                       dto.FullName == "Service Update" &&
                       dto.Email == "serviceupdate@example.com" &&
                       dto.Department == "Research" &&
                       dto.IsAuthorized == false
            )), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldCallServiceOnce()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<UserDto>());

            // Act
            await _controller.GetAll();

            // Assert
            _mockUserService.Verify(s => s.GetAllAsync(), Times.Once);
            _mockUserService.VerifyNoOtherCalls();
        }
    }
}
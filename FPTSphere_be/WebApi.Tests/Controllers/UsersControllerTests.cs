using System;
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

namespace WebApi.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UsersController(_userServiceMock.Object);
        }

        [Fact]
        public async Task GetAll_WithUsers_ShouldReturnOkWithUsers()
        {
            var users = new List<UserDto>
            {
                new UserDto { UserId = 1, FullName = "User 1", Email = "user1@test.com" },
                new UserDto { UserId = 2, FullName = "User 2", Email = "user2@test.com" }
            };
            _userServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(users);

            var result = await _controller.GetAll();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
            returnedUsers.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAll_WithNoUsers_ShouldReturnOkWithEmptyList()
        {
            _userServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<UserDto>());

            var result = await _controller.GetAll();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
            returnedUsers.Should().BeEmpty();
        }

        [Fact]
        public async Task GetById_WithExistingId_ShouldReturnOkWithUser()
        {
            var user = new UserDto { UserId = 1, FullName = "Test User", Email = "test@test.com" };
            _userServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(user);

            var result = await _controller.GetById(1);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUser = okResult.Value.Should().BeOfType<UserDto>().Subject;
            returnedUser.UserId.Should().Be(1);
        }

        [Fact]
        public async Task GetById_WithNonExistingId_ShouldReturnNotFound()
        {
            _userServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((UserDto?)null);

            var result = await _controller.GetById(999);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Create_WithValidDto_ShouldReturnCreatedAtAction()
        {
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
                Email = "new@test.com"
            };
            _userServiceMock.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdUser);

            var result = await _controller.Create(createDto);

            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(UsersController.GetById));
            createdResult.RouteValues!["id"].Should().Be(1);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_ShouldReturnBadRequest()
        {
            _controller.ModelState.AddModelError("FullName", "Required");
            var createDto = new CreateUserDto();

            var result = await _controller.Create(createDto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_WithValidDto_ShouldReturnNoContent()
        {
            var updateDto = new UpdateUserDto
            {
                UserId = 1,
                FullName = "Updated",
                Email = "updated@test.com"
            };
            _userServiceMock.Setup(s => s.UpdateAsync(updateDto)).ReturnsAsync(true);

            var result = await _controller.Update(1, updateDto);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Update_WithIdMismatch_ShouldReturnBadRequest()
        {
            var updateDto = new UpdateUserDto { UserId = 1, FullName = "Test" };

            var result = await _controller.Update(2, updateDto);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("ID mismatch");
        }

        [Fact]
        public async Task Update_WithNonExistingUser_ShouldReturnNotFound()
        {
            var updateDto = new UpdateUserDto { UserId = 999, FullName = "None" };
            _userServiceMock.Setup(s => s.UpdateAsync(updateDto)).ReturnsAsync(false);

            var result = await _controller.Update(999, updateDto);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
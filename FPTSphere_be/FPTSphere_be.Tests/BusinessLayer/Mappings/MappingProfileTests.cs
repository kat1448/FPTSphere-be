using AutoMapper;
using BusinessLayer.DTOs.User;
using BusinessLayer.Mappings;
using DataLayer.Models;
using FluentAssertions;
using Xunit;

namespace FPTSphere_be.Tests.BusinessLayer.Mappings
{
    public class MappingProfileTests
    {
        private readonly IMapper _mapper;

        public MappingProfileTests()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();
        }

        [Fact]
        public void MappingProfile_ShouldBeValid()
        {
            // Arrange
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            // Act & Assert
            configuration.AssertConfigurationIsValid();
        }

        [Fact]
        public void Map_UserToUserDto_ShouldMapCorrectly()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                DepartmentMajor = "Computer Science",
                GoogleId = "google123",
                UserType = "Student",
                IsAuthorized = true
            };

            // Act
            var userDto = _mapper.Map<UserDto>(user);

            // Assert
            userDto.Should().NotBeNull();
            userDto.UserId.Should().Be(user.UserId);
            userDto.FullName.Should().Be(user.FullName);
            userDto.Email.Should().Be(user.Email);
            userDto.DepartmentMajor.Should().Be(user.DepartmentMajor);
        }

        [Fact]
        public void Map_UserDtoToUser_ShouldMapCorrectly()
        {
            // Arrange
            var userDto = new UserDto
            {
                UserId = 2,
                FullName = "Jane Smith",
                Email = "jane@example.com",
                DepartmentMajor = "Business"
            };

            // Act
            var user = _mapper.Map<User>(userDto);

            // Assert
            user.Should().NotBeNull();
            user.UserId.Should().Be(userDto.UserId);
            user.FullName.Should().Be(userDto.FullName);
            user.Email.Should().Be(userDto.Email);
            user.DepartmentMajor.Should().Be(userDto.DepartmentMajor);
        }

        [Fact]
        public void Map_CreateUserDtoToUser_ShouldMapCorrectly()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                FullName = "New User",
                Email = "new@example.com",
                DepartmentMajor = "Engineering"
            };

            // Act
            var user = _mapper.Map<User>(createDto);

            // Assert
            user.Should().NotBeNull();
            user.FullName.Should().Be(createDto.FullName);
            user.Email.Should().Be(createDto.Email);
            user.DepartmentMajor.Should().Be(createDto.DepartmentMajor);
            user.UserId.Should().Be(0); // Default value
        }

        [Fact]
        public void Map_UpdateUserDtoToUser_ShouldMapCorrectly()
        {
            // Arrange
            var updateDto = new UpdateUserDto
            {
                UserId = 3,
                FullName = "Updated User",
                Email = "updated@example.com",
                Department = "HR",
                IsAuthorized = false
            };

            // Act
            var user = _mapper.Map<User>(updateDto);

            // Assert
            user.Should().NotBeNull();
            user.UserId.Should().Be(updateDto.UserId);
            user.FullName.Should().Be(updateDto.FullName);
            user.Email.Should().Be(updateDto.Email);
            user.IsAuthorized.Should().Be(updateDto.IsAuthorized);
        }

        [Fact]
        public void Map_UserWithNullProperties_ShouldHandleNulls()
        {
            // Arrange
            var user = new User
            {
                UserId = 4,
                FullName = "User With Nulls",
                Email = "nulls@example.com",
                GoogleId = "google456",
                UserType = "Admin",
                IsAuthorized = true,
                DepartmentMajor = null,
                ClassCode = null
            };

            // Act
            var userDto = _mapper.Map<UserDto>(user);

            // Assert
            userDto.Should().NotBeNull();
            userDto.UserId.Should().Be(user.UserId);
            userDto.FullName.Should().Be(user.FullName);
            userDto.Email.Should().Be(user.Email);
            userDto.DepartmentMajor.Should().BeNull();
        }

        [Fact]
        public void Map_ListOfUsers_ShouldMapToListOfUserDtos()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, FullName = "User 1", Email = "user1@example.com", GoogleId = "g1", UserType = "Student", IsAuthorized = true },
                new User { UserId = 2, FullName = "User 2", Email = "user2@example.com", GoogleId = "g2", UserType = "Teacher", IsAuthorized = false }
            };

            // Act
            var userDtos = _mapper.Map<List<UserDto>>(users);

            // Assert
            userDtos.Should().NotBeNull();
            userDtos.Should().HaveCount(2);
            userDtos[0].UserId.Should().Be(1);
            userDtos[1].UserId.Should().Be(2);
        }
    }
}
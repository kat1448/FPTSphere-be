using AutoMapper;
using BusinessLayer.DTOs.User;
using BusinessLayer.Mappings;
using DataLayer.Models;
using FluentAssertions;
using Xunit;

namespace BusinessLayer.Tests.Mappings
{
    public class MappingProfileTests
    {
        private readonly IMapper _mapper;

        public MappingProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void MappingProfile_Configuration_ShouldBeValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void Map_UserToUserDto_ShouldMapAllProperties()
        {
            var user = new User
            {
                UserId = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                DepartmentMajor = "CS",
                IsAuthorized = true
            };

            var dto = _mapper.Map<UserDto>(user);

            dto.Should().NotBeNull();
            dto.UserId.Should().Be(1);
            dto.FullName.Should().Be("John Doe");
            dto.Email.Should().Be("john@example.com");
        }

        [Fact]
        public void Map_CreateUserDtoToUser_ShouldMapAllProperties()
        {
            var dto = new CreateUserDto
            {
                FullName = "New User",
                Email = "new@example.com",
                DepartmentMajor = "Physics"
            };

            var user = _mapper.Map<User>(dto);

            user.Should().NotBeNull();
            user.FullName.Should().Be("New User");
            user.Email.Should().Be("new@example.com");
        }

        [Fact]
        public void Map_UpdateUserDtoToUser_ShouldMapAllProperties()
        {
            var dto = new UpdateUserDto
            {
                UserId = 1,
                FullName = "Updated",
                Email = "updated@example.com"
            };

            var user = _mapper.Map<User>(dto);

            user.Should().NotBeNull();
            user.UserId.Should().Be(1);
            user.FullName.Should().Be("Updated");
        }

        [Fact]
        public void Map_NullUser_ShouldReturnNull()
        {
            User? user = null;

            var dto = _mapper.Map<UserDto>(user);

            dto.Should().BeNull();
        }
    }
}
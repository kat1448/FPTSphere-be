using BusinessLayer.DTOs.User;
using FluentAssertions;
using Xunit;

namespace BusinessLayer.Tests.DTOs
{
    public class UserDtoTests
    {
        [Fact]
        public void UserDto_Properties_ShouldBeSettable()
        {
            var dto = new UserDto
            {
                UserId = 1,
                FullName = "Test User",
                Email = "test@example.com",
                DepartmentMajor = "CS"
            };

            dto.UserId.Should().Be(1);
            dto.FullName.Should().Be("Test User");
            dto.Email.Should().Be("test@example.com");
            dto.DepartmentMajor.Should().Be("CS");
        }

        [Fact]
        public void UserDto_WithNullValues_ShouldAcceptNulls()
        {
            var dto = new UserDto
            {
                UserId = 0,
                FullName = null,
                Email = null,
                DepartmentMajor = null
            };

            dto.FullName.Should().BeNull();
            dto.Email.Should().BeNull();
            dto.DepartmentMajor.Should().BeNull();
        }
    }

    public class CreateUserDtoTests
    {
        [Fact]
        public void CreateUserDto_Properties_ShouldBeSettable()
        {
            var dto = new CreateUserDto
            {
                FullName = "New User",
                Email = "new@example.com",
                DepartmentMajor = "Engineering"
            };

            dto.FullName.Should().Be("New User");
            dto.Email.Should().Be("new@example.com");
            dto.DepartmentMajor.Should().Be("Engineering");
        }

        [Fact]
        public void CreateUserDto_DefaultValues_ShouldBeEmptyStrings()
        {
            var dto = new CreateUserDto();

            dto.FullName.Should().Be(string.Empty);
            dto.Email.Should().Be(string.Empty);
            dto.DepartmentMajor.Should().Be(string.Empty);
        }
    }

    public class UpdateUserDtoTests
    {
        [Fact]
        public void UpdateUserDto_Properties_ShouldBeSettable()
        {
            var dto = new UpdateUserDto
            {
                UserId = 1,
                FullName = "Updated",
                Email = "updated@example.com",
                Department = "IT",
                IsAuthorized = true
            };

            dto.UserId.Should().Be(1);
            dto.FullName.Should().Be("Updated");
            dto.Email.Should().Be("updated@example.com");
            dto.Department.Should().Be("IT");
            dto.IsAuthorized.Should().BeTrue();
        }

        [Fact]
        public void UpdateUserDto_WithNullOptionalFields_ShouldAcceptNulls()
        {
            var dto = new UpdateUserDto
            {
                UserId = 1,
                FullName = "Required",
                Email = null,
                Department = null,
                IsAuthorized = null
            };

            dto.FullName.Should().NotBeNull();
            dto.Email.Should().BeNull();
            dto.Department.Should().BeNull();
            dto.IsAuthorized.Should().BeNull();
        }
    }
}
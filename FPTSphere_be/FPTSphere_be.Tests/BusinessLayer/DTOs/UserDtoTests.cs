using BusinessLayer.DTOs.User;
using FluentAssertions;
using Xunit;

namespace FPTSphere_be.Tests.BusinessLayer.DTOs
{
    public class UserDtoTests
    {
        [Fact]
        public void UserDto_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new UserDto();

            // Assert
            dto.UserId.Should().Be(0);
            dto.FullName.Should().BeNull();
            dto.Email.Should().BeNull();
            dto.DepartmentMajor.Should().BeNull();
        }

        [Fact]
        public void UserDto_ShouldSetUserId()
        {
            // Arrange
            var dto = new UserDto();
            var userId = 100;

            // Act
            dto.UserId = userId;

            // Assert
            dto.UserId.Should().Be(userId);
        }

        [Fact]
        public void UserDto_ShouldSetFullName()
        {
            // Arrange
            var dto = new UserDto();
            var fullName = "Test User";

            // Act
            dto.FullName = fullName;

            // Assert
            dto.FullName.Should().Be(fullName);
        }

        [Fact]
        public void UserDto_ShouldSetEmail()
        {
            // Arrange
            var dto = new UserDto();
            var email = "test@example.com";

            // Act
            dto.Email = email;

            // Assert
            dto.Email.Should().Be(email);
        }

        [Fact]
        public void UserDto_ShouldSetDepartmentMajor()
        {
            // Arrange
            var dto = new UserDto();
            var departmentMajor = "Engineering";

            // Act
            dto.DepartmentMajor = departmentMajor;

            // Assert
            dto.DepartmentMajor.Should().Be(departmentMajor);
        }

        [Fact]
        public void UserDto_ShouldSetAllProperties()
        {
            // Arrange & Act
            var dto = new UserDto
            {
                UserId = 999,
                FullName = "Complete User",
                Email = "complete@example.com",
                DepartmentMajor = "Business Administration"
            };

            // Assert
            dto.UserId.Should().Be(999);
            dto.FullName.Should().Be("Complete User");
            dto.Email.Should().Be("complete@example.com");
            dto.DepartmentMajor.Should().Be("Business Administration");
        }

        [Fact]
        public void UserDto_ShouldAllowNullProperties()
        {
            // Arrange & Act
            var dto = new UserDto
            {
                UserId = 555,
                FullName = null,
                Email = null,
                DepartmentMajor = null
            };

            // Assert
            dto.UserId.Should().Be(555);
            dto.FullName.Should().BeNull();
            dto.Email.Should().BeNull();
            dto.DepartmentMajor.Should().BeNull();
        }
    }
}
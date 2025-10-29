using BusinessLayer.DTOs.User;
using FluentAssertions;
using Xunit;

namespace FPTSphere_be.Tests.BusinessLayer.DTOs
{
    public class UpdateUserDtoTests
    {
        [Fact]
        public void UpdateUserDto_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new UpdateUserDto();

            // Assert
            dto.UserId.Should().Be(0);
            dto.FullName.Should().BeNull();
            dto.Email.Should().BeNull();
            dto.Department.Should().BeNull();
            dto.IsAuthorized.Should().BeNull();
        }

        [Fact]
        public void UpdateUserDto_ShouldSetUserId()
        {
            // Arrange
            var dto = new UpdateUserDto();
            var userId = 123;

            // Act
            dto.UserId = userId;

            // Assert
            dto.UserId.Should().Be(userId);
        }

        [Fact]
        public void UpdateUserDto_ShouldSetFullName()
        {
            // Arrange
            var dto = new UpdateUserDto();
            var fullName = "Updated Name";

            // Act
            dto.FullName = fullName;

            // Assert
            dto.FullName.Should().Be(fullName);
        }

        [Fact]
        public void UpdateUserDto_ShouldSetEmail()
        {
            // Arrange
            var dto = new UpdateUserDto();
            var email = "updated@example.com";

            // Act
            dto.Email = email;

            // Assert
            dto.Email.Should().Be(email);
        }

        [Fact]
        public void UpdateUserDto_ShouldSetDepartment()
        {
            // Arrange
            var dto = new UpdateUserDto();
            var department = "Marketing";

            // Act
            dto.Department = department;

            // Assert
            dto.Department.Should().Be(department);
        }

        [Fact]
        public void UpdateUserDto_ShouldSetIsAuthorized()
        {
            // Arrange
            var dto = new UpdateUserDto();

            // Act
            dto.IsAuthorized = true;

            // Assert
            dto.IsAuthorized.Should().BeTrue();
        }

        [Fact]
        public void UpdateUserDto_ShouldSetAllProperties()
        {
            // Arrange & Act
            var dto = new UpdateUserDto
            {
                UserId = 456,
                FullName = "Complete User",
                Email = "complete@example.com",
                Department = "HR",
                IsAuthorized = false
            };

            // Assert
            dto.UserId.Should().Be(456);
            dto.FullName.Should().Be("Complete User");
            dto.Email.Should().Be("complete@example.com");
            dto.Department.Should().Be("HR");
            dto.IsAuthorized.Should().BeFalse();
        }

        [Fact]
        public void UpdateUserDto_ShouldAllowNullableProperties()
        {
            // Arrange & Act
            var dto = new UpdateUserDto
            {
                UserId = 789,
                FullName = "Required Name",
                Email = null,
                Department = null,
                IsAuthorized = null
            };

            // Assert
            dto.UserId.Should().Be(789);
            dto.FullName.Should().Be("Required Name");
            dto.Email.Should().BeNull();
            dto.Department.Should().BeNull();
            dto.IsAuthorized.Should().BeNull();
        }
    }
}
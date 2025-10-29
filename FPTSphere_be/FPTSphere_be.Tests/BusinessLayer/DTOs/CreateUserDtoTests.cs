using BusinessLayer.DTOs.User;
using FluentAssertions;
using Xunit;

namespace FPTSphere_be.Tests.BusinessLayer.DTOs
{
    public class CreateUserDtoTests
    {
        [Fact]
        public void CreateUserDto_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new CreateUserDto();

            // Assert
            dto.FullName.Should().Be(string.Empty);
            dto.Email.Should().Be(string.Empty);
            dto.DepartmentMajor.Should().Be(string.Empty);
        }

        [Fact]
        public void CreateUserDto_ShouldSetFullName()
        {
            // Arrange
            var dto = new CreateUserDto();
            var fullName = "John Doe";

            // Act
            dto.FullName = fullName;

            // Assert
            dto.FullName.Should().Be(fullName);
        }

        [Fact]
        public void CreateUserDto_ShouldSetEmail()
        {
            // Arrange
            var dto = new CreateUserDto();
            var email = "john.doe@example.com";

            // Act
            dto.Email = email;

            // Assert
            dto.Email.Should().Be(email);
        }

        [Fact]
        public void CreateUserDto_ShouldSetDepartmentMajor()
        {
            // Arrange
            var dto = new CreateUserDto();
            var departmentMajor = "Computer Science";

            // Act
            dto.DepartmentMajor = departmentMajor;

            // Assert
            dto.DepartmentMajor.Should().Be(departmentMajor);
        }

        [Fact]
        public void CreateUserDto_ShouldSetAllProperties()
        {
            // Arrange & Act
            var dto = new CreateUserDto
            {
                FullName = "Jane Smith",
                Email = "jane.smith@example.com",
                DepartmentMajor = "Software Engineering"
            };

            // Assert
            dto.FullName.Should().Be("Jane Smith");
            dto.Email.Should().Be("jane.smith@example.com");
            dto.DepartmentMajor.Should().Be("Software Engineering");
        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData("Test User", "test@test.com", "Testing")]
        [InlineData("A", "a@b.c", "D")]
        public void CreateUserDto_ShouldHandleVariousInputs(string fullName, string email, string department)
        {
            // Arrange & Act
            var dto = new CreateUserDto
            {
                FullName = fullName,
                Email = email,
                DepartmentMajor = department
            };

            // Assert
            dto.FullName.Should().Be(fullName);
            dto.Email.Should().Be(email);
            dto.DepartmentMajor.Should().Be(department);
        }
    }
}
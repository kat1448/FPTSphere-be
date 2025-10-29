using AutoMapper;
using BusinessLayer.DTOs.User;
using BusinessLayer.Mappings;
using DataLayer.Models;
using FluentAssertions;
using Xunit;

namespace BusinessLayer.Tests
{
    public class MappingProfileTests
    {
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configuration;

        public MappingProfileTests()
        {
            _configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = _configuration.CreateMapper();
        }

        [Fact]
        public void MappingProfile_Configuration_ShouldBeValid()
        {
            // Assert
            _configuration.AssertConfigurationIsValid();
        }

        #region User to UserDto Mapping Tests

        [Fact]
        public void Map_UserToUserDto_ShouldMapAllProperties()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                FullName = "John Doe",
                Email = "john.doe@example.com",
                GoogleId = "google123",
                UserType = "Student",
                IsAuthorized = true,
                DepartmentMajor = "Computer Science",
                ClassCode = "CS101"
            };

            // Act
            var userDto = _mapper.Map<UserDto>(user);

            // Assert
            userDto.Should().NotBeNull();
            userDto.UserId.Should().Be(1);
            userDto.FullName.Should().Be("John Doe");
            userDto.Email.Should().Be("john.doe@example.com");
            userDto.DepartmentMajor.Should().Be("Computer Science");
        }

        [Fact]
        public void Map_UserToUserDto_WithNullableProperties_ShouldMapCorrectly()
        {
            // Arrange
            var user = new User
            {
                UserId = 2,
                FullName = "Jane Smith",
                Email = "jane@example.com",
                GoogleId = "google456",
                UserType = "Teacher",
                IsAuthorized = false,
                DepartmentMajor = null,
                ClassCode = null
            };

            // Act
            var userDto = _mapper.Map<UserDto>(user);

            // Assert
            userDto.Should().NotBeNull();
            userDto.UserId.Should().Be(2);
            userDto.FullName.Should().Be("Jane Smith");
            userDto.Email.Should().Be("jane@example.com");
            userDto.DepartmentMajor.Should().BeNull();
        }

        [Fact]
        public void Map_UserToUserDto_WithEmptyStrings_ShouldMapCorrectly()
        {
            // Arrange
            var user = new User
            {
                UserId = 3,
                FullName = "",
                Email = "",
                GoogleId = "google789",
                UserType = "Student",
                IsAuthorized = true,
                DepartmentMajor = "",
                ClassCode = ""
            };

            // Act
            var userDto = _mapper.Map<UserDto>(user);

            // Assert
            userDto.Should().NotBeNull();
            userDto.FullName.Should().BeEmpty();
            userDto.Email.Should().BeEmpty();
            userDto.DepartmentMajor.Should().BeEmpty();
        }

        #endregion

        #region UserDto to User Mapping Tests (ReverseMap)

        [Fact]
        public void Map_UserDtoToUser_ShouldMapAllProperties()
        {
            // Arrange
            var userDto = new UserDto
            {
                UserId = 10,
                FullName = "Alice Johnson",
                Email = "alice@example.com",
                DepartmentMajor = "Mathematics"
            };

            // Act
            var user = _mapper.Map<User>(userDto);

            // Assert
            user.Should().NotBeNull();
            user.UserId.Should().Be(10);
            user.FullName.Should().Be("Alice Johnson");
            user.Email.Should().Be("alice@example.com");
            user.DepartmentMajor.Should().Be("Mathematics");
        }

        [Fact]
        public void Map_UserDtoToUser_WithNullProperties_ShouldMapCorrectly()
        {
            // Arrange
            var userDto = new UserDto
            {
                UserId = 11,
                FullName = null,
                Email = null,
                DepartmentMajor = null
            };

            // Act
            var user = _mapper.Map<User>(userDto);

            // Assert
            user.Should().NotBeNull();
            user.UserId.Should().Be(11);
        }

        #endregion

        #region CreateUserDto to User Mapping Tests

        [Fact]
        public void Map_CreateUserDtoToUser_ShouldMapAllProperties()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                FullName = "Bob Williams",
                Email = "bob@example.com",
                DepartmentMajor = "Engineering"
            };

            // Act
            var user = _mapper.Map<User>(createDto);

            // Assert
            user.Should().NotBeNull();
            user.FullName.Should().Be("Bob Williams");
            user.Email.Should().Be("bob@example.com");
            user.DepartmentMajor.Should().Be("Engineering");
        }

        [Fact]
        public void Map_CreateUserDtoToUser_WithEmptyStrings_ShouldMapCorrectly()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                FullName = "",
                Email = "",
                DepartmentMajor = ""
            };

            // Act
            var user = _mapper.Map<User>(createDto);

            // Assert
            user.Should().NotBeNull();
            user.FullName.Should().BeEmpty();
            user.Email.Should().BeEmpty();
            user.DepartmentMajor.Should().BeEmpty();
        }

        [Fact]
        public void Map_CreateUserDtoToUser_ShouldNotSetUserId()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                FullName = "Charlie Brown",
                Email = "charlie@example.com",
                DepartmentMajor = "Physics"
            };

            // Act
            var user = _mapper.Map<User>(createDto);

            // Assert
            user.UserId.Should().Be(0); // Default value for int
        }

        #endregion

        #region UpdateUserDto to User Mapping Tests

        [Fact]
        public void Map_UpdateUserDtoToUser_ShouldMapAllProperties()
        {
            // Arrange
            var updateDto = new UpdateUserDto
            {
                UserId = 20,
                FullName = "Diana Prince",
                Email = "diana@example.com",
                Department = "Business",
                IsAuthorized = true
            };

            // Act
            var user = _mapper.Map<User>(updateDto);

            // Assert
            user.Should().NotBeNull();
            user.UserId.Should().Be(20);
            user.FullName.Should().Be("Diana Prince");
            user.Email.Should().Be("diana@example.com");
        }

        [Fact]
        public void Map_UpdateUserDtoToUser_WithNullOptionalFields_ShouldMapCorrectly()
        {
            // Arrange
            var updateDto = new UpdateUserDto
            {
                UserId = 21,
                FullName = "Edward Norton",
                Email = null,
                Department = null,
                IsAuthorized = null
            };

            // Act
            var user = _mapper.Map<User>(updateDto);

            // Assert
            user.Should().NotBeNull();
            user.UserId.Should().Be(21);
            user.FullName.Should().Be("Edward Norton");
        }

        [Fact]
        public void Map_UpdateUserDtoToUser_WithEmptyStrings_ShouldMapCorrectly()
        {
            // Arrange
            var updateDto = new UpdateUserDto
            {
                UserId = 22,
                FullName = "",
                Email = "",
                Department = ""
            };

            // Act
            var user = _mapper.Map<User>(updateDto);

            // Assert
            user.Should().NotBeNull();
            user.FullName.Should().BeEmpty();
        }

        #endregion

        #region Collection Mapping Tests

        [Fact]
        public void Map_UserListToUserDtoList_ShouldMapAllElements()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, FullName = "User 1", Email = "user1@example.com", GoogleId = "g1", UserType = "Student", IsAuthorized = true },
                new User { UserId = 2, FullName = "User 2", Email = "user2@example.com", GoogleId = "g2", UserType = "Teacher", IsAuthorized = false },
                new User { UserId = 3, FullName = "User 3", Email = "user3@example.com", GoogleId = "g3", UserType = "Admin", IsAuthorized = true }
            };

            // Act
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

            // Assert
            userDtos.Should().HaveCount(3);
            userDtos.Select(u => u.UserId).Should().ContainInOrder(1, 2, 3);
            userDtos.Select(u => u.FullName).Should().ContainInOrder("User 1", "User 2", "User 3");
        }

        [Fact]
        public void Map_EmptyUserListToUserDtoList_ShouldReturnEmptyList()
        {
            // Arrange
            var users = new List<User>();

            // Act
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

            // Assert
            userDtos.Should().NotBeNull();
            userDtos.Should().BeEmpty();
        }

        #endregion

        #region Mapping to Existing Instance Tests

        [Fact]
        public void Map_UpdateUserDtoToExistingUser_ShouldUpdateProperties()
        {
            // Arrange
            var existingUser = new User
            {
                UserId = 30,
                FullName = "Original Name",
                Email = "original@example.com",
                GoogleId = "google999",
                UserType = "Student",
                IsAuthorized = false,
                DepartmentMajor = "OriginalDept",
                ClassCode = "ORIG101"
            };

            var updateDto = new UpdateUserDto
            {
                UserId = 30,
                FullName = "Updated Name",
                Email = "updated@example.com",
                Department = "UpdatedDept",
                IsAuthorized = true
            };

            // Act
            _mapper.Map(updateDto, existingUser);

            // Assert
            existingUser.UserId.Should().Be(30);
            existingUser.FullName.Should().Be("Updated Name");
            existingUser.Email.Should().Be("updated@example.com");
        }

        #endregion
    }
}
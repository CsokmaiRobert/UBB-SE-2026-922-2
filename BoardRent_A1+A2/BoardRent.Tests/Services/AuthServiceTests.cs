namespace BoardRent.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BoardRent.Data;
    using BoardRent.DataTransferObjects;
    using BoardRent.Domain;
    using BoardRent.Repositories;
    using BoardRent.Services;
    using BoardRent.Utils;
    using Moq;
    using Xunit;

    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly Mock<IFailedLoginRepository> mockFailedLoginRepository;
        private readonly Mock<IUnitOfWorkFactory> mockUnitOfWorkFactory;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly Mock<ISessionContext> mockSessionContext;
        private readonly AuthService systemUnderTest;

        public AuthServiceTests()
        {
            this.mockUserRepository = new Mock<IUserRepository>();
            this.mockFailedLoginRepository = new Mock<IFailedLoginRepository>();
            this.mockUnitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            this.mockUnitOfWork = new Mock<IUnitOfWork>();
            this.mockSessionContext = new Mock<ISessionContext>();

            // Configurare standard pentru Unit of Work
            this.mockUnitOfWork.Setup(uow => uow.OpenAsync()).Returns(Task.CompletedTask);
            this.mockUnitOfWorkFactory.Setup(factory => factory.Create()).Returns(this.mockUnitOfWork.Object);

            this.systemUnderTest = new AuthService(
                this.mockUserRepository.Object,
                this.mockFailedLoginRepository.Object,
                this.mockUnitOfWorkFactory.Object,
                this.mockSessionContext.Object);
        }

        #region Register Tests

        [Fact]
        public async Task RegisterAsync_UsernameAlreadyExists_ReturnsFailResult()
        {
            // Arrange
            RegisterDataTransferObject registrationRequest = new RegisterDataTransferObject
            {
                Username = "existing_user",
                Password = "Password123!"
            };

            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync("existing_user"))
                .ReturnsAsync(new User { Username = "existing_user" });

            // Act
            ServiceResult<bool> registrationResult = await this.systemUnderTest.RegisterAsync(registrationRequest);

            // Assert
            Assert.False(registrationResult.Success);
            Assert.Contains("Username is already taken", registrationResult.Error);
        }

        [Fact]
        public async Task RegisterAsync_ValidData_AddsUserAndPopulatesSession()
        {
            // Arrange
            RegisterDataTransferObject registrationRequest = new RegisterDataTransferObject
            {
                Username = "new_user",
                DisplayName = "New User",
                Email = "new@test.com",
                Password = "Password123!"
            };

            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync("new_user"))
                .ReturnsAsync((User)null);

            // Act
            ServiceResult<bool> registrationResult = await this.systemUnderTest.RegisterAsync(registrationRequest);

            // Assert
            Assert.True(registrationResult.Success);
            this.mockUserRepository.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Once);
            this.mockUserRepository.Verify(repository => repository.AddRoleAsync(It.IsAny<Guid>(), "Standard User"), Times.Once);
            this.mockSessionContext.Verify(session => session.Populate(It.IsAny<User>(), "Standard User"), Times.Once);
        }

        #endregion

        #region Login Tests

        [Fact]
        public async Task LoginAsync_UserIsSuspended_ReturnsFailResult()
        {
            // Arrange
            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "suspended_user",
                Password = "AnyPassword123!"
            };

            User suspendedUser = new User { Username = "suspended_user", IsSuspended = true };
            this.mockUserRepository.Setup(repository => repository.GetByUsernameAsync("suspended_user")).ReturnsAsync(suspendedUser);

            // Act
            ServiceResult<UserProfileDataTransferObject> loginResult = await this.systemUnderTest.LoginAsync(loginRequest);

            // Assert
            Assert.False(loginResult.Success);
            Assert.Equal("This account has been suspended.", loginResult.Error);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_IncrementsFailedAttempts()
        {
            // Arrange
            string correctPassword = "CorrectPassword123!";
            string wrongPassword = "WrongPassword123!";

            User testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "test_user",
                PasswordHash = PasswordHasher.HashPassword(correctPassword),
                IsSuspended = false
            };

            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "test_user",
                Password = wrongPassword
            };

            this.mockUserRepository.Setup(repository => repository.GetByUsernameAsync("test_user")).ReturnsAsync(testUser);

            // Act
            ServiceResult<UserProfileDataTransferObject> loginResult = await this.systemUnderTest.LoginAsync(loginRequest);

            // Assert
            Assert.False(loginResult.Success);
            this.mockFailedLoginRepository.Verify(repository => repository.IncrementAsync(testUser.Id), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ResetsFailedAttemptsAndReturnsProfile()
        {
            // Arrange
            string password = "ValidPassword123!";
            User testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "valid_user",
                PasswordHash = PasswordHasher.HashPassword(password),
                IsSuspended = false,
                Roles = new List<Role> { new Role { Name = "Administrator" } }
            };

            LoginDataTransferObject loginRequest = new LoginDataTransferObject { UsernameOrEmail = "valid_user", Password = password };
            this.mockUserRepository.Setup(repository => repository.GetByUsernameAsync("valid_user")).ReturnsAsync(testUser);

            // Act
            ServiceResult<UserProfileDataTransferObject> loginResult = await this.systemUnderTest.LoginAsync(loginRequest);

            // Assert
            Assert.True(loginResult.Success);
            Assert.Equal("Administrator", loginResult.Data.Role.Name);
            this.mockFailedLoginRepository.Verify(repository => repository.ResetAsync(testUser.Id), Times.Once);
            this.mockSessionContext.Verify(session => session.Populate(testUser, "Administrator"), Times.Once);
        }

        #endregion

        #region Logout Tests

        [Fact]
        public async Task LogoutAsync_Invoked_ClearsSessionContext()
        {
            // Act
            ServiceResult<bool> logoutResult = await this.systemUnderTest.LogoutAsync();

            // Assert
            Assert.True(logoutResult.Success);
            this.mockSessionContext.Verify(session => session.Clear(), Times.Once);
        }

        #endregion
    }
}
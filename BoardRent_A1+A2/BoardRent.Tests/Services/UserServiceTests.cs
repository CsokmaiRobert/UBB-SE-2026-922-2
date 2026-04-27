namespace BoardRent.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using BoardRent.Data;
    using BoardRent.DataTransferObjects;
    using BoardRent.Domain;
    using BoardRent.Repositories;
    using BoardRent.Services;
    using BoardRent.Utils;
    using Xunit;
    using Moq;

    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly Mock<IUnitOfWorkFactory> mockUnitOfWorkFactory;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly Mock<ISessionContext> mockSessionContext;
        private readonly UserService systemUnderTest;

        public UserServiceTests()
        {
            this.mockUserRepository = new Mock<IUserRepository>();
            this.mockUnitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            this.mockUnitOfWork = new Mock<IUnitOfWork>();
            this.mockSessionContext = new Mock<ISessionContext>();

            // Configurare standard pentru Unit of Work
            this.mockUnitOfWork.Setup(unitOfWork => unitOfWork.OpenAsync()).Returns(Task.CompletedTask);
            this.mockUnitOfWorkFactory.Setup(factory => factory.Create()).Returns(this.mockUnitOfWork.Object);

            // Injectăm toate dependențele, inclusiv noul SessionContext
            this.systemUnderTest = new UserService(
                this.mockUserRepository.Object,
                this.mockUnitOfWorkFactory.Object,
                this.mockSessionContext.Object);
        }

        #region GetProfileAsync Tests

        [Fact]
        public async Task GetProfileAsync_UserDoesNotExist_ReturnsFailResult()
        {
            // Arrange
            Guid userIdentifier = Guid.NewGuid();
            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync((User)null);

            // Act
            var serviceResult = await this.systemUnderTest.GetProfileAsync(userIdentifier);

            // Assert
            Assert.False(serviceResult.Success);
            Assert.Equal("User not found.", serviceResult.Error);
        }

        [Fact]
        public async Task GetProfileAsync_UserExists_ReturnsSuccessResultWithProfileData()
        {
            // Arrange
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User
            {
                Id = userIdentifier,
                Username = "test_user",
                DisplayName = "Test User Display Name",
                Roles = new List<Role> { new Role { Id = Guid.NewGuid(), Name = "Standard User" } }
            };

            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync(testUser);

            // Act
            var serviceResult = await this.systemUnderTest.GetProfileAsync(userIdentifier);

            // Assert
            Assert.True(serviceResult.Success);
            Assert.NotNull(serviceResult.Data);
            Assert.Equal("test_user", serviceResult.Data.Username);
        }

        #endregion

        #region UpdateProfileAsync Tests

        [Fact]
        public async Task UpdateProfileAsync_ValidData_UpdatesUserAndReturnsSuccess()
        {
            // Arrange
            Guid userIdentifier = Guid.NewGuid();
            User existingUser = new User { Id = userIdentifier, Email = "original@test.com" };
            UserProfileDataTransferObject updateInformation = new UserProfileDataTransferObject
            {
                DisplayName = "Updated Display Name",
                Email = "updated@test.com"
            };

            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync(existingUser);
            this.mockUserRepository.Setup(repository => repository.GetByEmailAsync("updated@test.com")).ReturnsAsync((User)null);

            // Act
            var serviceResult = await this.systemUnderTest.UpdateProfileAsync(userIdentifier, updateInformation);

            // Assert
            Assert.True(serviceResult.Success);
            Assert.Equal("Updated Display Name", existingUser.DisplayName);
            this.mockUserRepository.Verify(repository => repository.UpdateAsync(existingUser), Times.Once);
        }

        #endregion

        #region ChangePasswordAsync Tests

        [Fact]
        public async Task ChangePasswordAsync_ValidPasswords_UpdatesPasswordAndClearsSession()
        {
            // Arrange
            Guid userIdentifier = Guid.NewGuid();
            string originalHash = PasswordHasher.HashPassword("OldPassword123!");
            User testUser = new User { Id = userIdentifier, PasswordHash = originalHash };

            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync(testUser);

            // Act
            var serviceResult = await this.systemUnderTest.ChangePasswordAsync(userIdentifier, "OldPassword123!", "NewSecurePass123!");

            // Assert
            Assert.True(serviceResult.Success);
            Assert.NotEqual(originalHash, testUser.PasswordHash);

            this.mockSessionContext.Verify(session => session.Clear(), Times.Once);
            this.mockUserRepository.Verify(repository => repository.UpdateAsync(testUser), Times.Once);
        }

        #endregion

        #region Avatar Tests

        [Fact]
        public async Task UploadAvatarAsync_UserExists_UpdatesAvatarPath()
        {
            // Arrange
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User { Id = userIdentifier };
            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync(testUser);

            string temporaryFilePath = Path.GetTempFileName();

            try
            {
                // Act
                string resultPath = await this.systemUnderTest.UploadAvatarAsync(userIdentifier, temporaryFilePath);

                // Assert
                Assert.NotNull(resultPath);
                Assert.Equal(resultPath, testUser.AvatarUrl);
                this.mockUserRepository.Verify(repository => repository.UpdateAsync(testUser), Times.Once);
            }
            finally
            {
                if (File.Exists(temporaryFilePath))
                {
                    File.Delete(temporaryFilePath);
                }
            }
        }

        [Fact]
        public async Task RemoveAvatarAsync_UserExists_SetsAvatarToNull()
        {
            // Arrange
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User { Id = userIdentifier, AvatarUrl = "C:/path/to/avatar.jpg" };
            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync(testUser);

            // Act
            await this.systemUnderTest.RemoveAvatarAsync(userIdentifier);

            // Assert
            Assert.Null(testUser.AvatarUrl);
            this.mockUserRepository.Verify(repository => repository.UpdateAsync(testUser), Times.Once);
        }

        #endregion
    }
}
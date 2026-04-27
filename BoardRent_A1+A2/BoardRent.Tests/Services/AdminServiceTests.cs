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

    public class AdminServiceTests
    {
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly Mock<IFailedLoginRepository> mockFailedLoginRepository;
        private readonly Mock<IUnitOfWorkFactory> mockUnitOfWorkFactory;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly Mock<ISessionContext> mockSessionContext;
        private readonly AdminService systemUnderTest;

        public AdminServiceTests()
        {
            this.mockUserRepository = new Mock<IUserRepository>();
            this.mockFailedLoginRepository = new Mock<IFailedLoginRepository>();
            this.mockUnitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            this.mockUnitOfWork = new Mock<IUnitOfWork>();
            this.mockSessionContext = new Mock<ISessionContext>();

            // Configurare Unit of Work
            this.mockUnitOfWork.Setup(unitOfWork => unitOfWork.OpenAsync()).Returns(Task.CompletedTask);
            this.mockUnitOfWorkFactory.Setup(factory => factory.Create()).Returns(this.mockUnitOfWork.Object);

            this.systemUnderTest = new AdminService(
                this.mockUserRepository.Object,
                this.mockFailedLoginRepository.Object,
                this.mockUnitOfWorkFactory.Object,
                this.mockSessionContext.Object);
        }

        private void SetupAdminSession()
        {
            this.mockSessionContext.Setup(session => session.IsLoggedIn).Returns(true);
            this.mockSessionContext.Setup(session => session.Role).Returns("Administrator");
        }

        #region Authorization Tests

        [Fact]
        public async Task GetAllUsersAsync_NotAdministrator_ReturnsFailResult()
        {
            // Arrange
            this.mockSessionContext.Setup(session => session.IsLoggedIn).Returns(true);
            this.mockSessionContext.Setup(session => session.Role).Returns("Standard User");

            // Act
            ServiceResult<List<UserProfileDataTransferObject>> serviceResult =
                await this.systemUnderTest.GetAllUsersAsync(1, 10);

            // Assert
            Assert.False(serviceResult.Success);
            Assert.Equal("Unauthorized access.", serviceResult.Error);
        }

        #endregion

        #region User Management Tests

        [Fact]
        public async Task SuspendUserAsync_UserExists_UpdatesStatusToSuspended()
        {
            // Arrange
            this.SetupAdminSession();
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User { Id = userIdentifier, IsSuspended = false };

            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync(testUser);

            // Act
            ServiceResult<bool> serviceResult = await this.systemUnderTest.SuspendUserAsync(userIdentifier);

            // Assert
            Assert.True(serviceResult.Success);
            Assert.True(testUser.IsSuspended);
            this.mockUserRepository.Verify(repository => repository.UpdateAsync(testUser), Times.Once);
        }

        [Fact]
        public async Task UnsuspendUserAsync_UserExists_UpdatesStatusToActive()
        {
            // Arrange
            this.SetupAdminSession();
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User { Id = userIdentifier, IsSuspended = true };

            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync(testUser);

            // Act
            ServiceResult<bool> serviceResult = await this.systemUnderTest.UnsuspendUserAsync(userIdentifier);

            // Assert
            Assert.True(serviceResult.Success);
            Assert.False(testUser.IsSuspended);
            this.mockUserRepository.Verify(repository => repository.UpdateAsync(testUser), Times.Once);
        }

        #endregion

        #region Account Security Tests

        [Fact]
        public async Task ResetPasswordAsync_PasswordTooShort_ReturnsFailResult()
        {
            // Arrange
            this.SetupAdminSession();
            Guid userIdentifier = Guid.NewGuid();

            // Act
            ServiceResult<bool> serviceResult = await this.systemUnderTest.ResetPasswordAsync(userIdentifier, "123");

            // Assert
            Assert.False(serviceResult.Success);
            Assert.Contains("at least 6 characters", serviceResult.Error);
        }

        [Fact]
        public async Task ResetPasswordAsync_ValidRequest_UpdatesPasswordHash()
        {
            // Arrange
            this.SetupAdminSession();
            Guid userIdentifier = Guid.NewGuid();
            string oldHash = "old_hash";
            User testUser = new User { Id = userIdentifier, PasswordHash = oldHash };

            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync(testUser);

            // Act
            ServiceResult<bool> serviceResult = await this.systemUnderTest.ResetPasswordAsync(userIdentifier, "NewSecurePass123!");

            // Assert
            Assert.True(serviceResult.Success);
            Assert.NotEqual(oldHash, testUser.PasswordHash);
            this.mockUserRepository.Verify(repository => repository.UpdateAsync(testUser), Times.Once);
        }

        [Fact]
        public async Task UnlockAccountAsync_Invoked_CallsResetOnFailedLoginRepository()
        {
            // Arrange
            this.SetupAdminSession();
            Guid userIdentifier = Guid.NewGuid();

            // Act
            ServiceResult<bool> serviceResult = await this.systemUnderTest.UnlockAccountAsync(userIdentifier);

            // Assert
            Assert.True(serviceResult.Success);
            this.mockFailedLoginRepository.Verify(repository => repository.ResetAsync(userIdentifier), Times.Once);
        }

        #endregion
    }
}
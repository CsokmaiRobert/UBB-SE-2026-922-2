namespace BoardRent.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BoardRent.DataTransferObjects;
    using BoardRent.Services;
    using BoardRent.Utils;
    using BoardRent.ViewModels;
    using Moq;
    using Xunit;

    public class AdminViewModelTests
    {
        private readonly Mock<IAdminService> mockAdminService;
        private readonly AdminViewModel systemUnderTest;

        public AdminViewModelTests()
        {
            this.mockAdminService = new Mock<IAdminService>();

            // ViewModel-ul apelează LoadUsersAsync în constructor via FireAndForgetSafeAsync.
            // Setup-ul de bază asigură că nu crapă la inițializare.
            this.mockAdminService
                .Setup(service => service.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(ServiceResult<List<UserProfileDataTransferObject>>.Ok(new List<UserProfileDataTransferObject>()));

            this.systemUnderTest = new AdminViewModel(this.mockAdminService.Object);
        }

        [Fact]
        public async Task LoadUsersAsync_ServiceReturnsData_PopulatesUsersCollection()
        {
            // Arrange
            List<UserProfileDataTransferObject> fakeUsers = new List<UserProfileDataTransferObject>
            {
                new UserProfileDataTransferObject { Username = "user1", DisplayName = "User One" },
                new UserProfileDataTransferObject { Username = "user2", DisplayName = "User Two" }
            };

            this.mockAdminService
                .Setup(service => service.GetAllUsersAsync(1, 10))
                .ReturnsAsync(ServiceResult<List<UserProfileDataTransferObject>>.Ok(fakeUsers));

            // Act
            await this.systemUnderTest.LoadUsersAsync();

            // Assert
            Assert.Equal(2, this.systemUnderTest.Users.Count);
            Assert.Equal("user1", this.systemUnderTest.Users[0].Username);
            Assert.False(this.systemUnderTest.IsLoading);
        }

        [Fact]
        public void SelectedUser_WhenChanged_NotifiesCommandState()
        {
            // Arrange
            UserProfileDataTransferObject testUser = new UserProfileDataTransferObject { Username = "target" };

            // Act
            this.systemUnderTest.SelectedUser = testUser;

            // Assert
            // Verificăm dacă metoda CanExecute a comenzilor s-a actualizat (trebuie să fie true acum)
            Assert.True(this.systemUnderTest.SuspendUserCommand.CanExecute(null));
            Assert.True(this.systemUnderTest.ResetPasswordCommand.CanExecute(null));
        }

        [Fact]
        public async Task SuspendUserAsync_UserSelected_CallsServiceAndReloads()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            this.systemUnderTest.SelectedUser = new UserProfileDataTransferObject { Id = userId, Username = "victim" };

            this.mockAdminService
                .Setup(service => service.SuspendUserAsync(userId))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // Act
            await this.systemUnderTest.SuspendUserCommand.ExecuteAsync(null);

            // Assert
            this.mockAdminService.Verify(service => service.SuspendUserAsync(userId), Times.Once);
            this.mockAdminService.Verify(service => service.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>()), Times.AtLeast(2));
        }

        [Fact]
        public async Task NextPageAsync_UnderTotalPages_IncrementsPageAndReloads()
        {
            // Arrange
            this.systemUnderTest.CurrentPage = 1;
            this.systemUnderTest.TotalPages = 5;

            this.mockAdminService
                .Setup(service => service.GetAllUsersAsync(2, 10))
                .ReturnsAsync(ServiceResult<List<UserProfileDataTransferObject>>.Ok(new List<UserProfileDataTransferObject>()));

            // Act
            await this.systemUnderTest.NextPageCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, this.systemUnderTest.CurrentPage);
            this.mockAdminService.Verify(service => service.GetAllUsersAsync(2, 10), Times.Once);
        }

        [Fact]
        public async Task UnlockAccountAsync_SuccessfulCall_SetsSuccessMessage()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            this.systemUnderTest.SelectedUser = new UserProfileDataTransferObject { Id = userId };

            this.mockAdminService
                .Setup(service => service.UnlockAccountAsync(userId))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // Act
            await this.systemUnderTest.UnlockAccountCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Account unlocked successfully.", this.systemUnderTest.ErrorMessage);
        }

        [Fact]
        public async Task ResetPasswordWithValueAsync_ValidPassword_SetsSuccessMessage()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            this.systemUnderTest.SelectedUser = new UserProfileDataTransferObject { Id = userId };
            string newSecret = "NewSecurePass123!";

            this.mockAdminService
                .Setup(service => service.ResetPasswordAsync(userId, newSecret))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // Act
            await this.systemUnderTest.ResetPasswordWithValueAsync(newSecret);

            // Assert
            Assert.Equal("Password has been reset successfully.", this.systemUnderTest.ErrorMessage);
        }
    }
}
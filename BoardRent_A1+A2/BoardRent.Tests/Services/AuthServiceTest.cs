using BoardRent.Data;
using BoardRent.Domain;
using BoardRent.Repositories;
using BoardRent.Services;
using BoardRent.Utils;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BoardRent.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IFailedLoginRepository> _failedLoginRepositoryMock;
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _failedLoginRepositoryMock = new Mock<IFailedLoginRepository>();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _failedLoginRepositoryMock.Object,
                _unitOfWorkFactoryMock.Object);

            SessionContext.GetInstance().Clear();
        }

        public void Dispose()
        {
            SessionContext.GetInstance().Clear();
        }

        [Fact]
        public async Task ForgotPasswordAsync_Always_ReturnsServiceResultWithAdminContactDetails()
        {
            var serviceResult = await _authService.ForgotPasswordAsync();

            Assert.Equal("Please contact the Administrator at admin@boardrent.com to reset your password.", serviceResult.Data);
        }

        [Fact]
        public async Task LogoutAsync_WhenUserIsLoggedIn_ClearsTheSessionContext()
        {
            var testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Email = "test@example.com"
            };
            SessionContext.GetInstance().Populate(testUser, "Standard User");
            Assert.True(SessionContext.GetInstance().IsLoggedIn);

            var serviceResult = await _authService.LogoutAsync();

            Assert.True(serviceResult.Data);
            Assert.False(SessionContext.GetInstance().IsLoggedIn);
        }

        [Fact]
        public async Task LogoutAsync_WhenCalled_AlwaysReturnsSuccessfulServiceResult()
        {
            var serviceResult = await _authService.LogoutAsync();

            Assert.True(serviceResult.Data);
        }
    }
}
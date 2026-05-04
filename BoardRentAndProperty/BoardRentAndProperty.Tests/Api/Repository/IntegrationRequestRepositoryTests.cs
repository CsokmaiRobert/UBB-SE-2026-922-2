using System;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Contracts.Models;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Repository
{
    [TestFixture]
    [Category("Integration")]
    public sealed class IntegrationRequestRepositoryTests : DataBaseTests
    {
        private RequestRepository requestRepository = null!;

        [SetUp]
        public void SetUp()
        {
            this.requestRepository = new RequestRepository(this.DbContextFactory);
        }

        [Test]
        public void AddRequest_ThenGetById_PreservesAllRequestFields()
        {
            int gameId = this.SeedGame(OwnerAccountId, "First Game");
            var newRequest = new Request(
                0,
                new Game { Id = gameId },
                new Account { Id = RenterAccountId, DisplayName = "Renter" },
                new Account { Id = OwnerAccountId, DisplayName = "Owner" },
                DateTime.UtcNow.AddDays(2),
                DateTime.UtcNow.AddDays(4));

            this.requestRepository.Add(newRequest);

            var fetchedRequest = this.requestRepository.Get(newRequest.Id);

            Assert.That(fetchedRequest.Id, Is.EqualTo(newRequest.Id));
            Assert.That(fetchedRequest.Game!.Id, Is.EqualTo(gameId));
            Assert.That(fetchedRequest.Renter!.Id, Is.EqualTo(RenterAccountId));
            Assert.That(fetchedRequest.Owner!.Id, Is.EqualTo(OwnerAccountId));
        }

        [Test]
        public void GetRequestsByGame_WithMultipleGames_ReturnsOnlyMatchingGameRequests()
        {
            int firstGameId = this.SeedGame(OwnerAccountId, "First Game");
            int secondGameId = this.SeedGame(OwnerAccountId, "Second Game");

            var requestForFirstGame = BuildRequest(firstGameId, 50, RequestStatus.Open);
            var requestForSecondGame = BuildRequest(secondGameId, 60, RequestStatus.Open);

            this.requestRepository.Add(requestForFirstGame);
            this.requestRepository.Add(requestForSecondGame);

            var requestsForFirstGame = this.requestRepository.GetRequestsByGame(firstGameId);

            Assert.That(requestsForFirstGame, Is.All.Matches<Request>(request => request.Game!.Id == firstGameId));
            Assert.That(requestsForFirstGame, Has.Some.Matches<Request>(request => request.Id == requestForFirstGame.Id));
            Assert.That(requestsForFirstGame, Has.None.Matches<Request>(request => request.Id == requestForSecondGame.Id));
        }

        private static Request BuildRequest(
            int gameId,
            int startOffsetInDays,
            RequestStatus status,
            Guid? offeringUserId = null)
        {
            DateTime startDate = new DateTime(2035, 1, 1, 12, 0, 0, DateTimeKind.Utc).AddDays(startOffsetInDays);
            DateTime endDate = startDate.AddDays(2);

            return new Request(
                0,
                new Game { Id = gameId },
                new Account { Id = RenterAccountId, DisplayName = "Renter" },
                new Account { Id = OwnerAccountId, DisplayName = "Owner" },
                startDate,
                endDate,
                status,
                offeringUserId.HasValue ? new Account { Id = offeringUserId.Value, DisplayName = "Offering User" } : null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class AvatarStorageServiceTests
    {
        private string temporaryRootPath = null!;
        private FakeWebHostEnvironment hostEnvironment = null!;
        private AvatarStorageService storageService = null!;

        [SetUp]
        public void SetUp()
        {
            this.temporaryRootPath = Path.Combine(Path.GetTempPath(), "brap-avatar-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.temporaryRootPath);
            this.hostEnvironment = new FakeWebHostEnvironment(this.temporaryRootPath);
            var configurationValues = new Dictionary<string, string?>
            {
                ["Storage:AvatarFolder"] = "TestAvatars",
                ["Storage:AvatarUrlPrefix"] = "/test-avatars",
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues)
                .Build();
            this.storageService = new AvatarStorageService(this.hostEnvironment, configuration);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(this.temporaryRootPath))
            {
                Directory.Delete(this.temporaryRootPath, recursive: true);
            }
        }

        [Test]
        public async Task SaveAsync_WithValidContent_WritesFileAndReturnsRelativeUrl()
        {
            var accountId = Guid.NewGuid();
            byte[] avatarBytes = new byte[] { 1, 2, 3, 4, 5 };
            using var contentStream = new MemoryStream(avatarBytes);

            string savedRelativeUrl = await this.storageService.SaveAsync(accountId, contentStream, ".png");

            string expectedFileName = accountId + ".png";
            string expectedFullPath = Path.Combine(this.temporaryRootPath, "TestAvatars", expectedFileName);
            Assert.That(savedRelativeUrl, Is.EqualTo("/test-avatars/" + expectedFileName));
            Assert.That(File.ReadAllBytes(expectedFullPath), Is.EqualTo(avatarBytes));
        }

        [Test]
        public async Task SaveAsync_NormalizesExtension_DefaultingToPngOrAddingDot()
        {
            var firstAccountId = Guid.NewGuid();
            using (var firstStream = new MemoryStream(new byte[] { 1 }))
            {
                string blankExtensionResult = await this.storageService.SaveAsync(firstAccountId, firstStream, "   ");
                Assert.That(blankExtensionResult, Does.EndWith(".png"));
            }

            var secondAccountId = Guid.NewGuid();
            using (var secondStream = new MemoryStream(new byte[] { 7 }))
            {
                string uppercaseExtensionResult = await this.storageService.SaveAsync(secondAccountId, secondStream, "JPG");
                Assert.That(uppercaseExtensionResult, Does.EndWith(".jpg"));
            }
        }

        [Test]
        public async Task SaveAsync_AccountAlreadyHasAvatar_RemovesPreviousFileBeforeWritingNewOne()
        {
            var accountId = Guid.NewGuid();
            using (var firstStream = new MemoryStream(new byte[] { 1 }))
            {
                await this.storageService.SaveAsync(accountId, firstStream, ".png");
            }

            using (var secondStream = new MemoryStream(new byte[] { 2 }))
            {
                await this.storageService.SaveAsync(accountId, secondStream, ".jpg");
            }

            string avatarFolderPath = Path.Combine(this.temporaryRootPath, "TestAvatars");
            string[] remainingFiles = Directory
                .EnumerateFiles(avatarFolderPath, accountId.ToString() + ".*")
                .ToArray();
            Assert.That(remainingFiles, Has.Length.EqualTo(1));
            Assert.That(remainingFiles[0], Does.EndWith(".jpg"));
        }

        [Test]
        public void Delete_HandlesExistingFileAndMissingFileAndWhitespaceUrl()
        {
            string avatarFolderPath = Path.Combine(this.temporaryRootPath, "TestAvatars");
            Directory.CreateDirectory(avatarFolderPath);
            string fileName = "to-delete.png";
            string absoluteFilePath = Path.Combine(avatarFolderPath, fileName);
            File.WriteAllBytes(absoluteFilePath, new byte[] { 0 });

            this.storageService.Delete("/test-avatars/" + fileName);
            Assert.That(File.Exists(absoluteFilePath), Is.False);

            Assert.DoesNotThrow(() => this.storageService.Delete("/test-avatars/missing-file.png"));
            Assert.DoesNotThrow(() => this.storageService.Delete("   "));
        }

        private sealed class FakeWebHostEnvironment : IWebHostEnvironment
        {
            public FakeWebHostEnvironment(string contentRootPath)
            {
                this.ContentRootPath = contentRootPath;
                this.WebRootPath = contentRootPath;
                this.ContentRootFileProvider = new NullFileProvider();
                this.WebRootFileProvider = new NullFileProvider();
                this.EnvironmentName = "Testing";
                this.ApplicationName = "BoardRentAndProperty.Tests";
            }

            public string ApplicationName { get; set; }

            public IFileProvider ContentRootFileProvider { get; set; }

            public string ContentRootPath { get; set; }

            public string EnvironmentName { get; set; }

            public IFileProvider WebRootFileProvider { get; set; }

            public string WebRootPath { get; set; }
        }
    }
}

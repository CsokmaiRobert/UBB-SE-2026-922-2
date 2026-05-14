using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace BoardRentAndProperty.Api.Services
{
    public class AvatarStorageService : IAvatarStorageService
    {
        private const string DefaultAvatarFolder = "Uploads/Avatars";
        private const string DefaultUrlPrefix = "/avatars";

        private readonly string avatarFolderAbsolute;
        private readonly string urlPrefix;

        public AvatarStorageService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            var folderRelative = configuration["Storage:AvatarFolder"] ?? DefaultAvatarFolder;
            this.urlPrefix = configuration["Storage:AvatarUrlPrefix"] ?? DefaultUrlPrefix;
            this.avatarFolderAbsolute = Path.Combine(environment.ContentRootPath, folderRelative);
            Directory.CreateDirectory(this.avatarFolderAbsolute);
        }

        public async Task<string> SaveAsync(Guid accountId, Stream content, string fileExtension)
        {
            string normalizedExtension = NormalizeExtension(fileExtension);
            string fileName = $"{accountId}{normalizedExtension}";

            DeleteExistingFilesForAccount(accountId);

            string destinationPath = Path.Combine(this.avatarFolderAbsolute, fileName);
            await using (var fileStream = File.Create(destinationPath))
            {
                await content.CopyToAsync(fileStream);
            }

            return $"{this.urlPrefix}/{fileName}";
        }

        public void Delete(string relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl))
            {
                return;
            }

            string fileName = Path.GetFileName(relativeUrl);
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            string absolutePath = Path.Combine(this.avatarFolderAbsolute, fileName);
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }

        private void DeleteExistingFilesForAccount(Guid accountId)
        {
            string accountPrefix = accountId.ToString();
            foreach (string existing in Directory.EnumerateFiles(this.avatarFolderAbsolute, accountPrefix + ".*"))
            {
                try
                {
                    File.Delete(existing);
                }
                catch
                {
                }
            }
        }

        private static string NormalizeExtension(string fileExtension)
        {
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                return ".png";
            }

            string trimmed = fileExtension.Trim();
            if (!trimmed.StartsWith('.'))
            {
                trimmed = "." + trimmed;
            }

            return trimmed.ToLowerInvariant();
        }
    }
}

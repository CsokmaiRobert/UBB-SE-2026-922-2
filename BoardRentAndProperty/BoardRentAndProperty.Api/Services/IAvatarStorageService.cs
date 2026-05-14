using System.IO;
using System.Threading.Tasks;

namespace BoardRentAndProperty.Api.Services
{
    public interface IAvatarStorageService
    {
        Task<string> SaveAsync(System.Guid accountId, Stream content, string fileExtension);

        void Delete(string relativeUrl);
    }
}

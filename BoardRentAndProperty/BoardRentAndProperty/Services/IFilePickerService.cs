namespace BoardRentAndProperty.Services
{
    using System.Threading.Tasks;

    public interface IFilePickerService
    {
        Task<string> PickImageFileAsync();
    }
}

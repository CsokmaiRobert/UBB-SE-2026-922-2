namespace BoardRent.Services
{
    using System;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Windows.Storage.Pickers;
    using WinRT.Interop;

    public class FilePickerService : IFilePickerService
    {
        public async Task<string> PickImageFileAsync()
        {
            // Verificăm dacă suntem în context de aplicație sau de test
            if (App.Window == null)
            {
                return null;
            }

            FileOpenPicker fileOpenPicker = new FileOpenPicker();

            IntPtr windowHandle = WindowNative.GetWindowHandle(App.Window);
            InitializeWithWindow.Initialize(fileOpenPicker, windowHandle);

            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");

            StorageFile selectedFile = await fileOpenPicker.PickSingleFileAsync();

            return selectedFile?.Path;
        }
    }
}
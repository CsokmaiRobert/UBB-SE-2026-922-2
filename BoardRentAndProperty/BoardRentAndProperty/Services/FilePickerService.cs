namespace BoardRentAndProperty.Services
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
            if (App.MainWindow == null)
            {
                return null;
            }

            FileOpenPicker fileOpenPicker = new FileOpenPicker();

#if WINDOWS
            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(fileOpenPicker, windowHandle);
#endif

            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");

            StorageFile selectedFile = await fileOpenPicker.PickSingleFileAsync();

            return selectedFile?.Path;
        }
    }
}

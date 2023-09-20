#if ANDROID
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using CommunityToolkit.Maui.Alerts;
#endif
using CommunityToolkit.Maui.Core;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        int count = 0;


        private FilePickerFileType customFileType = new FilePickerFileType(
                      new Dictionary<DevicePlatform, IEnumerable<string>>
                      {
                            { DevicePlatform.iOS, new[] { "public.image" } }, // UTType for images on iOS
                            { DevicePlatform.Android, new[] { "image/*" } }, // MIME type for images on Android
                            { DevicePlatform.WinUI, new[] { "" } }, // file extension
                      });

        public PickOptions options;


        public MainPage()
        {
            InitializeComponent();

            options = new()
            {
                PickerTitle = "Please select a comic file",
                FileTypes = customFileType,
            };

        }

#if ANDROID
        public async Task<bool> RequestPermissionAsync()
        {
            var activity = Platform.CurrentActivity ?? throw new NullReferenceException("Current activity is null");

            if (ContextCompat.CheckSelfPermission(activity, Manifest.Permission.ReadExternalStorage) == Permission.Granted)
            {
                return true;
            }

            if (ActivityCompat.ShouldShowRequestPermissionRationale(activity, Manifest.Permission.ReadExternalStorage))
            {
                await Toast.Make("Please grant access to external storage", ToastDuration.Short, 12).Show();
            }

            ActivityCompat.RequestPermissions(activity, new[] { Manifest.Permission.ReadExternalStorage }, 1);

            return false;
        }
#endif

        private async Task<Stream> GetStreamAsync(FileResult photo)
        {
#if WINDOWS
            // on Windows file.OpenReadAsync() throws an exception
            Stream sourceStream = File.OpenRead(photo.FullPath);
            //ImageSource = photo.FullPath;
#elif ANDROID
            Stream sourceStream = await photo.OpenReadAsync();
            //ImageSource = photo.FullPath;
#elif IOS
            Stream sourceStream = await photo.OpenReadAsync();
            //ImageSource = ImageSource.FromStream(() => photo.OpenReadAsync().Result);
#endif
            return sourceStream;
        }

        private async Task<FileResult> PickPhotoAsync()
        {
#if WINDOWS
            var result = await FilePicker.Default.PickAsync(options);
#elif ANDROID || IOS
            //var result = await MediaPicker.Default.PickPhotoAsync();

            //ChooserPopup popup = new();
            
            //await popupService.ShowPopupAsync(popup);
            FileResult result;

            //if (popup.Options == "File")
            //{
                result = await FilePicker.Default.PickAsync(options);
            //}
            //else if(popup.Options == "Image") {
            //    result = await MediaPicker.Default.PickPhotoAsync();
            //}
            //else
            //{
            //    await Toast.Make("No option choosen", ToastDuration.Long).Show(new CancellationToken());
            //    return null;
            //}
#endif
            return result;
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);

            Decoder de = new Decoder();
#if ANDROID

            await RequestPermissionAsync();
#endif
            FileResult r = await PickPhotoAsync();
            using Stream rStream = await GetStreamAsync(r);
            MemoryStream memoryStream = new MemoryStream();
            await rStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            await de.Main(memoryStream);
        }
    }
}
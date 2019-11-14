using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BTPaint
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        WriteableBitmap writableBitmap = new WriteableBitmap(512, 512);

        Point prevPosition;

        List<Point> drawPoints = new List<Point>();

        bool shouldErase = false;

        public MainPage()
        {
            this.InitializeComponent();

            ShowSplash();
        }

        private void MainCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            writableBitmap.FillEllipseCentered((int)e.GetCurrentPoint(MainCanvas).Position.X, (int)e.GetCurrentPoint(MainCanvas).Position.Y, (int)sizeSlider.Value / 2, (int)sizeSlider.Value / 2, colorPicker.Color);

            prevPosition = e.GetCurrentPoint(MainCanvas).Position;
            MainCanvas.PointerMoved += MainCanvas_PointerMoved;
        }

        private void MainCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            MainCanvas.PointerMoved -= MainCanvas_PointerMoved;
        }

        private void MainCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Point currentPosition = e.GetCurrentPoint(MainCanvas).Position;

            writableBitmap.DrawLineAa((int)prevPosition.X, (int)prevPosition.Y, (int)currentPosition.X, (int)currentPosition.Y, colorPicker.Color, (int)sizeSlider.Value);
            writableBitmap.FillEllipseCentered((int)e.GetCurrentPoint(MainCanvas).Position.X, (int)e.GetCurrentPoint(MainCanvas).Position.Y, (int)sizeSlider.Value / 2, (int)sizeSlider.Value / 2, colorPicker.Color);

            prevPosition = currentPosition;
        }

        private void MainCanvas_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            //Canvas right tapped (FIRED AFTER RELEASE)

            shouldErase = !shouldErase;
        }

        private void collapseSideBarBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SideBar.IsPaneOpen == true)
            {
                SideBar.IsPaneOpen = false;
            }
            else
            {
                SideBar.IsPaneOpen = true;
            }
        }

        private async void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileSavePicker.FileTypeChoices.Add("JPEG files", new List<string>() { ".jpg" });
            fileSavePicker.SuggestedFileName = "image";

            var outputFile = await fileSavePicker.PickSaveFileAsync();

            if (outputFile != null)
            {
                SoftwareBitmap outputBitmap = SoftwareBitmap.CreateCopyFromBuffer(
                writableBitmap.PixelBuffer,
                BitmapPixelFormat.Bgra8,
                writableBitmap.PixelWidth,
                writableBitmap.PixelHeight);
                SaveSoftwareBitmapToFile(outputBitmap, outputFile);
            }
        }

        private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);

                // Set additional encoding parameters, if needed
                encoder.BitmapTransform.ScaledWidth = 320;
                encoder.BitmapTransform.ScaledHeight = 240;
                encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise90Degrees;
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                encoder.IsThumbnailGenerated = true;

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                    switch (err.HResult)
                    {
                        case WINCODEC_ERR_UNSUPPORTEDOPERATION:
                            // If the encoder does not support writing a thumbnail, then try again
                            // but disable thumbnail generation.
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw;
                    }
                }

                if (encoder.IsThumbnailGenerated == false)
                {
                    await encoder.FlushAsync();
                }


            }
        }

        private async void loadBtn_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            IStorageFile file = await picker.PickSingleFileAsync();
            StorageFolder externalDevices = KnownFolders.RemovableDevices;
            await externalDevices.TryGetItemAsync(file.Name);
            if (file != null)
            {
                // Application now has read/write access to the picked file
                test.Text = "Picked photo: " + externalDevices.Name;
                string newfilepath = file.Path.Replace('\\', '/');

                testImg.Source = new BitmapImage(new Uri(newfilepath, UriKind.Absolute));
                test2.Text = "Picked photo: " + newfilepath;
                ;
            }
            else
            {
                this.test.Text = "Operation cancelled.";
            }

        }

        private void importBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }

        private void resizeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            writableBitmap = BitmapFactory.New((int)MainCanvas.ActualWidth, (int)MainCanvas.ActualHeight);
            writableBitmap.Clear(Color.FromArgb(0, 255, 0, 0));

            ImageControl.Source = writableBitmap;
        }

        private async void ShowSplash()
        {
            WelcomePage welcomePage = new WelcomePage();
            await welcomePage.ShowAsync();

            switch (welcomePage.Result)
            {
                case WelcomeSplashResult.Solo:
                    // do stuff?
                    break;
                case WelcomeSplashResult.Exit:
                    CoreApplication.Exit();
                    break;
            }

            SideBar.IsPaneOpen = true;
        }
    }
}

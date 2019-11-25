using BTPaint.Models;
using BTPaint.UserControls;
using Networking.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BTPaint
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Client client;
        
        private bool isConnected = false;
        private ImageProperties imageProperties;

        public MainPage()
        {
            this.InitializeComponent();

            sidesValue.Value = 1;
            
            ShowSplash();
        }

        private void collapseSideBarBtn_Click(object sender, RoutedEventArgs e)
        {
            SideBar.IsPaneOpen = !SideBar.IsPaneOpen;

            if (SideBar.IsPaneOpen)
            {
                collapseSideBarBtn.Icon = new SymbolIcon(Symbol.Back);
            }else
            {
                collapseSideBarBtn.Icon = new SymbolIcon(Symbol.Forward);
            }
        }

        private async void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileSavePicker.FileTypeChoices.Add("JPEG files", new List<string>() { ".jpg", ".jpeg" });
            fileSavePicker.FileTypeChoices.Add("PNG files", new List<string>() { ".png" });
            fileSavePicker.SuggestedFileName = "image";

            var outputFile = await fileSavePicker.PickSaveFileAsync();
            if (outputFile != null)
            {
                SoftwareBitmap outputBitmap = SoftwareBitmap.CreateCopyFromBuffer(
                mainCanvas.Bitmap.PixelBuffer,
                BitmapPixelFormat.Bgra8,
                mainCanvas.Bitmap.PixelWidth,
                mainCanvas.Bitmap.PixelHeight);
                SaveSoftwareBitmapToFile(outputBitmap, outputFile);
            }
        }

        private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder;
                if (outputFile.FileType == ".jpg" || outputFile.FileType == ".jpeg")
                {
                    encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                }
                else
                {
                    encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                }

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);

                // Set additional encoding parameters, if needed
                if (imageProperties != null)
                {
                    encoder.BitmapTransform.ScaledWidth = imageProperties.Width;
                    encoder.BitmapTransform.ScaledHeight = imageProperties.Height;
                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                    encoder.IsThumbnailGenerated = true;
                }
                else
                {
                    encoder.BitmapTransform.ScaledWidth = (uint)mainCanvas.Width;
                    encoder.BitmapTransform.ScaledHeight = (uint)mainCanvas.Height;
                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                    encoder.IsThumbnailGenerated = true;
                }

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
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var inputFile = await fileOpenPicker.PickSingleFileAsync();

            if (inputFile == null)
            {
                // The user cancelled the picking operation
                return;
            }

            SoftwareBitmap softwareBitmap;
            int scale = 4;
            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                if (stream.Size > 119)
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                    // Get the SoftwareBitmap representation of the file

                    ImageProperties x = await inputFile.Properties.GetImagePropertiesAsync();
                    imageProperties = x;
                    if (x.Width > 2000 && x.Height > 1600)
                    {
                        mainCanvas.Width = x.Width / (scale * 2);
                        mainCanvas.Height = x.Height / (scale * 2);
                    }
                    else if (x.Width > 1000 && x.Height > 800)
                    {
                        mainCanvas.Width = x.Width / scale;
                        mainCanvas.Height = x.Height / scale;
                    }
                    else
                    {
                        mainCanvas.Width = x.Width;
                        mainCanvas.Height = x.Height;
                    }
                    BitmapTransform bt = new BitmapTransform();
                    softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, (int)(mainCanvas.Width), (int)(mainCanvas.Height));
                    bt.ScaledHeight = (uint)mainCanvas.Height;
                    bt.ScaledWidth = (uint)mainCanvas.Width;
                    softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, bt, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);

                    await mainCanvas.Bitmap.SetSourceAsync(stream);

                    mainCanvas.Bitmap = BitmapFactory.New((int)mainCanvas.Width, (int)mainCanvas.Height);
                    mainCanvas.Bitmap.Clear(((SolidColorBrush)mainCanvas.Background).Color);

                    mainCanvas.ImageControlSource = mainCanvas.Bitmap;

                    softwareBitmap.CopyToBuffer(mainCanvas.Bitmap.PixelBuffer);
                }
            }
        }

        private void CanvasLineDrawn(DrawPacket line)
        {
            client.Send(line);
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
            mainCanvas.Clear();
        }

        private async void ShowSplash()
        {
            WelcomePage welcomePage = new WelcomePage();
            await welcomePage.ShowAsync();

            switch (welcomePage.Result)
            {
                case WelcomeSplashResult.Solo:
                    isConnected = false;
                    loadBtn.IsEnabled = true;
                    loadBtn.Visibility = Visibility.Visible;
                    break;
                case WelcomeSplashResult.Join:
                    loadBtn.IsEnabled = false;
                    loadBtn.Visibility = Visibility.Collapsed;
                    Join joinPage = new Join();
                    await joinPage.ShowAsync();
                    if (joinPage.Result == Join.JoinResult.MainMenu)
                    {
                        ShowSplash();
                    }
                    else if (joinPage.Result == Join.JoinResult.Connect)
                    {
                        client = new GuestClient();

                        try
                        {
                            ((GuestClient)client).BeginConnect(new IPEndPoint(IPAddress.Parse(joinPage.IPText), Client.DefaultPort));
                        } catch (FormatException ex)
                        {
                            ShowSplash();
                            return;
                        }
                        client.PacketReceived += mainCanvas.ProcessPacket;
                        isConnected = true;

                        mainCanvas.LineDrawn += CanvasLineDrawn;
                    }
                    break;
                case WelcomeSplashResult.Host:
                    loadBtn.IsEnabled = false;
                    loadBtn.Visibility = Visibility.Collapsed;
                    Host hostPage = new Host();
                    await hostPage.ShowAsync();
                    if (hostPage.Result == Host.HostResult.MainMenu)
                    {
                        ShowSplash();
                    }
                    else if (hostPage.Result == Host.HostResult.Host)
                    {
                        client = new HostClient();

                        ((HostClient)client).BeginAccept();
                        client.PacketReceived += mainCanvas.ProcessPacket;
                        isConnected = true;

                        mainCanvas.LineDrawn += CanvasLineDrawn;
                    }
                    break;
                case WelcomeSplashResult.Exit:
                    CoreApplication.Exit();
                    break;
            }

            if (isConnected)
            {
                connectBtn.Visibility = Visibility.Collapsed;
                disconnectBtn.Visibility = Visibility.Visible;
                fileSep1.Visibility = Visibility.Collapsed;
                clearBtn.Visibility = Visibility.Collapsed;
                loadBtn.Visibility = Visibility.Collapsed;
            }else
            {
                connectBtn.Visibility = Visibility.Visible;
                disconnectBtn.Visibility = Visibility.Collapsed;
                fileSep1.Visibility = Visibility.Visible;
                clearBtn.Visibility = Visibility.Visible;
                loadBtn.Visibility = Visibility.Visible;
            }
            SideBar.IsPaneOpen = true;
        }

        private void pencilBtn_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ShouldErase = false;
            mainCanvas.DrawColor = colorPicker.Color;
            eraserBtn.Background = new SolidColorBrush(Colors.Gray);
            pencilBtn.Background = new SolidColorBrush(Colors.White);
            polygonBtn.Background = new SolidColorBrush(Colors.Gray);
            sidesText.Visibility = Visibility.Collapsed;
            sidesSlider.Visibility = Visibility.Collapsed;
            sidesValue.Value = 1;
        }

        private void eraserBtn_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ShouldErase = true;
            pencilBtn.Background = new SolidColorBrush(Colors.Gray);
            eraserBtn.Background = new SolidColorBrush(Colors.White);
            polygonBtn.Background = new SolidColorBrush(Colors.Gray);
            sidesText.Visibility = Visibility.Collapsed;
            sidesSlider.Visibility = Visibility.Collapsed;
            sidesValue.Value = 1;
        }

        private void polygonBtn_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ShouldErase = false;
            mainCanvas.DrawColor = colorPicker.Color;
            colorPicker.Color = colorPicker.Color;
            pencilBtn.Background = new SolidColorBrush(Colors.Gray);
            eraserBtn.Background = new SolidColorBrush(Colors.Gray);
            polygonBtn.Background = new SolidColorBrush(Colors.White);
            sidesText.Visibility = Visibility.Visible;
            sidesSlider.Visibility = Visibility.Visible;
            sidesValue.Value = 3;
            sidesSlider.Value = 3;
        }

        private void sidesSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            sidesValue.Value = sidesSlider.Value;
        }

        private void colorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            mainCanvas.DrawColor = colorPicker.Color;
        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowSplash();
        }

        private void disconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowSplash();
            if (client != null) client.Close();
        }
    }
}

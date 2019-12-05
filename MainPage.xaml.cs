using BTPaint.Models;
using BTPaint.UserControls;
using Networking.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Notifications;
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

        private bool isConnected = false, splashOpen = false;
        private ImageProperties imageProperties;
        private LockDialog lockDialog = new LockDialog();

        public MainPage()
        {
            this.InitializeComponent();

            sidesValue.Value = 1;

            MainMenu();
        }

        private async void MainMenu(bool clearCanvas = true)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (clearCanvas)
                    mainCanvas.Clear();
                bool showSplashAgain = true;
                while (showSplashAgain)
                {
                    showSplashAgain = await ShowSplash();
                }
            });
        }
        private async Task<bool> ShowSplash()
        {
            bool showAgain = false;

            SideBar.IsPaneOpen = false;

            if (!splashOpen)
            {
                splashOpen = true;

                WelcomePage welcomePage = new WelcomePage();
                await welcomePage.ShowAsync();

                showAgain = await ExecuteMainMenuOption(welcomePage.Result);

                splashOpen = false;
            }
            
            UpdateConnectBtnVisibility();

            SideBar.IsPaneOpen = true;

            return showAgain;
        }
        private async Task<bool> ExecuteMainMenuOption(WelcomeSplashResult result)
        {
            bool mainShouldShowAgain = false;

            switch (result)
            {
                case WelcomeSplashResult.Solo:
                    isConnected = false;
                    loadBtn.IsEnabled = true;
                    loadBtn.Visibility = Visibility.Visible;
                    break;
                case WelcomeSplashResult.Join:
                    mainShouldShowAgain = await JoinSelected();
                    break;
                case WelcomeSplashResult.Host:
                    mainShouldShowAgain = await HostSelected();
                    break;
                case WelcomeSplashResult.Exit:
                    CoreApplication.Exit();
                    break;
            }

            return mainShouldShowAgain;
        }
        private async Task<bool> HostSelected()
        {
            bool mainMenuShouldShowAgain = false;

            client = new HostClient();
            client.RemoteConnectedHandler += ClientConnected;

            ((HostClient)client).BeginAccept();

            Host hostPage = new Host();
            await hostPage.ShowAsync();

            if (hostPage.Result == Host.HostResult.MainMenu)
            {
                mainMenuShouldShowAgain = true;
                if (client != null) client.Close();
            }
            else if (hostPage.Result == Host.HostResult.Host)
            {
                loadBtn.IsEnabled = false;
                loadBtn.Visibility = Visibility.Collapsed;

                ((HostClient)client).StopAccepting();

                mainCanvas.Clear(Colors.Transparent);

                isConnected = true;

                client.PacketReceived += mainCanvas.ProcessPacket;
                client.RemoteDisconnectedHandler += ClientDisconnected;
                mainCanvas.LineDrawn += CanvasLineDrawn;
            }

            return mainMenuShouldShowAgain;
        }
        private async Task<bool> JoinSelected()
        {
            bool mainShouldShowAgain = false;

            loadBtn.IsEnabled = false;
            loadBtn.Visibility = Visibility.Collapsed;

            Join joinPage = new Join();
            await joinPage.ShowAsync();

            if (joinPage.Result == Join.JoinResult.MainMenu)
            {
                mainShouldShowAgain = true;
            }
            else if (joinPage.Result == Join.JoinResult.Connect)
            {
                mainShouldShowAgain = await ConnectToClient(joinPage.IPText);
            }

            return mainShouldShowAgain;
        }
        private async Task<bool> ConnectToClient(string ipText)
        {
            bool mainShouldShowAgain = false;

            client = new GuestClient();

            try
            {
                ((GuestClient)client).BeginConnect(new IPEndPoint(IPAddress.Parse(ipText), Client.DefaultPort));

                client.ConnectionFailed += () => lockDialog.Hide();

                await lockDialog.ShowAsync();

                if (lockDialog.Success)
                {      
                    client.PacketReceived += FirstPacketReceived;
                    client.RemoteDisconnectedHandler += HostDisconnected;

                    mainCanvas.LineDrawn += CanvasLineDrawn;

                    isConnected = true;
                }
                else
                {
                    ShowMessage("Connection blocked by host.");
                    mainShouldShowAgain = true;
                }
            }
            catch (FormatException)
            {
                ShowMessage("IP Address is Invalid");
                mainShouldShowAgain = true;
            }

            return mainShouldShowAgain;
        }

        private void ShowMessage(string messageText)
        {
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");

            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(messageText));
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.Default");

            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = DateTime.Now.AddMilliseconds(300);
            ToastNotifier.Show(toast);
        }
        private void UpdateConnectBtnVisibility()
        {
            if (isConnected)
            {
                connectBtn.Visibility = Visibility.Collapsed;
                disconnectBtn.Visibility = Visibility.Visible;
                fileSep1.Visibility = Visibility.Collapsed;
                clearBtn.Visibility = Visibility.Collapsed;
                loadBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                connectBtn.Visibility = Visibility.Visible;
                disconnectBtn.Visibility = Visibility.Collapsed;
                fileSep1.Visibility = Visibility.Visible;
                clearBtn.Visibility = Visibility.Visible;
                loadBtn.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// Saves the Writeable Bitmap to a file the user chooses.
        /// </summary>
        /// <param name="softwareBitmap">The softwareBitmap that gets saved to the file path.</param>
        /// <param name="outputFile">The file path that the user wants to save to.</param>
        private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder;
                //Saves the file in the correct encoder
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
                // Scales the scaled photo back to the original size 
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

        private void CanvasLineDrawn(DrawPacket line)
        {
            client.Send(line);
        }
        private void HostDisconnected(IPEndPoint hostEndpoint, bool wasLastConnection = true)
        {
            MainMenu();
        }
        private void ClientDisconnected(IPEndPoint clientEndPoint, bool wasLastConnection)
        {
            // notify host of disconnection
            string userString = clientEndPoint.ToString();
            userString = userString.Substring(0, userString.IndexOf(':'));

            ShowMessage($"Aight, {userString} headed out.");

            if (wasLastConnection)
            {
                MainMenu();
            }
        }
        private void ClientConnected(IPEndPoint clientEndPoint)
        {
            string userString = clientEndPoint.ToString();
            userString = userString.Substring(0, userString.IndexOf(':'));

            ShowMessage($"{userString} is ready to party.");
        }
        private async void FirstPacketReceived(byte[] packets)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => lockDialog.Hide());
            client.PacketReceived -= FirstPacketReceived;
            client.PacketReceived += mainCanvas.ProcessPacket;
        }

        /// <summary>
        /// Gets the file location to save the Bitmap
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <summary>
        /// Closes the Program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void saveBtn_Command(XamlUICommand sender, ExecuteRequestedEventArgs args)
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
        /// <summary>
        /// Loads any Photo that is a Jpeg or PNG that the user chooses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void loadBtn_Command(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            //The types that are alowed to be seen by the user to load
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

                    //Scales the photo depending on size
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
                    //Transforms the Softwarebitmap so that it doesnt run out of memory
                    BitmapTransform bt = new BitmapTransform();
                    softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, (int)(mainCanvas.Width), (int)(mainCanvas.Height));
                    bt.ScaledHeight = (uint)mainCanvas.Height;
                    bt.ScaledWidth = (uint)mainCanvas.Width;
                    //Decodes the Image to write it to the Bitmap
                    softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, bt, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);

                    await mainCanvas.Bitmap.SetSourceAsync(stream);

                    mainCanvas.Bitmap = BitmapFactory.New((int)mainCanvas.Width, (int)mainCanvas.Height);
                    mainCanvas.Bitmap.Clear(((SolidColorBrush)mainCanvas.Background).Color);

                    mainCanvas.ImageControlSource = mainCanvas.Bitmap;

                    softwareBitmap.CopyToBuffer(mainCanvas.Bitmap.PixelBuffer);
                }
            }
        }

        /// <summary>
        /// Checks is the side bar is open. If not, open the side bar. If open, close the sidebar. Change the icon accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void collapseSideBarBtn_Click(object sender, RoutedEventArgs e)
        {
            SideBar.IsPaneOpen = !SideBar.IsPaneOpen;

            if (SideBar.IsPaneOpen)
            {
                collapseSideBarBtn.Icon = new SymbolIcon(Symbol.Back);
            }
            else
            {
                collapseSideBarBtn.Icon = new SymbolIcon(Symbol.Forward);
            }
        }
        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }
        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.Clear();
        }
        /// <summary>
        /// Highlights the pencil button, and sets the drawing style to line.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Highlights the eraser button, and sets the drawing style to eraser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Hightlights the polygon button, displays the number of sides slider, and sets the drawing style to polygon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        //show the splash screen
        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            MainMenu(false);
        }
        //show the splash screen, and close the client
        private void disconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (client != null) client.Close();
            MainMenu(false);
        }

        //event handler to set the raster canvas' current color to the color picker's color
        private void colorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            mainCanvas.DrawColor = colorPicker.Color;
        }
        //Event handler to set the sidesValue.value to the sidesSlider.value
        private void sidesSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            sidesValue.Value = sidesSlider.Value;
        }
    }
}

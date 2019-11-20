using Networking.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
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
        public TcpListener server;
        public TcpClient clientNetwork;

        public Socket listen;

        public MainPage()
        {
            this.InitializeComponent();

            //server = new TcpListener(IPAddress.Parse("192.168.0.158"), 10000);

            //server.Start();

            ShowSplash();
        }

        private void collapseSideBarBtn_Click(object sender, RoutedEventArgs e)
        {
            SideBar.IsPaneOpen = !SideBar.IsPaneOpen;
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
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);

                // Set additional encoding parameters, if needed
                encoder.BitmapTransform.ScaledWidth = (uint) mainCanvas.Width;
                encoder.BitmapTransform.ScaledHeight = (uint) mainCanvas.Height;
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

            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                var x = await inputFile.Properties.GetImagePropertiesAsync();
                mainCanvas.Width = x.Width;
                mainCanvas.Height = x.Height;

                await mainCanvas.Bitmap.SetSourceAsync(stream);
            }
        }

        private void importBtn_Click(object sender, RoutedEventArgs e)
        {
            

            //while (true)
            //{
            //    clientNetwork = server.AcceptTcpClient();

            //    byte[] receivedBuffer = new byte[4];
            //    NetworkStream stream = clientNetwork.GetStream();

            //    stream.Read(receivedBuffer, 0, receivedBuffer.Length);
            //}
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
                    // do stuff?
                    break;
                case WelcomeSplashResult.Exit:
                    CoreApplication.Exit();
                    break;
            }

            SideBar.IsPaneOpen = true;
        }

        async private void pencilBtn_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ShouldErase = false;
            eraserBtn.Background = new SolidColorBrush(Colors.Gray);
            pencilBtn.Background = new SolidColorBrush(Colors.White);

            //NETWORK CODE TESTING
            byte[] data = { 1, 2, 3, 4 };

            //IPEndPoint whereGo = new IPEndPoint(IPAddress.Parse("192.168.0.158"), 1000);

            //Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //socket.Connect(whereGo);

            //for (int i = 0; i < 10; i++)
            //{
            //    socket.Send(data);
            //}

            IPEndPoint whereFrom = new IPEndPoint(IPAddress.Parse("192.168.0.158"), 10000);
            //IPEndPoint whereCARSON = new IPEndPoint(IPAddress.Parse("192.168.0.157"), 10000);

            listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //listen.Connect(whereCARSON);
            listen.Bind(whereFrom);
            listen.Listen(100);
            listen.BeginAccept(OnConnectionEstablished, listen);

            //listen.BeginReceive(data, 0, data.Length, SocketFlags.None, ar => { Debug.WriteLine("hi"); }, null);
        }

        private struct AsyncPacket
        {
            public IAsyncResult result;
            public Socket socket;
        }

        private void OnConnectionEstablished(IAsyncResult result)
        {
            Debug.WriteLine("Attempting Connection");

            Socket clientSocket = ((Socket)result.AsyncState).EndAccept(result);

            if (clientSocket.Connected)
            {
                Debug.WriteLine("Connection established");

                AsyncPacket packet = new AsyncPacket();
                packet.result = result;
                packet.socket = clientSocket;

                byte[] data = new byte[256];
                clientSocket.BeginReceive(data, 0, data.Length, SocketFlags.None, OnPacketReceived, packet);
            }

            //((Socket)result.AsyncState).BeginAccept(OnConnectionEstablished, ((Socket)result.AsyncState));
        }

        private void OnPacketReceived(IAsyncResult result)
        {
            Debug.WriteLine("Packet received");

            AsyncPacket state = (AsyncPacket)result.AsyncState;
            int packetSize = state.socket.EndReceive(result);

            state.result = result;

            byte[] data = new byte[packetSize];
            state.socket.BeginReceive(data, 0, data.Length, SocketFlags.None, OnPacketReceived, state);
        }

        private void eraserBtn_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ShouldErase = true;
            pencilBtn.Background = new SolidColorBrush(Colors.Gray);
            eraserBtn.Background = new SolidColorBrush(Colors.White);

            listen.Close();
        }
    }
}

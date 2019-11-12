using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

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

            writableBitmap = BitmapFactory.New((int)MainCanvas.ActualWidth, (int)MainCanvas.ActualHeight);
            writableBitmap.Clear(((SolidColorBrush)MainCanvas.Background).Color);

            ImageControl.Source = writableBitmap;

            ShowSplash();
        }

        private void MainCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Color color;
            SolidColorBrush s = new SolidColorBrush(Colors.Red);

            if (shouldErase == false)
            {
                color = colorPicker.Color;
            }
            else
            {
                color = ((SolidColorBrush)MainCanvas.Background).Color;
            }

            writableBitmap.FillEllipseCentered((int)e.GetCurrentPoint(MainCanvas).Position.X, (int)e.GetCurrentPoint(MainCanvas).Position.Y, ((int)sizeSlider.Value / 2) - 1, ((int)sizeSlider.Value / 2) - 1, color);

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

            Color color;
            SolidColorBrush s = new SolidColorBrush(Colors.Red);

            if (shouldErase == false)
            {
                color = colorPicker.Color;
            }
            else
            {
                color = ((SolidColorBrush)MainCanvas.Background).Color;
            }

            writableBitmap.DrawLineAa((int)prevPosition.X, (int)prevPosition.Y, (int)currentPosition.X, (int)currentPosition.Y, color, (int)sizeSlider.Value);
            writableBitmap.FillEllipseCentered((int)e.GetCurrentPoint(MainCanvas).Position.X, (int)e.GetCurrentPoint(MainCanvas).Position.Y, ((int)sizeSlider.Value / 2) - 1, ((int)sizeSlider.Value / 2) - 1, color);

            prevPosition = currentPosition;
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

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void loadBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void importBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void resizeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {

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

        private void pencilBtn_Click(object sender, RoutedEventArgs e)
        {
            shouldErase = false;
            eraserBtn.Background = new SolidColorBrush(Colors.Gray);
            pencilBtn.Background = new SolidColorBrush(Colors.White);
        }

        private void eraserBtn_Click(object sender, RoutedEventArgs e)
        {
            shouldErase = true;
            pencilBtn.Background = new SolidColorBrush(Colors.Gray);
            eraserBtn.Background = new SolidColorBrush(Colors.White);
        }
    }
}

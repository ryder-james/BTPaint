using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
            writableBitmap.FillEllipseCentered((int)e.GetCurrentPoint(MainCanvas).Position.X, (int)e.GetCurrentPoint(MainCanvas).Position.Y, (int)sizeSlider.Value /2, (int)sizeSlider.Value /2, colorPicker.Color);

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
            writableBitmap = BitmapFactory.New((int)MainCanvas.ActualWidth, (int)MainCanvas.ActualHeight);
            writableBitmap.Clear(Color.FromArgb(0, 255, 0, 0));

            ImageControl.Source = writableBitmap;
        }

        private async void ShowSplash()
        {
            WelcomePage welcomePage = new WelcomePage();
            await welcomePage.ShowAsync();
        }
    }
}

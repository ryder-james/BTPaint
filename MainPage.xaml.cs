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

        }

        #region Mouse Input
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

        //private void MainCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        //{
        //    if (e.GetCurrentPoint(MainCanvas).Properties.IsLeftButtonPressed)
        //    {
        //        drawPoints.Add(e.GetCurrentPoint(MainCanvas).Position);

        //        if (drawPoints.Count() > 10)
        //        {
        //            writableBitmap.DrawCurve(PointsToInts(drawPoints), 1, shouldErase ? colorPicker.Color : Colors.Transparent);

        //            drawPoints.RemoveAt(0);
        //        }
        //    }
        //    else
        //    {
        //        drawPoints.Clear();
        //    }
        //}

        //private int[] PointsToInts(IEnumerable<Point> points)
        //{
        //    int[] pointArray = new int[points.Count() * 2];

        //    int i = 0;
        //    foreach (Point p in points) {
        //        pointArray[i++] = (int)p.X;
        //        pointArray[i++] = (int)p.Y;
        //    }

        //    return pointArray;
        //}
        #endregion

        #region Tapped Input
        private void MainCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Canvas tapped (FIRED ON RELEASE)
        }

        private void MainCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            //Canvas double tapped (FIRED AFTER FIRING TAP, IF IT'S A DOUBLE TAP)
        }

        private void MainCanvas_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            //Canvas right tapped (FIRED AFTER RELEASE)

            shouldErase = !shouldErase;
        }

        private void MainCanvas_Holding(object sender, HoldingRoutedEventArgs e)
        {
            //Canvas being held (CANNOT BE FIRED BY MOUSE)
        }
        #endregion

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

        private async void loadBtn_Click(object sender, RoutedEventArgs e)
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

        private void editBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            writableBitmap = BitmapFactory.New((int)MainCanvas.ActualWidth, (int)MainCanvas.ActualHeight);
            writableBitmap.Clear(Color.FromArgb(0, 255, 0, 0));

            ImageControl.Source = writableBitmap;
        }

        private void pencilBtn_Click(object sender, RoutedEventArgs e)
        {
            shouldErase = false;
            pencilBtn.Background = new SolidColorBrush(colorPicker.Color);
            eraserBtn.Background = new SolidColorBrush(Colors.White);
        }

        private void eraserBtn_Click(object sender, RoutedEventArgs e)
        {
            shouldErase = true;
            eraserBtn.Background = new SolidColorBrush(colorPicker.Color);
            pencilBtn.Background = new SolidColorBrush(Colors.White);
        }
    }
}

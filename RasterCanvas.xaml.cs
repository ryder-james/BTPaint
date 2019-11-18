using Networking.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BTPaint
{
    public sealed partial class RasterCanvas : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(RasterCanvas), null);
        public static readonly DependencyProperty DrawColorProperty = DependencyProperty.Register("DrawColor", typeof(Color), typeof(RasterCanvas), null);

        public event PropertyChangedEventHandler PropertyChanged;
        public event PacketReadyEventHandler LineDrawn;

        private bool shouldErase = false;
        private bool sizeInitialized = false;

        private Point prevPosition;
        private WriteableBitmap writeableBitmap = new WriteableBitmap(512, 512);

        public new Brush Background
        {
            get { return base.Background; }
            set
            {
                base.Background = value;
                MainCanvas.Background = value;
            }
        }

        public double Size
        {
            get { return (double) GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public new double Width
        {
            get { return base.Width; }
            set
            {
                base.Width = value;
                Bitmap.Resize((int) Width, (int) (Double.IsNaN(Height) ? Width : Height), WriteableBitmapExtensions.Interpolation.Bilinear);
                FieldChanged();
            }
        }

        public new double Height
        {
            get { return base.Height; }
            set
            {
                base.Height = value;
                Bitmap.Resize((int)Width, (int)Height, WriteableBitmapExtensions.Interpolation.Bilinear);
                FieldChanged();
            }
        }

        public Color DrawColor
        {
            get { return (Color)GetValue(DrawColorProperty); }
            set { SetValue(DrawColorProperty, value); }
        }
       
        public bool ShouldErase
        {
            get { return shouldErase; }
            set
            {
                shouldErase = value;
                FieldChanged();
            }
        }

        public WriteableBitmap Bitmap
        {
            get { return writeableBitmap; }
            set
            {
                writeableBitmap = value;
                FieldChanged();
            }
        }

        private void FieldChanged([CallerMemberName] string field = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(field));
        }

        public RasterCanvas()
        {
            this.InitializeComponent();

            ImageControl.Source = Bitmap;
        }

        public void Clear()
        {
            Clear(Colors.Transparent);
        }

        public void Clear(Color clearColor)
        {
            if (clearColor == Colors.Transparent)
            {
                clearColor = ((SolidColorBrush)MainCanvas.Background).Color;
            }

            Bitmap.Clear(clearColor);
        }

        private void MainCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            DrawColor = (ShouldErase ? ((SolidColorBrush)MainCanvas.Background).Color : DrawColor);

            int size = (int)Size;

            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                size = (int)Math.Ceiling(size * e.GetCurrentPoint(null).Properties.Pressure);
            }

            Bitmap.FillEllipseCentered((int)e.GetCurrentPoint(MainCanvas).Position.X, (int)e.GetCurrentPoint(MainCanvas).Position.Y, (int)Math.Ceiling(size / 2.0) - 1, (int)Math.Ceiling(size / 2.0) - 1, DrawColor);

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

            int size = (int)Size;

            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                size = (int)Math.Ceiling(size * e.GetCurrentPoint(null).Properties.Pressure);
            }

            Bitmap.DrawLineAa((int)prevPosition.X, (int)prevPosition.Y, (int)currentPosition.X, (int)currentPosition.Y, DrawColor, size);
            Bitmap.FillEllipseCentered((int)e.GetCurrentPoint(MainCanvas).Position.X, (int)e.GetCurrentPoint(MainCanvas).Position.Y, (int)Math.Ceiling(size / 2.0) - 1, (int)Math.Ceiling(size / 2.0) - 1, DrawColor);

            prevPosition = currentPosition;
        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!sizeInitialized)
            {
                sizeInitialized = true;
                Bitmap = new WriteableBitmap((int)MainCanvas.ActualWidth, (int)MainCanvas.ActualHeight);
                ImageControl.Source = Bitmap;
                Clear();
                MainCanvas.SizeChanged -= MainCanvas_SizeChanged;
            }
        }
    }
}

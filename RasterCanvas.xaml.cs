using BTPaint.Models;
using Networking.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public delegate void LineDrawnEventHandler(DrawPacket line);

    public sealed partial class RasterCanvas : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(RasterCanvas), null);
        public static readonly DependencyProperty PolySidesProperty = DependencyProperty.Register("PolySides", typeof(double), typeof(RasterCanvas), null);
        public static readonly DependencyProperty DrawColorProperty = DependencyProperty.Register("DrawColor", typeof(Color), typeof(RasterCanvas), null);

        public event PropertyChangedEventHandler PropertyChanged;
        public event LineDrawnEventHandler LineDrawn;

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

        public double PolySides
        {
            get { return (double)GetValue(PolySidesProperty); }
            set { SetValue(PolySidesProperty, value); }
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
            Color clearColor = ((SolidColorBrush)MainCanvas.Background).Color;
            DrawPacket(new DrawPacket(new System.Drawing.Point(0,0), new System.Drawing.Point(0,0), System.Drawing.Color.FromArgb(clearColor.A, clearColor.R, clearColor.G, clearColor.B), 999999999, 1));
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

            Point cp = e.GetCurrentPoint(MainCanvas).Position;
            System.Drawing.Point currentPosition = new System.Drawing.Point((int)cp.X, (int)cp.Y);
            System.Drawing.Color newColor = System.Drawing.Color.FromArgb(DrawColor.A, DrawColor.R, DrawColor.G, DrawColor.B);


            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                size = (int)Math.Ceiling(size * e.GetCurrentPoint(null).Properties.Pressure);
            }

            DrawPacket line = new DrawPacket(currentPosition, currentPosition, newColor, size, (byte)PolySides);

            DrawPacket(line);


            prevPosition = e.GetCurrentPoint(MainCanvas).Position;
            MainCanvas.PointerMoved += MainCanvas_PointerMoved;
        }

        private void MainCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            MainCanvas.PointerMoved -= MainCanvas_PointerMoved;
        }

        private void MainCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Point cp = e.GetCurrentPoint(MainCanvas).Position;

            System.Drawing.Point pp = new System.Drawing.Point((int)prevPosition.X, (int)prevPosition.Y);
            System.Drawing.Point currentPosition = new System.Drawing.Point((int)cp.X, (int)cp.Y);

            System.Drawing.Color newColor = System.Drawing.Color.FromArgb(DrawColor.A, DrawColor.R, DrawColor.G, DrawColor.B);

            int size = (int)Size;

            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                size = (int)Math.Ceiling(size * e.GetCurrentPoint(null).Properties.Pressure);
            }

            DrawPacket(new DrawPacket(pp, currentPosition, newColor, size, (byte)PolySides));

            prevPosition = cp;
        }

        private void DrawPacket(DrawPacket packet, bool invokeLineDrawn = true)
        {
            Color color = Color.FromArgb(packet.color.A, packet.color.R, packet.color.G, packet.color.B);

            if (packet.numOfSides < 3)
            {
                Bitmap.FillEllipseCentered(packet.pointA.X, packet.pointA.Y, (int)Math.Ceiling(packet.size / 2.0) - 1, (int)Math.Ceiling(packet.size / 2.0) - 1, color);
                Bitmap.DrawLineAa(packet.pointA.X, packet.pointA.Y, packet.pointB.X, packet.pointB.Y, color, packet.size);
                Bitmap.FillEllipseCentered(packet.pointB.X, packet.pointB.Y, (int)Math.Ceiling(packet.size / 2.0) - 1, (int)Math.Ceiling(packet.size / 2.0) - 1, color);
            }
            else
            {
                System.Drawing.Point[] prevPoints = new System.Drawing.Point[packet.numOfSides];
                System.Drawing.Point[] currPoints = new System.Drawing.Point[packet.numOfSides];
                int[] prevXY = new int[(prevPoints.Length * 2) + 2];
                int[] currXY = new int[(currPoints.Length * 2) + 2];
                int counter = 0;

                for (int i = 0; i < packet.numOfSides; i++)
                {
                    prevPoints[i] = new System.Drawing.Point((int)(packet.pointA.X + packet.size * Math.Sin(2 * Math.PI * i / packet.numOfSides)), (int)(packet.pointA.Y + packet.size * -Math.Cos(2 * Math.PI * i / packet.numOfSides)));
                    currPoints[i] = new System.Drawing.Point((int)(packet.pointB.X + packet.size * Math.Sin(2 * Math.PI * i / packet.numOfSides)), (int)(packet.pointB.Y + packet.size * -Math.Cos(2 * Math.PI * i / packet.numOfSides)));

                    prevXY[counter] = prevPoints[i].X;
                    currXY[counter] = currPoints[i].X;
                    counter++;
                    prevXY[counter] = prevPoints[i].Y;
                    currXY[counter] = currPoints[i].Y;
                    counter++;
                }
                prevXY[prevXY.Length - 1] = prevXY[1];
                prevXY[prevXY.Length - 2] = prevXY[0];
                currXY[prevXY.Length - 1] = currXY[1];
                currXY[prevXY.Length - 2] = currXY[0];
                Bitmap.FillPolygon(prevXY, color);
                Bitmap.FillPolygon(currXY, color);

                BTPaint.Models.DrawPacket.Restore(packet.ToByteArray());

            }


            if (invokeLineDrawn && LineDrawn != null)
                LineDrawn(packet);
        }

        private void DrawPackets(IEnumerable<DrawPacket> packets)
        {
            foreach(DrawPacket packet in packets)
            {
                DrawPacket(packet);
            }
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

        public async void ProcessPacket(byte[] packet)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                DrawPacket(Models.DrawPacket.Restore(packet), false);
            });
        }

    }
}

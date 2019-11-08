using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BTPaint
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        #region Mouse Input
        private void MainCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //Canvas click
            //throw new NotImplementedException();
        }

        private void MainCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //Canvas release click
            //throw new NotImplementedException();
        }

        private void MainCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //Canvas pointer moved
            //throw new NotImplementedException();
        }
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
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                this.test.Text = "Picked photo: " + file.Path;
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

        }

        private void resizeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void editBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

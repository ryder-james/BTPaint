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
        public MainPage()
        {
            this.InitializeComponent();

            ShowSplash();
        }

        private void collapseSideBarBtn_Click(object sender, RoutedEventArgs e)
        {
            SideBar.IsPaneOpen = !SideBar.IsPaneOpen;
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

        private void pencilBtn_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ShouldErase = false;
            eraserBtn.Background = new SolidColorBrush(Colors.Gray);
            pencilBtn.Background = new SolidColorBrush(Colors.White);
        }

        private void eraserBtn_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.ShouldErase = true;
            pencilBtn.Background = new SolidColorBrush(Colors.Gray);
            eraserBtn.Background = new SolidColorBrush(Colors.White);
        }
    }
}

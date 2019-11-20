using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public enum WelcomeSplashResult
    {
        Host,
        Join,
        Solo,
        Exit
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WelcomePage : ContentDialog
    {
        public WelcomeSplashResult Result { get; set; }

        public WelcomePage()
        {
            this.InitializeComponent();
            this.Result = WelcomeSplashResult.Solo;
        }

        private void soloBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Result = WelcomeSplashResult.Solo;
            this.Hide();
        }

        private void joinBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Result = WelcomeSplashResult.Join;
            this.Hide();
        }

        private void hostBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Result = WelcomeSplashResult.Solo;
            this.Hide();
        }
        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Result = WelcomeSplashResult.Exit;
            this.Hide();
        }
    }
}

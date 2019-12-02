using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BTPaint.UserControls
{
    public sealed partial class Host : ContentDialog
    {
        public enum HostResult {
            MainMenu,
            Host
        }

        public HostResult Result { get; set; }

        public Host()
        {
            this.InitializeComponent();

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPTextBlock.Text = ipHost.AddressList[1].ToString();
        }

        private void Host_Click(object sender, RoutedEventArgs e)
        {
            Result = HostResult.Host;
            this.Hide();
        }

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            Result = HostResult.MainMenu;
            this.Hide();
        }
    }
}

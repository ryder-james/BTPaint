using Networking.Models;
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
    public sealed partial class Join : ContentDialog
    {
        public enum JoinResult
        {
            MainMenu,
            Connect
        }

        public JoinResult Result { get; set; }

        public string IPText
        {
            get
            {
                return ipEnter.Text;
            }
            private set
            {
                ipEnter.Text = value;
            }
        }

        public Join()
        {
            this.InitializeComponent();
        }

        private void Join_Click(object sender, RoutedEventArgs e)
        {
            Result = JoinResult.Connect;
            this.Hide();
        }

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            Result = JoinResult.MainMenu;
            this.Hide();
        }
    }
}

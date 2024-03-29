﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BTPaint.UserControls
{
    public sealed partial class LockDialog : ContentDialog
    {
        public bool Success { get; private set; }

        public LockDialog()
        {
            this.InitializeComponent();

            Success = true;
        }

        public async void ConnectionFailed()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Success = false;

                this.Hide();
            });
 
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Success = false;

            this.Hide();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Dokumentaci k šabloně položky Prázdná stránka najdete na adrese https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x405

namespace MetropolisOnedriveKlient
{
    /// <summary>
    /// Prázdná stránka, která se dá použít samostatně nebo v rámci objektu Frame
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public static Frame ContentFrame;
        public static TextBlock PageHeader;

        public MainPage()
        {
            this.InitializeComponent();

            ContentFrame = NavigacniRamec;
            PageHeader = NadpisStrankyTextBlock;


            SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                if (ContentFrame.CanGoBack)
                {
                    ContentFrame.GoBack();
                    a.Handled = true;
                }
            };

            ContentFrame.Navigate(typeof(StrankaSoubory));
        }

        private void tlacitkoUcet_Click(object sender, RoutedEventArgs e)
        {
            // Header dělá přímo ta stránka
            ContentFrame.Navigate(typeof(StrankaPrihlaseni));
            ContentFrame.BackStack.Clear();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ContentFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void tlacitkoSoubory_Click(object sender, RoutedEventArgs e)
        {
            // Header dělá přímo ta stránka
            ContentFrame.Navigate(typeof(StrankaSoubory));
            ContentFrame.BackStack.Clear();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ContentFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void NavigacniRamec_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ContentFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }
    }
}

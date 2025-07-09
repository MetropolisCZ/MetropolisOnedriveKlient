using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using static MetropolisOnedriveKlient.ApiWebKlient;

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

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("OsobniPristupovyToken"))
            {
                var headers = httpClient.DefaultRequestHeaders;
                headers.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", ApplicationData.Current.LocalSettings.Values["OsobniPristupovyToken"].ToString());
                backgroundDownloader.SetRequestHeader("Authorization", "Bearer " + ApplicationData.Current.LocalSettings.Values["OsobniPristupovyToken"].ToString());
                ContentFrame.Navigate(typeof(StrankaSoubory));
            }
            else
            {
                ContentFrame.Navigate(typeof(StrankaNastaveni));
            }
        }

        public static void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
                e.Handled = true;
            }
        }


        private void TlacitkoUcet_Click(object sender, RoutedEventArgs e)
        {
            // Header dělá přímo ta stránka
            //ContentFrame.Navigate(typeof(StrankaPrihlaseni));
            //ContentFrame.BackStack.Clear();
            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ContentFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            NavigovatNaStranku(typeof(StrankaNastaveni));
        }

        private void TlacitkoSoubory_Click(object sender, RoutedEventArgs e)
        {
            // Header dělá přímo ta stránka
            int puvodniCacheSize = ContentFrame.CacheSize;
            ContentFrame.CacheSize = 0;
            ContentFrame.CacheSize = puvodniCacheSize;
            NavigovatNaStranku(typeof(StrankaSoubory));
        }

        private void NavigacniRamec_Navigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ContentFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void TlacitkoPrubeh_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(StrankaPrubehStahovani));
        }

        public static void NavigovatNaStranku(Type strankaKamNavigovatType)
        {
            ContentFrame.Navigate(strankaKamNavigovatType);
            ContentFrame.BackStack.Clear();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ContentFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

    }
}

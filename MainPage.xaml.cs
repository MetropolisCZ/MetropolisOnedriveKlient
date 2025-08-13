using System;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

            if (ApplicationData.Current.LocalSettings.Values["AktualizovatSlozkuPriZmeneSdileni"] == null)
            {
                ApplicationData.Current.LocalSettings.Values["AktualizovatSlozkuPriZmeneSdileni"] = false;
            }

            if (ApplicationData.Current.LocalSettings.Values["PodleCehoRaditSouboryVeSlozce"] == null)
            {
                ApplicationData.Current.LocalSettings.Values["PodleCehoRaditSouboryVeSlozce"] = 0; // Výchozí hodnota je 0 (= tedy bez zvoleného řazení)
            }

            if (ApplicationData.Current.LocalSettings.Values["UkladatRazeniSouboru"] == null)
            {
                ApplicationData.Current.LocalSettings.Values["UkladatRazeniSouboru"] = false;
            }

            ContentFrame = NavigacniRamec;
            PageHeader = NadpisStrankyTextBlock;

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("CurrentUserProviderId"))
            {
                NavigovatNaStranku(typeof(StrankaSoubory));
            }
            else
            {
                bool zobrazitPrihlaseniAutomaticky = true;
                NavigovatNaStranku(typeof(StrankaNastaveni), zobrazitPrihlaseniAutomaticky);
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

        public static void NavigovatNaStranku(Type strankaKamNavigovatType, object navigacniParametry = null)
        {
            int puvodniCacheSize = ContentFrame.CacheSize;
            ContentFrame.CacheSize = 0;
            ContentFrame.CacheSize = puvodniCacheSize;
            if (navigacniParametry == null)
            {
                ContentFrame.Navigate(strankaKamNavigovatType);
            }
            else
            {
                ContentFrame.Navigate(strankaKamNavigovatType, navigacniParametry);
            }
            ContentFrame.BackStack.Clear();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ContentFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void AppBarTlacitkoHledani_Click(object sender, RoutedEventArgs e)
        {
            NavigovatNaStranku(typeof(StrankaVyhledavac));
        }
    }
}

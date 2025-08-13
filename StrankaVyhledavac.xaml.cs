using System;
using Windows.Web.Http;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using static MetropolisOnedriveKlient.ApiWebKlient;
using Windows.Networking.BackgroundTransfer;
using Windows.ApplicationModel.Resources;

// Dokumentaci k šabloně Prázdná aplikace najdete na adrese https://go.microsoft.com/fwlink/?LinkId=234238

namespace MetropolisOnedriveKlient
{
    /// <summary>
    /// Prázdná stránka, která se dá použít samostatně nebo se na ni dá přejít v rámci
    /// </summary>
    public sealed partial class StrankaVyhledavac : Page
    {
        private static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        public StrankaVyhledavac()
        {
            InitializeComponent();

        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MainPage.PageHeader.Text = resourceLoader.GetString("AppBarTlacitkoHledani/Label");

        }

        private void VyhledavacOnedriveTlacitkoVyhledat_Click(object sender, RoutedEventArgs e)
        {
            if (VyhledavacOnedriveTextBox.Text.Length >= 1)
            { // Hodnota vyhledávání musí být dlouhá aspoň 1. Asi bych měl udělat chybovou zprávu, ale strašně se mi nechce 😂

                MainPage.NavigovatNaStranku(typeof(StrankaSoubory), VyhledavacOnedriveTextBox.Text);
            }
        }

        private void VyhledavacOnedriveTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                VyhledavacOnedriveTlacitkoVyhledat_Click(VyhledavacOnedriveTlacitkoVyhledat, new RoutedEventArgs());
            }
        }
    }
}

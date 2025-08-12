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
    public sealed partial class StrankaNastaveni : Page
    {
        private static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        public StrankaNastaveni()
        {
            this.InitializeComponent();


            NastaveniPrepinacAktualizovatPriZmeneSdileni.IsOn = (bool)ApplicationData.Current.LocalSettings.Values["AktualizovatSlozkuPriZmeneSdileni"];

            NastaveniPrepinacUkladatRazeniSouboru.IsOn = (bool)ApplicationData.Current.LocalSettings.Values["UkladatRazeniSouboru"];


            NastaveniVychoziMoznostRazeniSouboru.Items.Add(new ComboBoxItem { Content = resourceLoader.GetString("Vychozi") });
            NastaveniVychoziMoznostRazeniSouboru.Items.Add(new ComboBoxItem { Content = resourceLoader.GetString("ComboBoxRazeniPolozek/NazevAZ") });
            NastaveniVychoziMoznostRazeniSouboru.Items.Add(new ComboBoxItem { Content = resourceLoader.GetString("ComboBoxRazeniPolozek/NazevZA") });
            NastaveniVychoziMoznostRazeniSouboru.Items.Add(new ComboBoxItem { Content = resourceLoader.GetString("ComboBoxRazeniPolozek/DatumOdNejstarsiho") });
            NastaveniVychoziMoznostRazeniSouboru.Items.Add(new ComboBoxItem { Content = resourceLoader.GetString("ComboBoxRazeniPolozek/DatumOdNejnovejsiho") });
            NastaveniVychoziMoznostRazeniSouboru.SelectedIndex = (int)ApplicationData.Current.LocalSettings.Values["PodleCehoRaditSouboryVeSlozce"];

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AccountsSettingsPane.Show();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += NaplnitPrihlasovaciMoznosti;
            MainPage.PageHeader.Text = resourceLoader.GetString("StrankaNastaveniNadpis");

            if (e?.Parameter != null && (bool)e.Parameter) // zobrazitPrihlaseniAutomaticky
            {
                AccountsSettingsPane.Show();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested -= NaplnitPrihlasovaciMoznosti;
        }

        private async void NaplnitPrihlasovaciMoznosti(AccountsSettingsPane s, AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();

            var msaProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(
                "https://login.microsoft.com", "consumers");

            var command = new WebAccountProviderCommand(msaProvider, GetAadTokenAsync);

            e.WebAccountProviderCommands.Add(command);

            deferral.Complete();
        }

        private async void GetAadTokenAsync(WebAccountProviderCommand command)
        {
            string clientId = "d0342bc7-f4d3-422e-97d7-354ecdc21ae7"; // Obtain your clientId from the Azure Portal
            WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, "Files.ReadWrite.All", clientId);
            request.Properties.Add("resource", "https://graph.microsoft.com");
            WebTokenRequestResult result = await WebAuthenticationCoreManager.RequestTokenAsync(request);


            if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                string pristupovyToken = result.ResponseData[0].Token;
                App.OsobniPristupovyToken = pristupovyToken;

                ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"] = result.ResponseData[0].WebAccount.WebAccountProvider.Id;
                ApplicationData.Current.LocalSettings.Values["CurrentUserId"] = result.ResponseData[0].WebAccount.Id;

                backgroundDownloader = new BackgroundDownloader();
                backgroundDownloader.SetRequestHeader("Authorization", "Bearer " + pristupovyToken);

                //backgroundUploader = new BackgroundUploader() { Method = "PUT" };
                //backgroundUploader.SetRequestHeader("Authorization", "Bearer " + pristupovyToken);

                var headers = httpClient.DefaultRequestHeaders;
                headers.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", pristupovyToken);

                MainPage.NavigovatNaStranku(typeof(StrankaSoubory));

                //JObject repository_url = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me"));
            }
            else
            {
                _ = await new ContentDialog()
                {
                    Title = resourceLoader.GetString("Chyba") + " " + nameof(WebTokenRequestStatus),
                    CloseButtonText = resourceLoader.GetString("ZavritDialog")

            }.ShowAsync();
            }
        }

        private void NastaveniPrepinacAktualizovatPriZmeneSdileni_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch prepinacSender = sender as ToggleSwitch;

            ApplicationData.Current.LocalSettings.Values["AktualizovatSlozkuPriZmeneSdileni"] = prepinacSender.IsOn;
        }

        private void NastaveniPrepinacUkladatRazeniSouboru_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch prepinacSender = sender as ToggleSwitch;

            ApplicationData.Current.LocalSettings.Values["UkladatRazeniSouboru"] = prepinacSender.IsOn;
        }

        private void NastaveniVychoziMoznostRazeniSouboru_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox ZdrojovyComboBox = (ComboBox)sender;

            ApplicationData.Current.LocalSettings.Values["PodleCehoRaditSouboryVeSlozce"] = ZdrojovyComboBox.SelectedIndex;
        }
    }
}

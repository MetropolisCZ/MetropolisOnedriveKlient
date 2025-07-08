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

// Dokumentaci k šabloně Prázdná aplikace najdete na adrese https://go.microsoft.com/fwlink/?LinkId=234238

namespace MetropolisOnedriveKlient
{
    /// <summary>
    /// Prázdná stránka, která se dá použít samostatně nebo se na ni dá přejít v rámci
    /// </summary>
    public sealed partial class StrankaNastaveni : Page
    {

        public StrankaNastaveni()
        {
            this.InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AccountsSettingsPane.Show();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += NaplnitPrihlasovaciMoznosti;
            MainPage.PageHeader.Text = "Nastavení";
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
            WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, "Files.Read.All", clientId);
            request.Properties.Add("resource", "https://graph.microsoft.com");
            WebTokenRequestResult result = await WebAuthenticationCoreManager.RequestTokenAsync(request);


            if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                ApplicationData.Current.LocalSettings.Values["OsobniPristupovyToken"] = result.ResponseData[0].Token;
                var headers = httpClient.DefaultRequestHeaders;
                headers.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", result.ResponseData[0].Token);
                backgroundDownloader.SetRequestHeader("Authorization", "Bearer " + result.ResponseData[0].Token);

                //JObject repository_url = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me"));
            }
        }

        /*private void Button_Click(object sender, RoutedEventArgs e)
        {
            var item = new DownloadItem
            {
                FileName = "TestFile.txt",
                Progress = 0,
                Status = "Starting..."
            };
            DownloadManager.Instance.Downloads.Add(item);

            // Simulate progress
            _ = Task.Run(async () =>
            {
                for (int i = 0; i <= 100; i += 10)
                {
                    await Task.Delay(300);
                    item.Progress = i;
                    item.Status = $"Downloading... {i}%";
                }
                item.Status = "Completed!";
            });
        }*/
    }
}

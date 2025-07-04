using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Web.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web.Core;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Dokumentaci k šabloně Prázdná aplikace najdete na adrese https://go.microsoft.com/fwlink/?LinkId=234238

namespace MetropolisOnedriveKlient
{
    /// <summary>
    /// Prázdná stránka, která se dá použít samostatně nebo se na ni dá přejít v rámci
    /// </summary>
    public sealed partial class StrankaPrihlaseni : Page
    {

        private HttpClient httpClient = new HttpClient();

        public StrankaPrihlaseni()
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
            MainPage.PageHeader.Text = "Účet";
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
            WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, "User.Read", clientId);
            request.Properties.Add("resource", "https://graph.microsoft.com");
            WebTokenRequestResult result = await WebAuthenticationCoreManager.RequestTokenAsync(request);


            if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                string token = result.ResponseData[0].Token;

                var headers = httpClient.DefaultRequestHeaders;
                headers.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", token);

                JObject repository_url = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me"));
            }
        }

        public async Task<string> NacistStrankuRestApi(string UrlkZiskani)
        {
            HttpResponseMessage httpResponse = await httpClient.GetAsync(new Uri(UrlkZiskani));
            httpResponse.EnsureSuccessStatusCode();

            return await httpResponse.Content.ReadAsStringAsync();
        }
    }
}

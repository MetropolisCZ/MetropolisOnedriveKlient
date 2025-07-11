using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.BackgroundTransfer;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace MetropolisOnedriveKlient
{
    public class ApiWebKlient
    {

        public static HttpClient httpClient = new HttpClient();
        public static BackgroundDownloader backgroundDownloader = new BackgroundDownloader();

        public enum TypyHTTPrequestu
        {
            Get,
            Post,
            Put,
            Patch,
            Delete
        }

        public static async Task<string> NacistStrankuRestApi(string UrlkZiskani, TypyHTTPrequestu typHTTPrequestu = TypyHTTPrequestu.Get, string teloHTTPrequestu = null)
        {
            bool prvniPokus = true;
        druhyPokus:

            HttpResponseMessage httpResponse = new HttpResponseMessage();

            if (typHTTPrequestu == TypyHTTPrequestu.Get)
            {
                httpResponse = await httpClient.GetAsync(new Uri(UrlkZiskani));
            }
            else if (typHTTPrequestu == TypyHTTPrequestu.Patch)
            { // Upraví vlastnosti, zachová soubor

                httpResponse = await httpClient.SendRequestAsync(new HttpRequestMessage(HttpMethod.Patch, new Uri(UrlkZiskani)) { Content = new HttpStringContent(teloHTTPrequestu, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json") });
            }
            else if (typHTTPrequestu == TypyHTTPrequestu.Post)
            { // Posílá data na server, narozdíl od PUT je možné POST volat vícekrát což může mít za následek například vícenásobné vytvoření téže položky

                httpResponse = await httpClient.PostAsync(new Uri(UrlkZiskani), new HttpStringContent(teloHTTPrequestu, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json"));
                //httpResponse = await httpClient.SendRequestAsync(new HttpRequestMessage(HttpMethod.Patch, new Uri(UrlkZiskani)) { Content = new HttpStringContent(teloHTTPrequestu, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json") });
            }
            else if (typHTTPrequestu == TypyHTTPrequestu.Put)
            {

            }
            else if (typHTTPrequestu == TypyHTTPrequestu.Delete)
            {
                httpResponse = await httpClient.DeleteAsync(new Uri(UrlkZiskani));
            }

            if (httpResponse.IsSuccessStatusCode || (typHTTPrequestu == TypyHTTPrequestu.Delete && httpResponse.StatusCode == HttpStatusCode.NoContent))
            {

                return await httpResponse.Content.ReadAsStringAsync();

            }
            else
            {
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized && prvniPokus)
                {
                    if (await NacistPristupovyTokenNaPozadi())
                    {
                        backgroundDownloader = new BackgroundDownloader();
                        backgroundDownloader.SetRequestHeader("Authorization", "Bearer " + App.OsobniPristupovyToken);
                        var headers = httpClient.DefaultRequestHeaders;
                        headers.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", App.OsobniPristupovyToken);
                        prvniPokus = false;
                        goto druhyPokus;
                    }
                    else
                    {
                        bool zobrazitPrihlaseniAutomaticky = true;
                        MainPage.NavigovatNaStranku(typeof(StrankaNastaveni), zobrazitPrihlaseniAutomaticky);

                        throw new OperationCanceledException();
                    }

                }
                else
                {
                    ContentDialog dialogHTTPchyba = new ContentDialog()
                    {
                        Title = "HTTP odpověď " + httpResponse.StatusCode,
                        Content = await httpResponse.Content.ReadAsStringAsync() + "\n\n" + UrlkZiskani,
                        CloseButtonText = "Zavřit"
                    };

                    _ = await dialogHTTPchyba.ShowAsync();
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool zobrazitPrihlaseniAutomaticky = true;
                        MainPage.NavigovatNaStranku(typeof(StrankaNastaveni), zobrazitPrihlaseniAutomaticky);
                    }
                    else
                    {
                        MainPage.NavigovatNaStranku(typeof(StrankaNastaveni));
                    }

                    throw new System.Net.Http.HttpRequestException();
                }

            }

        }


        public static async Task<bool> NacistPristupovyTokenNaPozadi()
        {
            string providerId = ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"]?.ToString();
            string accountId = ApplicationData.Current.LocalSettings.Values["CurrentUserId"]?.ToString();

            if (null == providerId || null == accountId)
            {

                return false;
            }

            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);
            WebAccount account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountId);

            WebTokenRequest request = new WebTokenRequest(provider, "Files.ReadWrite.All");

            WebTokenRequestResult result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request, account);
            if (result.ResponseStatus == WebTokenRequestStatus.UserInteractionRequired)
            {
                // Unable to get a token silently - you'll need to show the UI
                return false;
            }
            else if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                // Success
                App.OsobniPristupovyToken = result.ResponseData[0].Token;
                return true;
            }
            else
            {
                // Other error
                return false;
            }
        }



        public static async Task StahnoutSoubory(List<OneDriveAdresarSoubory> souboryKeStazeni, bool jenomOtevritTemp = false)
        {
            if (souboryKeStazeni.Count > 0)
            {
                //new ToastContentBuilder().AddText("Stahování " + souboryKeStazeni.Count + " položek").Show();
                MainPage.ContentFrame.Navigate(typeof(StrankaPrubehStahovani)); // Navigovat na stahování


                foreach (OneDriveAdresarSoubory jedenSouborKeStazeni in souboryKeStazeni)
                {

                    DownloadItem polozkaSeznamStahovani = new DownloadItem
                    {
                        FileName = jedenSouborKeStazeni.Name,
                        Progress = 0,
                        Status = "Připraveno",
                        JenomTemp = jenomOtevritTemp
                    };

                    DownloadManager.Instance.Downloads.Add(polozkaSeznamStahovani);

                    try
                    {
                        StorageFile destinationFile;

                        if (!jenomOtevritTemp)
                        { // Normálně stáhnout –> do stažených souborů

                            destinationFile = await DownloadsFolder.CreateFileAsync(jedenSouborKeStazeni.Name, CreationCollisionOption.GenerateUniqueName);
                        }
                        else
                        { // Stáhnout do tempu (příklad stáhnu instalátor, otevřu ho a je mi jedno, jestli se smaže)

                            destinationFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(jedenSouborKeStazeni.Name, CreationCollisionOption.GenerateUniqueName);
                            polozkaSeznamStahovani.Status += " – dočasný soubor";
                        }


                        polozkaSeznamStahovani.StorageFile = destinationFile;

                        await backgroundDownloader.CreateDownload(new Uri("https://graph.microsoft.com/v1.0/me/drive/items/" + jedenSouborKeStazeni.Id + "/content"), destinationFile).StartAsync().AsTask(new Progress<DownloadOperation>(async progress =>
                        {
                            double percent = 100.0 * progress.Progress.BytesReceived / progress.Progress.TotalBytesToReceive;

                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                polozkaSeznamStahovani.Progress = percent;
                                polozkaSeznamStahovani.Status = progress.Progress.Status == BackgroundTransferStatus.Running ? "Probíhá" : progress.Progress.Status.ToString();     
                                if (jenomOtevritTemp)
                                {
                                    polozkaSeznamStahovani.Status += " – dočasný soubor";
                                }
                            });
                        }));

                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            polozkaSeznamStahovani.Status = "Dokončeno";
                            if (jenomOtevritTemp)
                            {
                                polozkaSeznamStahovani.Status += " – dočasný soubor";
                                await Windows.System.Launcher.LaunchFileAsync(destinationFile);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            polozkaSeznamStahovani.Status = "Chyba: " + ex.Message;
                        });
                    }
                }

            }
            else
            { // Žádný soubor ke stažení nebyl zvolen
                ContentDialog dialogHTTPchyba = new ContentDialog()
                {
                    Title = "Nebyl zvolen žádný soubor ke stažení",
                    CloseButtonText = "Zavřit"
                };

                _ = await dialogHTTPchyba.ShowAsync();

                //return null;
            }
        }

    }










    // ✅ Reactive model for a single download
    public class DownloadItem : INotifyPropertyChanged
    {
        private string fileName;
        private double progress;
        private string status;

        public string FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                OnPropertyChanged();
            }
        }

        public double Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        public StorageFile StorageFile { get; set; }
        public bool JenomTemp { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // ✅ Singleton manager that holds all downloads
    public class DownloadManager
    {
        private static DownloadManager _instance;

        public static DownloadManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DownloadManager();
                return _instance;
            }
        }

        public ObservableCollection<DownloadItem> Downloads { get; } = new ObservableCollection<DownloadItem>();
        private DownloadManager() { }
    }


}

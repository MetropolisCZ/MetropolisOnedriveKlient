using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Networking.BackgroundTransfer;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using static MetropolisOnedriveKlient.MainPage;

namespace MetropolisOnedriveKlient
{
    public class ApiWebKlient
    {
        private static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        public static HttpClient httpClient = new HttpClient();
        public static BackgroundDownloader backgroundDownloader = new BackgroundDownloader();
        public static BackgroundUploader backgroundUploader = new BackgroundUploader() { Method = "PUT" };

        public enum TypyHTTPrequestu
        {
            Get,
            Post,
            Put,
            Patch,
            Delete
        }

        public static async Task<string> NacistStrankuRestApi(string UrlkZiskani, TypyHTTPrequestu typHTTPrequestu = TypyHTTPrequestu.Get, string teloHTTPrequestu = null, StorageFile souborkNahrani = null)
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
                        prvniPokus = false;
                        goto druhyPokus;
                    }
                    else
                    {
                        bool zobrazitPrihlaseniAutomaticky = true;
                        NavigovatNaStranku(typeof(StrankaNastaveni), zobrazitPrihlaseniAutomaticky);

                        throw new OperationCanceledException();
                    }

                }
                else
                {
                    ContentDialog dialogHTTPchyba = new ContentDialog()
                    {
                        Title = resourceLoader.GetString("dialogHTTPchyba/Title") + " " + httpResponse.StatusCode,
                        Content = await httpResponse.Content.ReadAsStringAsync() + "\n\n" + UrlkZiskani,
                        CloseButtonText = resourceLoader.GetString("ZavritDialog"),
                        DefaultButton = ContentDialogButton.Primary
                    };

                    _ = await dialogHTTPchyba.ShowAsync();
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool zobrazitPrihlaseniAutomaticky = true;
                        NavigovatNaStranku(typeof(StrankaNastaveni), zobrazitPrihlaseniAutomaticky);
                    }
                    else
                    {
                        NavigovatNaStranku(typeof(StrankaNastaveni));
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
                string pristupovyToken = result.ResponseData[0].Token;
                App.OsobniPristupovyToken = pristupovyToken;

                backgroundDownloader = new BackgroundDownloader();
                backgroundDownloader.SetRequestHeader("Authorization", "Bearer " + pristupovyToken);

                //backgroundUploader = new BackgroundUploader() { Method = "PUT" };
                //backgroundUploader.SetRequestHeader("Authorization", "Bearer " + pristupovyToken);

                HttpRequestHeaderCollection headers = httpClient.DefaultRequestHeaders;
                headers.Authorization = new HttpCredentialsHeaderValue("Bearer", pristupovyToken);

                return true;
            }
            else
            {
                // Other error
                return false;
            }
        }


        public static async Task NahratSoubory(string cestaKamNahrat, IReadOnlyList<StorageFile> souboryKnahrani)
        {
            if (souboryKnahrani.Count > 0)
            {
                ContentFrame.Navigate(typeof(StrankaPrubehStahovani), 1); // Navigovat na stahování
                int puvodniCacheSize = ContentFrame.CacheSize;
                ContentFrame.CacheSize = 0;
                ContentFrame.CacheSize = puvodniCacheSize;

                foreach (StorageFile jedenSouborKnahrani in souboryKnahrani)
                {

                    Windows.Storage.FileProperties.BasicProperties vlastnostiJedenSouborKnahrani = await jedenSouborKnahrani.GetBasicPropertiesAsync();

                    string relaceKamNahravat_url = "";
                    string relaceKamNahravat_expirace = "";

                    // POZOR NA TO! "@microsoft.graph.conflictBehavior": "rename" musí být první!! Jinak to hází chybu! Očividně je tam jasně fixovaná možnost pořadí
                    string jedenSouborKnahrani_jsonBody = "{ '@microsoft.graph.conflictBehavior': 'rename', 'item': { 'name': '" + jedenSouborKnahrani.Name + "' } }";

                    try
                    {
                        if (cestaKamNahrat.Length == 0)
                        {
                            cestaKamNahrat = ":";
                        }
                        JObject odpovedVytvoreniRelaceNahravani = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root" + cestaKamNahrat + "/" + Uri.EscapeDataString(jedenSouborKnahrani.Name) + ":/createUploadSession", TypyHTTPrequestu.Post, jedenSouborKnahrani_jsonBody));
                        relaceKamNahravat_url = odpovedVytvoreniRelaceNahravani.SelectToken("uploadUrl").ToString();
                        relaceKamNahravat_expirace = odpovedVytvoreniRelaceNahravani.SelectToken("expirationDateTime").ToString();
                    }
                    catch
                    {
                        throw new TaskCanceledException();
                    }

                    DownloadItem polozkaSeznamNahravani = new DownloadItem
                    {
                        FileName = jedenSouborKnahrani.Name,
                        Progress = 0,
                        Status = resourceLoader.GetString("polozkaSeznamNahravani/Pripraveno")
                    };

                    DownloadManager.Instance.Uploads.Add(polozkaSeznamNahravani);

                    //string uriZadostiNahrani = "https://graph.microsoft.com/v1.0/me/drive/items/" + idSlozkyKamNahrat + ":/" + jedenSouborKnahrani.Name + ":/content";

                    try
                    {

                        /*UploadOperation operaceNahravaniNaPozadi = await backgroundUploader.CreateUpload(new Uri(relaceKamNahravat_url), jedenSouborKnahrani).StartAsync().AsTask(new Progress<UploadOperation>(async progress =>
                        {
                            double percent = 100.0 * progress.Progress.BytesSent / progress.Progress.TotalBytesToSend;

                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                polozkaSeznamNahravani.Progress = percent;
                                polozkaSeznamNahravani.Status = progress.Progress.Status == BackgroundTransferStatus.Running ? "Probíhá" : progress.Progress.Status.ToString();
                            });
                        }));


                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            polozkaSeznamNahravani.Status = "Dokončeno";
                        });*/

                        IRandomAccessStream randomAccessStream = await jedenSouborKnahrani.OpenAsync(FileAccessMode.Read);

                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, new Uri(relaceKamNahravat_url))
                        {
                            Content = new HttpStreamContent(randomAccessStream.GetInputStreamAt(0))
                        };
                        request.Content.Headers.TryAppendWithoutValidation("Content-Length", randomAccessStream.Size.ToString());
                        string contentRangeHeader = "bytes 0-" + (randomAccessStream.Size - 1) + "/" + randomAccessStream.Size;
                        request.Content.Headers.TryAppendWithoutValidation("Content-Range", contentRangeHeader);



                        HttpClient httpClientBezTokenu = new HttpClient();
                        
                        HttpResponseMessage odpovedNahravaniSouboru = await httpClientBezTokenu.SendRequestAsync(request).AsTask(new Progress<HttpProgress>(progress =>
                        {
                            double percent = 100.0 * progress.BytesSent / vlastnostiJedenSouborKnahrani.Size;

                            _ = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                polozkaSeznamNahravani.Progress = percent;
                                polozkaSeznamNahravani.Status = progress.Stage == HttpProgressStage.SendingContent
                                    ? resourceLoader.GetString("polozkaSeznamNahravani/Probiha")
                                    : progress.Stage.ToString();
                            });
                        }));

                        /*UploadOperation operaceNahravaniNaPozadi = await backgroundUploader.CreateUploadFromStreamAsync(new Uri(relaceKamNahravat_url), stream2);
                        await operaceNahravaniNaPozadi.StartAsync().AsTask(new Progress<UploadOperation>(progress =>
                        {
                            double percent = 100.0 * progress.Progress.BytesSent / progress.Progress.TotalBytesToSend;

                            _ = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                polozkaSeznamNahravani.Progress = percent;
                                polozkaSeznamNahravani.Status = progress.Progress.Status == BackgroundTransferStatus.Running
                                    ? "Probíhá"
                                    : progress.Progress.Status.ToString();
                            });
                        }));
                        */

                        odpovedNahravaniSouboru.EnsureSuccessStatusCode();

                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            polozkaSeznamNahravani.Status = resourceLoader.GetString("polozkaSeznamNahravani/Dokonceno");
                        });

                    }
                    catch (Exception ex)
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            polozkaSeznamNahravani.Status = resourceLoader.GetString("Chyba") + ": " + ex.Message;
                        });
                        throw new OperationCanceledException();
                    }

                }

                NavigovatNaStranku(typeof(StrankaSoubory));

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
                        Status = resourceLoader.GetString("polozkaSeznamNahravani/Pripraveno"),
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
                            polozkaSeznamStahovani.Status += " – " + resourceLoader.GetString("polozkaSeznamStahovani/DocasnySoubor");
                        }


                        polozkaSeznamStahovani.StorageFile = destinationFile;

                        DownloadOperation operaceStahovaniNaPozadi = await backgroundDownloader.CreateDownload(new Uri("https://graph.microsoft.com/v1.0/me/drive/items/" + jedenSouborKeStazeni.Id + "/content"), destinationFile).StartAsync().AsTask(new Progress<DownloadOperation>(async progress =>
                        {
                            double percent = 100.0 * progress.Progress.BytesReceived / progress.Progress.TotalBytesToReceive;

                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                polozkaSeznamStahovani.Progress = percent;
                                polozkaSeznamStahovani.Status = progress.Progress.Status == BackgroundTransferStatus.Running ? resourceLoader.GetString("polozkaSeznamNahravani/Probiha") : progress.Progress.Status.ToString();
                                if (jenomOtevritTemp)
                                {
                                    polozkaSeznamStahovani.Status += " – " + resourceLoader.GetString("polozkaSeznamStahovani/DocasnySoubor");
                                }
                            });
                        }));


                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            polozkaSeznamStahovani.Status = resourceLoader.GetString("polozkaSeznamNahravani/Dokonceno");
                            if (jenomOtevritTemp)
                            {
                                polozkaSeznamStahovani.Status += " – " + resourceLoader.GetString("polozkaSeznamStahovani/DocasnySoubor");
                                await Windows.System.Launcher.LaunchFileAsync(destinationFile);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        try
                        { // Když se stahování nepovede, tak nejdřív zkusíme načíst ještě jednou přístupový token

                            if (await NacistPristupovyTokenNaPozadi())
                            {
                                // Zkusit ještě jednou operaci stahování

                                StorageFile destinationFile;

                                if (!jenomOtevritTemp)
                                { // Normálně stáhnout –> do stažených souborů

                                    destinationFile = await DownloadsFolder.CreateFileAsync(jedenSouborKeStazeni.Name, CreationCollisionOption.GenerateUniqueName);
                                }
                                else
                                { // Stáhnout do tempu (příklad stáhnu instalátor, otevřu ho a je mi jedno, jestli se smaže)

                                    destinationFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(jedenSouborKeStazeni.Name, CreationCollisionOption.GenerateUniqueName);
                                    polozkaSeznamStahovani.Status += " – " + resourceLoader.GetString("polozkaSeznamStahovani/DocasnySoubor");
                                }


                                polozkaSeznamStahovani.StorageFile = destinationFile;

                                DownloadOperation operaceStahovaniNaPozadi = await backgroundDownloader.CreateDownload(new Uri("https://graph.microsoft.com/v1.0/me/drive/items/" + jedenSouborKeStazeni.Id + "/content"), destinationFile).StartAsync().AsTask(new Progress<DownloadOperation>(async progress =>
                                {
                                    double percent = 100.0 * progress.Progress.BytesReceived / progress.Progress.TotalBytesToReceive;

                                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                    {
                                        polozkaSeznamStahovani.Progress = percent;
                                        polozkaSeznamStahovani.Status = progress.Progress.Status == BackgroundTransferStatus.Running ? resourceLoader.GetString("polozkaSeznamStahovani/Probiha") : progress.Progress.Status.ToString();
                                        if (jenomOtevritTemp)
                                        {
                                            polozkaSeznamStahovani.Status += " – " + resourceLoader.GetString("polozkaSeznamStahovani/DocasnySoubor");
                                        }
                                    });
                                }));
                            }
                            else
                            { // Teď už je to ale fakt neřešitelně v háji

                                bool zobrazitPrihlaseniAutomaticky = true;
                                MainPage.NavigovatNaStranku(typeof(StrankaNastaveni), zobrazitPrihlaseniAutomaticky);

                                throw new OperationCanceledException();
                            }
                        }
                        catch
                        {
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                polozkaSeznamStahovani.Status = resourceLoader.GetString("Chyba") + ": " + ex.Message;
                            });
                        }
                    }
                }

            }
            else
            { // Žádný soubor ke stažení nebyl zvolen
                ContentDialog dialogNebylVybranSoubor = new ContentDialog()
                {
                    Title = resourceLoader.GetString("dialogNebylVybranSoubor/Title"),
                    CloseButtonText = resourceLoader.GetString("ZavritDialog"),
                    DefaultButton = ContentDialogButton.Primary
                };

                _ = await dialogNebylVybranSoubor.ShowAsync();

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
        public ObservableCollection<DownloadItem> Uploads { get; } = new ObservableCollection<DownloadItem>();
        private DownloadManager() { }
    }





    // ✅ Singleton manager that holds all uploads
    /*public class UploadManager
    {
        private static UploadManager _instance;

        public static UploadManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UploadManager();
                return _instance;
            }
        }

        public ObservableCollection<DownloadItem> Uploads { get; } = new ObservableCollection<DownloadItem>();
        private UploadManager() { }
    }*/


}

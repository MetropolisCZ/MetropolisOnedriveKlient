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


        public static async Task<string> NacistStrankuRestApi(string UrlkZiskani)
        {
            HttpResponseMessage httpResponse = await httpClient.GetAsync(new Uri(UrlkZiskani));
            //httpResponse.EnsureSuccessStatusCode();
            
            if (httpResponse.IsSuccessStatusCode)
            {

                return await httpResponse.Content.ReadAsStringAsync();

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

                return null;
            }

        }

        public static async Task StahnoutSoubory(List<OneDriveAdresarSoubory> souboryKeStazeni)
        {
            if (souboryKeStazeni.Count > 0)
            {
                //new ToastContentBuilder().AddText("Stahování " + souboryKeStazeni.Count + " položek").Show();

                foreach (OneDriveAdresarSoubory jedenSouborKeStazeni in souboryKeStazeni)
                {

                    var polozkaSeznamStahovani = new DownloadItem
                    {
                        FileName = jedenSouborKeStazeni.Name,
                        Progress = 0,
                        Status = "Připraveno"
                    };

                    DownloadManager.Instance.Downloads.Add(polozkaSeznamStahovani);

                    try
                    {
                        StorageFile destinationFile = await DownloadsFolder.CreateFileAsync(jedenSouborKeStazeni.Name, CreationCollisionOption.GenerateUniqueName);

                        polozkaSeznamStahovani.StorageFile = destinationFile;

                        await backgroundDownloader.CreateDownload(new Uri("https://graph.microsoft.com/v1.0/me/drive/items/" + jedenSouborKeStazeni.Id + "/content"), destinationFile).StartAsync().AsTask(new Progress<DownloadOperation>(async progress =>
                        {
                            double percent = 100.0 * progress.Progress.BytesReceived / progress.Progress.TotalBytesToReceive;

                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                polozkaSeznamStahovani.Progress = percent;
                                polozkaSeznamStahovani.Status = progress.Progress.Status == BackgroundTransferStatus.Running ? "Probíhá" : progress.Progress.Status.ToString();                                
                            });
                        }));

                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            polozkaSeznamStahovani.Status = "Dokončeno";
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

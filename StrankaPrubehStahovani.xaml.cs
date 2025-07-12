using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static MetropolisOnedriveKlient.ApiWebKlient;

// Dokumentaci k šabloně Prázdná aplikace najdete na adrese https://go.microsoft.com/fwlink/?LinkId=234238

namespace MetropolisOnedriveKlient
{
    /// <summary>
    /// Prázdná stránka, která se dá použít samostatně nebo se na ni dá přejít v rámci
    /// </summary>
    public sealed partial class StrankaPrubehStahovani : Page
    {
        public DownloadManager ViewModel => DownloadManager.Instance;

        public StrankaPrubehStahovani()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MainPage.PageHeader.Text = "Průběh";

            if (e.Parameter != null)
            {
                PivotPrubehStahovani.SelectedIndex = (int)e.Parameter;
            }
        }

        private async void StazeneSoubory_ItemClick(object sender, ItemClickEventArgs e)
        {
            DownloadItem kliknutySoubor = (DownloadItem)e.ClickedItem;
            await Windows.System.Launcher.LaunchFileAsync(kliknutySoubor.StorageFile);
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

            _ = Task.Run(async () =>
            {
                for (int i = 0; i <= 100; i += 10)
                {
                    await Task.Delay(300);

                    // 👇 This ensures the UI sees the update
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            item.Progress = i;
                            item.Status = $"Downloading... {i}%";
                        });
                }

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        item.Status = "Completed!";
                    });
            });
        }*/
    }
}

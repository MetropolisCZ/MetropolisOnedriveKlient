using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
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
        private static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        public DownloadManager ViewModel => DownloadManager.Instance;

        public StrankaPrubehStahovani()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MainPage.PageHeader.Text = resourceLoader.GetString("StrankaPrubehNadpis");

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

    }
}

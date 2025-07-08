using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static MetropolisOnedriveKlient.ApiWebKlient;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Core;

// Dokumentaci k šabloně Prázdná aplikace najdete na adrese https://go.microsoft.com/fwlink/?LinkId=234238

namespace MetropolisOnedriveKlient
{
    /// <summary>
    /// Prázdná stránka, která se dá použít samostatně nebo se na ni dá přejít v rámci
    /// </summary>
    public sealed partial class StrankaSoubory : Page
    {
        private ComboBox NavigacniPanelCesty;
        private ListView ListViewSouboryaSlozky;
        private List<OneDriveAdresarSoubory> obsahSlozkyOneDrive_korenove;
        private List<OneDriveAdresarSoubory> obsahSlozkyOneDrive_aktualni;
        private ObservableCollection<string> onedriveNavigacniCesta = new ObservableCollection<string> {
            "Moje soubory"
        };

        

        public StrankaSoubory()
        {
            InitializeComponent();

            NacistObsahKorenovehoAdresare();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MainPage.PageHeader.Text = "Adresář";

        }

        private async void NacistObsahKorenovehoAdresare()
        {
           

            try
            {
                obsahSlozkyOneDrive_korenove = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root/children?$select=name,folder,createdDateTime,lastModifiedDateTime,webUrl,size,file&$expand=thumbnails")).SelectToken("value").ToObject<List<OneDriveAdresarSoubory>>();
            }
            catch
            {
                MainPage.NavigovatNaStranku(typeof(StrankaNastaveni));
                return;
            }

            

            NavigacniPanelCesty = new ComboBox
            {
                Name = "NavigacniPanelCesty",
                ItemsSource = onedriveNavigacniCesta,
                Margin = new Thickness(15, 10, 15, 15),
                SelectedIndex = 0
            };

            NavigacniPanelCesty.SelectionChanged += NavigacniPanelCesty_SelectionChanged;




            ////// LISTVIEW SOUBORY – HLAVNÍ OBSAH OKNA

            ListViewSouboryaSlozky = new ListView
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                SelectionMode = ListViewSelectionMode.None,
                IsSwipeEnabled = false, // Disabling the animation when it's not needed can improve the performance of your app.
                IsItemClickEnabled = true,
                ItemTemplate = Application.Current.Resources["SablonaSouboryRepozitarGithub"] as DataTemplate,
                Name = "ListViewSouboryaSlozky",
                ItemsSource = obsahSlozkyOneDrive_korenove,
                //Header = navigacniPanelCesty
            };

            ListViewSouboryaSlozky.ItemClick += ListViewSouboryaSlozky_ItemClick;


            







            StackPanel StackPanelHlavniObsah = new StackPanel
            {
                Padding = new Thickness(0)
            };


            // Celkové přidání
            StackPanelHlavniObsah.Children.Add(NavigacniPanelCesty);
            StackPanelHlavniObsah.Children.Add(ListViewSouboryaSlozky);

            ScrollViewer ScrollViewerHlavniObsah = new ScrollViewer
            {
                Content = StackPanelHlavniObsah
            };

            Content = ScrollViewerHlavniObsah;
        }

        private async void NavigacniPanelCesty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Debug.WriteLine(e.AddedItems[0].ToString());
            NavigacniPanelCesty.IsEnabled = false;

            int indexVybranehoUmisteniZCelkoveCesty = onedriveNavigacniCesta.IndexOf(e.AddedItems[0].ToString());
            int pocetItineraci = onedriveNavigacniCesta.Count - indexVybranehoUmisteniZCelkoveCesty - 1;
            for (int i = 0; i < pocetItineraci; i++) // Oříznout vyšší indexy (cesta, kterou potřebujeme smazat)
            {
                //NavigacniPanelCesty.SelectionChanged -= NavigacniPanelCesty_SelectionChanged;
                onedriveNavigacniCesta.RemoveAt(indexVybranehoUmisteniZCelkoveCesty + 1);
                //NavigacniPanelCesty.SelectionChanged += NavigacniPanelCesty_SelectionChanged;
            }

            if (indexVybranehoUmisteniZCelkoveCesty == 0)
            { // Kořenový adresář
                NavigacniPanelCesty.SelectionChanged -= NavigacniPanelCesty_SelectionChanged;
                onedriveNavigacniCesta.Clear();
                onedriveNavigacniCesta.Add("Moje soubory");
                NavigacniPanelCesty.SelectedIndex = 0;
                NavigacniPanelCesty.SelectionChanged += NavigacniPanelCesty_SelectionChanged;

                ListViewSouboryaSlozky.ItemsSource = obsahSlozkyOneDrive_korenove;
            }
            else
            {
                string cestaAktualni = "";
                for (int i = 1; i < onedriveNavigacniCesta.Count; i++)
                {
                    cestaAktualni += "/" + onedriveNavigacniCesta[i];
                }


                try
                {
                    obsahSlozkyOneDrive_aktualni = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root:" + cestaAktualni + ":/children?$select=name,folder,createdDateTime,lastModifiedDateTime,webUrl,size,file&$expand=thumbnails")).SelectToken("value").ToObject<List<OneDriveAdresarSoubory>>();
                }
                catch
                {
                    MainPage.NavigovatNaStranku(typeof(StrankaNastaveni));
                    return;
                }

                //obsahSlozkyOneDrive_aktualni.Sort((x, y) => x.Type.CompareTo(y.Type));
                /*obsahSlozkyOneDrive_aktualni.Insert(0, new GithubAdresarSoubory
                {
                    Name = "Kořenový adresář",
                    Type = "root"
                });*/

                ListViewSouboryaSlozky.ItemsSource = obsahSlozkyOneDrive_aktualni;
                /*NavigacniPanelCesty.SelectionChanged -= NavigacniPanelCesty_SelectionChanged;
                NavigacniPanelCesty.SelectedItem = NavigacniPanelCesty.Items[NavigacniPanelCesty.Items.Count - 1];
                NavigacniPanelCesty.SelectionChanged += NavigacniPanelCesty_SelectionChanged;*/
                
            }

            NavigacniPanelCesty.IsEnabled = true;
        }

        private async void ListViewSouboryaSlozky_ItemClick(object sender, ItemClickEventArgs e)
        {
            NavigacniPanelCesty.IsEnabled = false;

            OneDriveAdresarSoubory kliknutySoubor = (OneDriveAdresarSoubory)e.ClickedItem;

            if (kliknutySoubor.Folder == null)
            {
                ContentDialogResult contentDialogResult = await new ContentDialog()
                {
                    Content = kliknutySoubor.Name + "\nVelikost souboru: " + (kliknutySoubor.Size / 1024 / 1024) + " MB",
                    PrimaryButtonText = "Otevřít",
                    SecondaryButtonText = "Stáhnout",
                    CloseButtonText = "Zrušit"

                }.ShowAsync();

                /*if (contentDialogResult == ContentDialogResult.Primary)
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(kliknutySoubor.Download_url));
                }
                else if (contentDialogResult == ContentDialogResult.Secondary)
                {
                    StahnoutSouborzGithubu(kliknutySoubor.Download_url, kliknutySoubor.Name);
                }*/
            }
            /*else if (kliknutySoubor.Type == "root")
            {
                ListViewSouboryaSlozky.ItemsSource = contents_url_vychozi;
            }*/
            else
            { // Složka
                onedriveNavigacniCesta.Add(kliknutySoubor.Name);
                ListViewSouboryaSlozky.ItemsSource = null;

                string cestaAktualni = "";
                for (int i = 1; i < onedriveNavigacniCesta.Count; i++)
                {
                    cestaAktualni += "/" + onedriveNavigacniCesta[i];
                }

                try
                {
                    obsahSlozkyOneDrive_aktualni = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root:" + cestaAktualni + ":/children?$select=name,folder,createdDateTime,lastModifiedDateTime,webUrl,size&$expand=thumbnails")).SelectToken("value").ToObject<List<OneDriveAdresarSoubory>>();
                }
                catch
                {
                    MainPage.NavigovatNaStranku(typeof(StrankaNastaveni));
                    return;
                }

                //obsahSlozkyOneDrive_aktualni.Sort((x, y) => x.Type.CompareTo(y.Type));
                /*obsahSlozkyOneDrive_aktualni.Insert(0, new GithubAdresarSoubory
                {
                    Name = "Kořenový adresář",
                    Type = "root"
                });*/

                ListViewSouboryaSlozky.ItemsSource = obsahSlozkyOneDrive_aktualni;
                NavigacniPanelCesty.SelectionChanged -= NavigacniPanelCesty_SelectionChanged;
                NavigacniPanelCesty.SelectedItem = NavigacniPanelCesty.Items[NavigacniPanelCesty.Items.Count - 1];
                NavigacniPanelCesty.SelectionChanged += NavigacniPanelCesty_SelectionChanged;
            }

            NavigacniPanelCesty.IsEnabled = true;

        }
    }
}

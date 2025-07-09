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
using System.Threading.Tasks;

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
        private ObservableCollection<OneDriveAdresarSoubory> obsahSlozkyOneDrive_korenove;
        private ObservableCollection<OneDriveAdresarSoubory> obsahSlozkyOneDrive_aktualni;
        private string obsahSlozkyOneDrive_aktualni_adresaNext = null;
        private ObservableCollection<string> onedriveNavigacniCesta = new ObservableCollection<string> {
            "Moje soubory"
        };
        StackPanel StackPanelHlavniObsah = new StackPanel
        {
            Padding = new Thickness(0)
        };
        Button TlacitkoNacistDalsiSoubory = new Button
        {
            Content = "Načíst další",
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(15, 10, 15, 15)
        };



        public StrankaSoubory()
        {
            InitializeComponent();

            /*foreach (AppBarButton jednoTlacitko in commandBarStrankaSoubory_tlacitkaVychozi)
            {
                commandBarStrankaSoubory.PrimaryCommands.Add(jednoTlacitko);
            }*/

            //NacistTlacitkaCommandBar();

            NacistObsahKorenovehoAdresare();

        }

        private void PrepinacTlacitkaCommandBar(bool zapnoutMultiVyber = false)
        {
            BottomAppBar = null;
            if (zapnoutMultiVyber)
            {
                ListViewSouboryaSlozky.IsItemClickEnabled = false;
                ListViewSouboryaSlozky.IsMultiSelectCheckBoxEnabled = true;
                ListViewSouboryaSlozky.SelectionMode = ListViewSelectionMode.Multiple;
                BottomAppBar = (CommandBar)((DataTemplate)Resources["CommandBarTemplate_multiVyber"]).LoadContent();
            }
            else // Vypnout multivýběr
            {
                ListViewSouboryaSlozky.IsItemClickEnabled = true;
                ListViewSouboryaSlozky.IsMultiSelectCheckBoxEnabled = false;
                ListViewSouboryaSlozky.SelectionMode = ListViewSelectionMode.None;
                BottomAppBar = (CommandBar)((DataTemplate)Resources["CommandBarTemplate_vychozi"]).LoadContent();
            }

        }


        private async void OnBackRequestedZpetAdresar(object sender, BackRequestedEventArgs e)
        {
            //Debug.WriteLine("Zpět v navigaci");
            
            if (ListViewSouboryaSlozky.IsMultiSelectCheckBoxEnabled)
            { // Režim multivýběru –> nejdřív vypnout multivýběr, nepřecházet v adresáři zpět
                e.Handled = true;
                PrepinacTlacitkaCommandBar();
            }
            else
            { // Přejít v adresáří zpět
                if (onedriveNavigacniCesta.Count >= 2)
                { // Kromě kořenové složky tam jsou i další
                    e.Handled = true;
                    await NavigovatDleIndexuVhistoriiNavigace(onedriveNavigacniCesta.Count - 2);
                }
                else
                { // Jenom kořenová složka. Nechat systém pořešit si navigaci

                }
            }



        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MainPage.PageHeader.Text = "Soubory";

            SystemNavigationManager.GetForCurrentView().BackRequested -= MainPage.OnBackRequested;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequestedZpetAdresar;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequestedZpetAdresar;
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage.OnBackRequested;

        }

        private async void NacistObsahKorenovehoAdresare()
        {
           

            try
            {
                JObject obsahSlozkyOneDrive_korenove_JObject = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root/children?$select=id,name,folder,createdDateTime,lastModifiedDateTime,webUrl,size,file&$expand=thumbnails"));
                obsahSlozkyOneDrive_korenove = obsahSlozkyOneDrive_korenove_JObject.SelectToken("value").ToObject<ObservableCollection<OneDriveAdresarSoubory>>();
                obsahSlozkyOneDrive_aktualni_adresaNext = obsahSlozkyOneDrive_korenove_JObject.SelectToken("@odata.nextLink")?.ToString();

                if (obsahSlozkyOneDrive_aktualni_adresaNext != null)
                {
                    TlacitkoNacistDalsiSoubory.Visibility = Visibility.Visible;
                }
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
                IsRightTapEnabled = true
                //Header = navigacniPanelCesty
            };

            ListViewSouboryaSlozky.ItemClick += ListViewSouboryaSlozky_ItemClick;
            ListViewSouboryaSlozky.RightTapped += ListViewSouboryaSlozky_RightTapped;
            ListViewSouboryaSlozky.SelectionChanged += ListViewSouboryaSlozky_SelectionChanged;


            







            


            // Celkové přidání
            StackPanelHlavniObsah.Children.Add(NavigacniPanelCesty);
            StackPanelHlavniObsah.Children.Add(ListViewSouboryaSlozky);
            StackPanelHlavniObsah.Children.Add(TlacitkoNacistDalsiSoubory);
            TlacitkoNacistDalsiSoubory.Click += TlacitkoNacistDalsiSoubory_Click;

            ScrollViewer ScrollViewerHlavniObsah = new ScrollViewer
            {
                Content = StackPanelHlavniObsah
            };

            Content = ScrollViewerHlavniObsah;

            PrepinacTlacitkaCommandBar();
        }

        private void ListViewSouboryaSlozky_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ZdrojovyListView = (ListView)sender;
            if (ZdrojovyListView.SelectedRanges.Count == 0)
            {
                PrepinacTlacitkaCommandBar();
            }
        }

        private void ListViewSouboryaSlozky_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            MenuFlyout myFlyout = new MenuFlyout();

            var originalSource = e.OriginalSource as FrameworkElement;
            OneDriveAdresarSoubory kliknutySoubor = (OneDriveAdresarSoubory)originalSource.DataContext;
            if (kliknutySoubor?.Folder != null)
            { // Složka

            }
            else
            { // Soubor

                if (!ListViewSouboryaSlozky.IsMultiSelectCheckBoxEnabled)
                { // Normální výběr –> normální kontextová nabídka

                    MenuFlyoutItem flyoutTlacitkoSdilet = new MenuFlyoutItem { Text = "Sdílet", Icon = new SymbolIcon { Symbol = Symbol.Share }, DataContext = kliknutySoubor };
                    myFlyout.Items.Add(flyoutTlacitkoSdilet);

                    MenuFlyoutItem flyoutTlacitkoOdstranit = new MenuFlyoutItem { Text = "Odstranit", Icon = new SymbolIcon { Symbol = Symbol.Delete }, DataContext = kliknutySoubor };
                    myFlyout.Items.Add(flyoutTlacitkoOdstranit);

                    MenuFlyoutItem flyoutTlacitkoPodrobnosti = new MenuFlyoutItem { Text = "Podrobnosti", Icon = new SymbolIcon { Symbol = Symbol.List }, DataContext = kliknutySoubor };
                    myFlyout.Items.Add(flyoutTlacitkoPodrobnosti);

                    MenuFlyoutItem flyoutTlacitkoStahnout = new MenuFlyoutItem { Text = "Stáhnout", Icon = new SymbolIcon { Symbol = Symbol.Download }, DataContext = kliknutySoubor };
                    flyoutTlacitkoStahnout.Click += FlyoutTlacitkoStahnout_Click;
                    myFlyout.Items.Add(flyoutTlacitkoStahnout);

                    MenuFlyoutItem flyoutTlacitkoPresunout = new MenuFlyoutItem { Text = "Přesunout", Icon = new SymbolIcon { Symbol = Symbol.MoveToFolder }, DataContext = kliknutySoubor };
                    myFlyout.Items.Add(flyoutTlacitkoPresunout);

                    MenuFlyoutItem flyoutTlacitkoPrejmenovat = new MenuFlyoutItem { Text = "Přejmenovat", Icon = new SymbolIcon { Symbol = Symbol.Rename }, DataContext = kliknutySoubor };
                    flyoutTlacitkoPrejmenovat.Click += FlyoutTlacitkoPrejmenovat_Click;
                    myFlyout.Items.Add(flyoutTlacitkoPrejmenovat);
                }
                else
                { // Multivýběr –> speciální kontextová nabídka

                    MenuFlyoutItem flyoutTlacitkoSdilet = new MenuFlyoutItem { Text = "Sdílet", Icon = new SymbolIcon { Symbol = Symbol.Share }, DataContext = kliknutySoubor };
                    myFlyout.Items.Add(flyoutTlacitkoSdilet);

                    MenuFlyoutItem flyoutTlacitkoOdstranit = new MenuFlyoutItem { Text = "Odstranit", Icon = new SymbolIcon { Symbol = Symbol.Delete }, DataContext = kliknutySoubor };
                    myFlyout.Items.Add(flyoutTlacitkoOdstranit);

                    MenuFlyoutItem flyoutTlacitkoStahnout = new MenuFlyoutItem { Text = "Stáhnout", Icon = new SymbolIcon { Symbol = Symbol.Download }, DataContext = kliknutySoubor };
                    flyoutTlacitkoStahnout.Click += FlyoutTlacitkoStahnout_Click;
                    myFlyout.Items.Add(flyoutTlacitkoStahnout);

                    MenuFlyoutItem flyoutTlacitkoPresunout = new MenuFlyoutItem { Text = "Přesunout", Icon = new SymbolIcon { Symbol = Symbol.MoveToFolder }, DataContext = kliknutySoubor };
                    myFlyout.Items.Add(flyoutTlacitkoPresunout);
                }

                
            }

            


         

            //the code can show the flyout in your mouse click 
            myFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));

        }

        private async void FlyoutTlacitkoPrejmenovat_Click(object sender, RoutedEventArgs e)
        {
            var originalSource = e.OriginalSource as FrameworkElement;
            OneDriveAdresarSoubory kliknutySoubor = (OneDriveAdresarSoubory)originalSource.DataContext;

            StackPanel contentDialogPrejmenovat_stackPanel = new StackPanel();
            TextBox contentDialogPrejmenovat_textBox = new TextBox()
            {
                PlaceholderText = "Nový název (včetně koncovky)",
                Text = kliknutySoubor.Name
            };

            contentDialogPrejmenovat_stackPanel.Children.Add(contentDialogPrejmenovat_textBox);

            ContentDialog contentDialogPrejmenovat = new ContentDialog()
            {
                Title = "Přejmenovat soubor",
                PrimaryButtonText = "Přejmenovat",
                CloseButtonText = "Zrušit",
                Content = contentDialogPrejmenovat_stackPanel
            };

            ContentDialogResult contentDialogResult = await contentDialogPrejmenovat.ShowAsync();

            //            PATCH https://graph.microsoft.com/v1.0/me/drive/items/12345
            //Authorization: Bearer { access - token}
            //            Content - Type: application / json

            //{
            //                "name": "newname.txt"
            //}

        }

        private async void FlyoutTlacitkoStahnout_Click(object sender, RoutedEventArgs e)
        {
            var originalSource = e.OriginalSource as FrameworkElement;
            List<OneDriveAdresarSoubory> souboryKeStazeni = new List<OneDriveAdresarSoubory>();

            if (!ListViewSouboryaSlozky.IsMultiSelectCheckBoxEnabled)
            { // Normální výběr –> stáhnout vybraný soubor

                OneDriveAdresarSoubory kliknutySoubor = (OneDriveAdresarSoubory)originalSource.DataContext;
                souboryKeStazeni.Add(kliknutySoubor);
            }
            else
            { // Multivýběr –> stáhnout soubory zaškrtlé v ListView

                foreach (var kliknuteSoubory in ListViewSouboryaSlozky.SelectedItems)
                {
                    souboryKeStazeni.Add((OneDriveAdresarSoubory)kliknuteSoubory);
                }
            }
            
            await StahnoutSoubory(souboryKeStazeni);
        }

        private async void NavigacniPanelCesty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Debug.WriteLine(e.AddedItems[0].ToString());
            await NavigovatDleIndexuVhistoriiNavigace(onedriveNavigacniCesta.IndexOf(e.AddedItems[0].ToString()));
        }

        private async void ListViewSouboryaSlozky_ItemClick(object sender, ItemClickEventArgs e)
        {
            NavigacniPanelCesty.IsEnabled = false;

            OneDriveAdresarSoubory kliknutySoubor = (OneDriveAdresarSoubory)e.ClickedItem;

            if (kliknutySoubor.Folder == null)
            { // Soubor
                /*ContentDialogResult contentDialogResult = await new ContentDialog()
                {
                    Content = kliknutySoubor.Name + "\nVelikost souboru: " + (kliknutySoubor.Size / 1024 / 1024) + " MB",
                    PrimaryButtonText = "Otevřít",
                    SecondaryButtonText = "Stáhnout",
                    CloseButtonText = "Zrušit"

                }.ShowAsync();*/

                /*if (contentDialogResult == ContentDialogResult.Primary)
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(kliknutySoubor.Download_url));
                }
                else if (contentDialogResult == ContentDialogResult.Secondary)
                {
                    StahnoutSouborzGithubu(kliknutySoubor.Download_url, kliknutySoubor.Name);
                }*/
                await StahnoutSoubory(new List<OneDriveAdresarSoubory>() {
                    kliknutySoubor
                }, true);
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
                    JObject obsahSlozkyOneDrive_aktualni_JObject = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root:" + cestaAktualni + ":/children?$select=id,name,folder,createdDateTime,lastModifiedDateTime,webUrl,size&$expand=thumbnails"));
                    obsahSlozkyOneDrive_aktualni = obsahSlozkyOneDrive_aktualni_JObject.SelectToken("value").ToObject<ObservableCollection<OneDriveAdresarSoubory>>();
                    obsahSlozkyOneDrive_aktualni_adresaNext = obsahSlozkyOneDrive_aktualni_JObject.Value<string>("@odata.nextLink");

                    if (obsahSlozkyOneDrive_aktualni_adresaNext != null)
                    {
                        TlacitkoNacistDalsiSoubory.Visibility = Visibility.Visible;
                        TlacitkoNacistDalsiSoubory.IsEnabled = true;
                    }

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

        private async void TlacitkoNacistDalsiSoubory_Click(object sender, RoutedEventArgs e)
        {
            TlacitkoNacistDalsiSoubory.IsEnabled = false;

            //ListViewSouboryaSlozky.ItemsSource = null;


            try
            {
                JObject obsahSlozkyOneDrive_aktualni_JObject = JObject.Parse(await NacistStrankuRestApi(obsahSlozkyOneDrive_aktualni_adresaNext));

                TlacitkoNacistDalsiSoubory.Visibility = Visibility.Collapsed;

                List<OneDriveAdresarSoubory> obsahSlozkyOneDrive_aktualni_next_docasna = obsahSlozkyOneDrive_aktualni_JObject.SelectToken("value").ToObject<List<OneDriveAdresarSoubory>>();

                foreach(OneDriveAdresarSoubory soubor_next_docasna in obsahSlozkyOneDrive_aktualni_next_docasna)
                {
                    obsahSlozkyOneDrive_aktualni.Add(soubor_next_docasna);
                }


                obsahSlozkyOneDrive_aktualni_adresaNext = obsahSlozkyOneDrive_aktualni_JObject.Value<string>("@odata.nextLink");

                if (obsahSlozkyOneDrive_aktualni_adresaNext != null)
                {
                    TlacitkoNacistDalsiSoubory.Visibility = Visibility.Visible;
                    TlacitkoNacistDalsiSoubory.IsEnabled = true;
                }

            }
            catch
            {
                MainPage.NavigovatNaStranku(typeof(StrankaNastaveni));
                return;
            }

        }

        private async Task NavigovatDleIndexuVhistoriiNavigace(int indexHistorieNavigace)
        {
            NavigacniPanelCesty.IsEnabled = false;
            ListViewSouboryaSlozky.ItemsSource = null;

            NavigacniPanelCesty.SelectionChanged -= NavigacniPanelCesty_SelectionChanged;

            if (indexHistorieNavigace == 0)
            { // Kořenový adresář
                onedriveNavigacniCesta.Clear();
                onedriveNavigacniCesta.Add("Moje soubory");
                NavigacniPanelCesty.SelectedIndex = 0;

                ListViewSouboryaSlozky.ItemsSource = obsahSlozkyOneDrive_korenove;
            }
            else
            {

                //NavigacniPanelCesty.SelectionChanged -= NavigacniPanelCesty_SelectionChanged;
                //NavigacniPanelCesty.ItemsSource = null;


                int pocetItineraci = onedriveNavigacniCesta.Count - indexHistorieNavigace - 1;
                for (int i = 0; i < pocetItineraci; i++) // Oříznout vyšší indexy (cesta, kterou potřebujeme smazat)
                {
                    //NavigacniPanelCesty.SelectionChanged -= NavigacniPanelCesty_SelectionChanged;
                    onedriveNavigacniCesta.RemoveAt(indexHistorieNavigace + 1);
                    //NavigacniPanelCesty.SelectionChanged += NavigacniPanelCesty_SelectionChanged;
                }
                NavigacniPanelCesty.SelectedIndex = onedriveNavigacniCesta.Count - 1;


                string cestaAktualni = "";
                for (int i = 1; i < onedriveNavigacniCesta.Count; i++)
                {
                    cestaAktualni += "/" + onedriveNavigacniCesta[i];
                }


                try
                {

                    JObject obsahSlozkyOneDrive_aktualni_JObject = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root:" + cestaAktualni + ":/children?$select=id,name,folder,createdDateTime,lastModifiedDateTime,webUrl,size,file&$expand=thumbnails"));
                    obsahSlozkyOneDrive_aktualni = obsahSlozkyOneDrive_aktualni_JObject.SelectToken("value").ToObject<ObservableCollection<OneDriveAdresarSoubory>>();
                    obsahSlozkyOneDrive_aktualni_adresaNext = obsahSlozkyOneDrive_aktualni_JObject.SelectToken("@odata.nextLink")?.ToString();

                    if (obsahSlozkyOneDrive_aktualni_adresaNext != null)
                    {
                        TlacitkoNacistDalsiSoubory.Visibility = Visibility.Visible;
                    }
                }
                catch
                {
                    MainPage.NavigovatNaStranku(typeof(StrankaNastaveni));
                    return;
                }

                ListViewSouboryaSlozky.ItemsSource = obsahSlozkyOneDrive_aktualni;
                

            }

            NavigacniPanelCesty.IsEnabled = true;
            NavigacniPanelCesty.SelectionChanged += NavigacniPanelCesty_SelectionChanged;

        }

        // TLAČÍTKA COMMANDBAR VÝCHOZÍ

        private void TlacitkoNovaSlozka_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TlacitkoNahrat_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TlacitkoVyber_Click(object sender, RoutedEventArgs e)
        {
            PrepinacTlacitkaCommandBar(true);
        }

        private async void TlacitkoAktualizovat_Click(object sender, RoutedEventArgs e)
        {
            NavigacniPanelCesty.IsEnabled = false;
            ListViewSouboryaSlozky.ItemsSource = null;


            try
            {

                if (onedriveNavigacniCesta.Count == 1)
                { // Kořenový adresář

                    JObject obsahSlozkyOneDrive_korenove_JObject = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root/children?$select=id,name,folder,createdDateTime,lastModifiedDateTime,webUrl,size,file&$expand=thumbnails"));
                    obsahSlozkyOneDrive_korenove = obsahSlozkyOneDrive_korenove_JObject.SelectToken("value").ToObject<ObservableCollection<OneDriveAdresarSoubory>>();
                    obsahSlozkyOneDrive_aktualni_adresaNext = obsahSlozkyOneDrive_korenove_JObject.SelectToken("@odata.nextLink")?.ToString();

                    ListViewSouboryaSlozky.ItemsSource = obsahSlozkyOneDrive_korenove;
                }
                else
                { // Jiná složka

                    string cestaAktualni = "";
                    for (int i = 1; i < onedriveNavigacniCesta.Count; i++)
                    {
                        cestaAktualni += "/" + onedriveNavigacniCesta[i];
                    }
                    
                    JObject obsahSlozkyOneDrive_aktualni_JObject = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root:" + cestaAktualni + ":/children?$select=id,name,folder,createdDateTime,lastModifiedDateTime,webUrl,size&$expand=thumbnails"));
                    obsahSlozkyOneDrive_aktualni = obsahSlozkyOneDrive_aktualni_JObject.SelectToken("value").ToObject<ObservableCollection<OneDriveAdresarSoubory>>();
                    obsahSlozkyOneDrive_aktualni_adresaNext = obsahSlozkyOneDrive_aktualni_JObject.Value<string>("@odata.nextLink");

                    ListViewSouboryaSlozky.ItemsSource = obsahSlozkyOneDrive_aktualni;
                    
                }

                NavigacniPanelCesty.IsEnabled = true;

                if (obsahSlozkyOneDrive_aktualni_adresaNext != null)
                {
                    TlacitkoNacistDalsiSoubory.Visibility = Visibility.Visible;
                    TlacitkoNacistDalsiSoubory.IsEnabled = true;
                }

            }
            catch
            {
                MainPage.NavigovatNaStranku(typeof(StrankaNastaveni));
                return;
            }



        }

        // TLAČÍTKA COMMANDBAR MULTIVÝBĚR
        private void TlacitkoSdilet_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TlacitkoOdstranit_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

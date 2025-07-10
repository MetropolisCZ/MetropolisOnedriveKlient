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
        //private ObservableCollection<OneDriveAdresarSoubory> obsahSlozkyOneDrive_korenove;
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
        private enum MoznostiTlacitekCommandBar
        {
            Vychozi,
            MultiVyber,
            PresouvaniSouboru
        }
        MoznostiTlacitekCommandBar moznostiTlacitekCommandBar_aktualni = MoznostiTlacitekCommandBar.Vychozi;
        private List<OneDriveAdresarSoubory> souboryKpresunuti = new List<OneDriveAdresarSoubory>();



        public StrankaSoubory()
        {
            InitializeComponent();

            /*foreach (AppBarButton jednoTlacitko in commandBarStrankaSoubory_tlacitkaVychozi)
            {
                commandBarStrankaSoubory.PrimaryCommands.Add(jednoTlacitko);
            }*/

            //NacistTlacitkaCommandBar();

            NacistOvladaciPrvkyStranky();

        }

        private void PrepinacTlacitkaCommandBar(MoznostiTlacitekCommandBar moznostiTlacitekCommandBar = MoznostiTlacitekCommandBar.Vychozi)
        {
            BottomAppBar = null;
            if (moznostiTlacitekCommandBar == MoznostiTlacitekCommandBar.Vychozi)
            {
                moznostiTlacitekCommandBar_aktualni = MoznostiTlacitekCommandBar.Vychozi;
                MainPage.PageHeader.Text = "Soubory";
                ListViewSouboryaSlozky.IsItemClickEnabled = true;
                ListViewSouboryaSlozky.IsMultiSelectCheckBoxEnabled = false;
                ListViewSouboryaSlozky.SelectionMode = ListViewSelectionMode.None;
                BottomAppBar = (CommandBar)((DataTemplate)Resources["CommandBarTemplate_vychozi"]).LoadContent();
            }
            else if (moznostiTlacitekCommandBar == MoznostiTlacitekCommandBar.MultiVyber)
            {
                moznostiTlacitekCommandBar_aktualni = MoznostiTlacitekCommandBar.MultiVyber;
                ListViewSouboryaSlozky.IsItemClickEnabled = false;
                ListViewSouboryaSlozky.IsMultiSelectCheckBoxEnabled = true;
                ListViewSouboryaSlozky.SelectionMode = ListViewSelectionMode.Multiple;
                BottomAppBar = (CommandBar)((DataTemplate)Resources["CommandBarTemplate_multiVyber"]).LoadContent();
            }
            else if (moznostiTlacitekCommandBar == MoznostiTlacitekCommandBar.PresouvaniSouboru)
            {
                moznostiTlacitekCommandBar_aktualni = MoznostiTlacitekCommandBar.PresouvaniSouboru;
                MainPage.PageHeader.Text = "Přesunout";
                ListViewSouboryaSlozky.IsItemClickEnabled = true;
                ListViewSouboryaSlozky.IsMultiSelectCheckBoxEnabled = false;
                ListViewSouboryaSlozky.SelectionMode = ListViewSelectionMode.None;
                BottomAppBar = (CommandBar)((DataTemplate)Resources["CommandBarTemplate_presouvaniSouboru"]).LoadContent();
            }

        }


        private async void OnBackRequestedZpetAdresar(object sender, BackRequestedEventArgs e)
        {
            //Debug.WriteLine("Zpět v navigaci");
            
            if (moznostiTlacitekCommandBar_aktualni != MoznostiTlacitekCommandBar.Vychozi)
            { // Režim jiný než normální –> nejdřív vypnout jiný režim, nepřecházet v adresáři zpět
                e.Handled = true;
                PrepinacTlacitkaCommandBar();
            }
            else
            { // Výchozí režim –> přejít v adresáří zpět
                if (onedriveNavigacniCesta.Count >= 2)
                { // Kromě kořenové složky tam jsou i další
                    e.Handled = true;
                    //await NavigovatDleIndexuVhistoriiNavigace(onedriveNavigacniCesta.Count - 2);
                    await NavigovatAdresarAsync(false, true, onedriveNavigacniCesta.Count - 2);
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

        private async void NacistOvladaciPrvkyStranky()
        {

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
                ItemsSource = obsahSlozkyOneDrive_aktualni,
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

            await NavigovatAdresarAsync(true);

        }

        private void ListViewSouboryaSlozky_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ZdrojovyListView = (ListView)sender;
            if (moznostiTlacitekCommandBar_aktualni != MoznostiTlacitekCommandBar.PresouvaniSouboru && ZdrojovyListView.SelectedRanges.Count == 0)
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

                if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.Vychozi)
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
                    flyoutTlacitkoPresunout.Click += FlyoutTlacitkoPresunout_Click;
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
                    flyoutTlacitkoPresunout.Click += FlyoutTlacitkoPresunout_Click;
                    myFlyout.Items.Add(flyoutTlacitkoPresunout);
                }

                
            }

            


         

            //the code can show the flyout in your mouse click 
            myFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));

        }

        private void FlyoutTlacitkoPresunout_Click(object sender, RoutedEventArgs e)
        {
            
            FrameworkElement originalSource = e.OriginalSource as FrameworkElement;
            souboryKpresunuti.Clear();
            
            if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.Vychozi)
            { // Normální výběr –> přesunout vybraný soubor

                OneDriveAdresarSoubory kliknutySoubor = (OneDriveAdresarSoubory)originalSource.DataContext;
                souboryKpresunuti.Add(kliknutySoubor);
            }
            else if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.MultiVyber)
            { // Multivýběr –> přesunout soubory zaškrtlé v ListView

                foreach (var kliknuteSoubory in ListViewSouboryaSlozky.SelectedItems)
                {
                    souboryKpresunuti.Add((OneDriveAdresarSoubory)kliknuteSoubory);
                }
            }

            PrepinacTlacitkaCommandBar(MoznostiTlacitekCommandBar.PresouvaniSouboru);

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

            contentDialogPrejmenovat_textBox.SelectAll();
            ListViewSouboryaSlozky.IsEnabled = false;

            ContentDialogResult contentDialogResult = await contentDialogPrejmenovat.ShowAsync();
            

            if (contentDialogResult == ContentDialogResult.Primary)
            {
                if (contentDialogPrejmenovat_textBox.Text.Length > 0)
                {
                    try
                    {
                        
                        _ = await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/items/" + kliknutySoubor.Id, TypyHTTPrequestu.Patch, "{ \"name\": \"" + contentDialogPrejmenovat_textBox.Text + "\" }");
                        kliknutySoubor.Name = contentDialogPrejmenovat_textBox.Text;
                        //_ = await new ContentDialog()
                        //{
                        //    Title = "Přejmenováno",
                        //    CloseButtonText = "Zavřít"
                        //}.ShowAsync();

                        TlacitkoAktualizovat_Click(sender, e);
                        
                    }
                    catch
                    {
                        MainPage.NavigovatNaStranku(typeof(StrankaNastaveni));
                        return;
                    }

                }
                else
                {
                    _ = await new ContentDialog()
                    {
                        Title = "Zadejte platný název",
                        CloseButtonText = "Zavřít"
                    }.ShowAsync();
                }

                ListViewSouboryaSlozky.IsEnabled = true;

            }


        }

        private async void FlyoutTlacitkoStahnout_Click(object sender, RoutedEventArgs e)
        {
            var originalSource = e.OriginalSource as FrameworkElement;
            List<OneDriveAdresarSoubory> souboryKeStazeni = new List<OneDriveAdresarSoubory>();

            if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.Vychozi)
            { // Normální výběr –> stáhnout vybraný soubor

                OneDriveAdresarSoubory kliknutySoubor = (OneDriveAdresarSoubory)originalSource.DataContext;
                souboryKeStazeni.Add(kliknutySoubor);
            }
            else if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.MultiVyber)
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
            //await NavigovatDleIndexuVhistoriiNavigace(onedriveNavigacniCesta.IndexOf(e.AddedItems[0].ToString()));
            await NavigovatAdresarAsync(false, true, onedriveNavigacniCesta.IndexOf(e.AddedItems[0].ToString()));
        }

        private async void ListViewSouboryaSlozky_ItemClick(object sender, ItemClickEventArgs e)
        {

            OneDriveAdresarSoubory kliknutySoubor = (OneDriveAdresarSoubory)e.ClickedItem;

            if (kliknutySoubor.Folder == null)
            { // Soubor

                await StahnoutSoubory(new List<OneDriveAdresarSoubory>() {
                    kliknutySoubor
                }, true);
            }
            else
            { // Složka

                onedriveNavigacniCesta.Add(kliknutySoubor.Name);
                await NavigovatAdresarAsync();
            }

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





        private async Task NavigovatAdresarAsync(bool navigovatNaKorenovyAdresar = false, bool navigovatNaIndexHistorie = false, int indexHistorieNavigace = 0)
        {
            ListViewSouboryaSlozky.ItemsSource = null;
            NavigacniPanelCesty.IsEnabled = false;
            NavigacniPanelCesty.SelectionChanged -= NavigacniPanelCesty_SelectionChanged;
            string adresaKamNavigovat = "";

            if (navigovatNaIndexHistorie)
            { // Navigovat dle indexu v historii

                int pocetIteraci = onedriveNavigacniCesta.Count - indexHistorieNavigace - 1;
                for (int i = 0; i < pocetIteraci; i++) // Oříznout vyšší indexy (cesta, kterou potřebujeme smazat)
                {
                    //NavigacniPanelCesty.SelectionChanged -= NavigacniPanelCesty_SelectionChanged;
                    onedriveNavigacniCesta.RemoveAt(indexHistorieNavigace + 1);
                    //NavigacniPanelCesty.SelectionChanged += NavigacniPanelCesty_SelectionChanged;
                }
                //NavigacniPanelCesty.SelectedIndex = onedriveNavigacniCesta.Count - 1;
            }


            // Výpočet nové adresy
            if (navigovatNaKorenovyAdresar || onedriveNavigacniCesta.Count == 1)
            { // Kořenový adresář

                onedriveNavigacniCesta.Clear();
                onedriveNavigacniCesta.Add("Moje soubory");
                NavigacniPanelCesty.SelectedIndex = 0;

                //adresaKamNavigovat = "";
            }
            else
            { // Adresář dle pole onedriveNavigacniCesta

                string cestaAktualni = "";
                for (int i = 1; i < onedriveNavigacniCesta.Count; i++)
                {
                    cestaAktualni += "/" + onedriveNavigacniCesta[i];
                }

                adresaKamNavigovat = ":" + cestaAktualni + ":";


            }

            adresaKamNavigovat += "/children?$select=id,name,folder,createdDateTime,lastModifiedDateTime,webUrl,size&$expand=thumbnails";

            // Provést navigaci
            try
            {
                JObject obsahSlozkyOneDrive_aktualni_JObject = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root" + adresaKamNavigovat));
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

            
            ListViewSouboryaSlozky.ItemsSource = obsahSlozkyOneDrive_aktualni;
            NavigacniPanelCesty.SelectedItem = NavigacniPanelCesty.Items[NavigacniPanelCesty.Items.Count - 1];
            NavigacniPanelCesty.SelectionChanged += NavigacniPanelCesty_SelectionChanged;
            NavigacniPanelCesty.IsEnabled = true;

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
            PrepinacTlacitkaCommandBar(MoznostiTlacitekCommandBar.MultiVyber);
        }

        private async void TlacitkoAktualizovat_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                if (onedriveNavigacniCesta.Count == 1)
                { // Kořenový adresář

                    await NavigovatAdresarAsync(true);
                }
                else
                { // Jiná složka

                    await NavigovatAdresarAsync();
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




        // TLAČÍTKA COMMANDBAR PŘESUN SOUBORŮ
        private async void TlacitkoPresunoutSem_Click(object sender, RoutedEventArgs e)
        {
            ListViewSouboryaSlozky.IsEnabled = false;
            BottomAppBar.IsEnabled = false;

            string cestaAktualni = "";
            if (onedriveNavigacniCesta.Count != 1)
            { // Normální nekořenový adresář –> vytvořit cestu
                cestaAktualni += ":";
                for (int i = 1; i < onedriveNavigacniCesta.Count; i++)
                {
                    cestaAktualni += "/" + onedriveNavigacniCesta[i];
                }
                cestaAktualni += ":";
            }
            else
            { // Kořenový adresář

            }


            try
            {
                string idSlozkyKamPresunout = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root" + cestaAktualni + "?$select=id")).SelectToken("id").ToString();

                foreach (OneDriveAdresarSoubory jedenSouborKpresunuti in souboryKpresunuti)
                {
                    await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/items/" + jedenSouborKpresunuti.Id, TypyHTTPrequestu.Patch, "{ 'parentReference': { 'id': '" + idSlozkyKamPresunout + "' } }");
                }

                TlacitkoAktualizovat_Click(sender, e);
                PrepinacTlacitkaCommandBar();
            }
            catch
            {
                MainPage.NavigovatNaStranku(typeof(StrankaNastaveni));
                return;
            }

            ListViewSouboryaSlozky.IsEnabled = true;
            BottomAppBar.IsEnabled = true;
        }

        private void FlyoutTlacitkoZrusitPresun_Click(object sender, RoutedEventArgs e)
        {
            PrepinacTlacitkaCommandBar();
        }
    }
}

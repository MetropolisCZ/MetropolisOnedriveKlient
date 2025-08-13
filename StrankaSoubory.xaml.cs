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
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.ApplicationModel.Resources;
using Newtonsoft.Json;
using Windows.ApplicationModel.DataTransfer;
using System.Globalization;
using Windows.UI.Xaml.Media;
using Windows.UI;

// Dokumentaci k šabloně Prázdná aplikace najdete na adrese https://go.microsoft.com/fwlink/?LinkId=234238

namespace MetropolisOnedriveKlient
{
    /// <summary>
    /// Prázdná stránka, která se dá použít samostatně nebo se na ni dá přejít v rámci
    /// </summary>
    public sealed partial class StrankaSoubory : Page
    {
        private static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        private ComboBox NavigacniPanelCesty;
        private ListView ListViewSouboryaSlozky;
        public ComboBox ComboBoxRazeniPolozek;
        //private ObservableCollection<OneDriveAdresarSoubory> obsahSlozkyOneDrive_korenove;
        private ObservableCollection<OneDriveAdresarSoubory> obsahSlozkyOneDrive_aktualni;
        private string obsahSlozkyOneDrive_aktualni_adresaNext = null;
        private ObservableCollection<string> onedriveNavigacniCesta = new ObservableCollection<string> {
            resourceLoader.GetString("KorenovyAdresarNazev")
        };
        //private OneDriveOpravneniSouboru oneDriveOpravneniSouboru = new OneDriveOpravneniSouboru();
        StackPanel StackPanelHlavniObsah = new StackPanel
        {
            Padding = new Thickness(0)
        };
        Button TlacitkoNacistDalsiSoubory = new Button
        {
            Content = resourceLoader.GetString("TlacitkoNacistDalsi/Content"),
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
        private readonly bool NastaveniAktualizovatSlozkuPriZmeneSdileni = (bool)ApplicationData.Current.LocalSettings.Values["AktualizovatSlozkuPriZmeneSdileni"];
        private int NastaveniPodleCehoRaditSouboryVeSlozce = (int)ApplicationData.Current.LocalSettings.Values["PodleCehoRaditSouboryVeSlozce"];
        private readonly bool NastaveniUkladatRazeniSouboru = (bool)ApplicationData.Current.LocalSettings.Values["UkladatRazeniSouboru"];
        private string hledanyVyrazParametr;
        private bool strankaUzJeInicializovana = false;
        StackPanel StackPanelRazeniPolozek = new StackPanel();


        public StrankaSoubory()
        {
            InitializeComponent();

            NacistOvladaciPrvkyStranky();
        }

        private void PrepinacTlacitkaCommandBar(MoznostiTlacitekCommandBar moznostiTlacitekCommandBar = MoznostiTlacitekCommandBar.Vychozi)
        {
            BottomAppBar = null;
            if (moznostiTlacitekCommandBar == MoznostiTlacitekCommandBar.Vychozi)
            {
                moznostiTlacitekCommandBar_aktualni = MoznostiTlacitekCommandBar.Vychozi;
                MainPage.PageHeader.Text = resourceLoader.GetString("StrankaSouboryNadpis");
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
                MainPage.PageHeader.Text = resourceLoader.GetString("PresunoutNadpis");
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MainPage.PageHeader.Text = resourceLoader.GetString("StrankaSouboryNadpis");

            SystemNavigationManager.GetForCurrentView().BackRequested -= MainPage.OnBackRequested;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequestedZpetAdresar;

            if (e?.Parameter != null) // Parametr – vyhledávaný výraz
            {
                hledanyVyrazParametr = (string)e.Parameter;

                StackPanelRazeniPolozek.Visibility = Visibility.Collapsed;
            }

            if (!strankaUzJeInicializovana)
            { // Navigovat jenom při prvním načtení stránky – další načtení budou z navigace zpět (třeba ze stránky průběh)

                await NavigovatAdresarAsync(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequestedZpetAdresar;
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage.OnBackRequested;

        }

        private void NacistOvladaciPrvkyStranky()
        {

            NavigacniPanelCesty = new ComboBox
            {
                Name = "NavigacniPanelCesty",
                ItemsSource = onedriveNavigacniCesta,
                Margin = new Thickness(15, 10, 15, 5),
                SelectedIndex = 0
            };

            NavigacniPanelCesty.SelectionChanged += NavigacniPanelCesty_SelectionChanged;





            /// VÝBĚR SEŘADIT PODLE:
            
            StackPanelRazeniPolozek = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 10, 0)
            };

            TextBlock RazeniPolozekTextBlock = new TextBlock
            {
                Text = resourceLoader.GetString("RazeniPolozekTextBlock/Text") + ":",
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.DarkGray),
                Margin = new Thickness(0, 0, 0, 2),
                //Padding = new Thickness(0)
            };


            // Vlastní stylování IsEnabled = false – vlastně přepíšu tu barvu pozadí, a to jenom pro tenhle element
            ResourceDictionary ComboBoxRazeniPolozekResourceDictionary = new ResourceDictionary();
            ComboBoxRazeniPolozekResourceDictionary.ThemeDictionaries["Default"] = new ResourceDictionary
            {
                ["ComboBoxBackgroundDisabled"] = new SolidColorBrush(Colors.Transparent)
            };

            ComboBoxRazeniPolozek = new ComboBox
            {
                BorderThickness = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                Resources = ComboBoxRazeniPolozekResourceDictionary
            };




            ComboBoxRazeniPolozek.Items.Add(new ComboBoxItem { Content = resourceLoader.GetString("Vychozi") });
            ComboBoxRazeniPolozek.Items.Add(new ComboBoxItem { Content = resourceLoader.GetString("ComboBoxRazeniPolozek/NazevAZ") });
            ComboBoxRazeniPolozek.Items.Add(new ComboBoxItem { Content = resourceLoader.GetString("ComboBoxRazeniPolozek/NazevZA") });
            /*ComboBoxRazeniPolozek.Items.Add(new ComboBoxItem { Content = resourceLoader.GetString("ComboBoxRazeniPolozek/VelikostOdNejmensiho") });
            ComboBoxRazeniPolozek.Items.Add(new ComboBoxItem { Content = resourceLoader.GetString("ComboBoxRazeniPolozek/VelikostOdNejvetsiho") });*/
            ComboBoxRazeniPolozek.Items.Add(new ComboBoxItem { Content = resourceLoader.GetString("ComboBoxRazeniPolozek/DatumOdNejstarsiho") });
            ComboBoxRazeniPolozek.Items.Add(new ComboBoxItem { Content = resourceLoader.GetString("ComboBoxRazeniPolozek/DatumOdNejnovejsiho") });

            ComboBoxRazeniPolozek.SelectedIndex = NastaveniPodleCehoRaditSouboryVeSlozce;


            ComboBoxRazeniPolozek.SelectionChanged += ComboBoxRazeniPolozek_SelectionChanged;


            StackPanelRazeniPolozek.Children.Add(RazeniPolozekTextBlock);
            StackPanelRazeniPolozek.Children.Add(ComboBoxRazeniPolozek);





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
                IsRightTapEnabled = true,
                Margin = new Thickness(0, 8, 0, 0)
                //Header = navigacniPanelCesty
            };

            ListViewSouboryaSlozky.ItemClick += ListViewSouboryaSlozky_ItemClick;
            ListViewSouboryaSlozky.RightTapped += ListViewSouboryaSlozky_RightTapped;
            ListViewSouboryaSlozky.SelectionChanged += ListViewSouboryaSlozky_SelectionChanged;


            







            


            // Celkové přidání
            StackPanelHlavniObsah.Children.Add(NavigacniPanelCesty);
            StackPanelHlavniObsah.Children.Add(StackPanelRazeniPolozek);
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

        private void ComboBoxRazeniPolozek_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox ZdrojovyComboBox = (ComboBox)sender;

            NastaveniPodleCehoRaditSouboryVeSlozce = ZdrojovyComboBox.SelectedIndex;

            if (NastaveniUkladatRazeniSouboru)
            {
                ApplicationData.Current.LocalSettings.Values["PodleCehoRaditSouboryVeSlozce"] = ZdrojovyComboBox.SelectedIndex;
            }

            TlacitkoAktualizovat_Click(sender, e);
        }


        private void ListViewSouboryaSlozky_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView ZdrojovyListView = (ListView)sender;
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


            // Definice položek flyoutu
            MenuFlyoutItem flyoutTlacitkoOdstranit = new MenuFlyoutItem { Text = resourceLoader.GetString("flyoutTlacitkoOdstranit/Text"), Icon = new SymbolIcon { Symbol = Symbol.Delete }, DataContext = kliknutySoubor };
            flyoutTlacitkoOdstranit.Click += TlacitkoOdstranit_Click;

            MenuFlyoutItem flyoutTlacitkoPresunout = new MenuFlyoutItem { Text = resourceLoader.GetString("flyoutTlacitkoPresunout/Text"), Icon = new SymbolIcon { Symbol = Symbol.MoveToFolder }, DataContext = kliknutySoubor };
            flyoutTlacitkoPresunout.Click += FlyoutTlacitkoPresunout_Click;

            MenuFlyoutItem flyoutTlacitkoSdilet = new MenuFlyoutItem { Text = resourceLoader.GetString("flyoutTlacitkoSdilet/Text"), Icon = new SymbolIcon { Symbol = Symbol.Share }, DataContext = kliknutySoubor };
            flyoutTlacitkoSdilet.Click += FlyoutTlacitkoSdilet_Click;

            MenuFlyoutItem flyoutTlacitkoStahnout = new MenuFlyoutItem { Text = resourceLoader.GetString("flyoutTlacitkoStahnout/Text"), Icon = new SymbolIcon { Symbol = Symbol.Download }, DataContext = kliknutySoubor };
            flyoutTlacitkoStahnout.Click += FlyoutTlacitkoStahnout_Click;

            MenuFlyoutItem flyoutTlacitkoPrejmenovat = new MenuFlyoutItem { Text = resourceLoader.GetString("flyoutTlacitkoPrejmenovat/Text"), Icon = new SymbolIcon { Symbol = Symbol.Rename }, DataContext = kliknutySoubor };
            flyoutTlacitkoPrejmenovat.Click += FlyoutTlacitkoPrejmenovat_Click;

            //MenuFlyoutItem flyoutTlacitkoPodrobnosti = new MenuFlyoutItem { Text = "Podrobnosti", Icon = new SymbolIcon { Symbol = Symbol.List }, DataContext = kliknutySoubor };




            // Přiřazení dle druhu výběru
            if (kliknutySoubor?.Folder != null)
            { // Složka



                if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.Vychozi)
                {
                    myFlyout.Items.Add(flyoutTlacitkoSdilet);
                    myFlyout.Items.Add(flyoutTlacitkoOdstranit);
                    myFlyout.Items.Add(flyoutTlacitkoPresunout);
                    myFlyout.Items.Add(flyoutTlacitkoPrejmenovat);
                }
                else if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.MultiVyber)
                {
                    //myFlyout.Items.Add(flyoutTlacitkoSdilet);
                    myFlyout.Items.Add(flyoutTlacitkoOdstranit);
                    myFlyout.Items.Add(flyoutTlacitkoPresunout);
                }
                else if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.PresouvaniSouboru)
                {
                    myFlyout.Items.Add(flyoutTlacitkoOdstranit);
                }

            }
            else
            { // Soubor

                if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.Vychozi)
                { // Normální výběr –> normální kontextová nabídka

                    
                    myFlyout.Items.Add(flyoutTlacitkoSdilet);
                    myFlyout.Items.Add(flyoutTlacitkoOdstranit);
                    myFlyout.Items.Add(flyoutTlacitkoStahnout);
                    myFlyout.Items.Add(flyoutTlacitkoPresunout);
                    myFlyout.Items.Add(flyoutTlacitkoPrejmenovat);
                }
                else if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.MultiVyber)
                { // Multivýběr –> speciální kontextová nabídka

                    //myFlyout.Items.Add(flyoutTlacitkoSdilet);
                    myFlyout.Items.Add(flyoutTlacitkoOdstranit);
                    myFlyout.Items.Add(flyoutTlacitkoStahnout);
                    myFlyout.Items.Add(flyoutTlacitkoPresunout);
                }
                else if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.PresouvaniSouboru)
                { // Přesouvání souboru –> speciální kontextová nabídka

                    myFlyout.Items.Add(flyoutTlacitkoOdstranit);
                    myFlyout.Items.Add(flyoutTlacitkoPrejmenovat);
                }

                
            }

            


         

            //the code can show the flyout in your mouse click 
            myFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));

        }

        public Button VytvoritTlacitkoSikonou(string textTlacitka, Symbol? symbol, bool symbolIkona = true, string glyphFontIkony = null)
        {
            Button TlacitkoSikonou = new Button();

            StackPanel stackPanelVnitrni = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (symbolIkona)
            {
                SymbolIcon ikona = new SymbolIcon
                {
                    Symbol = (Symbol)symbol,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                stackPanelVnitrni.Children.Add(ikona);
            }
            else if (!string.IsNullOrEmpty(glyphFontIkony))
            {
                FontIcon ikonaGlyph = new FontIcon
                {
                    Glyph = glyphFontIkony,
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Margin = new Thickness(0, 0, 8, 0)
                };
                stackPanelVnitrni.Children.Add(ikonaGlyph);
            }

            TextBlock textBlock = new TextBlock
            {
                Text = textTlacitka,
                VerticalAlignment = VerticalAlignment.Center
            };

            stackPanelVnitrni.Children.Add(textBlock);

            TlacitkoSikonou.Content = stackPanelVnitrni;
            return TlacitkoSikonou;
        }

        private void ZapnoutVypnoutUzivatelskeRozhrani(bool stavAktivovani = true)
        {
            ListViewSouboryaSlozky.IsEnabled = stavAktivovani;
            NavigacniPanelCesty.IsEnabled = stavAktivovani;
            ComboBoxRazeniPolozek.IsEnabled = stavAktivovani;
            BottomAppBar.IsEnabled = stavAktivovani;
        }

        private async void FlyoutTlacitkoSdilet_Click(object sender, RoutedEventArgs e)
        {
            ZapnoutVypnoutUzivatelskeRozhrani(false);

            OneDriveOpravneniSouboru oneDriveOpravneniSouboru = new OneDriveOpravneniSouboru();

            var originalSource = e.OriginalSource as FrameworkElement;
            OneDriveAdresarSoubory kliknutySoubor = (OneDriveAdresarSoubory)originalSource.DataContext;

            try
            {
                oneDriveOpravneniSouboru = JsonConvert.DeserializeObject<OneDriveOpravneniSouboru>(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/items/" + kliknutySoubor.Id + "/permissions"));
                var juhuihuh = oneDriveOpravneniSouboru;
            }
            catch
            {
                return;
            }

            ContentDialog contentDialogSdileni = new ContentDialog()
            {
                Title = resourceLoader.GetString("contentDialogSdileni/Nadpis"),
                CloseButtonText = resourceLoader.GetString("ZavritDialog"),
                DefaultButton = ContentDialogButton.Primary
            };

            StackPanel contentDialogSdileni_stackPanel = new StackPanel();


            if (oneDriveOpravneniSouboru.Value.Length == 1)
            { // Je tam jenom oprávnění pro přihlášeného uživatele

                contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                {
                    Text = resourceLoader.GetString("contentDialogSdileni/SouborNeniSdilen"),
                    TextWrapping = TextWrapping.Wrap
                });
            }
            else
            {

                ListView ListViewSdileniSouboru = new ListView
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    SelectionMode = ListViewSelectionMode.None,
                    IsSwipeEnabled = false, // Disabling the animation when it's not needed can improve the performance of your app.
                    IsItemClickEnabled = true,
                    ItemTemplate = Application.Current.Resources["SablonaOpravneniSdileni"] as DataTemplate,
                    //Name = "ListViewSouboryaSlozky",
                    ItemsSource = oneDriveOpravneniSouboru.Value
                };

                ListViewSdileniSouboru.ItemClick += (_s, _e) =>
                {
                    ValueOpravneni vybranaMoznostSdileni = (ValueOpravneni)_e.ClickedItem;
                    // Generate your sharing link or do what you need here
                    //contentDialogSdileni.Hide();
                    contentDialogSdileni_stackPanel.Children.Clear();

                    contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                    {
                        Text = vybranaMoznostSdileni.TypSdileniNadpis,
                        TextWrapping = TextWrapping.Wrap,
                        FontWeight = Windows.UI.Text.FontWeights.Bold
                    });

                    if (vybranaMoznostSdileni?.Link?.WebUrl != null)
                    { // Odkaz

                        KonvertorOpravneniSouboruRoles konvertorOpravneniSouboruRoles = new KonvertorOpravneniSouboruRoles();

                        contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                        {
                            Text = /*resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/OpravneniOdkazu") + ": " + */(string)konvertorOpravneniSouboruRoles.Convert(vybranaMoznostSdileni.Roles, typeof(string), null, "cs-CZ"),
                            TextWrapping = TextWrapping.Wrap
                        });

                        

                        if (vybranaMoznostSdileni.Roles[0] != "owner")
                        { // Když oprávnění není typu vlastník (což jsem stejně nikdy moc nepochopil, co to jako je), tak zobrazíme tlačítka pro sdílení

                            Button tlacitkoKopirovatOdkaz = VytvoritTlacitkoSikonou(resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/KopirovatOdkaz"), Symbol.Copy);
                            tlacitkoKopirovatOdkaz.Margin = new Thickness(0, 10, 0, 10);

                            tlacitkoKopirovatOdkaz.Click += (__s, __e) =>
                            {
                                DataPackage dataPackage = new DataPackage
                                {
                                    RequestedOperation = DataPackageOperation.Copy
                                };
                                dataPackage.SetText(vybranaMoznostSdileni.Link.WebUrl.ToString());
                                Clipboard.SetContent(dataPackage);


                                contentDialogSdileni_stackPanel.Children.Clear();

                                contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                                {
                                    Text = vybranaMoznostSdileni.TypSdileniNadpis,
                                    TextWrapping = TextWrapping.Wrap,
                                    FontWeight = Windows.UI.Text.FontWeights.Bold
                                });

                                contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                                {
                                    Text = resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/OdkazZkopirovan")
                                });
                            };

                            Button tlacitkoSdiletOdkaz = VytvoritTlacitkoSikonou(resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/SdiletOdkaz"), Symbol.Share); // Ikona sdílení NENÍ dostupná na W10M! Zrada!

                            /*Button tlacitkoSdiletOdkaz = new Button
                            {
                                Content = resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/SdiletOdkaz")
                                //Margin = new Thickness(0, 0, 0, 10)
                            };*/
                            tlacitkoSdiletOdkaz.Click += (__s, __e) =>
                            {
                                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                                dataTransferManager.DataRequested += (___s, ___e) =>
                                {
                                    DataRequest request = ___e.Request;
                                    request.Data.Properties.Title = kliknutySoubor.Name;
                                    //request.Data.Properties.Description = "Popis";
                                    request.Data.SetWebLink(vybranaMoznostSdileni.Link.WebUrl);
                                };
                                DataTransferManager.ShowShareUI();


                                contentDialogSdileni.Hide();
                            };

                            contentDialogSdileni_stackPanel.Children.Add(tlacitkoKopirovatOdkaz);

                            contentDialogSdileni_stackPanel.Children.Add(tlacitkoSdiletOdkaz);
                        }
                        
                    }

                    if (vybranaMoznostSdileni.InheritedFrom != null)
                    { // Zděděno, není možné upravovat

                        contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                        {
                            //Text = string.Format(resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/ZdedenoNejdeOdstranit"), vybranaMoznostSdileni.InheritedFrom.Path),
                            Text = resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/ZdedenoNejdeOdstranit"),
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 10, 0, 0)
                        });
                    }
                    else if (vybranaMoznostSdileni.Roles[0] != "owner")
                    {
                        Button tlacitkoOdstranitOdkaz = VytvoritTlacitkoSikonou(resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/OdstranitOdkaz"), Symbol.Delete);

                        tlacitkoOdstranitOdkaz.Margin = new Thickness(0, 25, 0, 0);
                        tlacitkoOdstranitOdkaz.FontStyle = Windows.UI.Text.FontStyle.Italic;


                        tlacitkoOdstranitOdkaz.Click += async (__s, __e) =>
                        { // Odstranit odkaz ke sdílení
                            tlacitkoOdstranitOdkaz.IsEnabled = false;

                            try
                            {
                                await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/items/" + kliknutySoubor.Id + "/permissions/" + vybranaMoznostSdileni.Id, TypyHTTPrequestu.Delete);
                            }
                            catch
                            {
                                contentDialogSdileni.Hide();
                                return;
                            }

                            contentDialogSdileni_stackPanel.Children.Clear();

                            contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                            {
                                Text = vybranaMoznostSdileni.TypSdileniNadpis,
                                TextWrapping = TextWrapping.Wrap,
                                FontWeight = Windows.UI.Text.FontWeights.Bold
                            });

                            contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                            {
                                Text = resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/OdkazOdstranen")
                            });

                            if (NastaveniAktualizovatSlozkuPriZmeneSdileni)
                            {
                                TlacitkoAktualizovat_Click(sender, e);
                            }
                        };

                        contentDialogSdileni_stackPanel.Children.Add(tlacitkoOdstranitOdkaz);
                    }
                };

                contentDialogSdileni_stackPanel.Children.Add(ListViewSdileniSouboru);
            }



            Button TlacitkoVytvoritOdkazKeSdileni = new Button
            {
                Content = resourceLoader.GetString("contentDialogSdileni/TlacitkoVytvoritOdkazKeSdileni"),
                Margin = new Thickness(0, 10, 0, 0)
            };
            TlacitkoVytvoritOdkazKeSdileni.Click += (_s, _e) =>
            {
                contentDialogSdileni_stackPanel.Children.Clear();

                contentDialogSdileni.Title = resourceLoader.GetString("contentDialogSdileni/TlacitkoVytvoritOdkazKeSdileni");
                contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                {
                    Text = kliknutySoubor.Name,
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = Windows.UI.Text.FontWeights.Bold
                });

                //contentDialogSdileni.Title = kliknutySoubor.Name;

                contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                {
                    Text = resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/VyberteOpravneniOdkazu"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 18, 0, 5)
                });
                RadioButton radioButtonVyberDruhuOpravneni1 = new RadioButton
                {
                    Content = resourceLoader.GetString("contentDialogSdileni/OpravneniCteni"),
                    IsChecked = true,
                    Tag = "view",
                    GroupName = "RadioTlacitkaOpravneniSdileni"
                };
                contentDialogSdileni_stackPanel.Children.Add(radioButtonVyberDruhuOpravneni1);

                RadioButton radioButtonVyberDruhuOpravneni2 = new RadioButton
                {
                    Content = resourceLoader.GetString("contentDialogSdileni/OpravneniUpravy"),
                    Tag = "edit",
                    GroupName = "RadioTlacitkaOpravneniSdileni"
                };
                contentDialogSdileni_stackPanel.Children.Add(radioButtonVyberDruhuOpravneni2);


                Button tlacitkoPotvrditVytvoreniOdkazu = new Button
                {
                    Content = resourceLoader.GetString("contentDialogSdileni/TlacitkoVytvoritOdkazKeSdileni"),
                    Margin = new Thickness(0, 25, 0, 0)
                };
                tlacitkoPotvrditVytvoreniOdkazu.Click += async (__s, __e) =>
                { // Potvrdit vytvoření odkazu ke sdílení se zvolenou možností úprav

                    tlacitkoPotvrditVytvoreniOdkazu.IsEnabled = false;

                    string vybraneOpravneniNovehoOdkazu = radioButtonVyberDruhuOpravneni1.IsChecked == true
                        ? radioButtonVyberDruhuOpravneni1.Tag as string
                        : radioButtonVyberDruhuOpravneni2.Tag as string;

                    string novyOdkazKeSdileniUrl;

                    try
                    {
                        novyOdkazKeSdileniUrl = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/items/" + kliknutySoubor.Id + "/createLink", TypyHTTPrequestu.Post, "{ \"type\": \"" + vybraneOpravneniNovehoOdkazu + "\" }")).SelectToken("link").SelectToken("webUrl").ToString();

                    }
                    catch
                    {
                        contentDialogSdileni.Hide();
                        return;
                    }

                    contentDialogSdileni.Title = resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/OdkazBylVytvoren");
                    contentDialogSdileni_stackPanel.Children.Clear();

                    contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                    {
                        Text = kliknutySoubor.Name,
                        TextWrapping = TextWrapping.Wrap,
                        FontWeight = Windows.UI.Text.FontWeights.Bold
                    });
                    contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                    {
                        Text = radioButtonVyberDruhuOpravneni1.IsChecked == true
                        ? resourceLoader.GetString("contentDialogSdileni/OpravneniCteni")
                        : resourceLoader.GetString("contentDialogSdileni/OpravneniUpravy"),
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 10, 0, 0)
                    });


                    Button tlacitkoKopirovatOdkaz = VytvoritTlacitkoSikonou(resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/KopirovatOdkaz"), Symbol.Copy);
                    tlacitkoKopirovatOdkaz.Margin = new Thickness(0, 10, 0, 10);

                    tlacitkoKopirovatOdkaz.Click += (___s, ___e) =>
                    {
                        DataPackage dataPackage = new DataPackage
                        {
                            RequestedOperation = DataPackageOperation.Copy
                        };
                        dataPackage.SetText(novyOdkazKeSdileniUrl);
                        Clipboard.SetContent(dataPackage);


                        contentDialogSdileni_stackPanel.Children.Clear();

                        contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                        {
                            Text = radioButtonVyberDruhuOpravneni1.IsChecked == true
                        ? resourceLoader.GetString("contentDialogSdileni/OpravneniCteni")
                        : resourceLoader.GetString("contentDialogSdileni/OpravneniUpravy"),
                            TextWrapping = TextWrapping.Wrap,
                            FontWeight = Windows.UI.Text.FontWeights.Bold
                        });

                        contentDialogSdileni_stackPanel.Children.Add(new TextBlock
                        {
                            Text = resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/OdkazZkopirovan")
                        });
                    };

                    Button tlacitkoSdiletOdkaz = VytvoritTlacitkoSikonou(resourceLoader.GetString("contentDialogVytvoritOdkazKeSdileni/SdiletOdkaz"), Symbol.Share);

                    tlacitkoSdiletOdkaz.Click += (___s, ___e) =>
                    {
                        DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                        dataTransferManager.DataRequested += (____s, ____e) =>
                        {
                            DataRequest request = ____e.Request;
                            request.Data.Properties.Title = kliknutySoubor.Name;
                            //request.Data.Properties.Description = "Popis";
                            request.Data.SetWebLink(new Uri(novyOdkazKeSdileniUrl));
                        };
                        DataTransferManager.ShowShareUI();


                        contentDialogSdileni.Hide();
                    };

                    contentDialogSdileni_stackPanel.Children.Add(tlacitkoKopirovatOdkaz);

                    contentDialogSdileni_stackPanel.Children.Add(tlacitkoSdiletOdkaz);

                    if (NastaveniAktualizovatSlozkuPriZmeneSdileni)
                    {
                        TlacitkoAktualizovat_Click(sender, e);
                    }
                };

                contentDialogSdileni_stackPanel.Children.Add(tlacitkoPotvrditVytvoreniOdkazu);

                //contentDialogSdileni.Hide();
            };


            contentDialogSdileni.Content = new ScrollViewer() { Content = contentDialogSdileni_stackPanel };
            contentDialogSdileni_stackPanel.Children.Add(TlacitkoVytvoritOdkazKeSdileni);


            _ = await contentDialogSdileni.ShowAsync();

            ZapnoutVypnoutUzivatelskeRozhrani();
        }


        private void FlyoutTlacitkoOdstranit_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
                PlaceholderText = resourceLoader.GetString("contentDialogPrejmenovat_textBox/PlaceholderText"),
                Text = kliknutySoubor.Name
            };

            contentDialogPrejmenovat_stackPanel.Children.Add(contentDialogPrejmenovat_textBox);

            ContentDialog contentDialogPrejmenovat = new ContentDialog()
            {
                Title = resourceLoader.GetString("contentDialogPrejmenovat/Title"),
                PrimaryButtonText = resourceLoader.GetString("contentDialogPrejmenovat/PrimaryButtonText"),
                CloseButtonText = resourceLoader.GetString("ZrusitDialog"),
                Content = contentDialogPrejmenovat_stackPanel,
                DefaultButton = ContentDialogButton.Primary
            };

            contentDialogPrejmenovat_textBox.SelectAll();
            ZapnoutVypnoutUzivatelskeRozhrani(false);

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
                        return;
                    }

                }
                else
                {
                    _ = await new ContentDialog()
                    {
                        Title = resourceLoader.GetString("contentDialogPrejmenovat/ZadejtePlatnyNazev"),
                        CloseButtonText = resourceLoader.GetString("ZavritDialog")
                    }.ShowAsync();
                }

            }

            ZapnoutVypnoutUzivatelskeRozhrani();

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

                if (hledanyVyrazParametr != null)
                { // Když se ve vyhledávání klikne na složku, tak prostě přejdu na tu složku a přepíšu historii jako kdybych se tam proklikal :) –> takže vymazat obsah ComboBoxu navigační cesty a vytvořit nový

                    hledanyVyrazParametr = null;

                    NavigacniPanelCesty.SelectionChanged -= NavigacniPanelCesty_SelectionChanged;
                    onedriveNavigacniCesta.Clear();
                    onedriveNavigacniCesta.Add(resourceLoader.GetString("KorenovyAdresarNazev"));

                    string[] cestaKliknutehoSouboruRozdelena = kliknutySoubor.ParentReference.Path.Split('/');
                    for (int i = 3; i < cestaKliknutehoSouboruRozdelena.Length; i++) // 0 je prázdný, 1 je drive, 2 je root:, teprve 3 je hodnota
                    {
                        onedriveNavigacniCesta.Add(cestaKliknutehoSouboruRozdelena[i]);
                    }

                }

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
                return;
            }

        }




        private string AktualniCestaDoStringu(bool bezDvojteckyNaKonci = false)
        {
            string cestaAktualni = "";
            if (onedriveNavigacniCesta.Count != 1)
            { // Normální nekořenový adresář –> vytvořit cestu
                cestaAktualni += ":";
                for (int i = 1; i < onedriveNavigacniCesta.Count; i++)
                {
                    cestaAktualni += "/" + onedriveNavigacniCesta[i];
                }

                return !bezDvojteckyNaKonci ? (cestaAktualni += ":") : cestaAktualni;
            }
            else
            { // Kořenový adresář
                return "";
            }
        }


        private async Task NavigovatAdresarAsync(bool navigovatNaKorenovyAdresar = false, bool navigovatNaIndexHistorie = false, int indexHistorieNavigace = 0)
        {
            ListViewSouboryaSlozky.ItemsSource = null;
            ZapnoutVypnoutUzivatelskeRozhrani(false);
            TlacitkoNacistDalsiSoubory.Visibility = Visibility.Collapsed;
            NavigacniPanelCesty.SelectionChanged -= NavigacniPanelCesty_SelectionChanged;
            string adresaKamNavigovat = "";

            if (hledanyVyrazParametr == null)
            {

                if (navigovatNaIndexHistorie)
                { // Navigovat dle indexu v historii

                    int pocetIteraci = onedriveNavigacniCesta.Count - indexHistorieNavigace - 1;
                    for (int i = 0; i < pocetIteraci; i++) // Oříznout vyšší indexy (cesta, kterou potřebujeme smazat)
                    {
                        onedriveNavigacniCesta.RemoveAt(indexHistorieNavigace + 1);
                    }
                    //NavigacniPanelCesty.SelectedIndex = onedriveNavigacniCesta.Count - 1;
                }


                // Výpočet nové adresy
                if (navigovatNaKorenovyAdresar || onedriveNavigacniCesta.Count == 1)
                { // Kořenový adresář

                    onedriveNavigacniCesta.Clear();
                    onedriveNavigacniCesta.Add(resourceLoader.GetString("KorenovyAdresarNazev"));
                    NavigacniPanelCesty.SelectedIndex = 0;
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

                    //adresaKamNavigovat = "";
                }
                else
                { // Adresář dle pole onedriveNavigacniCesta

                    adresaKamNavigovat += AktualniCestaDoStringu();
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

                }


                adresaKamNavigovat += "/children";


                // Řazení dle
                switch (NastaveniPodleCehoRaditSouboryVeSlozce)
                {
                    case 0:
                        // Výchozí řazení
                        break;
                    case 1:
                        adresaKamNavigovat += "&$orderby=name asc";
                        break;
                    case 2:
                        adresaKamNavigovat += "&$orderby=name desc";
                        break;
                    /*case 3:
                        adresaKamNavigovat += "&$orderby=size asc";
                        break;
                    case 4:
                        adresaKamNavigovat += "&$orderby=size desc";
                        break;*/
                    case 3:
                        adresaKamNavigovat += "&$orderby=lastModifiedDateTime asc";
                        break;
                    case 4:
                        adresaKamNavigovat += "&$orderby=lastModifiedDateTime desc";
                        break;
                    default:
                        break;
                }
            }
            else
            { // Vyhledávání

                onedriveNavigacniCesta.Clear();
                onedriveNavigacniCesta.Add(resourceLoader.GetString("VysledkyVyhledavani") + " „" + hledanyVyrazParametr + "“");
                NavigacniPanelCesty.SelectedIndex = 0;

                adresaKamNavigovat += "/search(q='" + hledanyVyrazParametr + "')";

                // Řazení při vyhledávání nefunguje (prý možná jenom v OneDrive for Business)
            }

            adresaKamNavigovat += "?$select=id,name,folder,parentReference,createdDateTime,lastModifiedDateTime,webUrl,size,shared&$expand=thumbnails";

            


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
                return;
            }

            
            ListViewSouboryaSlozky.ItemsSource = obsahSlozkyOneDrive_aktualni;
            NavigacniPanelCesty.SelectedItem = NavigacniPanelCesty.Items[NavigacniPanelCesty.Items.Count - 1];
            NavigacniPanelCesty.SelectionChanged += NavigacniPanelCesty_SelectionChanged;
            ZapnoutVypnoutUzivatelskeRozhrani();

            strankaUzJeInicializovana = true;
        }

        // TLAČÍTKA COMMANDBAR VÝCHOZÍ

        private async void TlacitkoNovaSlozka_Click(object sender, RoutedEventArgs e)
        {
            StackPanel contentDialogNovaSlozka_stackPanel = new StackPanel();
            TextBox contentDialogNovaSlozka_textBox = new TextBox()
            {
                PlaceholderText = resourceLoader.GetString("contentDialogNovaSlozka_textBox/PlaceholderText")
            };

            contentDialogNovaSlozka_stackPanel.Children.Add(contentDialogNovaSlozka_textBox);

            ContentDialog contentDialogNovaSlozka = new ContentDialog()
            {
                Title = resourceLoader.GetString("contentDialogNovaSlozka/Title"),
                PrimaryButtonText = resourceLoader.GetString("contentDialogNovaSlozka/PrimaryButtonText"),
                CloseButtonText = resourceLoader.GetString("ZrusitDialog"),
                Content = contentDialogNovaSlozka_stackPanel,
                DefaultButton = ContentDialogButton.Primary
            };

            contentDialogNovaSlozka_textBox.SelectAll();
            ZapnoutVypnoutUzivatelskeRozhrani(false);

            ContentDialogResult contentDialogResult = await contentDialogNovaSlozka.ShowAsync();


            if (contentDialogResult == ContentDialogResult.Primary)
            {
                if (contentDialogNovaSlozka_textBox.Text.Length > 0)
                {

                    try
                    {

                        _ = await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root" + AktualniCestaDoStringu() + "/children", TypyHTTPrequestu.Post, "{ 'name': '" + contentDialogNovaSlozka_textBox.Text + "', 'folder': { }, '@microsoft.graph.conflictBehavior': 'rename' }");

                        //_ = await new ContentDialog()
                        //{
                        //    Title = "Přejmenováno",
                        //    CloseButtonText = "Zavřít"
                        //}.ShowAsync();

                        TlacitkoAktualizovat_Click(sender, e);

                    }
                    catch
                    {
                        return;
                    }

                }
                else
                {
                    _ = await new ContentDialog()
                    {
                        Title = resourceLoader.GetString("contentDialogNovaSlozka/ZadejtePlatnyNazev"),
                        CloseButtonText = resourceLoader.GetString("ZavritDialog")
                    }.ShowAsync();
                }

            }

            ZapnoutVypnoutUzivatelskeRozhrani();
        }

        private async void TlacitkoNahrat_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker pickerSouboruNahrat = new FileOpenPicker();
            pickerSouboruNahrat.FileTypeFilter.Add("*");

            IReadOnlyList<StorageFile> souboryKnahrani = await pickerSouboruNahrat.PickMultipleFilesAsync();

            try
            {
                //string idSlozkyKamPresunout = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root" + AktualniCestaDoStringu() + "?$select=id")).SelectToken("id").ToString();
                await NahratSoubory(AktualniCestaDoStringu(true), souboryKnahrani);
            }
            catch
            {
                return;
            }

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

        private async void TlacitkoOdstranit_Click(object sender, RoutedEventArgs e)
        {
            ZapnoutVypnoutUzivatelskeRozhrani(false);

            List<OneDriveAdresarSoubory> souboryKodstraneni = new List<OneDriveAdresarSoubory>();

            ContentDialog contentDialogOdstranit = new ContentDialog()
            {
                PrimaryButtonText = resourceLoader.GetString("contentDialogOdstranit/PrimaryButtonText"),
                CloseButtonText = resourceLoader.GetString("ZrusitDialog"),
                DefaultButton = ContentDialogButton.Primary
            };

            if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.Vychozi)
            { // Normální výběr –> přesunout vybraný soubor

                var originalSource = e.OriginalSource as FrameworkElement;
                OneDriveAdresarSoubory kliknutySoubor = (OneDriveAdresarSoubory)originalSource.DataContext;
                souboryKodstraneni.Add(kliknutySoubor);
            }
            else if (moznostiTlacitekCommandBar_aktualni == MoznostiTlacitekCommandBar.MultiVyber)
            { // Multivýběr –> přesunout soubory zaškrtlé v ListView

                foreach (var kliknuteSoubory in ListViewSouboryaSlozky.SelectedItems)
                {
                    souboryKodstraneni.Add((OneDriveAdresarSoubory)kliknuteSoubory);
                }
                
            }

            if (souboryKodstraneni.Count > 1)
            { // Více položek (více než 1)

                contentDialogOdstranit.Title = resourceLoader.GetString("contentDialogOdstranit/TitleMulti");
                contentDialogOdstranit.Content = souboryKodstraneni[0].Name + " a " + (souboryKodstraneni.Count - 1) + " dalších";
            }
            else
            {
                contentDialogOdstranit.Title = resourceLoader.GetString("contentDialogOdstranit/TitleJeden");
                contentDialogOdstranit.Content = souboryKodstraneni[0].Name;
            }

            ContentDialogResult contentDialogResult = await contentDialogOdstranit.ShowAsync();


            if (contentDialogResult == ContentDialogResult.Primary)
            {
                try
                {
                    foreach (OneDriveAdresarSoubory jedenSouborKodstraneni in souboryKodstraneni)
                    {
                        await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/items/" + jedenSouborKodstraneni.Id, TypyHTTPrequestu.Delete);
                    }
                    TlacitkoAktualizovat_Click(sender, e);

                }
                catch
                {
                    return;
                }

            }

            ZapnoutVypnoutUzivatelskeRozhrani();
        }




        // TLAČÍTKA COMMANDBAR PŘESUN SOUBORŮ
        private async void TlacitkoPresunoutSem_Click(object sender, RoutedEventArgs e)
        {
            ZapnoutVypnoutUzivatelskeRozhrani(false);

            try
            {
                string idSlozkyKamPresunout = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/root" + AktualniCestaDoStringu() + "?$select=id")).SelectToken("id").ToString();

                foreach (OneDriveAdresarSoubory jedenSouborKpresunuti in souboryKpresunuti)
                {
                    await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me/drive/items/" + jedenSouborKpresunuti.Id, TypyHTTPrequestu.Patch, "{ 'parentReference': { 'id': '" + idSlozkyKamPresunout + "' } }");
                }

                TlacitkoAktualizovat_Click(sender, e);
                PrepinacTlacitkaCommandBar();
            }
            catch
            {
                return;
            }

            ZapnoutVypnoutUzivatelskeRozhrani();
        }

        private void FlyoutTlacitkoZrusitPresun_Click(object sender, RoutedEventArgs e)
        {
            PrepinacTlacitkaCommandBar();
        }
    }
}

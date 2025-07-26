using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace MetropolisOnedriveKlient
{
    public partial class OneDriveOpravneniSouboru
    {
        [JsonProperty("value")]
        public ValueOpravneni[] Value { get; set; }

    }


    public partial class ValueOpravneni
    {
        private static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();


        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("grantedToIdentities")]
        public GrantedToIdentities_User[] GrantedToIdentities { get; set; }

        [JsonProperty("roles")]
        public string[] Roles { get; set; }

        [JsonProperty("link", NullValueHandling = NullValueHandling.Ignore)]
        public Link Link { get; set; }

        [JsonProperty("grantedTo", NullValueHandling = NullValueHandling.Ignore)]
        public GrantedTo GrantedTo { get; set; }

        [JsonProperty("inheritedFrom", NullValueHandling = NullValueHandling.Ignore)]
        public InheritedFrom InheritedFrom { get; set; }


        public string TypSdileniNadpis
        {
            get
            {
                string typSdileniNadpis;
                if (Link != null)
                { // Odkaz

                    if ((Link?.Application) == null)
                    {
                        if (GrantedToIdentities?.Length == null || GrantedToIdentities.Length == 0)
                        {
                            typSdileniNadpis = resourceLoader.GetString("contentDialogSdileni/Odkaz");
                        }
                        else
                        {
                            string uzivatelePristupOdkaz = "";
                            foreach (GrantedToIdentities_User jedenUzivatelPristupOdkaz in GrantedToIdentities)
                            {
                                uzivatelePristupOdkaz += jedenUzivatelPristupOdkaz.User.OpravneniDisplayName + ", ";
                            }
                            typSdileniNadpis = resourceLoader.GetString("contentDialogSdileni/OdkazUzivatele") + " " + uzivatelePristupOdkaz.Remove(uzivatelePristupOdkaz.Length - 2) + ")";
                        }
                        //typSdileniNadpis = resourceLoader.GetString("contentDialogSdileni/Odkaz");
                    }
                    else
                    { // Odkaz aplikace

                        typSdileniNadpis = resourceLoader.GetString("contentDialogSdileni/OdkazAplikace") + " " + Link.Application.DisplayName + ")";
                    }
                }
                else if (GrantedTo?.User != null)
                { // Uživatel

                    typSdileniNadpis = GrantedTo.User.OpravneniDisplayName;
                }
                else
                {
                    typSdileniNadpis = resourceLoader.GetString("Chyba");
                }

                if (InheritedFrom != null)
                {
                    return typSdileniNadpis + " (" + resourceLoader.GetString("contentDialogSdileni/OpravneniZdedenoOd") + " " + InheritedFrom.Path.Split('/').Last() + ")";
                }
                else
                {
                    return typSdileniNadpis;
                }
            }
        }
    }

    public partial class GrantedTo
    {
        [JsonProperty("user")]
        public OpravneniUser User { get; set; }
    }

    public partial class GrantedToIdentities_User
    {
        [JsonProperty("user")]
        public OpravneniUser User { get; set; }
    }

    public partial class OpravneniUser
    {
        [JsonProperty("id")]
        public string OpravneniId { get; set; }

        [JsonProperty("displayName")]
        public string OpravneniDisplayName { get; set; }

        [JsonProperty("email")]
        public string OpravneniEmail { get; set; }
    }

    public partial class InheritedFrom
    {
        [JsonProperty("driveId")]
        public string DriveId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }

    public partial class Link
    {
        [JsonProperty("webUrl")]
        public Uri WebUrl { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("application", NullValueHandling = NullValueHandling.Ignore)]
        public User Application { get; set; }
    }
}
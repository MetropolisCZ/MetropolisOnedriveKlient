using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace MetropolisOnedriveKlient
{
    public class ApiWebKlient
    {

        public static HttpClient httpClient = new HttpClient();


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

    }
}

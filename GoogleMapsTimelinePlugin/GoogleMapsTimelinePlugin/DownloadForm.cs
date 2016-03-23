using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Services;

using Google.Apis.Coordinate.v1;


namespace GoogleMapsTimelinePlugin
{
    public partial class DownloadForm : Form
    {


        public DownloadForm()
        {
            InitializeComponent();
        }

        private void _download_Click(object sender, EventArgs e)
        {
            try
            {
                ClientSecrets cs = new ClientSecrets();
                cs.ClientId = "728822159738-252ghnvc1l1kjssq0d6dtrf2h2t48ihp.apps.googleusercontent.com";
                cs.ClientSecret = "lO67xySyeUClGRYmwQ1LIyee";


                UserCredential uc = GoogleWebAuthorizationBroker.AuthorizeAsync(cs, new string[] { Google.Apis.Coordinate.v1.CoordinateService.Scope.Coordinate }, "user", System.Threading.CancellationToken.None, new FileDataStore("Timelines.MyLibrary")).Result;

                var coordinateService = new CoordinateService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = uc,
                    ApplicationName = "Google maps timeline downloader"
                });


                // ここからどうすんだ？

                /*
                Google.Apis.Http.ConfigurableHttpClient http = coordinateService.HttpClient;

                DateTime target = dateTimePicker1.Value;

                string targetUrl = string.Format("https://www.google.com/maps/timeline/kml?authuser=0&pb=!1m8!1m3!1i{0}!2i{1}!3i{2}!2m3!1i{0}!2i{1}!3i{2}", target.Year, target.Month - 1, target.Day);

                System.Net.Http.HttpResponseMessage res = http.SendAsync(new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, targetUrl)).Result;

                string result = res.Content.ReadAsStringAsync().Result;

                System.Diagnostics.Debug.Print(result);
                */

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                throw;
            }
        }
    }
}

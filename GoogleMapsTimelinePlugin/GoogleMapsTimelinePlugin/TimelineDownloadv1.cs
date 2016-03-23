using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bykIFv1;

namespace GoogleMapsTimelinePlugin
{
    public class TimelineDownloadv1 : bykIFv1.PlugInInterface
    {

        private const string CLIENT_ID = "";
        private const string CLIENT_SECRET = "";
        private const string API_KEY = "";
        private const string PROFILE_ID = "";

        //private readonly AnalyticsService _service;

        public System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GMTDownload_ICON;
            }
        }

        public string Name
        {
            get
            {
                return Properties.Resources.GMTDownload_NAME;
            }
        }

        public TrackItem[] GetTrackItems(System.Windows.Forms.IWin32Window owner)
        {
            DownloadForm dl = new DownloadForm();

            dl.ShowDialog(owner);

            return new TrackItem[] { };
        }
        /*
        public AnalyticsServiceWrapper()
        {
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = CLIENT_ID,
                ClientSecret = CLIENT_SECRET
            }, new[] { AnalyticsService.Scope.Analytics }
                , "user"
                , CancellationToken.None
                , new FileDataStore("TumblingDiceAnalytics")
            ).Result;

            _service = new AnalyticsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "TumblingDice",
                ApiKey = API_KEY,
            });
        }*/
    }
}
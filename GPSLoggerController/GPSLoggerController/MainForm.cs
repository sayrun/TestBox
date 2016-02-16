using SkyTraq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace GPSLoggerController
{
    public partial class MainForm : Form
    {
        GT730FLSController _gt730;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            button2.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _gt730 = new GT730FLSController("COM6");

            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;

            return;
        }

        System.IO.BinaryReader _com;

        private void button2_Click(object sender, EventArgs e)
        {

            List<TrackPoint> items = _gt730.ReadLatLonData();

            return;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _gt730.sendRestart();

            return;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _gt730.setBaudRate(GT730FLSController.BaudRate.BaudRate_115200);

            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;

        }

        private void Write(List<TrackPoint> e)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.Encoding = Encoding.UTF8;
            using (XmlWriter xw = XmlWriter.Create(@"C:\Users\Tomo\Documents\hoge.gpx", settings))
            {
                xw.WriteStartElement("gps");
                {
                    xw.WriteAttributeString("version", "1.0");
                    xw.WriteAttributeString("creator", "TTTT");

                    xw.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    xw.WriteAttributeString("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd");

                    // 作成時間
                    xw.WriteElementString("time", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"));

                    xw.WriteStartElement("bounds");
                    {
                        xw.WriteAttributeString("minlat", "1.0");
                        xw.WriteAttributeString("minlon", "1.0");
                        xw.WriteAttributeString("maxlat", "1.0");
                        xw.WriteAttributeString("maxlon", "1.0");
                    }
                    xw.WriteEndElement();


                    xw.WriteStartElement("trk");
                    {
                        xw.WriteStartElement("trkseg");
                        {
                            int index = 1;
                            foreach (TrackPoint point in e)
                            {
                                xw.WriteStartElement("trkpt");
                                {
                                    decimal lat = point.Lat;
                                    if( "S" == point.LatMark)
                                    {
                                        lat *= -1;
                                    }
                                    decimal lon = point.Lon;
                                    if ("W" == point.LonMark)
                                    {
                                        lon *= -1;
                                    }
                                    decimal ele = point.Ele;

                                    xw.WriteAttributeString("lat", lat.ToString());
                                    xw.WriteAttributeString("lon", lon.ToString());

                                    xw.WriteElementString("ele", point.Ele.ToString());
                                    xw.WriteElementString("time", point.Time.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                                    xw.WriteElementString("speed", point.Speed.ToString());
                                    xw.WriteElementString("name", string.Format( "TP{0:4}", index));
                                    index++;
                                }
                                xw.WriteEndElement();
                            }
                        }
                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                }
                xw.WriteEndElement();
            }


        }

    }
}

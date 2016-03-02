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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if( null != _gt730)
            {
                _gt730.Dispose();
                _gt730 = null;
            }

            string portName = comboBox1.SelectedItem.ToString();

            _gt730 = new GT730FLSController(portName);

            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            comboBox1.Enabled = false;

            return;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            List<TrackPoint> items = _gt730.ReadLatLonData();

            return;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _gt730.Dispose();

            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            comboBox1.Enabled = true;


            return;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string msg = string.Empty;
            if( _gt730.EraceLatLonData())
            {
                msg = "消去しました";
            }
            else
            {
                msg = "消去できませんでした";
            }
            MessageBox.Show(msg, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                                    decimal lat = point.Latitude;
                                    if( "S" == point.LatMark)
                                    {
                                        lat *= -1;
                                    }
                                    decimal lon = point.Longitude;
                                    if ("W" == point.LongitudeMark)
                                    {
                                        lon *= -1;
                                    }
                                    decimal ele = point.Elevation;

                                    xw.WriteAttributeString("lat", lat.ToString());
                                    xw.WriteAttributeString("lon", lon.ToString());

                                    xw.WriteElementString("ele", point.Elevation.ToString());
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            foreach (string portName in System.IO.Ports.SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(portName);
            }
        }
    }
}

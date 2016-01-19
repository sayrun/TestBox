using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JpegExifTest
{
    public partial class Form1 : Form
    {
        private Dictionary<string, List< TrackPointItem>> _tracks;

        public Form1()
        {
            InitializeComponent();

            _tracks = new Dictionary<string, List<TrackPointItem>>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                TestFunc1(openFileDialog1.FileName);
            }
        }

        private void TestFunc1(string fileName)
        {
            Bitmap bmp = new Bitmap(fileName);

            foreach (System.Drawing.Imaging.PropertyItem p in bmp.PropertyItems)
            {
                // 0x9003 - 原画像データの生成日時
                // 0x9004 - デジタルデータの作成日時

                // 0x0001-北緯(N) or 南緯(S)(Ascii)
                // 0x0002-緯度(数値)(Rational)
                // 0x0003-東経(E) or 西経(W)(Ascii)
                // 0x0004-経度(数値)(Rational)
                // 0x0005-高度の基準(Byte)
                // 0x0006-高度(数値)(Rational)
                // 0x0007-GPS時間(原子時計の時間)(Rational)
                // 0x0008-測位に使った衛星信号(Ascii)
                // 0x0009-GPS受信機の状態(Ascii)
                // 0x000A-GPSの測位方法(Ascii)
                // 0x000B-測位の信頼性(Rational)
                // 0x000C-速度の単位(Ascii)
                // 0x000D-速度(数値)(Rational)


                if (p.Type == 2 && p.Len > 0)
                {
                    string s = System.Text.Encoding.ASCII.GetString(p.Value);
                    s = s.Trim(new char[] { '\0' });
                    System.Diagnostics.Debug.Print("{0} - [{1}][{2}]", p.Id, s, p.Type);
                }
                else
                {
                    System.Diagnostics.Debug.Print("{0} - [{1}][{2}]", p.Id, p.Value, p.Type);
                }
            }
            System.Drawing.Imaging.PropertyItem p1 = bmp.PropertyItems[0];
            {
                // 緯度
                p1.Id = 1;
                p1.Type = 2;
                p1.Value = ConvertTo("N");
                p1.Len = p1.Value.Length;
                bmp.SetPropertyItem(p1);

                p1.Id = 2;
                p1.Type = 5;
                p1.Value = ConvertTo(new UInt32[] { 38, 1, 2, 1, 37, 10 });
                p1.Len = p1.Value.Length;
                bmp.SetPropertyItem(p1);

                // 経度
                p1.Id = 3;
                p1.Type = 2;
                p1.Value = ConvertTo("E");
                p1.Len = p1.Value.Length;
                bmp.SetPropertyItem(p1);

                p1.Id = 4;
                p1.Type = 5;
                p1.Value = ConvertTo(new UInt32[] { 140, 1, 44, 1, 175, 10 });
                p1.Len = p1.Value.Length;
                bmp.SetPropertyItem(p1);

                // 高度
                p1.Id = 5;
                p1.Type = 7;
                p1.Value = new byte[] { 0x00};
                p1.Len = p1.Value.Length;
                bmp.SetPropertyItem(p1);

                p1.Id = 6;
                p1.Type = 5;
                p1.Value = ConvertTo(new UInt32[] { 40, 1 });
                p1.Len = p1.Value.Length;

                bmp.SetPropertyItem(p1);
            }


            String newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fileName), "HOGE.jpg");

            bmp.Save(newPath);
        }

        private byte[] ConvertTo(UInt32[] param1)
        {
            byte[] result = new byte[sizeof(UInt32) * param1.Length];
            System.Buffer.BlockCopy(param1, 0, result, 0, result.Length);

            return result;
        }

        private byte[] ConvertTo(string param1)
        {
            char[] value = param1.ToCharArray();
            byte[] result = new byte[value.Length + 1];
            System.Buffer.BlockCopy(value, 0, result, 0, value.Length);
            result[value.Length] = 0x00;

            return result;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if( DialogResult.OK == openFileDialog2.ShowDialog(this))
            {
                TestFunc2(openFileDialog2.FileName);
            }
        }

        private TrackPointItem GetLocData( string trkptXml)
        {
            string lon = string.Empty;
            string lat = string.Empty;
            string sEle = string.Empty;
            string sTime = string.Empty;
            string sSpeed = string.Empty;

            using (System.Xml.XmlReader xr = System.Xml.XmlReader.Create( new System.IO.StringReader( trkptXml)))
            {
                while (xr.Read())
                {
                    switch (xr.NodeType)
                    {
                        case System.Xml.XmlNodeType.Element:
                            if (0 == string.Compare(xr.Name, "ele", true))
                            {
                                sEle = xr.ReadString();
                            }
                            else if (0 == string.Compare(xr.Name, "time", true))
                            {
                                sTime = xr.ReadString();
                            }
                            else if (0 == string.Compare(xr.Name, "speed", true))
                            {
                                sSpeed = xr.ReadString();
                            }
                            else if( 0 == string.Compare( xr.Name, "trkpt", true))
                            {
                                lon = xr.GetAttribute("lon");
                                lat = xr.GetAttribute("lat");

                            }
                            break;
                    }
                }
            }

            TrackPointItem trkPt = new TrackPointItem(lon, lat, DateTime.Parse(sTime));
            trkPt.Speed = sSpeed;
            trkPt.Ele = sEle;

            return trkPt;
        }

        private void TestFunc2( string filePath)
        {
            List<TrackPointItem> trkPtList = new List<TrackPointItem>();

            DateTime target = DateTime.Parse("2010/04/25 12:21:09");

            using (System.Xml.XmlReader xr = System.Xml.XmlReader.Create(new System.IO.StreamReader(filePath)))
            {
                while( xr.Read())
                {
                    switch (xr.NodeType)
                    {
                        case System.Xml.XmlNodeType.Element:
                            System.Diagnostics.Debug.Print(xr.Name);
                            if ( 0== string.Compare( xr.Name, "trkpt", true))
                            {
                                string sxml = xr.ReadOuterXml();

                                string lon = xr.GetAttribute("lon");
                                string lat = xr.GetAttribute("lat");

                                TrackPointItem item = GetLocData(sxml);

                                trkPtList.Add(item);

                                if (0 == item.CompareTo(target))
                                {
                                    UInt32[] re = item.LonArray();

                                    UInt32[] el = item.EleArray();

                                    UInt32[] sp = item.SpeedArray();

                                    string s = string.Format("http://maps.google.com/maps?q={0},{1}", item.Lat, item.Lon);
                                    System.Diagnostics.Process.Start(s);

                                    System.Diagnostics.Debug.Print("");
                                }
                            }
                            break;

                        case System.Xml.XmlNodeType.Text:
                            System.Diagnostics.Debug.Print(xr.Value);
                            break;

                        default:
                            System.Diagnostics.Debug.Print(xr.NodeType.ToString());
                            break;
                    }
                }
            }

            trkPtList.Sort();

            if (_tracks.ContainsKey(filePath))
            {
                _tracks[filePath] = trkPtList;
            }
            else
            {
                _tracks.Add(filePath, trkPtList);
            }
            return;
        }
    }
}

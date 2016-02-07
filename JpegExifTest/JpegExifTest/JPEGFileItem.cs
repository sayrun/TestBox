using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegExifTest
{
    class JPEGFileItem
    {
        private readonly string _filePath;

        private DateTime _targetdate;
        private GPSLocation _currentLocation;
        private GPSLocation _newLocation;

        public JPEGFileItem( string filePath)
        {
            _filePath = filePath;

            AnalyzeExif(_filePath);

            _newLocation = new GPSLocation();
        }

        public string FilePath
        {
            get
            {
                return _filePath;
            }
        }

        public string FileName
        {
            get
            {
                return System.IO.Path.GetFileName(_filePath);
            }
        }


        /// <summary>
        /// 撮影時間
        /// </summary>
        public DateTime DateTimeOriginal
        {
            get
            {
                return _targetdate;
            }
        }

        public GPSLocation CurrentLocation
        {
            get
            {
                return _currentLocation;
            }
        }

        public GPSLocation NewLocation
        {
            get
            {
                return _newLocation;
            }
        }

        private void AnalyzeExif(string filePath)
        {
            DateTime dt = DateTime.MinValue;
            string sNS = string.Empty;
            string sEW = string.Empty;

            UInt32[] uLon = null;
            UInt32[] uLat = null;


            using (Bitmap bmp = new Bitmap(filePath))
            {
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
                foreach (System.Drawing.Imaging.PropertyItem item in bmp.PropertyItems)
                {
                    switch (item.Id)
                    {
                        // 0x9004 - デジタルデータの作成日時
                        case 0x9004:
                            if (0 == dt.CompareTo(DateTime.MinValue))
                            {
                                string s = System.Text.Encoding.ASCII.GetString(item.Value);
                                s = s.Trim(new char[] { '\0' });
                                {
                                    s = System.Text.RegularExpressions.Regex.Replace(s,
                                            @"^(?<year>(?:\d\d)?\d\d):(?<month>\d\d?):(?<day>\d\d?)",
                                            "${year}/${month}/${day}");
                                }
                                dt = DateTime.Parse(s);
                            }
                            break;

                        // 0x9003 - 原画像データの生成日時
                        case 0x9003:
                            {
                                string s = System.Text.Encoding.ASCII.GetString(item.Value);
                                s = s.Trim(new char[] { '\0' });
                                {
                                    s = System.Text.RegularExpressions.Regex.Replace(s,
                                            @"^(?<year>(?:\d\d)?\d\d):(?<month>\d\d?):(?<day>\d\d?)",
                                            "${year}/${month}/${day}");
                                }
                                dt = DateTime.Parse(s);
                            }
                            break;

                        // 0x0001-北緯(N) or 南緯(S)(Ascii)
                        case 0x0001:
                            sNS = System.Text.Encoding.ASCII.GetString(item.Value);
                            sNS = sNS.Trim(new char[] { '\0' });
                            break;

                        // 0x0002-緯度(数値)(Rational):latitude - 緯度
                        case 0x0002:
                            {
                                if (4 <= item.Len)
                                {
                                    uLat = new UInt32[item.Len / 4];
                                    System.Buffer.BlockCopy(item.Value, 0, uLat, 0, item.Len);
                                }
                            }
                            break;

                        // 0x0003-東経(E) or 西経(W)(Ascii)
                        case 0x0003:
                            sEW = System.Text.Encoding.ASCII.GetString(item.Value);
                            sEW = sEW.Trim(new char[] { '\0' });
                            break;
                        // 0x0004-経度(数値)(Rational)：longitude - 経度
                        case 0x0004:
                            {
                                if (4 <= item.Len)
                                {
                                    uLon = new UInt32[item.Len / 4];
                                    System.Buffer.BlockCopy(item.Value, 0, uLon, 0, item.Len);
                                }
                            }
                            break;
                    }
                }
            }

            _targetdate = dt;

            // 経度
            GPSLocation.LongitudeFlag lonFlag = GPSLocation.LongitudeFlag.INVALID;
            if (0 == string.Compare(sEW, "E", true))
            {
                lonFlag = GPSLocation.LongitudeFlag.EAST;
            }
            else if (0 == string.Compare(sEW, "W", true))
            {
                lonFlag = GPSLocation.LongitudeFlag.WEST;
            }

            // 緯度
            GPSLocation.LatitudeFlag latFlag = GPSLocation.LatitudeFlag.INVALID;
            if (0 == string.Compare(sNS, "N", true))
            {
                latFlag = GPSLocation.LatitudeFlag.NORTH;
            }
            else if (0 == string.Compare(sNS, "S", true))
            {
                latFlag = GPSLocation.LatitudeFlag.SOUTH;
            }

            _currentLocation = new GPSLocation(lonFlag, uLon, latFlag, uLat);
        }

    }
}

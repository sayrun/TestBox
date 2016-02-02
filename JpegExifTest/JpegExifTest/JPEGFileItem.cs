﻿using System;
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

        private decimal _dcLon = 0;
        private UInt32[] _longitude;
        private string _lonFlag;

        private decimal _dcLat = 0;
        private UInt32[] _latitude;
        private string _latFlag;

        private const int DISPLAY_LENGTH = 4;

        public JPEGFileItem( string filePath)
        {
            _filePath = filePath;

            AnalyzeExif(_filePath);
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

        /// <summary>
        /// 経度生値
        /// </summary>
        public UInt32[] LongitudeRaw
        {
            get
            {
                return _longitude;
            }

            set
            {
                _longitude = null;
                _longitude = new UInt32[value.Length];
                System.Buffer.BlockCopy(value, 0, _longitude, 0, _longitude.Length);
            }
        }

        public enum LongitudeFlag
        {
            INVALID,
            NORTH,
            SOUTH
        }

        public LongitudeFlag LongitudeRef
        {
            get
            {
                if( 0 == string.Compare( _lonFlag,"N",true ))
                {
                    return LongitudeFlag.NORTH;
                }
                if (0 == string.Compare(_lonFlag, "S", true))
                {
                    return LongitudeFlag.SOUTH;
                }
                return LongitudeFlag.INVALID;
            }

            set
            {
                if (LongitudeFlag.INVALID == value)
                    throw new Exception("設定できなよ");

                if(LongitudeFlag.NORTH == value)
                {
                    _lonFlag = "N";
                }
                else
                {
                    _lonFlag = "S";
                }
            }
        }

        /// <summary>
        /// 経度
        /// </summary>
        public string Longitude
        {
            get
            {
                string result = _dcLon.ToString();

                int index = result.IndexOf('.');
                if (0 < index && result.Length > (index + DISPLAY_LENGTH))
                {
                    result = result.Substring(0, index + DISPLAY_LENGTH);
                }

                return result;
            }
        }

        public enum LatitudeFlag
        {
            INVALID,
            EAST,
            WEST
        }

        public LatitudeFlag LatitudeRef
        {
            get
            {
                if (0 == string.Compare(_latFlag, "E", true))
                {
                    return LatitudeFlag.EAST;
                }
                if (0 == string.Compare(_latFlag, "W", true))
                {
                    return LatitudeFlag.WEST;
                }
                return LatitudeFlag.INVALID;
            }

            set
            {
                if (LatitudeFlag.INVALID == value)
                    throw new Exception("設定できなよ");

                if (LatitudeFlag.EAST == value)
                {
                    _latFlag = "E";
                }
                else
                {
                    _latFlag = "W";
                }
            }
        }

        public string GPSPosition
        {
            get
            {
                return string.Format("{0}, {1}", this.Longitude, this.Latitude);
            }
        }

        /// <summary>
        /// 緯度生値
        /// </summary>
        public UInt32[] LatitudeRaw
        {
            get
            {
                return _latitude;
            }

            set
            {
                _latitude = null;
                _latitude = new UInt32[value.Length];
                System.Buffer.BlockCopy(value, 0, _latitude, 0, _latitude.Length);
            }
        }

        /// <summary>
        /// 緯度
        /// </summary>
        public string Latitude
        {
            get
            {
                string result = _dcLat.ToString();

                int index = result.IndexOf('.');
                if (0 < index && result.Length > (index + DISPLAY_LENGTH))
                {
                    result = result.Substring(0, index + DISPLAY_LENGTH);
                }

                return result;
            }
        }


        private void AnalyzeExif(string filePath)
        {
            DateTime dt = DateTime.MinValue;
            string sNS = string.Empty;
            string sEW = string.Empty;
            decimal dcLon = decimal.Zero;
            decimal dcLat = decimal.Zero;

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

                                    dcLat = uLat[0];
                                    dcLat /= uLat[1];

                                    decimal dPar1 = 60;
                                    for (int index = 1; index < uLat.Length / 2; ++index)
                                    {
                                        decimal dWork = uLat[index * 2];
                                        dWork /= dPar1;
                                        dWork /= uLat[index * 2 + 1];

                                        dcLat += dWork;
                                        dPar1 *= 60;
                                    }

                                    System.Diagnostics.Debug.Print("{0}[{1}]", sNS, dcLat);
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

                                    dcLon = uLon[0];
                                    dcLon /= uLon[1];

                                    decimal dPar1 = 60;
                                    for (int index = 1; index < uLon.Length / 2; ++index)
                                    {
                                        decimal dWork = uLon[index * 2];
                                        dWork /= dPar1;
                                        dWork /= uLon[index * 2 + 1];

                                        dcLon += dWork;
                                        dPar1 *= 60;
                                    }

                                    System.Diagnostics.Debug.Print("{0}[{1}]", sEW, dcLon);
                                }
                            }
                            break;
                    }
                }
            }

            _targetdate = dt;
            if (decimal.Zero != dcLat)
            {
                _dcLat = dcLat;
                if(0==sNS.CompareTo("S"))
                {
                    _dcLat = decimal.Zero - _dcLat;
                }
            }
            if( decimal.Zero != dcLon)
            {
                _dcLon = dcLon;
                if( 0== sEW.CompareTo("W"))
                {
                    _dcLon = decimal.Zero - _dcLon;
                }
            }

            _latitude = uLat;
            _latFlag = sNS;
            _longitude = uLon;
            _lonFlag = sEW;

#if false

            bool blMatch = false;
            foreach (ListViewItem itema in listView1.Items)
            {
                GPXFileItem gpx = itema.Tag as GPXFileItem;
                if (null == gpx) continue;

                foreach (TrackPointItem trkPt in gpx.Items)
                {
                    if (dt == trkPt.Time)
                    {
                        blMatch = true;

                        listItem.SubItems.Add(string.Format("{0} {1}, {2} {3}", trkPt.LonMark, trkPt.Lon, trkPt.LatMark, trkPt.Lat));

                        string s = string.Format("http://maps.google.com/maps?q={0},{1}", dcLat, dcLon);
                        System.Diagnostics.Process.Start(s);

                        break;
                    }
                }

                if (blMatch) break;
            }

#endif
        }

    }
}
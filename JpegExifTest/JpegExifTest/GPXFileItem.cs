using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JpegExifTest
{
    class GPXFileItem
    {
        private List<TrackPointItem> _items;
        private readonly string _filePath;

        public GPXFileItem(string filePath)
        {
            // GPXファイルを読み込む
            _items = LoadGpxFile(filePath);
            // 時間でソート
            _items.Sort();
            // ファイルパスを記録
            _filePath = filePath;
        }

        /// <summary>
        /// Track Points
        /// </summary>
        public List<TrackPointItem> Items
        {
            get
            {
                return _items;
            }
        }

        /// <summary>
        /// GPS記録の開始時間
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return _items[0].Time;
            }
        }

        /// <summary>
        /// GPS記録の終了時間
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                return _items[_items.Count - 1].Time;
            }
        }

        /// <summary>
        /// GPXファイル名
        /// </summary>
        public string FileName
        {
            get
            {
                return System.IO.Path.GetFileName(_filePath);
            }
        }

        /// <summary>
        /// GPXファイルパス
        /// </summary>
        public string FilePath
        {
            get
            {
                return _filePath;
            }
        }
        
        /// <summary>
        /// ポイント数
        /// </summary>
        public int PointCount
        {
            get
            {
                return _items.Count;
            }
        }
#if _OLD_
        private TrackPointItem GetLocData(string trkptXml)
        {
            string lon = string.Empty;
            string lat = string.Empty;
            string sEle = string.Empty;
            string sTime = string.Empty;
            string sSpeed = string.Empty;

            using (System.Xml.XmlReader xr = System.Xml.XmlReader.Create(new System.IO.StringReader(trkptXml)))
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
                            else if (0 == string.Compare(xr.Name, "trkpt", true))
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

        private List<TrackPointItem> LoadGpxFile(string filePath)
        {
            List<TrackPointItem> trkPtList = new List<TrackPointItem>();

            using (System.Xml.XmlReader xr = System.Xml.XmlReader.Create(new System.IO.StreamReader(filePath)))
            {
                while (xr.Read())
                {
                    switch (xr.NodeType)
                    {
                        case System.Xml.XmlNodeType.Element:
                            //System.Diagnostics.Debug.Print(xr.Name);
                            if (0 == string.Compare(xr.Name, "trkpt", true))
                            {
                                string sxml = xr.ReadOuterXml();

                                string lon = xr.GetAttribute("lon");
                                string lat = xr.GetAttribute("lat");

                                TrackPointItem item = GetLocData(sxml);

                                trkPtList.Add(item);
                            }
                            break;
/*
                        case System.Xml.XmlNodeType.Text:
                            System.Diagnostics.Debug.Print(xr.Value);
                            break;

                        default:
                            System.Diagnostics.Debug.Print(xr.NodeType.ToString());
                            break;*/
                    }
                }
            }

            return trkPtList;
        }
#else
        /// <summary>
        /// GPXファイルの読み込み
        /// </summary>
        /// <param name="filePath">GPXファイルのパス</param>
        /// <returns>PGXから読み込んだ座標情報リスト</returns>
        private static List<TrackPointItem> LoadGpxFile(string filePath)
        {
            List<TrackPointItem> trkPtList = new List<TrackPointItem>();

            using (System.Xml.XmlReader xr = System.Xml.XmlReader.Create(new System.IO.StreamReader(filePath)))
            {
                while (xr.Read())
                {
                    switch (xr.NodeType)
                    {
                        case System.Xml.XmlNodeType.Element:
                            if (0 == string.Compare(xr.Name, "trkpt", true))
                            {
                                if (!xr.IsEmptyElement)
                                {
                                    string lon = xr.GetAttribute("lon");
                                    string lat = xr.GetAttribute("lat");

                                    string sEle = string.Empty;
                                    string sTime = string.Empty;
                                    string sSpeed = string.Empty;

                                    while( xr.Read())
                                    {
                                        switch( xr.NodeType)
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
                                                else
                                                {
                                                    if( !xr.IsEmptyElement)
                                                    {
                                                        while (xr.Read())
                                                            if (xr.NodeType == System.Xml.XmlNodeType.EndElement) break;
                                                    }
                                                }
                                                continue;

                                            case System.Xml.XmlNodeType.EndElement:
                                                break;

                                            default:
                                                continue;
                                        }
                                        break;
                                    }

                                    DateTime dt;
                                    if (DateTime.TryParse(sTime, out dt))
                                    {
                                        TrackPointItem item = new TrackPointItem(lon, lat, dt);
                                        item.Speed = sSpeed;
                                        item.Ele = sEle;
                                        trkPtList.Add(item);
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            return trkPtList;
        }
#endif
    }
}

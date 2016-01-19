using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JpegExifTest
{
    class TrackPointItem : IComparable<TrackPointItem>, IComparable<DateTime>
    {
        private string _lon;
        private string _lat;
        private string _ele;
        private string _speed;
        private DateTime _time;

        public TrackPointItem(string lon, string lat, DateTime time)
        {
            _lon = lon;
            _lat = lat;
            _ele = string.Empty;
            _speed = string.Empty;
            _time = time;
        }

        public string Lon
        {
            get
            {
                return _lon;
            }
        }

        public string Lat
        {
            get
            {
                return _lat;
            }
        }

        public string Ele
        {
            get
            {
                return _ele;
            }
            set
            {
                _ele = value;
            }
        }

        public string Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = value;
            }
        }

        public string SpeedUnit
        {
            get
            {
                return "m/s";
            }
        }


        public DateTime Time
        {
            get
            {
                return _time;
            }
        }

        private UInt32[] ToEleArray(decimal value)
        {
            decimal lonA = Math.Floor(Math.Abs(value) * 1000);

            lonA *= Math.Sign(value);

            UInt32[] result = new UInt32[] { (UInt32)lonA, 1000 };

            return result;
        }

        private UInt32[] ToLonLatArray(decimal value)
        {
            value = Math.Abs(value);

            decimal lonA = Math.Floor(value);

            decimal lonB = Math.Floor((value - lonA) * 60);

            decimal lonC = Math.Floor(((value - lonA) - (lonB / 60)) * 60 * 60 * 1000);

            UInt32[] result = new UInt32[] { (UInt32)lonA, 1, (UInt32)lonB, 1, (UInt32)lonC, 1000 };

            return result;
        }

        public string LonMark
        {
            get
            {
                if (0 <= decimal.Parse(_lon))
                {
                    return "N";
                }
                else
                {
                    return "S";
                }
            }
        }

        public UInt32[] LonArray()
        {
            decimal db = decimal.Parse(_lon);

            return ToLonLatArray(db);
        }

        public string LatMark
        {
            get
            {
                if( 0 <= decimal.Parse(_lat) )
                {
                    return "E";
                }
                else
                {
                    return "W";
                }
            }

        }

        public UInt32[] LatArray()
        {
            decimal db = decimal.Parse(_lat);

            return ToLonLatArray(db);
        }

        public UInt32[] EleArray()
        {
            decimal db = decimal.Parse(_ele);

            return ToEleArray(db);
        }

        public UInt32[] SpeedArray()
        {
            decimal db = decimal.Parse(_speed);

            return ToEleArray(db);
        }
        public int CompareTo(TrackPointItem other)
        {
            return _time.CompareTo(other._time);
        }

        public int CompareTo(DateTime other)
        {
            return _time.CompareTo(other);
        }
    }
}

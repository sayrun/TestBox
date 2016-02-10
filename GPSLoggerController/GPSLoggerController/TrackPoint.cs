using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyTraq
{
    class TrackPoint : IComparable<TrackPoint>, IComparable<DateTime>
    {
        private decimal _lon;
        private decimal _lat;
        private decimal _ele;
        private decimal _speed;
        private DateTime _time;

        public TrackPoint(decimal lon, decimal lat, DateTime time)
        {
            _lon = lon;
            _lat = lat;
            _ele = decimal.MaxValue;
            _speed = decimal.Zero;
            _time = time;
        }

        public decimal Lon
        {
            get
            {
                return _lon;
            }
        }

        public decimal Lat
        {
            get
            {
                return _lat;
            }
        }

        public decimal Ele
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

        public decimal Speed
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
                if (0 <= _lon)
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
            return ToLonLatArray(_lon);
        }

        public string LatMark
        {
            get
            {
                if (0 <= _lat)
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
            return ToLonLatArray(_lat);
        }

        public UInt32[] EleArray()
        {
            return ToEleArray(_ele);
        }

        public UInt32[] SpeedArray()
        {
            return ToEleArray(_speed);
        }
        public int CompareTo(TrackPoint other)
        {
            return _time.CompareTo(other._time);
        }

        public int CompareTo(DateTime other)
        {
            return _time.CompareTo(other);
        }
    }
}

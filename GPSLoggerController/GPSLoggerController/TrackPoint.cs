using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyTraq
{
    class TrackPoint : IComparable<TrackPoint>, IComparable<DateTime>
    {
        private decimal _longitude;
        private decimal _latitude;
        private decimal _elevation;
        private decimal _speed;
        private DateTime _time;

        public TrackPoint(decimal lon, decimal lat, DateTime time)
        {
            _longitude = lon;
            _latitude = lat;
            _elevation = decimal.MaxValue;
            _speed = decimal.Zero;
            _time = time;
        }

        public decimal Longitude
        {
            get
            {
                return _longitude;
            }
        }

        public decimal Latitude
        {
            get
            {
                return _latitude;
            }
        }

        public decimal Elevation
        {
            get
            {
                return _elevation;
            }
            set
            {
                _elevation = value;
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

        public string LongitudeMark
        {
            get
            {
                if (0 <= _longitude)
                {
                    return "N";
                }
                else
                {
                    return "S";
                }
            }
        }

        public UInt32[] LongitudeArray()
        {
            return ToLonLatArray(_longitude);
        }

        public string LatMark
        {
            get
            {
                if (0 <= _latitude)
                {
                    return "E";
                }
                else
                {
                    return "W";
                }
            }

        }

        public UInt32[] LatitudeArray()
        {
            return ToLonLatArray(_latitude);
        }

        public UInt32[] ElevationArray()
        {
            return ToEleArray(_elevation);
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

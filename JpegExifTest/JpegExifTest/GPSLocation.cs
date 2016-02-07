using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegExifTest
{
    class GPSLocation
    {
        public enum LongitudeFlag
        {
            INVALID,
            EAST,
            WEST
        }

        public enum LatitudeFlag
        {
            INVALID,
            NORTH,
            SOUTH
        }

        private UInt32[] _longitude;
        private LongitudeFlag _lonFlag;

        private UInt32[] _latitude;
        private LatitudeFlag _latFlag;

        private const int DISPLAY_LENGTH = 6;

        public GPSLocation()
        {
            _longitude = null;
            _lonFlag = LongitudeFlag.INVALID;

            _latitude = null;
            _latFlag = LatitudeFlag.INVALID;
        }

        public GPSLocation(LongitudeFlag lonFlag, UInt32[] longitude, LatitudeFlag latFlag, UInt32[] latitude)
        {
            _lonFlag = lonFlag;
            _longitude = new UInt32[longitude.Length];
            System.Buffer.BlockCopy(longitude, 0, _longitude, 0, _longitude.Length * sizeof(UInt32));

            _latFlag = latFlag;
            _latitude = new UInt32[latitude.Length];
            System.Buffer.BlockCopy(latitude, 0, _latitude, 0, _latitude.Length * sizeof(UInt32));
        }

        public bool HasLocation()
        {
            return (0 < _longitude.Length && 0 < _latitude.Length);
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
                System.Buffer.BlockCopy(value, 0, _longitude, 0, _longitude.Length * sizeof(UInt32));
            }
        }


        public LongitudeFlag LongitudeRef
        {
            get
            {
                return _lonFlag;
            }

            set
            {
                if (LongitudeFlag.INVALID == value)
                    throw new Exception("設定できなよ");

                _lonFlag = value;
            }
        }

        private decimal ToDegree(UInt32[] source)
        {
            decimal result = source[0];
            result /= source[1];

            decimal dPar1 = 60;
            for (int index = 1; index < source.Length / 2; ++index)
            {
                decimal dWork = source[index * 2];
                dWork /= dPar1;
                dWork /= source[index * 2 + 1];

                result += dWork;
                dPar1 *= 60;
            }

            return result;
        }

        /// <summary>
        /// 経度
        /// </summary>
        public string Longitude
        {
            get
            {
                try
                {
                    decimal workLongitude = ToDegree(_longitude);

                    if (decimal.Zero != workLongitude)
                    {
                        if (LongitudeFlag.WEST == _lonFlag)
                        {
                            workLongitude = decimal.Zero - workLongitude;
                        }
                    }

                    string result = workLongitude.ToString();

                    int indexOf = result.IndexOf('.');
                    if (0 < indexOf && result.Length > (indexOf + DISPLAY_LENGTH))
                    {
                        result = result.Substring(0, indexOf + DISPLAY_LENGTH);
                    }

                    return result;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print("{0}(JPEGFileItem::Longitude)", e.Message);
                    return string.Empty;
                }
            }
        }


        public LatitudeFlag LatitudeRef
        {
            get
            {
                return _latFlag;
            }

            set
            {
                if (LatitudeFlag.INVALID == value)
                    throw new Exception("設定できなよ");

                _latFlag = value;
            }
        }

        public override string ToString()
        {
            return  string.Format("{0}, {1}", this.Longitude, this.Latitude);
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
                System.Buffer.BlockCopy(value, 0, _latitude, 0, _latitude.Length * sizeof(UInt32));
            }
        }

        /// <summary>
        /// 緯度
        /// </summary>
        public string Latitude
        {
            get
            {
                try
                {
                    decimal workLatitude = ToDegree(_latitude);
                    if (decimal.Zero != workLatitude)
                    {
                        if (LatitudeFlag.SOUTH == _latFlag)
                        {
                            workLatitude = decimal.Zero - workLatitude;
                        }
                    }

                    string result = workLatitude.ToString();

                    int index = result.IndexOf('.');
                    if (0 < index && result.Length > (index + DISPLAY_LENGTH))
                    {
                        result = result.Substring(0, index + DISPLAY_LENGTH);
                    }

                    return result;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print("{0}(JPEGFileItem::Longitude)", e.Message);
                    return string.Empty;
                }
            }
        }
    }
}

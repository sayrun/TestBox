using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JpegExifTest
{
    class DataConvertor
    {
        public enum ENDIAN
        {
            BIG,
            LITTLE
        };

        private readonly ENDIAN _endian;

        public DataConvertor( ENDIAN endian)
        {
            _endian = endian;
        }

        public ENDIAN Endian
        {
            get
            {
                return _endian;
            }
        }

        #region バイト配列への変換
        public void ToByte( UInt16 value, byte[] data, int offset)
        {
            if (ENDIAN.LITTLE == _endian)
            {
                data[offset] = (byte)(0x00ff & value);
                data[offset + 1] = (byte)(0x00ff & ( value >> 8));
            }
            else
            {
                data[offset + 1] = (byte)(0x00ff & value);
                data[offset] = (byte)(0x00ff & (value >> 8));
            }
        }
        #endregion

        #region バイト配列からの変換
        public Int16 ToInt16( byte[] data, int offset)
        {
            Int16 result = 0;
            if(ENDIAN.LITTLE == _endian)
            {
                result = (Int16)data[offset + 1];
                result <<= 8;
                result |= (Int16)data[offset];
            }
            else
            {
                result = (Int16)data[offset];
                result <<= 8;
                result |= (Int16)data[offset + 1];
            }
            return result;
        }

        public UInt16 ToUInt16(byte[] data, int offset)
        {
            UInt16 result = 0;
            if (ENDIAN.LITTLE == _endian)
            {
                result = (UInt16)data[offset + 1];
                result <<= 8;
                result |= (UInt16)data[offset];
            }
            else
            {
                result = (UInt16)data[offset];
                result <<= 8;
                result |= (UInt16)data[offset + 1];
            }
            return result;
        }

        public Int32 ToInt32(byte[] data, int offset)
        {
            Int32 result = 0;
            if (ENDIAN.LITTLE == _endian)
            {
                result = (Int32)data[offset + 3];
                result <<= 8;
                result |= (Int32)data[offset + 2];
                result <<= 8;
                result |= (Int32)data[offset + 1];
                result <<= 8;
                result |= (Int32)data[offset];
            }
            else
            {
                result = (Int32)data[offset];
                result <<= 8;
                result |= (Int32)data[offset + 1];
                result <<= 8;
                result |= (Int32)data[offset + 2];
                result <<= 8;
                result |= (Int32)data[offset + 3];
            }
            return result;
        }

        public UInt32 ToUInt32(byte[] data, int offset)
        {
            UInt32 result = 0;
            if (ENDIAN.LITTLE == _endian)
            {
                result = (UInt32)data[offset + 3];
                result <<= 8;
                result |= (UInt32)data[offset + 2];
                result <<= 8;
                result |= (UInt32)data[offset + 1];
                result <<= 8;
                result |= (UInt32)data[offset];
            }
            else
            {
                result = (UInt32)data[offset];
                result <<= 8;
                result |= (UInt32)data[offset + 1];
                result <<= 8;
                result |= (UInt32)data[offset + 2];
                result <<= 8;
                result |= (UInt32)data[offset + 3];
            }
            return result;
        }
        #endregion
    }
}

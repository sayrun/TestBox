using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyTraq
{
    internal class DataLogFixFull
    {
        public const UInt16 DATA_SIZE = 18;

        public UInt16 V;   // user velocity
        public UInt16 WN;  // GPS Week Number
        public UInt32 TOW; // GPS Time of Week
        public UInt32 X;   // X position in ECEF Coordinate
        public UInt32 Y;   // X position in ECEF Coordinate
        public UInt32 Z;   // X position in ECEF Coordinate
    }

    internal class DataLogFixCompact
    {
        public const UInt16 DATA_SIZE = 8;

        public UInt16 V;       // user velocity
        public UInt16 diffTOW; // difference between the current and last TOW
        public UInt16 diffX;   // defferance of last and current X in ECEF Coordinate
        public UInt16 diffY;   // defferance of last and current Y in ECEF Coordinate
        public UInt16 diffZ;   // defferance of last and current Z in ECEF Coordinate
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyTraq
{
    internal class GT730FLSController : IDisposable
    {
        private SkyTraqPort _skytraq;

        public GT730FLSController( string portName)
        {
            _skytraq = new SkyTraqPort(portName);

            _skytraq.Open();
        }

        #region 内部処理
        #endregion


        public void Read()
        {
            UInt16 totalSectors;
            UInt16 freeSectors;
            bool dataLogEnable;
            // ボーレートの設定をする
            setBaudRate(BaudRate.BaudRate_115200);

            // セクタ数を見る
            RequestBufferStatus(out totalSectors, out freeSectors, out dataLogEnable);

            // データが無効なら終わる
            if (!dataLogEnable) return;

            UInt16 sectors = totalSectors;
            sectors -= freeSectors;

            byte[] readLog = new byte[sectors * 4096];
            int retryCount = 0;
            for (UInt16 index = 0; index < sectors; ++index)
            {
                // 読み出しを指示
                sendReadBuffer(index, 1);

                // データを取得する
                _skytraq.ReadLogBuffer(readLog, index * 4096, 1);

                // CS取得
                UInt16 resultCS = _skytraq.ReadLogBufferCS();

                UInt16 calcCS = 0;
                using (MemoryStream ms = new MemoryStream(readLog, index * 4096, 4096))
                {
                    using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
                    {
                        while (true)
                        {
                            try
                            {
                                calcCS ^= br.ReadUInt16();
                            }
                            catch ( System.IO.EndOfStreamException)
                            {
                                break;
                            }
                        }
                    }
                }

                if(calcCS != resultCS)
                {
                    if( 3 <= retryCount)
                    {
                        throw new Exception("ほげ");
                    }
                    ++retryCount;
                }
                else
                {
                    retryCount = 0;
                }
            }

            List<TrackPoint> items = new List<TrackPoint>();
            using (BinaryReader br = new BinaryReader(new MemoryStream(readLog)))
            {
                DataLogFixFull local = null;
                while (true)
                {
                    try
                    {
                        // ECEFに変換する
                        local = ReadLocation(br, local);
                        if (null != local)
                        {
                            // longitude/latitudeに変換する
                            items.Add(ECEF2LonLat(local));
                        }
                    }
                    catch (System.IO.EndOfStreamException)
                    {
                        break;
                    }
                }

            }

            // longitude/latitudeの配列を返す
        }

        private TrackPoint ECEF2LonLat(DataLogFixFull local)
        {
            TrackPoint result = null;

            double x = (double)local.X;
            double y = (double)local.Y;
            double z = (double)local.Z;

            double lat;
            double lon;
            double alt;

            ECEF_to_LLA(x, y, z, out lat, out lon, out alt);

            DateTime dt = DateTime.FromBinary(gpstime_to_timet(local.WN, (int)local.TOW));

            result = new TrackPoint((decimal)lon, (decimal)lat, DateTime.Now);

            return result;
        }

        private void setBaudRate(BaudRate rate)
        {
            Payload p = new Payload(MessageID.Configure_Serial_Port, new byte[] { 0x00, (byte)rate, 0x02 });
            _skytraq.Write(p);

            if (RESULT.RESULT_ACK == this.waitResult(MessageID.Configure_Serial_Port))
            {
                // 成功したから、COM ポートのボーレートも変更する
                int[] ParaRate = { 4800, 9600, 19200, 38400, 57600, 115200, 230400 };
                _skytraq.BaudRate = ParaRate[(int)rate];
            }
        }

        public enum BaudRate
        {
            BaudRate_4800 = 0,
            BaudRate_9600 = 1,
            BaudRate_19200 = 2,
            BaudRate_38400 = 3,
            BaudRate_57600 = 4,
            BaudRate_115200 = 5,
            BaudRate_230400 = 6
        };


        private enum RESULT
        {
            RESULT_ACK,
            RESULT_NACK
        };

        private RESULT waitResult(MessageID id)
        {
            Payload p = null;
            while (true)
            {
                p = _skytraq.Read();

                if (p.ID == MessageID.ACK)
                {
                    if (MessageID.Configure_Serial_Port == (MessageID)p.Body[0])
                    {
                        return RESULT.RESULT_ACK;
                    }
                }
                else if (p.ID == MessageID.NACK)
                {
                    if (MessageID.Configure_Serial_Port == (MessageID)p.Body[0])
                    {
                        return RESULT.RESULT_NACK;
                    }
                }
            }
        }

        private void sendRestart()
        {
            Payload p = new Payload(MessageID.System_Restart, new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            _skytraq.Write(p);
            this.waitResult(MessageID.Configure_Serial_Port);
        }

        private void sendReadBuffer( int offsetSector, int sectorCount)
        {
            Payload p = new Payload(MessageID.Output_Status_of_the_Log_Buffer, new byte[] { 0x00, 0x00, 0x00, 0x02 });
            p.Body[0] = (byte)(0x00FF & (offsetSector >> 8));
            p.Body[1] = (byte)(0x00FF & (offsetSector >> 0));
            p.Body[2] = (byte)(0x00FF & (sectorCount >> 8));
            p.Body[3] = (byte)(0x00FF & (sectorCount >> 0));
            _skytraq.Write(p);
            this.waitResult(MessageID.Configure_Serial_Port);
        }

        private void RequestBufferStatus( out UInt16 totalSectors, out UInt16 freeSectors, out bool dataLogEnable)
        {
            Payload p = new Payload(MessageID.Request_Information_of_the_Log_Buffer_Status);
            _skytraq.Write(p);

            Payload result;
            if (RESULT.RESULT_ACK != this.waitResult(MessageID.Request_Information_of_the_Log_Buffer_Status))
            {
                throw new Exception("NACK!");
            }

            result = _skytraq.Read();
            if (result.ID != MessageID.Output_Status_of_the_Log_Buffer)
            {
                throw new Exception("Sequence error");
            }

            totalSectors = (UInt16)result.Body[8];
            totalSectors <<= 8;
            totalSectors |= (UInt16)result.Body[7];

            freeSectors = (UInt16)result.Body[6];
            freeSectors <<= 8;
            freeSectors |= (UInt16)result.Body[5];

            dataLogEnable = (0x01 == result.Body[33]);
        }

        private DataLogFixFull ReadLocation( BinaryReader br, DataLogFixFull current)
        {
            UInt16 pos1 = (byte)(0x00FF & br.ReadByte());
            pos1 <<= 8;
            pos1 |= (byte)(0x00FF & br.ReadByte());

            UInt16 velocity = pos1;
            velocity &= (UInt16)0x03ff;

            switch ((pos1 & 0xE000))
            {
                // empty
                case 0xE000:
                    return null;
                    break;

                // FIX FULL POI
                case 0x6000:
                // FIX FULL
                case 0x4000:
                    {
                        DataLogFixFull data = new DataLogFixFull();
                        data.V = velocity;

                        byte b = (byte)(0x00FF & br.ReadByte());
                        data.TOW = (byte)(0x000f & (b >> 4));
                        data.WN = (UInt16)(0x0030 & b);
                        data.WN <<= 4;
                        data.WN |= (UInt16)(0x00FF & br.ReadByte());
                        UInt32 un = (UInt32)(0x00FF & br.ReadByte());
                        un <<= 8;
                        un |= (UInt32)(0x00FF & br.ReadByte());
                        un <<= 4;
                        data.TOW |= un;

                        {
                            data.X = (UInt32)(0x00FF & br.ReadByte());
                            data.X <<= 8;
                            data.X |= (UInt32)(0x00FF & br.ReadByte());

                            un = (UInt32)(0x00FF & br.ReadByte());
                            un <<= 8;
                            un |= (UInt32)(0x00FF & br.ReadByte());

                            un <<= 16;
                            un &= 0xffff0000;
                            data.X |= un;
                        }


                        {
                            data.Y = (UInt32)(0x00FF & br.ReadByte());
                            data.Y <<= 8;
                            data.Y |= (UInt32)(0x00FF & br.ReadByte());

                            un = (UInt32)(0x00FF & br.ReadByte());
                            un <<= 8;
                            un |= (UInt32)(0x00FF & br.ReadByte());

                            un <<= 16;
                            un &= 0xffff0000;
                            data.Y |= un;

                        }

                        {
                            data.Z = (UInt32)(0x00FF & br.ReadByte());
                            data.Z <<= 8;
                            data.Z |= (UInt32)(0x00FF & br.ReadByte());

                            un = (UInt32)(0x00FF & br.ReadByte());
                            un <<= 8;
                            un |= (UInt32)(0x00FF & br.ReadByte());

                            un <<= 16;
                            un &= 0xffff0000;
                            data.Z |= un;

                        }
                        return data;
                    }
                    break;

                // FIX COMPACT
                case 0x8000:
                    {
                        DataLogFixCompact data = new DataLogFixCompact();
                        data.V = velocity;

                        data.diffTOW = (UInt16)(0x00FF & br.ReadByte());
                        data.diffTOW <<= 8;
                        data.diffTOW |= (UInt16)(0x00FF & br.ReadByte());

                        data.diffX = (UInt16)(0x00FF & br.ReadByte());
                        data.diffX <<= 2;
                        UInt16 un = (UInt16)(0x00FF & br.ReadByte());
                        data.diffX = (UInt16)(0x0003 & (un >> 6));

                        data.diffY = (UInt16)(un & 0x003f);
                        un = (UInt16)(0x00FF & br.ReadByte());
                        data.diffY |= (UInt16)(0x03C0 & (un << 6));  // 11 1100 0000

                        data.diffZ = (UInt16)(0x0003 & un);
                        data.diffZ <<= 8;
                        data.diffZ |= (UInt16)(0x00FF & br.ReadByte());

                        if (null == current)
                        {
                            return null;
                        }

                        DataLogFixFull result = new DataLogFixFull();

                        result.V = data.V;

                        result.WN = current.WN;
                        result.TOW = current.TOW + data.diffTOW;

                        result.X = current.X;
                        if (data.diffX < 512)
                        {
                            result.X += data.diffX;
                        }
                        else
                        {
                            result.X -= (UInt16)(0x01ff & (0x0000 ^ data.diffX));
                        }

                        result.Y = current.Y;
                        if (data.diffY < 512)
                        {
                            result.Y += data.diffY;
                        }
                        else
                        {
                            result.Y -= (UInt16)(0x01ff & (0x0000 ^ data.diffY));
                        }

                        result.Z = current.Z;
                        if (data.diffZ < 512)
                        {
                            result.Z += data.diffZ;
                        }
                        else if (data.diffZ > 511)
                        {
                            result.Z -= (UInt16)(0x01ff & (0x0000 ^ data.diffZ));
                        }

                        return result;
                    }
                    break;

                default:
                    throw new Exception("type error!");
            }
        }


        public void Dispose()
        {
            this.sendRestart();
            _skytraq.Dispose();
        }
    }
}

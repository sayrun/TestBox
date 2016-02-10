using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyTraq
{
    internal class GT730FLSController : IDisposable
    {
        private SerialPort _com;

        private const byte START_OF_SEQUENCE_1 = 0xa0;
        private const byte START_OF_SEQUENCE_2 = 0xa1;

        private const byte END_OF_SEQUENCE_1 = 0x0d;
        private const byte END_OF_SEQUENCE_2 = 0x0a;
        private const UInt16 MASK_LOBYTE = 0x00FF;

        public GT730FLSController(string portName)
        {
            _com = new SerialPort(portName, 38400, Parity.None, 8, StopBits.One);

            _com.Open();
        }

        #region 内部処理
        public Payload Read()
        {
            bool findHeader1 = false;
            bool findHeader2 = false;

            // header check
            byte readData;
            while (true)
            {
                readData = (byte)(0x00FF & _com.ReadByte());

                if (findHeader1)
                {
                    if (START_OF_SEQUENCE_2 == readData)
                    {
                        findHeader2 = true;
                        break;
                    }
                    findHeader1 = false;
                }
                else
                {
                    if (START_OF_SEQUENCE_1 == readData)
                    {
                        findHeader2 = false;
                        findHeader1 = true;
                    }
                }
            }

            // payload length
            UInt16 payloadLength = (UInt16)(0x00FF & _com.ReadByte());
            payloadLength <<= 8;
            payloadLength |= (UInt16)(0x00FF & _com.ReadByte());

            // read payload
            System.Threading.Thread.Sleep(10);
            byte[] rawPayload = new byte[payloadLength];
            for (int readCount = 0; readCount < payloadLength;)
            {
                readCount += _com.Read(rawPayload, readCount, payloadLength - readCount);
            }

            Payload result = new Payload(rawPayload, 1, rawPayload.Length);

            // CS
            byte checkSum = (byte)(0x00FF & _com.ReadByte());

            byte calcChecSum = 0;
            for (int index = 0; index < rawPayload.Length; ++index)
            {
                calcChecSum ^= rawPayload[index];
            }

            if (checkSum != calcChecSum)
            {
                throw new Exception("check sum error");
            }

            // footer check
            bool findFooter1 = false;
            bool findFooter2 = false;

            while (true)
            {
                readData = (byte)(0x00FF & _com.ReadByte());

                if (findFooter1)
                {
                    if (END_OF_SEQUENCE_2 == readData)
                    {
                        findFooter2 = true;
                        break;
                    }
                    findFooter1 = false;
                }
                else
                {
                    if (END_OF_SEQUENCE_1 == readData)
                    {
                        findFooter2 = false;
                        findFooter1 = true;
                    }
                }
            }

            return result;
        }

        public void Write(Payload payload)
        {
            byte[] command = convert(payload);

            _com.Write(command, 0, command.Length);
        }

        private byte[] convert(Payload payload)
        {
            // payload length
            UInt16 payloadLength = (UInt16)(payload.ByteLength);
            // 2 = sizeof([Start of Sequence])
            // 2 = sizeof([Payload Length])
            // 1 = sizeof([CS])
            // 2 = sizeof([End of Sequence])
            int size = payloadLength + 7;

            // command buffer
            byte[] result = new byte[size];

            // set [Start of Sequence]
            result[0] = START_OF_SEQUENCE_1;
            result[1] = START_OF_SEQUENCE_2;

            // set [Payload Length] 
            result[2] = (byte)(MASK_LOBYTE & (payloadLength >> 8));
            result[3] = (byte)((MASK_LOBYTE & payloadLength));

            // payload
            payload.CopyTo(result, 5, payload.Body.Length);

            // [CS]
            byte checkSum = 0x00;
            for (int index = 0; index < payload.Body.Length + 1; ++index)
            {
                checkSum ^= result[index + 4];
            }
            result[size - 3] = checkSum;

            // [End of Sequence]
            result[size - 2] = END_OF_SEQUENCE_1;
            result[size - 1] = END_OF_SEQUENCE_2;

            return result;
        }

        private static int SECTOR_SIZE = 4096;

        public void ReadLogBuffer(byte[] buffer, int offset, int sectors)
        {
            for (int readCount = 0; readCount < (SECTOR_SIZE * sectors);)
            {
                readCount += _com.Read(buffer, readCount, (SECTOR_SIZE * sectors) - readCount);
            }
        }

        private static byte[] CHECKSUM_MARKER = { 0x45, 0x4e, 0x44, 0x00, 0x43, 0x48, 0x45, 0x43, 0x4b, 0x53, 0x55, 0x4d, 0x3d };
        private static byte[] CHECKSUM_MARKER_FOOTER = { 0x0d, 0x0a };

        public UInt16 ReadLogBufferCS()
        {
            byte d;
            for (int index = 0; index < CHECKSUM_MARKER.Length; ++index)
            {
                d = (byte)_com.ReadByte();
                if (d != CHECKSUM_MARKER[index])
                {
                    index = 0;
                }
            }

            UInt16 result = (byte)_com.ReadByte();
            result <<= 8;
            result |= (byte)_com.ReadByte();

            for (int index = 0; index < CHECKSUM_MARKER_FOOTER.Length; ++index)
            {
                d = (byte)_com.ReadByte();
                if (d != CHECKSUM_MARKER_FOOTER[index])
                {
                    index = 0;
                }
            }

            return result;
        }
        #endregion

        public void ReadLatLonData()
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
                ReadLogBuffer(readLog, index * 4096, 1);

                // CS取得
                UInt16 resultCS = ReadLogBufferCS();

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
                            catch (System.IO.EndOfStreamException)
                            {
                                break;
                            }
                        }
                    }
                }

                if (calcCS != resultCS)
                {
                    if (3 <= retryCount)
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

            long time = gpstime_to_timet(local.WN, (int)local.TOW) - 315964800;
            DateTime dt = new DateTime(1980, 1, 6, 0, 0, 0);
            dt = dt.AddSeconds(time);

            result = new TrackPoint((decimal)lon, (decimal)lat, dt);

            result.Speed = (local.V * 1000);
            result.Speed /= 3600;

            return result;
        }

        private void setBaudRate(BaudRate rate)
        {
            Payload p = new Payload(MessageID.Configure_Serial_Port, new byte[] { 0x00, (byte)rate, 0x02 });
            Write(p);

            if (RESULT.RESULT_ACK == this.waitResult(MessageID.Configure_Serial_Port))
            {
                // 成功したから、COM ポートのボーレートも変更する
                int[] ParaRate = { 4800, 9600, 19200, 38400, 57600, 115200, 230400 };
                _com.BaudRate = ParaRate[(int)rate];
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
                p = Read();

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
            Write(p);
            this.waitResult(MessageID.Configure_Serial_Port);
        }

        private void sendReadBuffer(int offsetSector, int sectorCount)
        {
            Payload p = new Payload(MessageID.Output_Status_of_the_Log_Buffer, new byte[] { 0x00, 0x00, 0x00, 0x02 });
            p.Body[0] = (byte)(0x00FF & (offsetSector >> 8));
            p.Body[1] = (byte)(0x00FF & (offsetSector >> 0));
            p.Body[2] = (byte)(0x00FF & (sectorCount >> 8));
            p.Body[3] = (byte)(0x00FF & (sectorCount >> 0));
            Write(p);
            this.waitResult(MessageID.Configure_Serial_Port);
        }

        private void RequestBufferStatus(out UInt16 totalSectors, out UInt16 freeSectors, out bool dataLogEnable)
        {
            Payload p = new Payload(MessageID.Request_Information_of_the_Log_Buffer_Status);
            Write(p);

            Payload result;
            if (RESULT.RESULT_ACK != this.waitResult(MessageID.Request_Information_of_the_Log_Buffer_Status))
            {
                throw new Exception("NACK!");
            }

            result = Read();
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

        private DataLogFixFull ReadLocation(BinaryReader br, DataLogFixFull current)
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
                            data.X = (Int32)(0x00FF & br.ReadByte());
                            data.X <<= 8;
                            data.X |= (Int32)(0x00FF & br.ReadByte());

                            un = (UInt32)(0x00FF & br.ReadByte());
                            un <<= 8;
                            un |= (UInt32)(0x00FF & br.ReadByte());

                            un <<= 16;
                            un &= 0xffff0000;
                            data.X |= (Int32)un;
                        }


                        {
                            data.Y = (Int32)(0x00FF & br.ReadByte());
                            data.Y <<= 8;
                            data.Y |= (Int32)(0x00FF & br.ReadByte());

                            un = (UInt32)(0x00FF & br.ReadByte());
                            un <<= 8;
                            un |= (UInt32)(0x00FF & br.ReadByte());

                            un <<= 16;
                            un &= 0xffff0000;
                            data.Y |= (Int32)un;

                        }

                        {
                            data.Z = (Int32)(0x00FF & br.ReadByte());
                            data.Z <<= 8;
                            data.Z |= (Int32)(0x00FF & br.ReadByte());

                            un = (UInt32)(0x00FF & br.ReadByte());
                            un <<= 8;
                            un |= (UInt32)(0x00FF & br.ReadByte());

                            un <<= 16;
                            un &= 0xffff0000;
                            data.Z |= (Int32)un;

                        }
                        return data;
                    }
                    break;

                // FIX COMPACT
                case 0x8000:
                    {
                        UInt16 diffTOW = (UInt16)(0x00FF & br.ReadByte());
                        diffTOW <<= 8;
                        diffTOW |= (UInt16)(0x00FF & br.ReadByte());

                        Int16 diffX = (Int16)(0x00FF & br.ReadByte());
                        diffX <<= 2;
                        UInt16 un = (UInt16)(0x00FF & br.ReadByte());
                        diffX = (Int16)(0x0003 & (un >> 6));

                        if (0 != (diffX & 0x0200))
                        {
                            UInt16 unWork = 0xfC00;
                            diffX |= (Int16)unWork;   // 1111 1100 0000 0000
                        }

                        Int16 diffY = (Int16)(un & 0x003f);
                        un = (UInt16)(0x00FF & br.ReadByte());
                        diffY |= (Int16)(0x03C0 & (un << 6));  // 11 1100 0000

                        if (0 != (diffY & 0x0200))
                        {
                            UInt16 unWork = 0xfC00;
                            diffY |= (Int16)unWork;   // 1111 1100 0000 0000
                        }


                        Int16 diffZ = (Int16)(0x0003 & un);
                        diffZ <<= 8;
                        diffZ |= (Int16)(0x00FF & br.ReadByte());

                        if (0 != (diffZ & 0x0200))
                        {
                            UInt16 unWork = 0xfC00;
                            diffZ |= (Int16)unWork;   // 1111 1100 0000 0000
                        }

                        if (null == current)
                        {
                            return null;
                        }

                        DataLogFixFull result = new DataLogFixFull();

                        result.V = velocity;

                        result.WN = current.WN;
                        result.TOW = current.TOW + diffTOW;

                        result.X = current.X;
                        result.X += diffX;

                        result.Y = current.Y;
                        result.Y += diffY;

                        result.Z = current.Z;
                        result.Z += diffZ;

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
            if (_com.IsOpen)
            {
                _com.Close();
                _com = null;
            }
        }
    }
}

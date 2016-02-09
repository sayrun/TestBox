using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace SkyTraq
{
    internal class SkyTraqPort : IDisposable
    {
        private SerialPort _com;

        private const byte START_OF_SEQUENCE_1 = 0xa0;
        private const byte START_OF_SEQUENCE_2 = 0xa1;

        private const byte END_OF_SEQUENCE_1 = 0x0d;
        private const byte END_OF_SEQUENCE_2 = 0x0a;
        private const UInt16 MASK_LOBYTE = 0x00FF;

        public SkyTraqPort(string portName)
        {
            _com = new SerialPort(portName, 38400, Parity.None, 8, StopBits.One);
        }

        public void Open()
        {
            _com.Open();
        }

        public void Close()
        {
            if (_com.IsOpen)
            {
                _com.Close();
            }
        }

        private void fillBuffer(byte[] buffer, int offset, int length)
        {
            for( int readCount = 0; readCount < length; )
            {
                readCount += _com.Read(buffer, offset + readCount, length - readCount);
            }
        }

        public DataLogFixFull ReadLocation(DataLogFixFull current)
        {
            UInt16 pos1 = (byte)(0x00FF & _com.ReadByte());
            pos1 <<= 8;
            pos1 |= (byte)(0x00FF & _com.ReadByte());

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

                        byte b = (byte)(0x00FF & _com.ReadByte());
                        data.TOW = (byte)(0x000f & (b >> 4));
                        data.WN = (UInt16)(0x0030 & b);
                        data.WN <<= 4;
                        data.WN |=(UInt16)(0x00FF & _com.ReadByte());
                        UInt32 un = (UInt32)(0x00FF & _com.ReadByte());
                        un <<= 8;
                        un |= (UInt32)(0x00FF & _com.ReadByte());
                        un <<= 4;
                        data.TOW |= un;

                        {
                            data.X = (UInt32)(0x00FF & _com.ReadByte());
                            data.X <<= 8;
                            data.X |= (UInt32)(0x00FF & _com.ReadByte());

                            un = (UInt32)(0x00FF & _com.ReadByte());
                            un <<= 8;
                            un |= (UInt32)(0x00FF & _com.ReadByte());

                            un <<= 16;
                            un &= 0xffff0000;
                            data.X |= un;
                        }


                        {
                            data.Y = (UInt32)(0x00FF & _com.ReadByte());
                            data.Y <<= 8;
                            data.Y |= (UInt32)(0x00FF & _com.ReadByte());

                            un = (UInt32)(0x00FF & _com.ReadByte());
                            un <<= 8;
                            un |= (UInt32)(0x00FF & _com.ReadByte());

                            un <<= 16;
                            un &= 0xffff0000;
                            data.Y |= un;

                        }

                        {
                            data.Z = (UInt32)(0x00FF & _com.ReadByte());
                            data.Z <<= 8;
                            data.Z |= (UInt32)(0x00FF & _com.ReadByte());

                            un = (UInt32)(0x00FF & _com.ReadByte());
                            un <<= 8;
                            un |= (UInt32)(0x00FF & _com.ReadByte());

                            un <<= 16;
                            un &= 0xffff0000;
                            data.Z |= un;

                        }

                    }
                    break;

                // FIX COMPACT
                case 0x8000:
                    {
                        DataLogFixCompact data = new DataLogFixCompact();
                        data.V = velocity;

                        data.diffTOW = (UInt16)(0x00FF & _com.ReadByte());
                        data.diffTOW <<= 8;
                        data.diffTOW |= (UInt16)(0x00FF & _com.ReadByte());

                        data.diffX = (UInt16)(0x00FF & _com.ReadByte());
                        data.diffX <<= 2;
                        UInt16 un = (UInt16)(0x00FF & _com.ReadByte());
                        data.diffX = (UInt16)( 0x0003 & (un >> 6));

                        data.diffY = (UInt16)(un & 0x003f);
                        un = (UInt16)(0x00FF & _com.ReadByte());
                        data.diffY |= (UInt16)(0x03C0 & (un << 6));  // 11 1100 0000

                        data.diffZ = (UInt16)(0x0003 & un);
                        data.diffZ <<= 8;
                        data.diffZ |= (UInt16)(0x00FF & _com.ReadByte());
                    }
                    break;
            }


        }

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
            byte[] rawPayload = new byte[payloadLength];
            fillBuffer(rawPayload, 0, payloadLength);

            Payload result = new Payload(rawPayload, 1, rawPayload.Length);

            // CS
            byte checkSum = (byte)(0x00FF & _com.ReadByte());

            byte calcChecSum = 0;
            for(int index = 0; index < rawPayload.Length; ++index)
            {
                calcChecSum ^= rawPayload[index];
            }

            if(checkSum != calcChecSum)
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

        public int BaudRate
        {
            get
            {
                return _com.BaudRate;
            }
            set
            {
                _com.BaudRate = value;
            }
        }

        public void Write( Payload payload)
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

        public void Dispose()
        {
            if (_com.IsOpen)
            {
                _com.Close();
                _com = null;
            }
        }
    }
}

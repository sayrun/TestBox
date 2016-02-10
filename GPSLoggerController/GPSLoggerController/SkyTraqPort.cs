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

using System;
using System.Collections.Generic;
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

        public void Dispose()
        {
            this.sendRestart();
            _skytraq.Dispose();
        }
    }
}

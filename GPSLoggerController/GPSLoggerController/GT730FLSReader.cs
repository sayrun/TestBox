using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace GPSLoggerController
{
    internal class GT730FLSReader : IDisposable
    {
        private SerialPort _com;
        private List<byte> _buffer;

        private List<Payload> _payload;
        private EventWaitHandle _wait;

        private const byte START_OF_SEQUENCE_1 = 0xa0;
        private const byte START_OF_SEQUENCE_2 = 0xa1;

        private const byte END_OF_SEQUENCE_1 = 0x0d;
        private const byte END_OF_SEQUENCE_2 = 0x0a;
        private const UInt16 MASK_LOBYTE = 0x00FF;

        public GT730FLSReader(string portName)
        {
            _com = new SerialPort(portName, 38400, Parity.None, 8, StopBits.One);

            _buffer = new List<byte>();
            _payload = new List<Payload>();
            _wait = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);

            _com.DataReceived += _com_DataReceived;

            _com.Open();
        }

        private enum RESULT
        {
            RESULT_ACK,
            RESULT_NACK
        };

        private RESULT waitResult(Payload.MessageID id)
        {
            Payload p = null;
            while (true)
            {
                p = this.Read();

                if (p.ID == Payload.MessageID.ACK)
                {
                    if (Payload.MessageID.Configure_Serial_Port == (Payload.MessageID)p.Body[0])
                    {
                        return RESULT.RESULT_ACK;
                    }
                }
                else if (p.ID == Payload.MessageID.NACK)
                {
                    if (Payload.MessageID.Configure_Serial_Port == (Payload.MessageID)p.Body[0])
                    {
                        return RESULT.RESULT_NACK;
                    }
                }
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

        public void setBaudRate(BaudRate rate)
        {
            Payload p = new Payload(Payload.MessageID.Configure_Serial_Port, new byte[] { 0x00, (byte)rate, 0x02 });
            this.Write(p);

            if( RESULT.RESULT_ACK == this.waitResult(Payload.MessageID.Configure_Serial_Port))
            {
                // 成功したから、COM ポートのボーレートも変更する
                int[] ParaRate = { 4800, 9600, 19200, 38400, 57600, 115200, 230400 };
                _com.BaudRate = ParaRate[(int)rate];
            }
        }

        public void sendRestart()
        {
            Payload p = new Payload(Payload.MessageID.System_Restart, new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            this.Write(p);
            this.waitResult(Payload.MessageID.Configure_Serial_Port);
        }

        private void _com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (null != _com)
            {
                byte[] readWork = new byte[_com.BytesToRead];

                int readSize = _com.Read(readWork, 0, _com.BytesToRead);

                _buffer.AddRange(readWork);

                bool find = false;
                foreach(byte data in readWork.Reverse())
                {
                    if(END_OF_SEQUENCE_2 == data)
                    {
                        find = true;
                    }
                    else if( find && END_OF_SEQUENCE_1 == data)
                    {
                        AnalyzeData();
                        break;
                    }
                    else
                    {
                        find = false;
                    }
                }

                readWork = null;
            }
        }

        public void Test()
        {
            _buffer.AddRange(new byte[] {
                0xa0, 0xa1, 0x00, 0x02, 0x83, 0x00, 0x83, 0x0d, 0x0a,
                0xa0, 0xa1, 0x00, 0x29, 0x94, 0x2a, 0xca, 0x01, 0x00, 0xe4, 0x01, 0xfe, 0x01, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6d, 0x0d
            });

            AnalyzeData();

            //_buffer.Add(0x0a);
            AnalyzeData();

            Payload p = Read();
            Payload p2 = Read();

            Write(p2);

        }

        private void AnalyzeData()
        {
            int startIndex = 0;
            int index = 0;

            startIndex = index;
            index = _buffer.FindIndex(index, delegate (byte value) { return (0x0a == value); });
            while (index > 0)
            {
                if (0x0d == _buffer[index - 1])
                {
                    byte[] commandArea = new byte[(index + 1) - startIndex];

                    _buffer.CopyTo(startIndex, commandArea, 0, commandArea.Length);

                    if (AnalizeCommand(commandArea))
                    {
                        _buffer.RemoveRange(startIndex, commandArea.Length);
                        index = 0;
                    }
                }

                startIndex = index+1;
                index = _buffer.FindIndex(startIndex, delegate (byte value) { return (0x0a == value); });
            }
        }

        private bool AnalizeCommand(byte[] command)
        {
            try
            {
                // [Start of Sequence]
                if (START_OF_SEQUENCE_1 != command[0]) return false;
                if (START_OF_SEQUENCE_2 != command[1]) return false;

                // [End of Sequence]
                if (END_OF_SEQUENCE_1 != command[command.Length - 2]) return false;
                if (END_OF_SEQUENCE_2 != command[command.Length - 1]) return false;


                // [Payload Length]
                UInt16 payloadLength = (UInt16)command[2];
                payloadLength <<= 8;
                payloadLength |= (UInt16)command[3];

                // check length
                // 2 = sizeof([Start of Sequence])
                // 2 = sizeof([Payload Length])
                // 1 = sizeof([CS])
                // 2 = sizeof([End of Sequence])
                if (payloadLength != (command.Length - (2 + 2 + 1 + 2))) return false;

                // check sum 算出
                byte checkSum = 0;
                for( int index = 0; index < payloadLength; ++index)
                {
                    checkSum ^= command[index + 4];
                }
                // check sumが一致しないけど、コマンドフォーマットは正しいっぽい
                if (checkSum != command[payloadLength + 4]) return true;

                Payload p = new Payload(command, 4, payloadLength - 1);

                lock( _payload)
                {
                    _payload.Add(p);
                    _wait.Set();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Payload Read()
        {
            return Read(Timeout.Infinite);
        }

        public Payload Read(int millisecondsTimeout)
        {
            try
            {
                _wait.WaitOne(millisecondsTimeout);

                Payload result;
                lock (_payload)
                {
                    result = _payload[0];
                    _payload.RemoveAt(0);

                    if (0 >= _payload.Count)
                    {
                        _wait.Reset();
                    }
                }

                return result;
            }
            catch( TimeoutException e)
            {
                System.Diagnostics.Debug.Print(e.Message);
                return null;
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
            this.sendRestart();

            _com.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSLoggerController
{
    internal class Payload
    {
        public enum MessageID
        {
            // Input System Messages
            System_Restart = 0x01,
            Query_Software_version = 0x02,
            Query_Software_CRC = 0x03,
            Set_Factory_Default = 0x04,
            Configure_Serial_Port = 0x05,
            Reserved1 = 0x06,
            Reserved2 = 0x07,
            Configure_NMEA = 0x08,
            Configure_Output_Message_Format = 0x09,
            Configure_Power_Mode = 0x0C,
            Configure_position_update_rate = 0x0E,
            Query_position_update_rate = 0x10,
            Configure_Navigation_Data_Message_Interval = 0x11,
            //
            Request_Information_of_the_Log_Buffer_Status = 0x17,
            Enable_data_read_from_the_log_buffer = 0x1d,
            // Output System Messages
            Software_version = 0x80,
            Software_CRC = 0x81,
            Reserved3 = 0x82,
            ACK = 0x83,
            NACK = 0x84,
            Position_Update_rate = 0x86,
            // Output GPS Messages
            Navigation_Data_Message = 0xA8,
            GPS_Datum = 0xAE,
            GPS_DOP_Mask = 0xAF,
            GPS_WAAS_status = 0xB3,
            GPS_Position_pinning_status = 0xB4,
            GPS_Navigation_mode = 0xB5,
            GPS_Meaurement_Mode = 0xB6,
            // Output Log Status
            Output_Status_of_the_Log_Buffer = 0x94
        };

        private readonly MessageID _id;
        private readonly byte[] _body;

        public Payload(byte[] payload, int offset, int length)
        {
            _id = (MessageID)payload[offset];
            _body = new byte[length];
            System.Buffer.BlockCopy(payload, offset + 1, _body, 0, length);
        }

        public Payload(MessageID id, byte[] body)
        {
            _id = id;
            _body = new byte[body.Length];
            System.Buffer.BlockCopy(body, 0, _body, 0, _body.Length);
        }

        public Payload(MessageID id)
        {
            _id = id;
            _body = null;
        }

        public MessageID ID
        {
            get
            {
                return _id;
            }
        }

        public byte[] Body
        {
            get
            {
                return _body;
            }
        }

        public int ByteLength
        {
            get
            {
                return (null == _body) ? 1 : _body.Length + 1;
            }
        }

        public void CopyTo(byte[] buffer, int offset, int length)
        {
            int size = 1 + ((null == _body) ? 0 : _body.Length);
            byte[] result = new byte[size];

            buffer[offset] = (byte)_id;
            if (null != _body)
            {
                System.Buffer.BlockCopy(_body, 0, buffer, 1, Math.Min(length, _body.Length));
            }
        }
    }
}

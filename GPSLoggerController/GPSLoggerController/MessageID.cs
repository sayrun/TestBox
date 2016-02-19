using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyTraq
{
    internal enum MessageID
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
        Clear_Data_Logging_Buffer = 0x19,
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
}

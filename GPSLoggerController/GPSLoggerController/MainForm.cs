using SkyTraq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GPSLoggerController
{
    public partial class MainForm : Form
    {
        GT730FLSController _gt730;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            button2.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _gt730 = new GT730FLSController("COM4");

            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;

            return;
        }

        System.IO.BinaryReader _com;

        private void button2_Click(object sender, EventArgs e)
        {
            {
                byte[] work = new byte[] {
                     0x40, 0x00, 0x73, 0x5a, 0x87, 0x93, 0x00, 0xa6, 0xff, 0xc7, 0x29, 0xe9, 0x00, 0x38, 0x35, 0xdd, 0x00, 0x37
                    , 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0x80, 0x00, 0x00, 0x01
                    };

                _com = new System.IO.BinaryReader(new System.IO.MemoryStream(work));
            }

            DataLogFixFull data = ReadLocation(null);
            data = ReadLocation(data);
#if false
            Payload p = new Payload(MessageID.Request_Information_of_the_Log_Buffer_Status);

            _gt730.Write(p);
            Payload read = null;
            while(true)
            {
                read = _gt730.Read();
                if( read.ID == MessageID.ACK)
                {
                    if(MessageID.Request_Information_of_the_Log_Buffer_Status == (MessageID)p.Body[0])
                    {
                        break;
                    }
                }
            }
            while(true)
            {
                read = _gt730.Read();
                if (read.ID == MessageID.Output_Status_of_the_Log_Buffer)
                {
                    break;
                }
            }
#endif
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _gt730.Dispose();
            _gt730 = null;
        }

        private void button4_Click(object sender, EventArgs e)
        {
#if false
            _gt730.setBaudRate(GT730FLSReader.BaudRate.BaudRate_115200);
#endif
        }



        private DataLogFixFull ReadLocation(DataLogFixFull current)
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
                        data.WN |= (UInt16)(0x00FF & _com.ReadByte());
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
                        return data;
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
                        data.diffX = (UInt16)(0x0003 & (un >> 6));

                        data.diffY = (UInt16)(un & 0x003f);
                        un = (UInt16)(0x00FF & _com.ReadByte());
                        data.diffY |= (UInt16)(0x03C0 & (un << 6));  // 11 1100 0000

                        data.diffZ = (UInt16)(0x0003 & un);
                        data.diffZ <<= 8;
                        data.diffZ |= (UInt16)(0x00FF & _com.ReadByte());

                        if( null == current)
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

    }
}

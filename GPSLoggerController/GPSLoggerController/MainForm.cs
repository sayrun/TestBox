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
                    , 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
                    , 0xff, 0xff
                    };

                _com = new System.IO.BinaryReader(new System.IO.MemoryStream(work));
            }

            DataLogFixFull data = null;
            TrackPoint pt;

            try
            {
                while (true)
                {
                    data = ReadLocation(data);
                    if (null != data)
                    {
                        pt = ECEF2LonLat(data);
                        System.Diagnostics.Debug.Print(string.Format("{0}, {1}, {2}, ({3})", pt.Time.ToString(), pt.Lon, pt.Lat, pt.Speed));
                    }
                }
            }
            catch(System.IO.EndOfStreamException)
            {
                System.Diagnostics.Debug.Print("hoge");
            }
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
    }
}

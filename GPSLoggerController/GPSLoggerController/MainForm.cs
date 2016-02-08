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
        GT730FLSReader _gt730;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _gt730 = new GT730FLSReader("COM4");

            button2.Enabled = true;
            button3.Enabled = true;
            button3.Enabled = true;

            return;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Payload p = new Payload(Payload.MessageID.Request_Information_of_the_Log_Buffer_Status);

            _gt730.Write(p);
            Payload read = null;
            while(true)
            {
                read = _gt730.Read();
                if( read.ID == Payload.MessageID.ACK)
                {
                    if(Payload.MessageID.Request_Information_of_the_Log_Buffer_Status == (Payload.MessageID)p.Body[0])
                    {
                        break;
                    }
                }
            }
            while(true)
            {
                read = _gt730.Read();
                if (read.ID == Payload.MessageID.Output_Status_of_the_Log_Buffer)
                {
                    break;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _gt730.Dispose();
            _gt730 = null;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _gt730.setBaudRate(GT730FLSReader.BaudRate.BaudRate_115200);
        }
    }
}

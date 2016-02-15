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
            _gt730 = new GT730FLSController("COM6");

            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;

            return;
        }

        System.IO.BinaryReader _com;

        private void button2_Click(object sender, EventArgs e)
        {

            List<TrackPoint> items = _gt730.ReadLatLonData();

            return;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _gt730.sendRestart();

            return;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _gt730.setBaudRate(GT730FLSController.BaudRate.BaudRate_115200);

            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;

        }
    }
}

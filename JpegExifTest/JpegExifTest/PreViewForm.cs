using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JpegExifTest
{
    internal partial class PreViewForm : Form
    {
        private readonly JPEGFileItem _fileItem;

        public PreViewForm(JPEGFileItem fileItem)
        {
            InitializeComponent();

            _fileItem = fileItem;
        }

        private void PreViewForm_Load(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(_fileItem.FilePath);
            pictureBox1.Image = bmp;

            if (_fileItem.CurrentLocation.HasLocation())
            {
                currentMap.DocumentText = Properties.Resources.googlemapsHTML;
            }

            if (_fileItem.NewLocation.HasLocation())
            {
                newMap.DocumentText = Properties.Resources.googlemapsHTML;
            }
        }

        private void currentMap_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_fileItem.CurrentLocation.HasLocation())
            {
                currentMap.Url = new Uri(string.Format("javascript:movePos({0},{1});", _fileItem.CurrentLocation.Longitude, _fileItem.CurrentLocation.Latitude));
            }
        }

        private void newMap_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if(_fileItem.NewLocation.HasLocation())
            {
                newMap.Url = new Uri(string.Format("javascript:movePos({0},{1});", _fileItem.NewLocation.Longitude, _fileItem.NewLocation.Latitude));
            }
        }
    }
}

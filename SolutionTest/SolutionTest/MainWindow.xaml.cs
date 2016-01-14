using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SolutionTest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void _1stButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog op = new Microsoft.Win32.OpenFileDialog();
            op.DefaultExt = "jpg";
            op.Title = "JPEG file";
            op.Filter = "JPEG file(*.jpg;*.jpeg)|*.jpg;*.jpeg|All(*.*)|*.*";
            if( true == op.ShowDialog(this))
            {
                ReadTest(op.FileName);
            }
        }

        private void ReadTest(string fileName)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(fileName);

            string s;
            foreach( System.Drawing.Imaging.PropertyItem item in bmp.PropertyItems)
            {
                s = string.Empty;
                switch (item.Type)
                {
                    // BYTE
                    case 1:
                    case 7:
                        if (1 <= item.Len)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("0x{0:X2}", item.Value[0]);
                            for (int index = 1; index < item.Len; ++index)
                            {
                                sb.AppendFormat(",0x{0:X2}", item.Value[index]);
                            }
                            s = sb.ToString();
                        }
                        break;
                    // ASCII
                    case 2:
                        s = System.Text.Encoding.ASCII.GetString(item.Value);
                        s = s.Trim(new char[] { '\0' });
                        break;
                    // SHORT
                    case 3:
                        if (4 <= item.Len)
                        {
                            Int16[] srt = new Int16[item.Len / 2];

                            System.Buffer.BlockCopy(item.Value, 0, srt, 0, item.Len);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("{0}", srt[0]);
                            for (int index = 1; index < srt.Length; ++index)
                            {
                                sb.AppendFormat(",{0}", srt[index]);
                            }
                            s = sb.ToString();
                        }
                        break;
                    // LONG
                    case 4:
                        if (4 <= item.Len)
                        {
                            Int32[] lng = new Int32[item.Len / 4];

                            System.Buffer.BlockCopy(item.Value, 0, lng, 0, item.Len);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("{0}", lng[0]);
                            for (int index = 1; index < lng.Length; ++index)
                            {
                                sb.AppendFormat(",{0}", lng[index]);
                            }
                            s = sb.ToString();
                        }
                        break;
                    // Real
                    case 5:
                        if(4 <= item.Len)
                        {
                            UInt32[] lng = new UInt32[item.Len / 4];

                            System.Buffer.BlockCopy(item.Value, 0, lng, 0, item.Len);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("{0}/{1}", lng[0], lng[1]);
                            for (int index = 1; index < lng.Length / 2; ++index)
                            {
                                sb.AppendFormat(",{0}/{1}", lng[index * 2], lng[index * 2 + 1]);
                            }
                            s = sb.ToString();
                        }
                        break;
                    // signed Real
                    case 10:
                        if (4 <= item.Len)
                        {
                            Int32[] lng = new Int32[item.Len / 4];

                            System.Buffer.BlockCopy(item.Value, 0, lng, 0, item.Len);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("{0}/{1}", lng[0], lng[1]);
                            for (int index = 1; index < lng.Length / 2; ++index)
                            {
                                sb.AppendFormat(",{0}/{1}", lng[index * 2], lng[index * 2 + 1]);
                            }
                            s = sb.ToString();
                        }
                        break;
                    default:
                        s = item.Value.ToString();
                        break;
                }
                System.Diagnostics.Debug.Print("0x{0:X4}({0}):[{2}]({3}) - {1}", item.Id, s, item.Type, item.Len);
                System.Diagnostics.Debug.Flush();
            }
        }
    }
}

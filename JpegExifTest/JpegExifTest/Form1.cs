using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JpegExifTest
{
    public partial class Form1 : Form
    {
        private List<GPXFileItem> _gpxFile;
        private List<JPEGFileItem> _jpgFile;

        public Form1()
        {
            InitializeComponent();

            _gpxFile = new List<GPXFileItem>();
            _jpgFile = new List<JPEGFileItem>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openJpegFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                foreach (string fileName in openJpegFileDialog.FileNames)
                {
                    System.Diagnostics.Debug.Print("◆◆◆◆ {0}", fileName);
                    TestFunc3(fileName);
                    LoadJpeg(fileName);
                }

                
                //LoadJpeg(openFileDialog1.FileName);
            }
        }

        private void LoadJpeg( string filePath)
        {
            JPEGFileItem item = new JPEGFileItem(filePath);

            _jpgFile.Add(item);

            ListViewItem listItem = new ListViewItem( System.IO.Path.GetFileName( item.FilePath));
            listItem.SubItems.Add(item.DateTimeOriginal.ToString());
            listItem.SubItems.Add(item.GPSPosition);
            listView2.Items.Add(listItem);
        }

        private void TestFunc3(string fileName)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(fileName);

            long lLen = fi.Length;

            using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                byte[] readBuff = new byte[lLen];
                fs.Read(readBuff, 0, (int)lLen);

                // JPEG SOI確認
                if (0xFF != readBuff[0] || 0xD8 != readBuff[1])
                {
                    throw new Exception("ファイルフォーマットが違う(JPEG SOI)");
                }
                // JPEG EOI確認
                if( 0xFF != readBuff[lLen - 2] || 0xD9 != readBuff[lLen - 1])
                {
                    throw new Exception("ファイルフォーマットが違う(JPEG EOI)");
                }

                for ( int index = 2; index < (int)lLen; ++index)
                {
                    // Marker
                    byte marker1 = readBuff[index];
                    byte marker2 = readBuff[++index];

                    if (0xff == marker1 && 0xD9 == marker2) break;
                    if( 0xff == marker1 && 0xE1 == marker2)
                    {
                        // APP1 size
                        byte size1 = readBuff[index + 1];
                        byte size2 = readBuff[index + 2];
                        UInt16 app1Size = (UInt16)size1;
                        app1Size <<= 8;
                        app1Size |= (UInt16)size2;

                        byte[] app1Buffer = new byte[app1Size];

                        System.Buffer.BlockCopy(readBuff, index + 3, app1Buffer, 0, app1Size);

                        TestFunc4(app1Buffer);
                    }

                    // Offset get
                    byte offset1 = readBuff[index + 1];
                    byte offset2 = readBuff[index + 2];
                    UInt16 offset = (UInt16)offset1;
                    offset <<= 8;
                    offset |= (UInt16)offset2;
                    index += offset;
                }
            }
        }

        private delegate void TagAnalyze(UInt16 tagID, UInt16 tagType, UInt32 tagLen, byte[] tagValue);

        private void TestFunc4(byte[] app1Buffer)
        {
            if ('E' != app1Buffer[0]) return;
            if ('x' != app1Buffer[1]) return;
            if ('i' != app1Buffer[2]) return;
            if ('f' != app1Buffer[3]) return;
            if (0x00 != app1Buffer[4]) return;
            if (0x00 != app1Buffer[5]) return;

            DataConvertor convertor;
            if ( 0x4D == app1Buffer[6] && 0x4D == app1Buffer[7])
            {
                convertor = new DataConvertor(DataConvertor.ENDIAN.BIG);

                // TIFF識別コード
                if (0x00 != app1Buffer[8]) return;
                if (0x2A != app1Buffer[9]) return;

            }
            else if(0x49 == app1Buffer[6] && 0x49 == app1Buffer[7])
            {
                convertor = new DataConvertor(DataConvertor.ENDIAN.LITTLE);

                // TIFF識別コード
                if (0x00 != app1Buffer[9]) return;
                if (0x2A != app1Buffer[8]) return;
            }
            else
            {
                throw new Exception("Endian不明");
            }


            UInt32 offset = convertor.ToUInt32(app1Buffer, 10);

            TagAnalyze funcGPSInfo = delegate (UInt16 tagID, UInt16 tagType, UInt32 tagLen, byte[] tagValue)
            {
                GPSInfoCore(tagID, tagType, tagLen, tagValue, app1Buffer, convertor);
            };

            TagAnalyze func = delegate(UInt16 tagID, UInt16 tagType, UInt32 tagLen, byte[] tagValue) {
                switch (tagID)
                {
                    // EXIF (0x8769)
                    case 0x8769:
                        {
                            //TestFunc5(app1Buffer, tagValue, convertor, funcGPSInfo);
                        }
                        break;

                    // GPS Info(0x8825)
                    case 0x8825:
                        {
                            UInt32 gpsInfoOffset = convertor.ToUInt32(tagValue, 0);
                            TestFunc5(app1Buffer, gpsInfoOffset, convertor, funcGPSInfo);
                        }
                        break;
                    default:
                        //GPSInfoCore(tagID, tagType, tagLen, tagValue, app1Buffer, convertor);
                        break;
                }
            };

            TestFunc5( app1Buffer, offset, convertor, func);

        }

        private void TestFunc5(byte[] app1Buffer, UInt32 offset, DataConvertor convertor, TagAnalyze analyze)
        {
            for (int index = ((int)offset) + 6; index < app1Buffer.Length; ++index)
            {
                // タグの数
                UInt16 tagCount = convertor.ToUInt16(app1Buffer, index);
                index += 2;

                // タグ領域
                for (int lndex = 0; lndex < tagCount; ++lndex)
                {
                    // TAG ID
                    UInt16 tagID = convertor.ToUInt16(app1Buffer, index);

                    // TAG型
                    UInt16 tagType = convertor.ToUInt16(app1Buffer, index + 2);

                    // TAGデータ長
                    UInt32 tagLen = convertor.ToUInt32(app1Buffer, index + 4);

                    // TAG値
                    byte[] tagValue = new byte[4];
                    System.Buffer.BlockCopy(app1Buffer, index + 8, tagValue, 0, tagValue.Length);

                    index += 12;

                    // delegate
                    analyze(tagID, tagType, tagLen, tagValue);
                }

                // 次のIFDへのポインタ
                UInt32 nextPointer = convertor.ToUInt32( app1Buffer, index);
                index += 4;

                if (0 == nextPointer)
                {
                    break;
                }

                index = (((int)nextPointer) - 1) + 6;
            }
        }

        private void GPSInfoCore(UInt16 tagID, UInt16 tagType, UInt32 tagLen, byte[] tagValue, byte[] app1Buffer, DataConvertor convertor)
        {
            switch (tagType)
            {
                // 1:BYTE(1byte)8ビット符合なし整数
                case 1:
                    {
                        StringBuilder sb = new StringBuilder();
                        if (tagLen <= 4)
                        {
                            sb.AppendFormat("{0}", tagValue[0]);
                            for (int jndex = 1; jndex < tagLen; ++jndex)
                            {
                                sb.AppendFormat(",{0}", tagValue[jndex]);
                            }
                        }
                        else
                        {
                            int line = 1;
                            int offsetvalue = (int)(convertor.ToUInt32(tagValue, 0)) + 6;
                            sb.AppendFormat("0x{0:X2}", app1Buffer[offsetvalue + 8]);
                            for (int jndex = 1; jndex < tagLen; ++jndex)
                            {
                                sb.AppendFormat(",0x{0:X2}", app1Buffer[offsetvalue + 8 + jndex]);

                                line++;
                                if (16 <= line)
                                {
                                    line = 0;
                                    sb.Append("\r\n");
                                }
                            }

                        }
                        System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                    }
                    break;
                // 2:ASCII(?byte)ASCII文字の集合'0.H'で終端
                case 2:
                    {
                        string value = string.Empty;
                        if (tagLen <= 4)
                        {
                            for (int jndex = 0; jndex < tagLen; ++jndex)
                            {
                                value += (char)tagValue[jndex];
                            }
                        }
                        else
                        {
                            int offsetvalue = (int)(convertor.ToUInt32(tagValue, 0)) + 6;
                            for (int jndex = 0; jndex < tagLen; ++jndex)
                            {
                                value += (char)app1Buffer[offsetvalue + jndex];
                            }
                        }
                        value = value.TrimEnd(new char[] { '\0' });
                        System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, value);
                    }
                    break;
                // 3:SHORT(2byte)16ビット符合なし整数
                case 3:
                    {
                        UInt16[] values = new UInt16[tagLen];
                        if ((tagLen * 2) <= 4)
                        {
                            for (int jndex = 0; jndex < tagLen; ++jndex)
                            {
                                values[jndex] = convertor.ToUInt16(tagValue, (2 * jndex));
                            }
                        }
                        else
                        {
                            int offsetvalue = (int)(convertor.ToUInt32(tagValue, 0)) + 6;
                            for (int jndex = 0; jndex < tagLen; ++jndex)
                            {
                                values[jndex] = convertor.ToUInt16(app1Buffer, offsetvalue + (2 * jndex));
                            }
                        }
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("{0}", values[0]);
                        for (int jndex = 1; jndex < values.Length; ++jndex)
                        {
                            sb.AppendFormat(",{0}", values[jndex]);
                        }
                        System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                    }
                    break;
                // 4:LONG(4byte)32ビット符合なし整数
                case 4:
                    {
                        UInt32[] values = new UInt32[tagLen];
                        if ((tagLen * 4) <= 4)
                        {
                            values[0] = convertor.ToUInt32(tagValue, 0);
                        }
                        else
                        {
                            int offsetvalue = (int)(convertor.ToUInt32(tagValue, 0)) + 6;
                            for (int jndex = 0; jndex < tagLen; ++jndex)
                            {
                                values[jndex] = convertor.ToUInt32(app1Buffer, offsetvalue + (2 * jndex));
                            }
                        }
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("0x{0:X4}", values[0]);
                        for (int jndex = 1; jndex < values.Length; ++jndex)
                        {
                            sb.AppendFormat(",0x{0:X4}", values[jndex]);
                        }
                        System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                    }
                    break;
                // 5:RATIONAL(8byte)LONG２つ、分子／分母
                case 5:
                    {
                        int offsetvalue = (int)(convertor.ToUInt32(tagValue, 0)) + 6;

                        UInt32[] values = new UInt32[tagLen * 2];
                        for (int jndex = 0; jndex < values.Length; ++jndex)
                        {
                            values[jndex] = convertor.ToUInt32(app1Buffer, offsetvalue + (jndex * 4));
                        }
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("{0}/{1}", values[0], values[1]);
                        for (int jndex = 2; jndex < values.Length; jndex += 2)
                        {
                            sb.AppendFormat(",{0}/{1}", values[jndex], values[jndex + 1]);
                        }
                        System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                    }
                    break;
                // 7:UNDEFINED(?byte)未定義のバイト列
                case 7:
                    {
                        StringBuilder sb = new StringBuilder();
                        if (tagLen <= 4)
                        {
                            sb.AppendFormat("{0}", tagValue[0]);
                            for (int jndex = 1; jndex < tagLen; ++jndex)
                            {
                                sb.AppendFormat(",{0}", tagValue[ jndex]);
                            }
                        }
                        else
                        {
                            int line = 1;
                            int offsetvalue = (int)(convertor.ToUInt32(tagValue, 0)) + 6;
                            sb.AppendFormat("0x{0:X2}", app1Buffer[offsetvalue + 8]);
                            for (int jndex = 1; jndex < tagLen; ++jndex)
                            {
                                sb.AppendFormat(",0x{0:X2}", app1Buffer[offsetvalue + 8 + jndex]);

                                line++;
                                if (16 <= line)
                                {
                                    line = 0;
                                    sb.Append("\r\n");
                                }
                            }

                        }
                        System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                    }
                    break;
                // 9:SLONG(4byte)32ビット符合あり整数
                case 9:
                    {
                        Int32[] values = new Int32[tagLen];
                        if ((tagLen * 4) <= 4)
                        {
                            values[0] = convertor.ToInt32(tagValue, 0);
                        }
                        else
                        {
                            int offsetvalue = (int)(convertor.ToUInt32(tagValue, 0)) + 6;
                            for (int jndex = 0; jndex < tagLen; ++jndex)
                            {
                                values[jndex] = convertor.ToInt32(app1Buffer, offsetvalue + (2 * jndex));
                            }
                        }
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("0x{0:X4}", values[0]);
                        for (int jndex = 1; jndex < values.Length; ++jndex)
                        {
                            sb.AppendFormat(",0x{0:X4}", values[jndex]);
                        }
                        System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                    }
                    break;
                // 10:SRATIONAL(8byte)SLONG２つ、分子／分母
                case 10:
                    {
                        int offsetvalue = (int)(convertor.ToUInt32(tagValue, 0)) + 6;

                        Int32[] values = new Int32[tagLen * 2];
                        for (int jndex = 0; jndex < values.Length; ++jndex)
                        {
                            values[jndex] = convertor.ToInt32(app1Buffer, offsetvalue + (jndex * 4));
                        }
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("{0}/{1}", values[0], values[1]);
                        for (int jndex = 2; jndex < values.Length; jndex += 2)
                        {
                            sb.AppendFormat(",{0}/{1}", values[jndex], values[jndex + 1]);
                        }
                        System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                    }
                    break;
                default:
                    {
                        StringBuilder sb = new StringBuilder();
                        if (tagLen <= 4)
                        {
                            sb.AppendFormat("{0}", tagValue[0]);
                            for (int jndex = 1; jndex < tagLen; ++jndex)
                            {
                                sb.AppendFormat(",{0}", tagValue[ jndex]);
                            }
                        }
                        else
                        {
                            int offsetvalue = (int)(convertor.ToUInt32(tagValue, 0)) + 6;
                            sb.AppendFormat("{0}", app1Buffer[offsetvalue + 8]);
                            for (int jndex = 1; jndex < tagLen; ++jndex)
                            {
                                sb.AppendFormat(",{0}", app1Buffer[offsetvalue + 8 + jndex]);
                            }

                        }
                        System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                    }
                    break;
            }
        }

        private void TestFuncGPSInfo( byte[] app1Buffer, UInt32 offset, DataConvertor convertor)
        {
            for (int index = ((int)offset) + 6; index < app1Buffer.Length; ++index)
            {
                // タグの数
                UInt16 tagCount = convertor.ToUInt16(app1Buffer, index);
                index += 2;

                // タグ領域
                byte[] tagBuffer = new byte[12];
                for (int lndex = 0; lndex < tagCount; ++lndex)
                {
                    System.Buffer.BlockCopy(app1Buffer, index, tagBuffer, 0, tagBuffer.Length);
                    index += 12;

                    UInt16 tagID = convertor.ToUInt16(tagBuffer, 0);

                    UInt16 tagType = convertor.ToUInt16(tagBuffer, 2);

                    UInt32 tagLen = convertor.ToUInt32(tagBuffer, 4);

                    UInt32 tagValue = convertor.ToUInt32(tagBuffer, 8);

                    switch (tagType)
                    {
                        // 1:BYTE(1byte)8ビット符合なし整数
                        // 2:ASCII(?byte)ASCII文字の集合'0.H'で終端
                        case 2:
                            {
                                string value = string.Empty;
                                if (tagLen <= 4)
                                {
                                    for (int jndex = 0; jndex < tagLen; ++jndex)
                                    {
                                        value += (char)tagBuffer[8 + jndex];
                                    }
                                }
                                else
                                {
                                    int offsetvalue = (int)tagValue + 6;
                                    for (int jndex = 0; jndex < tagLen; ++jndex)
                                    {
                                        value += (char)app1Buffer[offsetvalue + jndex];
                                    }
                                }
                                value = value.TrimEnd(new char[] { '\0' });
                                System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, value);
                            }
                            break;
                        // 3:SHORT(2byte)16ビット符合なし整数
                        case 3:
                            {
                                UInt16[] values = new UInt16[tagLen];
                                if ((tagLen * 2) <= 4)
                                {
                                    for (int jndex = 0; jndex < tagLen; ++jndex)
                                    {
                                        values[jndex] = convertor.ToUInt16(tagBuffer, 8 + (2 * jndex));
                                    }
                                }
                                else
                                {
                                    int offsetvalue = (int)tagValue + 6;
                                    for (int jndex = 0; jndex < tagLen; ++jndex)
                                    {
                                        values[jndex] = convertor.ToUInt16(app1Buffer, offsetvalue + (2 * jndex));
                                    }
                                }
                                StringBuilder sb = new StringBuilder();
                                sb.AppendFormat("{0}", values[0]);
                                for (int jndex = 1; jndex < values.Length; ++jndex)
                                {
                                    sb.AppendFormat(",{0}", values[jndex]);
                                }
                                System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                            }
                            break;
                        // 4:LONG(4byte)32ビット符合なし整数
                        case 4:
                            {
                                UInt32[] values = new UInt32[tagLen];
                                if ( (tagLen * 4) <= 4)
                                {
                                    values[0] = convertor.ToUInt32(tagBuffer, 8);
                                }
                                else
                                {
                                    int offsetvalue = (int)tagValue + 6;
                                    for (int jndex = 0; jndex < tagLen; ++jndex)
                                    {
                                        values[jndex] = convertor.ToUInt32(app1Buffer, offsetvalue + (2 * jndex));
                                    }
                                }
                                StringBuilder sb = new StringBuilder();
                                sb.AppendFormat("0x{0:X4}", values[0]);
                                for (int jndex = 1; jndex < values.Length; ++jndex)
                                {
                                    sb.AppendFormat(",0x{0:X4}", values[jndex]);
                                }
                                System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                            }
                            break;
                        // 5:RATIONAL(8byte)LONG２つ、分子／分母
                        case 5:
                            {
                                int offsetvalue = (int)tagValue + 6;

                                UInt32[] values = new UInt32[tagLen * 2];
                                for (int jndex = 0; jndex < values.Length; ++jndex)
                                {
                                    values[jndex] = convertor.ToUInt32(app1Buffer, offsetvalue + (jndex * 4));
                                }
                                StringBuilder sb = new StringBuilder();
                                sb.AppendFormat("{0}/{1}", values[0], values[1]);
                                for (int jndex = 2; jndex < values.Length; jndex += 2)
                                {
                                    sb.AppendFormat(",{0}/{1}", values[jndex], values[jndex + 1]);
                                }
                                System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                            }
                            break;
                        // 7:UNDEFINED(?byte)未定義のバイト列
                        case 7:
                            {
                                StringBuilder sb = new StringBuilder();
                                if (tagLen <= 4)
                                {
                                    sb.AppendFormat("{0}", tagBuffer[8]);
                                    for (int jndex = 1; jndex < tagLen; ++jndex)
                                    {
                                        sb.AppendFormat(",{0}", tagBuffer[8 + jndex]);
                                    }
                                }
                                else
                                {
                                    int line = 1;
                                    int offsetvalue = (int)tagValue + 6;
                                    sb.AppendFormat("0x{0:X2}", app1Buffer[offsetvalue + 8]);
                                    for (int jndex = 1; jndex < tagLen; ++jndex)
                                    {
                                        sb.AppendFormat(",0x{0:X2}", app1Buffer[offsetvalue + 8 + jndex]);

                                        line++;
                                        if( 16 <= line)
                                        {
                                            line = 0;
                                            sb.Append("\r\n");
                                        }
                                    }

                                }
                                System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                            }
                            break;
                        // 9:SLONG(4byte)32ビット符合あり整数
                        case 9:
                            {
                                Int32[] values = new Int32[tagLen];
                                if ((tagLen * 4) <= 4)
                                {
                                    values[0] = convertor.ToInt32(tagBuffer, 8);
                                }
                                else
                                {
                                    int offsetvalue = (int)tagValue + 6;
                                    for (int jndex = 0; jndex < tagLen; ++jndex)
                                    {
                                        values[jndex] = convertor.ToInt32(app1Buffer, offsetvalue + (2 * jndex));
                                    }
                                }
                                StringBuilder sb = new StringBuilder();
                                sb.AppendFormat("0x{0:X4}", values[0]);
                                for (int jndex = 1; jndex < values.Length; ++jndex)
                                {
                                    sb.AppendFormat(",0x{0:X4}", values[jndex]);
                                }
                                System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                            }
                            break;
                        // 10:SRATIONAL(8byte)SLONG２つ、分子／分母
                        case 10:
                            {
                                int offsetvalue = (int)tagValue + 6;

                                Int32[] values = new Int32[tagLen * 2];
                                for (int jndex = 0; jndex < values.Length; ++jndex)
                                {
                                    values[jndex] = convertor.ToInt32(app1Buffer, offsetvalue + (jndex * 4));
                                }
                                StringBuilder sb = new StringBuilder();
                                sb.AppendFormat("{0}/{1}", values[0], values[1]);
                                for (int jndex = 2; jndex < values.Length; jndex += 2)
                                {
                                    sb.AppendFormat(",{0}/{1}", values[jndex], values[jndex + 1]);
                                }
                                System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                            }
                            break;
                        default:
                            {
                                StringBuilder sb = new StringBuilder();
                                if( tagLen <= 4)
                                {
                                    sb.AppendFormat("{0}", tagBuffer[8]);
                                    for (int jndex = 1; jndex < tagLen; ++jndex)
                                    {
                                        sb.AppendFormat(",{0}", tagBuffer[8 + jndex]);
                                    }
                                }
                                else
                                {
                                    int offsetvalue = (int)tagValue + 6;
                                    sb.AppendFormat("{0}", app1Buffer[offsetvalue + 8]);
                                    for (int jndex = 1; jndex < tagLen; ++jndex)
                                    {
                                        sb.AppendFormat(",{0}", app1Buffer[offsetvalue + 8 + jndex]);
                                    }

                                }
                                System.Diagnostics.Debug.Print("0x{0:X4}(0x{1:X4}) - 0x{2:X8}/{3}", tagID, tagType, tagLen, sb.ToString());
                            }
                            break;
                    }


                }

                // 次のIFDへのポインタ
                UInt32 nextPointer = convertor.ToUInt32(app1Buffer, index);
                index += 4;

                if (0 == nextPointer)
                {
                    break;
                }

                index = (((int)nextPointer) - 1) + 6;
            }
        }

        private void TestFunc1(string fileName)
        {
            Bitmap bmp = new Bitmap(fileName);

            foreach (System.Drawing.Imaging.PropertyItem p in bmp.PropertyItems)
            {
                // 0x9003 - 原画像データの生成日時
                // 0x9004 - デジタルデータの作成日時

                // 0x0001-北緯(N) or 南緯(S)(Ascii)
                // 0x0002-緯度(数値)(Rational)
                // 0x0003-東経(E) or 西経(W)(Ascii)
                // 0x0004-経度(数値)(Rational)
                // 0x0005-高度の基準(Byte)
                // 0x0006-高度(数値)(Rational)
                // 0x0007-GPS時間(原子時計の時間)(Rational)
                // 0x0008-測位に使った衛星信号(Ascii)
                // 0x0009-GPS受信機の状態(Ascii)
                // 0x000A-GPSの測位方法(Ascii)
                // 0x000B-測位の信頼性(Rational)
                // 0x000C-速度の単位(Ascii)
                // 0x000D-速度(数値)(Rational)


                if (p.Type == 2 && p.Len > 0)
                {
                    string s = System.Text.Encoding.ASCII.GetString(p.Value);
                    s = s.Trim(new char[] { '\0' });
                    System.Diagnostics.Debug.Print("{0} - [{1}][{2}]", p.Id, s, p.Type);
                }
                else
                {
                    System.Diagnostics.Debug.Print("{0} - [{1}][{2}]", p.Id, p.Value, p.Type);
                }
            }
            System.Drawing.Imaging.PropertyItem p1 = bmp.PropertyItems[0];
            {
                // 緯度
                p1.Id = 1;
                p1.Type = 2;
                p1.Value = ConvertTo("N");
                p1.Len = p1.Value.Length;
                bmp.SetPropertyItem(p1);

                p1.Id = 2;
                p1.Type = 5;
                p1.Value = ConvertTo(new UInt32[] { 38, 1, 2, 1, 37, 10 });
                p1.Len = p1.Value.Length;
                bmp.SetPropertyItem(p1);

                // 経度
                p1.Id = 3;
                p1.Type = 2;
                p1.Value = ConvertTo("E");
                p1.Len = p1.Value.Length;
                bmp.SetPropertyItem(p1);

                p1.Id = 4;
                p1.Type = 5;
                p1.Value = ConvertTo(new UInt32[] { 140, 1, 44, 1, 175, 10 });
                p1.Len = p1.Value.Length;
                bmp.SetPropertyItem(p1);

                // 高度
                p1.Id = 5;
                p1.Type = 7;
                p1.Value = new byte[] { 0x00};
                p1.Len = p1.Value.Length;
                bmp.SetPropertyItem(p1);

                p1.Id = 6;
                p1.Type = 5;
                p1.Value = ConvertTo(new UInt32[] { 40, 1 });
                p1.Len = p1.Value.Length;

                bmp.SetPropertyItem(p1);
            }


            String newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fileName), "HOGE.jpg");

            bmp.Save(newPath);
        }


        private byte[] ConvertTo(UInt32[] param1)
        {
            byte[] result = new byte[sizeof(UInt32) * param1.Length];
            System.Buffer.BlockCopy(param1, 0, result, 0, result.Length);

            return result;
        }

        private byte[] ConvertTo(string param1)
        {
            char[] value = param1.ToCharArray();
            byte[] result = new byte[value.Length + 1];
            System.Buffer.BlockCopy(value, 0, result, 0, value.Length);
            result[value.Length] = 0x00;

            return result;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if( DialogResult.OK == openFileDialog2.ShowDialog(this))
            {
                LoadGpxFile(openFileDialog2.FileName);
            }
        }

        private void LoadGpxFile(string fileName)
        {
            GPXFileItem gpxItem = new GPXFileItem(fileName);

            ListViewItem item = new ListViewItem(gpxItem.StartTime.ToString());
            item.SubItems.Add(gpxItem.EndTime.ToString());
            item.SubItems.Add(gpxItem.PointCount.ToString());
            item.SubItems.Add(gpxItem.FileName);
            item.ToolTipText = gpxItem.FilePath;
            item.Tag = gpxItem;

            _gpxFile.Add(gpxItem);

            listView1.Items.Add(item);

        }

        private TrackPointItem GetLocData( string trkptXml)
        {
            string lon = string.Empty;
            string lat = string.Empty;
            string sEle = string.Empty;
            string sTime = string.Empty;
            string sSpeed = string.Empty;

            using (System.Xml.XmlReader xr = System.Xml.XmlReader.Create( new System.IO.StringReader( trkptXml)))
            {
                while (xr.Read())
                {
                    switch (xr.NodeType)
                    {
                        case System.Xml.XmlNodeType.Element:
                            if (0 == string.Compare(xr.Name, "ele", true))
                            {
                                sEle = xr.ReadString();
                            }
                            else if (0 == string.Compare(xr.Name, "time", true))
                            {
                                sTime = xr.ReadString();
                            }
                            else if (0 == string.Compare(xr.Name, "speed", true))
                            {
                                sSpeed = xr.ReadString();
                            }
                            else if( 0 == string.Compare( xr.Name, "trkpt", true))
                            {
                                lon = xr.GetAttribute("lon");
                                lat = xr.GetAttribute("lat");

                            }
                            break;
                    }
                }
            }

            TrackPointItem trkPt = new TrackPointItem(lon, lat, DateTime.Parse(sTime));
            trkPt.Speed = sSpeed;
            trkPt.Ele = sEle;

            return trkPt;
        }

        private void TestFunc2( string filePath)
        {
            List<TrackPointItem> trkPtList = new List<TrackPointItem>();

            DateTime target = DateTime.Parse("2010/04/25 12:21:09");

            using (System.Xml.XmlReader xr = System.Xml.XmlReader.Create(new System.IO.StreamReader(filePath)))
            {
                while( xr.Read())
                {
                    switch (xr.NodeType)
                    {
                        case System.Xml.XmlNodeType.Element:
                            System.Diagnostics.Debug.Print(xr.Name);
                            if ( 0== string.Compare( xr.Name, "trkpt", true))
                            {
                                string sxml = xr.ReadOuterXml();

                                string lon = xr.GetAttribute("lon");
                                string lat = xr.GetAttribute("lat");

                                TrackPointItem item = GetLocData(sxml);

                                trkPtList.Add(item);

                                if (0 == item.CompareTo(target))
                                {
                                    UInt32[] re = item.LonArray();

                                    UInt32[] el = item.EleArray();

                                    UInt32[] sp = item.SpeedArray();

                                    string s = string.Format("http://maps.google.com/maps?q={0},{1}", item.Lat, item.Lon);
                                    System.Diagnostics.Process.Start(s);

                                    System.Diagnostics.Debug.Print("");
                                }
                            }
                            break;

                        case System.Xml.XmlNodeType.Text:
                            System.Diagnostics.Debug.Print(xr.Value);
                            break;

                        default:
                            System.Diagnostics.Debug.Print(xr.NodeType.ToString());
                            break;
                    }
                }
            }

            trkPtList.Sort();

            return;
        }
    }
}

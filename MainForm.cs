using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Imaging;
using LibTakamin.Drawing.Imaging;

namespace ImageMetaData {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }


        private void MainForm_DragEnter(object sender, DragEventArgs e) {
            string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filenames.Length == 1 && LibTakamin.FileUtil.GetSize(filenames[0]) >= 0) {
                e.Effect = DragDropEffects.All;
            }

        }


        private void MainForm_DragDrop(object sender, DragEventArgs e) {
            string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filenames.Length == 1 && LibTakamin.FileUtil.GetSize(filenames[0]) >= 0) {
                e.Effect = DragDropEffects.All;
                ImagePropertyItemWriter meta = new ImagePropertyItemWriter();
                try {
                    PropertyItem[] propertyItems = meta.Attach(filenames[0]).PropertyItem;
                    labelFilename.Text = filenames[0];
                    pictureBox.Image = Image.FromFile(filenames[0]);

                    StringBuilder sb = new StringBuilder();
                    int i = 0;
                    foreach (PropertyItem pi in propertyItems) {
                        sb.Append("METADATA[").Append(i.ToString()).Append("] ")
                        .Append("Id: 0x").Append(pi.Id.ToString("X4"))
                        .Append(", Type: 0x").Append(pi.Type.ToString("X02"))
                        .Append(", Len: ").Append(pi.Type.ToString()).Append(" bytes\r\n")
                        .Append("Value:");
                        switch (pi.Type) {
                        case (short)PropertyTagType.PropertyTagTypeASCII: {
                                char[] ca = new char[pi.Value.Length - 1];
                                for (int j = 0; j < pi.Value.Length - 1; j++) {
                                    ca[j] = (char)pi.Value[j];
                                }
                                string strValue = new String(ca);
                                sb.Append("'").Append(strValue).Append("'");
                            }
                            break;
                        case (short)PropertyTagType.PropertyTagTypeLong: {
                                int sizeOfElement = sizeof(uint);

                                if (pi.Len > sizeOfElement) { sb.Append("["); }
                                uint n = 0;
                                for (int j = 0; j < pi.Len; j++) {
                                    int octidx = j % sizeOfElement;
                                    if (octidx == 0) {
                                        n = 0;
                                    }
                                    n |= ((uint)pi.Value[j] << (octidx * 8));
                                    if (octidx >= sizeOfElement - 1 || j >= pi.Len - 1) {
                                        sb.Append(n.ToString())
                                            .Append("(0x").Append(n.ToString("X4")).Append(")");
                                    }
                                    if (pi.Len > sizeOfElement) { sb.Append(","); }
                                }
                                if (pi.Len > sizeOfElement) { sb.Append("]"); }
                                sb.Append(",");

                            }
                            break;
                        case (short)PropertyTagType.PropertyTagTypeByte:
                        default: {
                                sb.Append("[");
                                for (int j = 0; j < pi.Len; j++) {
                                    sb.Append("0x").Append(((int)pi.Value[j]).ToString("X2"))
                                        .Append(",");
                                }
                                sb.Append("]");
                            }
                            break;
                        }
                        sb.Append("\r\n");
                        sb.Append("----------------------------\r\n");
                        i++;
                    }
                    textBoxMetaData.Text = sb.ToString();
                } catch (Exception ex) {
                    MessageBox.Show(
                        "ファイルの読み込みに失敗しました。\r\n" + ex.Message,
                        Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }
    }
}

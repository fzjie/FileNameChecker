using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileNameChecker
{
    public partial class MainForm : Form
    {
        private string scanPath;
        private int maxLength = 113;
        double formWidth;
        double formHeight;
        double scaleX;
        double scaleY;
        Dictionary<string, string> controlsInfo = new Dictionary<string, string>();
        public MainForm()
        {
            InitializeComponent();
            GetAllInitInfo(this.Controls[0]);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.scanPath))
            {
                MessageBox.Show("请选择路径");
            }else {
                listView1.BeginUpdate();
                string[] files = Directory.GetFiles(this.scanPath, "*.*",SearchOption.AllDirectories);
                toolStripProgressBar1.Maximum = files.Length;
                int longFileName=0;
                toolStripProgressBar1.Value = 0;
                foreach (string file in files) {
                    string fileName=Path.GetFileName(file);
                    if (fileName.Length > this.maxLength) {
                        ListViewItem listViewItem = new ListViewItem(file);
                        listViewItem.SubItems.Add(fileName.Length.ToString());
                        listView1.Items.Add(listViewItem);
                        longFileName++;
                    }
                    toolStripProgressBar1.Value++;
                }
                listView1.EndUpdate();
                toolStripStatusLabel1.Text = "分析结果：" + "扫描到文件"+files.Length.ToString()+"个，文件名过长文件" + longFileName.ToString() + "个。";
            }
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo("Explorer.exe");
            psi.Arguments = "/e,/select,"+ listView1.SelectedItems[0].Text;
            Process.Start(psi);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件夹";
            if (dialog.ShowDialog() == DialogResult.OK) {
                textBox1.Text = dialog.SelectedPath;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.scanPath = textBox1.Text;
        }


        private void SubToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Settings settingsForm = new Settings(this.maxLength);
            settingsForm.ShowDialog();
            if (settingsForm.DialogResult == DialogResult.OK) {
                this.maxLength = settingsForm.maxLength;
            }
            settingsForm.Dispose();
        }

        private void GetAllInitInfo(Control ctrlContainer)
        {
            if (ctrlContainer.Parent == this)
            {
                this.formWidth = Convert.ToDouble(ctrlContainer.Width);
                this.formHeight = Convert.ToDouble(ctrlContainer.Height);
            }
            foreach (Control item in ctrlContainer.Controls)
            {
                if (item.Name.Trim() != "")
                {
                    this.controlsInfo.Add(item.Name, (item.Left + item.Width / 2) + "," + (item.Top + item.Height / 2) + "," + item.Width + "," + item.Height + "," + item.Font.Size);
                }
                if ((item as UserControl) == null && item.Controls.Count > 0)
                {
                    GetAllInitInfo(item);
                }
            }

        }

        private void ControlsChangeInit(Control ctrlContainer)
        {
            this.scaleX = (Convert.ToDouble(ctrlContainer.Width) / this.formWidth);
            this.scaleY = (Convert.ToDouble(ctrlContainer.Height) / this.formHeight);
        }

        private void ControlsChange(Control ctrlContainer)
        {
            double[] pos = new double[5];
            foreach (Control item in ctrlContainer.Controls)
            {
                if (item.Name.Trim() != "")
                {
                    if ((item as UserControl) == null && item.Controls.Count > 0)
                    {
                        ControlsChange(item);
                    }
                    string[] strs = this.controlsInfo[item.Name].Split(',');

                    for (int i = 0; i < 5; i++)
                    {
                        pos[i] = Convert.ToDouble(strs[i]);
                    }
                    double itemWidth = pos[2] * this.scaleX;
                    double itemHeight = pos[3] * this.scaleY;
                    item.Left = Convert.ToInt32(pos[0] * this.scaleX - itemWidth / 2);
                    item.Top = Convert.ToInt32(pos[1] * this.scaleY - itemHeight / 2);
                    item.Width = Convert.ToInt32(itemWidth);
                    item.Height = Convert.ToInt32(itemHeight);
                    item.Font = new Font(item.Font.Name, float.Parse((pos[4] * Math.Min(this.scaleX, this.scaleY)).ToString()));//字体

                }
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.controlsInfo.Count > 0)
            {
                ControlsChangeInit(this.Controls[0]);
                ControlsChange(this.Controls[0]);
            }
        }
    }
}

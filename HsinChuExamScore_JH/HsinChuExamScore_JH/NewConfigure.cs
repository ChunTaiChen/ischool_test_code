using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace HsinChuExamScore_JH
{
    public partial class NewConfigure : FISCA.Presentation.Controls.BaseForm
    {
        public Aspose.Words.Document Template { get; private set; }
        public int SubjectLimit { get; private set; }
        public int AttendanceCountLimit  { get; private set; }
        public int AttendanceDetailLimit  { get; private set; }
        public int DisciplineDetailLimit  { get; private set; }
        public int ServiceLearningDetailLimit { get; private set; }


        public string ConfigName { get; private set; }

        public NewConfigure()
        {
            InitializeComponent();
            checkBoxX1.CheckedChanged += new EventHandler(SetupTemplate);
            checkBoxX2.CheckedChanged += new EventHandler(UploadTemplate);
        }
        private void SetupTemplate(object sender, EventArgs e)
        {
            if (checkBoxX1.Checked)
            {
                Template = new Aspose.Words.Document(new MemoryStream(Properties.Resources.新竹_評量成績通知單樣板));
                this.SubjectLimit = 25;
            }
        }
        private void UploadTemplate(object sender, EventArgs e)
        {
            if (checkBoxX2.Checked)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "上傳樣板";
                dialog.Filter = "Excel檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        Template = new Aspose.Words.Document(dialog.FileName);
                        List<string> fields = new List<string>(Template.MailMerge.GetFieldNames());
                        this.SubjectLimit = 0;
                        
                        while (fields.Contains("科目名稱" + (this.SubjectLimit + 1)))
                        {
                            this.SubjectLimit++;
                        }

                        // 缺曠區間統計
                        
                        this.AttendanceCountLimit = 0;
                        while (fields.Contains("缺曠區間統計" + (this.AttendanceCountLimit + 1)))
                        {
                            this.AttendanceCountLimit++;
                        }

                        // 缺曠區間明細
                        this.AttendanceDetailLimit = 0;
                        while (fields.Contains("缺曠區間明細日期" + (this.AttendanceDetailLimit + 1)))
                        {
                            this.AttendanceDetailLimit++;
                        }

                        // 獎懲區間明細
                        this.DisciplineDetailLimit = 0;
                        while (fields.Contains("獎懲區間明細日期" + (this.DisciplineDetailLimit + 1)))
                        {
                            this.DisciplineDetailLimit++;
                        }

                        // 學習服務區間明細
                        this.ServiceLearningDetailLimit = 0;
                        while (fields.Contains("學習服務區間明細日期" + (this.ServiceLearningDetailLimit + 1)))
                        {
                            this.ServiceLearningDetailLimit++;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("樣板開啟失敗");
                        checkBoxX2.Checked = false;
                    }
                }
                else
                    checkBoxX2.Checked = false;
            }
        }

        private void checkReady(object sender, EventArgs e)
        {
            bool ready = true;
            if (txtName.Text == "")
                ready = false;
            else
                ConfigName = txtName.Text;
            if (!checkBoxX1.Checked && !checkBoxX2.Checked)
            {
                ready = false;
            }
            btnSubmit.Enabled = ready;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            #region 儲存檔案
            string inputReportName = "新竹_評量成績通知單樣板.doc";
            string reportName = inputReportName;

            string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".doc");

            if (File.Exists(path))
            {
                int i = 1;
                while (true)
                {
                    string newPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + (i++) + Path.GetExtension(path);
                    if (!File.Exists(newPath))
                    {
                        path = newPath;
                        break;
                    }
                }
            }

            try
            {                
                System.IO.FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                stream.Write(Properties.Resources.新竹_評量成績通知單樣板, 0, Properties.Resources.新竹_評量成績通知單樣板.Length);
                stream.Flush();
                stream.Close();
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                System.Windows.Forms.SaveFileDialog sd = new System.Windows.Forms.SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".doc";
                sd.Filter = "Excel檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
                if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {                
                        System.IO.FileStream stream = new FileStream(sd.FileName, FileMode.Create, FileAccess.Write);
                        stream.Write(Properties.Resources.新竹_評量成績通知單樣板, 0, Properties.Resources.新竹_評量成績通知單樣板.Length);
                        stream.Flush();
                        stream.Close();

                    }
                    catch
                    {
                        FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            #endregion
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Global.ExportMappingFieldWord();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
                return;

            // 檢查是否是預設設定檔名稱，如果是無法新增
            if (Global.DefaultConfigNameList().Contains(txtName.Text))
            {
                FISCA.Presentation.Controls.MsgBox.Show("樣板名稱與預設設定檔名稱相同，無法新增!");
                return;
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void lnkMore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            
        }

    }
}

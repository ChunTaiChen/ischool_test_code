using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.IO;
using Aspose.Words;
using System.Xml;
using SmartSchool.Common;
using DevComponents.DotNetBar.Rendering;

namespace SmartSchool.Evaluation.Reports.MultiSemesterScore.Forms
{
    public partial class TemplateSetupForm : BaseForm
    {
        private ReportOptions _option;
        private ReportOptions Option { get { return _option; } }

        private byte[] temp_buffer = null;

        public TemplateSetupForm(ReportOptions option)
        {
            InitializeComponent();

            #region 如果系統的Renderer是Office2007Renderer，同化_ClassTeacherView,_CategoryView的顏色
            if ( GlobalManager.Renderer is Office2007Renderer )
            {
                ( (Office2007Renderer)GlobalManager.Renderer ).ColorTableChanged += new EventHandler(ScoreCalcRuleEditor_ColorTableChanged);
                SetForeColor(this);
            }
            #endregion

            _option = option;
            Load();
        }
        void ScoreCalcRuleEditor_ColorTableChanged(object sender, EventArgs e)
        {
            SetForeColor(this);
        }

        private void SetForeColor(Control parent)
        {
            foreach ( Control var in parent.Controls )
            {
                if ( var is CheckBox )
                    var.ForeColor = ( (Office2007Renderer)GlobalManager.Renderer ).ColorTable.CheckBoxItem.Default.Text;
                SetForeColor(var);
            }
        } 

        private void Load()
        {
            if (Option.IsDefaultTemplate)
                radioButton1.Checked = true;
            else
                radioButton2.Checked = true;

            foreach ( CheckBox cbox in new CheckBox[] { checkBox1,checkBox2,checkBox3,checkBox4,checkBox5,checkBox6} )
            {
                cbox.Checked = Option.PrintEntries.Contains(cbox.Text);
            }
            checkBox7.Checked = !Option.FixMoralScore;

            integerInput1.Value = Option.PrintSemester;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)            
                radioButton2.Checked = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                radioButton1.Checked = false;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "另存新檔";
            sfd.FileName = "多學期成績單.doc";
            sfd.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    fs.Write(Option.DefaultTemplate, 0, Option.DefaultTemplate.Length);
                    fs.Close();
                    System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch
                {
                    MsgBox.Show("指定路徑無法存取。", "另存檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "另存新檔";
            sfd.FileName = "自訂多學期成績單.doc";
            sfd.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    if (Aspose.Words.Document.DetectFileFormat(new MemoryStream(Option.CustomizeTemplate)) == Aspose.Words.LoadFormat.Doc)
                        fs.Write(Option.CustomizeTemplate, 0, Option.CustomizeTemplate.Length);
                    fs.Close();
                    System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch
                {
                    MsgBox.Show("指定路徑無法存取。", "另存檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "選擇自訂的多學期成績單範本";
            ofd.Filter = "Word檔案 (*.doc)|*.doc";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (Document.DetectFileFormat(ofd.FileName) == LoadFormat.Doc)
                    {
                        FileStream fs = new FileStream(ofd.FileName, FileMode.Open);

                        temp_buffer = new byte[fs.Length];
                        fs.Read(temp_buffer, 0, temp_buffer.Length);
                        fs.Close();
                        MsgBox.Show("上傳成功。");
                    }
                    else
                        MsgBox.Show("上傳檔案格式不符");
                }
                catch
                {
                    MsgBox.Show("指定路徑無法存取。", "開啟檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            Option.CustomizeTemplate = temp_buffer;
            Option.IsDefaultTemplate = radioButton1.Checked;
            Option.PrintSemester = integerInput1.Value;

            List<string> list = new List<string>();
            foreach ( CheckBox cbox in new CheckBox[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6 } )
            {
                if ( cbox.Checked )
                {
                    list.Add(cbox.Text);
                }
            }
            Option.PrintEntries = list;
            Option.FixMoralScore = !checkBox7.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
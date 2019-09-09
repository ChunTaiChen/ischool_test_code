using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using SmartSchool.Customization.Data;

namespace SmartSchool.Evaluation.Process
{
    public partial class WarnViewer : SmartSchool.Common.BaseForm
    {
        private bool _hasErorr = false;

        // 2018/5/28 穎驊 自 ErrorViewer 偷過來 改良
        public WarnViewer(bool hasError)
        {
            InitializeComponent();

            _hasErorr = hasError;

            buttonX1.Enabled = ! _hasErorr; // 如果有錯誤清單則就不給點選上傳
        }
        public void SetMessage(string item, string messages)
        {

            dataGridViewX1.Rows.Add(item, messages);
            
            toolStripStatusLabel1.Text ="總計"+dataGridViewX1.Rows.Count+"個警告。";
        }

        public void Clear()
        {
            //this.Hide();
            this.dataGridViewX1.Rows.Clear();
            toolStripStatusLabel1.Text = "";
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
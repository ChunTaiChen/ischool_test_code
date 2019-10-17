using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;

namespace KaoHsiungExamScore_JH
{
    public partial class CloneConfigure : BaseForm
    {
        public CloneConfigure()
        {
            InitializeComponent();
        }

        private void CloneConigure_Load(object sender, EventArgs e)
        {
            this.textBoxX2.Focus();
        }

        public string ParentName { get { return textBoxX1.Text; } set { textBoxX1.Text = value; } }
        public string NewConfigureName { get { return textBoxX2.Text; } set { textBoxX2.Text = value; } }

        private void textBoxX2_TextChanged(object sender, EventArgs e)
        {
            this.buttonX1.Enabled = (textBoxX2.Text != "");
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}

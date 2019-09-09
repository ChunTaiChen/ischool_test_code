using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using SmartSchool.Common;
using System.Xml;

namespace SmartSchool.Evaluation.Reports
{
    public partial class SemesterMoralScoreTotalConfig : BaseForm
    {
        public SemesterMoralScoreTotalConfig(bool over100, int size)
        {
            InitializeComponent();

            comboBoxEx1.SelectedIndex = size;
            checkBoxX1.Checked = over100;
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            #region 儲存 Preference

            XmlElement config = CurrentUser.Instance.Preference["德行成績總表"];

            if (config == null)
            {
                config = new XmlDocument().CreateElement("德行成績總表");
            }

            XmlElement print = config.OwnerDocument.CreateElement("Print");
            print.SetAttribute("AllowMoralScoreOver100", checkBoxX1.Checked.ToString());
            print.SetAttribute("PaperSize", comboBoxEx1.SelectedIndex.ToString());

            if (config.SelectSingleNode("Print") == null)
                config.AppendChild(print);
            else
                config.ReplaceChild(print, config.SelectSingleNode("Print"));

            CurrentUser.Instance.Preference["德行成績總表"] = config;

            #endregion

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
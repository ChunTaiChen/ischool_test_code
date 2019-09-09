using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.Xml;
using SmartSchool.Common;

namespace SmartSchool.Evaluation.Reports.MultiSemesterScore.Forms
{
    public partial class MultiSemesterScoreForm : BaseForm
    {
        private ReportOptions _option;
        public ReportOptions Option
        {
            get { return _option; }
        }

        public MultiSemesterScoreForm(ReportOptions option)
        {
            InitializeComponent();
            _option = option;
            Load();
        }

        private void Load()
        {
            //comboBoxEx1.SelectedIndex = (int)Option.RatingMethod;
            comboBoxEx2.SelectedIndex = (int)Option.ScoreType;
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            //Option.RatingMethod = (RatingMethod)comboBoxEx1.SelectedIndex;
            Option.ScoreType = (ScoreType)comboBoxEx2.SelectedIndex;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            TemplateSetupForm form = new TemplateSetupForm(Option);
            form.ShowDialog();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //MoralLevelForm form = new MoralLevelForm()
            //form.ShowDialog();
        }
    }
}
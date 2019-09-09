using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using SmartSchool.Common;

namespace SmartSchool.Evaluation.Reports
{
    public partial class ClassSemesterScoreForm : SelectSemesterForm
    {
        private bool _over100 = false;
        private bool _UseSourceScore = false;
        public bool AllowMoralScoreOver100
        {
            get { return _over100; }
        }

        private int _paperSize = 0;

        public bool UseSourceScore
        {
            get { return _UseSourceScore; }
        }

        public int PaperSize
        {
            get { return _paperSize; }
        }
	

        public ClassSemesterScoreForm()
        {
            InitializeComponent();
            LoadPreference();
        }

        void LoadPreference()
        {
            #region 讀取 Preference

            XmlElement config = CurrentUser.Instance.Preference["班級學生學期成績一覽表"];
            if (config != null)
            {
                //列印資訊
                XmlElement print = (XmlElement)config.SelectSingleNode("Print");
                if (print != null)
                {
                    if (!string.IsNullOrEmpty(print.GetAttribute("AllowMoralScoreOver100")))
                        _over100 = bool.Parse(print.GetAttribute("AllowMoralScoreOver100"));
                    if (!string.IsNullOrEmpty(print.GetAttribute("PaperSize")))
                        _paperSize = int.Parse(print.GetAttribute("PaperSize"));
                    if (!string.IsNullOrEmpty(print.GetAttribute("UseSourceScore")))
                        _UseSourceScore = bool.Parse(print.GetAttribute("UseSourceScore"));

                }
                else
                {
                    XmlElement newPrint = config.OwnerDocument.CreateElement("Print");
                    newPrint.SetAttribute("AllowMoralScoreOver100", "False");
                    newPrint.SetAttribute("UseSourceScore", "False");
                    newPrint.SetAttribute("PaperSize", "0");
                    _over100 = false;
                    _paperSize = 0;
                    config.AppendChild(newPrint);
                    CurrentUser.Instance.Preference["班級學生學期成績一覽表"] = config;
                }
            }
            else
            {
                #region 產生空白設定檔
                config = new XmlDocument().CreateElement("班級學生學期成績一覽表");
                CurrentUser.Instance.Preference["班級學生學期成績一覽表"] = config;
                #endregion
            }

            #endregion
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ClassSemesterScoreConfig form = new ClassSemesterScoreConfig(_over100, _paperSize,_UseSourceScore);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadPreference();
            }
        }
    }
}
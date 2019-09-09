using System;
using System.ComponentModel;
using System.Xml;
using DevComponents.Editors;
using SmartSchool.Evaluation.ScoreCalcRule;
using SmartSchool.Feature.ScoreCalcRule;

namespace SmartSchool.Evaluation.Configuration
{
    public partial class ScoreCalcRuleCreator : FISCA.Presentation.Controls.BaseForm
    {
        private BackgroundWorker _BGWScoreCalcRuleLoader;

        private XmlElement _copyContent;

        public ScoreCalcRuleCreator()
        {
            InitializeComponent();

            //_copyElement = new XmlDocument().CreateElement("ScoreCalcRule");
            //_copyElement.AppendChild(new XmlDocument().CreateElement("Name"));
            //_copyElement.AppendChild(new XmlDocument().CreateElement("Content"));
            comboItem1.Tag = _copyContent;
            comboBoxEx1.SelectedIndex = 0;

            _BGWScoreCalcRuleLoader = new BackgroundWorker();
            _BGWScoreCalcRuleLoader.DoWork += new DoWorkEventHandler(_BGWScoreCalcRuleLoader_DoWork);
            _BGWScoreCalcRuleLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BGWScoreCalcRuleLoader_RunWorkerCompleted);
            _BGWScoreCalcRuleLoader.RunWorkerAsync();

            // 預設為目前的學年度
            iiSchoolYear.Text = K12.Data.School.DefaultSchoolYear;
        }

        void _BGWScoreCalcRuleLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            comboBoxEx1.Items.Remove(comboItem2);

            ScoreCalcRuleInfoCollection resp = (ScoreCalcRuleInfoCollection)e.Result;
            foreach (ScoreCalcRuleInfo scr in resp)
            {
                ComboItem item = new ComboItem();
                item.Text = scr.Name;
                item.Tag = scr.ScoreCalcRuleElement;
                comboBoxEx1.Items.Add(item);
            }
        }

        void _BGWScoreCalcRuleLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = ScoreCalcRule.ScoreCalcRule.Instance.Items;
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBoxX1_TextChanged(object sender, EventArgs e)
        {
            CheckPass();
        }

        private void iiSchoolYear_ValueChanged(object sender, EventArgs e)
        {
            CheckPass();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            if (textBoxX1.Text != "")
            {
                string ruleName = (iiSchoolYear.Value > 0 ? "" + iiSchoolYear.Value : "") + textBoxX1.Text;

                _copyContent = ReviseXmlContent(_copyContent, iiSchoolYear.Value);

                if (_copyContent == null)
                    AddScoreCalcRule.Insert(ruleName);
                else
                {
                    AddScoreCalcRule.Insert(ruleName, _copyContent);
                }
                ScoreCalcRule.ScoreCalcRule.Instance.Invok_ScoreCalcRuleInserted();
                FrmScoreCalcConfiguration.SetAdvTreeExpandStatus(iiSchoolYear.Value.ToString(), true);
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            else
                this.Close();
        }

        private void comboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEx1.SelectedItem == comboItem2)
            {
                comboBoxEx1.SelectedIndex = 0;
            }
            else
            {
                _copyContent = ((comboBoxEx1.SelectedItem as ComboItem).Tag as XmlElement);

                if (_copyContent != null && _copyContent.HasAttribute("SchoolYear"))
                {
                    // 把學年度設定為欲複製的學年度
                    iiSchoolYear.Text = _copyContent.GetAttribute("SchoolYear");
                }
            }
        }

        /// <summary>
        /// 修改XML的內容
        /// </summary>
        /// <param name="content"></param>
        /// <param name="schoolYear"></param>
        /// <returns></returns>
        private XmlElement ReviseXmlContent(XmlElement content, int schoolYear)
        {
            if (schoolYear > 0)
            {
                if (content == null)
                {
                    content = new XmlDocument().CreateElement("ScoreCalcRule");
                }

                content.RemoveAttribute("SchoolYear");
                content.SetAttribute("SchoolYear", schoolYear.ToString());
            }
            else
            {
                if (content != null)
                {
                    content.RemoveAttribute("SchoolYear");
                }
            }
            return content;
        }

        /// <summary>
        /// 驗證資料
        /// </summary>
        private void CheckPass()
        {
            errorProvider1.Clear();
            if (textBoxX1.Text == "")
            {
                SetError("不可空白。");
                button_save.Enabled = false;
                return;
            }

            string ruleName = (iiSchoolYear.Value > 0 ? "" + iiSchoolYear.Value : "") + textBoxX1.Text;

            if (CheckRuleNameDup(ruleName) == true)
            {
                SetError("成績計算規則名稱重複。");
                button_save.Enabled = false;
                return;
            }

            button_save.Enabled = true;
        }

        private void SetError(string errMsg)
        {
            this.errorProvider1.Clear();
            this.errorProvider1.SetIconPadding(this.textBoxX1, -18);

            this.errorProvider1.SetError(this.textBoxX1, errMsg);
        }

        /// <summary>
        /// 檢查名稱是否重複
        /// </summary>
        /// <param name="ruleName"></param>
        /// <returns></returns>
        private bool CheckRuleNameDup(string newRuleName)
        {
            foreach (ComboItem item in comboBoxEx1.Items)
            {
                string ruleName = item.Text;
                if (newRuleName == ruleName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
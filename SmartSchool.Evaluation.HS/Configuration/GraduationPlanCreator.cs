using System;
using System.ComponentModel;
using System.Xml;
using DevComponents.Editors;
using SmartSchool.Evaluation.GraduationPlan;
using SmartSchool.Feature.GraduationPlan;

namespace SmartSchool.Evaluation.Configuration
{
    public partial class GraduationPlanCreator : FISCA.Presentation.Controls.BaseForm
    {
        private BackgroundWorker _BKWGraduationPlanLoader;

        private XmlElement _CopyElement;

        public GraduationPlanCreator()
        {
            InitializeComponent();
            _CopyElement = new XmlDocument().CreateElement("GraduationPlan");
            comboItem1.Tag = _CopyElement;
            comboBoxEx1.SelectedIndex = 0;
            _BKWGraduationPlanLoader = new BackgroundWorker();
            _BKWGraduationPlanLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BKWGraduationPlanLoader_RunWorkerCompleted);
            _BKWGraduationPlanLoader.DoWork += new DoWorkEventHandler(_BKWGraduationPlanLoader_DoWork);
            _BKWGraduationPlanLoader.RunWorkerAsync();

            // 預設為目前的學年度
            iiSchoolYear.Text = K12.Data.School.DefaultSchoolYear;
        }

        private void _BKWGraduationPlanLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = SmartSchool.Evaluation.GraduationPlan.GraduationPlan.Instance.Items;
        }

        private void _BKWGraduationPlanLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            comboBoxEx1.Items.Remove(comboItem2);
            GraduationPlanInfoCollection resp = (GraduationPlanInfoCollection)e.Result;
            foreach (GraduationPlanInfo gPlan in resp)
            {
                DevComponents.Editors.ComboItem item = new DevComponents.Editors.ComboItem();
                item.Text = gPlan.Name;
                item.Tag = gPlan.GraduationPlanElement;
                comboBoxEx1.Items.Add(item);
            }
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (textBoxX1.Text != "")
            {
                _CopyElement = ReviseXmlContent(_CopyElement, iiSchoolYear.Value);
                string ruleName = (iiSchoolYear.Value > 0 ? "" + iiSchoolYear.Value : "") + textBoxX1.Text;

                AddGraduationPlan.Insert(ruleName, _CopyElement);
                EventHub.Instance.InvokGraduationPlanInserted();
                FrmGraduationPlanConfiguration.SetAdvTreeExpandStatus(iiSchoolYear.Value.ToString(), true);
                //GraduationPlanManager.Instance.LoadGraduationPlan(true);
                //if (GraduationPlanManager.Instance.Visible == false)
                //    GraduationPlanManager.Instance.ShowDialog();
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            else
                this.Close();
        }

        private void comboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEx1.SelectedItem == comboItem2)
                comboBoxEx1.SelectedIndex = 0;
            else
            {
                _CopyElement = (XmlElement)((ComboItem)comboBoxEx1.SelectedItem).Tag;

                if (_CopyElement != null && _CopyElement.HasAttribute("SchoolYear"))
                {
                    // 把學年度設定為欲複製的學年度
                    iiSchoolYear.Text = _CopyElement.GetAttribute("SchoolYear");
                }
            }
        }

        private void textBoxX1_TextChanged(object sender, EventArgs e)
        {
            CheckPass();
        }

        private void iiSchoolYear_ValueChanged(object sender, EventArgs e)
        {
            CheckPass();
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
                    content = new XmlDocument().CreateElement("GraduationPlan");
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
                buttonX1.Enabled = false;
                return;
            }

            string ruleName = (iiSchoolYear.Value > 0 ? "" + iiSchoolYear.Value : "") + textBoxX1.Text;

            if (CheckRuleNameDup(ruleName) == true)
            {
                SetError("名稱不可重複。");
                buttonX1.Enabled = false;
                return;
            }
            buttonX1.Enabled = true;
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
            foreach (GraduationPlanInfo gPlan in SmartSchool.Evaluation.GraduationPlan.GraduationPlan.Instance.Items)
            {
                if (gPlan.Name == newRuleName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
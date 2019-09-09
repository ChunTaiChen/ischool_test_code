using System;
using System.Xml;
using SmartSchool.Common;
using SmartSchool.ExceptionHandler;

namespace SmartSchool.Evaluation.Configuration
{
    public partial class SubjectTableCreator : FISCA.Presentation.Controls.BaseForm
    {
        private string _Catalog;

        private XmlElement _CopyElement;

        public SubjectTableCreator(string catalog)
        {
            this._Catalog=catalog;
            InitializeComponent();

            this.Text = "新增" + catalog;

            comboBoxEx1.SelectedItem = comboItem1;
            foreach ( SubjectTableItem var in SubjectTable.Items[_Catalog])
            {
                comboBoxEx1.Items.Add(var) ;   
            }

            textBoxX1.Focus();

            // 預設為目前的學年度
            iiSchoolYear.Text = K12.Data.School.DefaultSchoolYear;
        }

        private void textBoxX1_TextChanged(object sender, EventArgs e)
        {
            CheckPass();
        }

        private void iiSchoolYear_ValueChanged(object sender, EventArgs e)
        {
            CheckPass();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if ( textBoxX1.Text != "" )
            {
                string ruleName = (iiSchoolYear.Value > 0 ? "" + iiSchoolYear.Value : "") + textBoxX1.Text;

                _CopyElement = ReviseXmlContent(_CopyElement, iiSchoolYear.Value);

                try
                {
                    SmartSchool.Feature.SubjectTable.AddSubejctTable.Insert(ruleName, _Catalog, _CopyElement);
                }
                catch(Exception ex)
                {
                    CurrentUser user = CurrentUser.Instance;
                    BugReporter.ReportException("SmartSchool", user.SystemVersion, ex, false);
                    MsgBox.Show("新增" + _Catalog + "時發生未預期之錯誤。\n系統已回報此錯誤內容。");
                }
                SubjectTable.Items[_Catalog].Reflash();
                FrmSubjectTableConfiguration.SetAdvTreeExpandStatus(iiSchoolYear.Value.ToString(), true);
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            else
            {
                MsgBox.Show("必需輸入" + _Catalog + "名稱。");
            }
        }

        private void comboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( comboBoxEx1.SelectedItem == comboItem1 )
                _CopyElement = new XmlDocument().CreateElement("SubjectTableContent");
            else
            {
                _CopyElement = (XmlElement)( (SubjectTableItem)comboBoxEx1.SelectedItem ).Content.SelectSingleNode("SubjectTableContent");

                if (_CopyElement != null && _CopyElement.HasAttribute("SchoolYear"))
                {
                    // 把學年度設定為欲複製的學年度
                    iiSchoolYear.Text = _CopyElement.GetAttribute("SchoolYear");
                }
            }
        }

        /// <summary>
        /// 驗證資料
        /// </summary>
        private void CheckPass()
        {
            errorProvider1.Clear();
            buttonX1.Enabled = true;
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
            foreach (SubjectTableItem var in SubjectTable.Items[_Catalog])
            {
                if (var.Name == newRuleName)
                {
                    return true;
                }
            }
            return false;
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
                    content = new XmlDocument().CreateElement("SubjectTableContent");
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
    }
}
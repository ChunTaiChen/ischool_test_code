using FISCA.Presentation.Controls;
using SmartSchool.Evaluation.ScoreCalcRule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SmartSchool.Evaluation.Configuration
{
    public partial class FrmReviseRuleName : BaseForm
    {
        public event EventHandler<ReviseRuleNameEventArgs> SaveEvent;
        public event EventHandler<ReviseRuleNameEventArgs> ValidateEvent;
        private string _OldSchoolYear;
        private string _OldRuleName;

        public FrmReviseRuleName(string formTitle,
                                string lblName,
                                string ruleName,
                                string schoolYear)
        {
            InitializeComponent();

            this.Text = formTitle;
            this.labelX2.Text = lblName;
            this.textBoxX1.Text = ruleName;
            if (string.IsNullOrEmpty(schoolYear))
                this.integerInput1.Text = K12.Data.School.DefaultSchoolYear;
            else
                this.integerInput1.Text = schoolYear;

            _OldSchoolYear = schoolYear;
            _OldRuleName = ruleName;
        }

        private void integerInput1_ValueChanged(object sender, EventArgs e)
        {
            CheckPass();
        }

        private void textBoxX1_TextChanged(object sender, EventArgs e)
        {
            CheckPass();
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            if (SaveEvent != null)
            {
                SaveEvent.Invoke(this, GetArgs());
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private ReviseRuleNameEventArgs GetArgs()
        {
            ReviseRuleNameEventArgs args = new ReviseRuleNameEventArgs();
            string schoolYear = integerInput1.Text;
            string ruleName = textBoxX1.Text;

            args.OldSchoolYear = _OldSchoolYear;
            args.OldRuleName = _OldRuleName;
            args.NewSchoolYear = schoolYear;
            args.NewRuleName = ruleName;

            return args;
        }

        public class ReviseRuleNameEventArgs : EventArgs
        {
            private string _NewSchoolYear;
            private string _NewRuleName;
            private string _OldSchoolYear;
            private string _OldRuleName;
            private bool _Error;
            private string _ErrorString;

            /// <summary>
            /// 原來的SchoolYear
            /// </summary>
            public string OldSchoolYear { set { _OldSchoolYear = value; } get { return _OldSchoolYear; } }
            /// <summary>
            /// 原來的RuleName
            /// </summary>
            public string OldRuleName { set { _OldRuleName = value; } get { return _OldRuleName; } }
            /// <summary>
            /// 
            /// </summary>
            public string OldFullName { get { return _OldSchoolYear + _OldRuleName; } }
            public string NewSchoolYear { set { _NewSchoolYear = value; } get { return _NewSchoolYear; } }
            public string NewRuleName { set { _NewRuleName = value; } get { return _NewRuleName; } }
            public string NewFullName { get { return _NewSchoolYear + _NewRuleName; } }
            public bool Error { set { _Error = value; } get { return _Error; } }
            public string ErrorString { set { _ErrorString = value; } get { return _ErrorString; } }

            public bool IsSame
            {
                get
                {
                    if (_OldSchoolYear == _NewSchoolYear && _OldRuleName == _NewRuleName)
                        return true;
                    else
                        return false;
                }
            }
        }

        private void CheckPass()
        {
            errorProvider1.Clear();
            if (textBoxX1.Text == "")
            {
                SetError("不可空白。");
                btn_save.Enabled = false;
                return;
            }

            if (ValidateEvent != null)
            {
                ReviseRuleNameEventArgs eventArg = GetArgs();

                ValidateEvent.Invoke(this, eventArg);

                if (eventArg.Error == true)
                {
                    SetError(eventArg.ErrorString);
                    btn_save.Enabled = false;
                    return;
                }
                else
                {
                    btn_save.Enabled = true;
                }
            }
        }

        private void SetError(string errMsg)
        {
            this.errorProvider1.Clear();
            this.errorProvider1.SetIconPadding(this.textBoxX1, -18);
            this.errorProvider1.SetError(this.textBoxX1, errMsg);
        }
    }
}

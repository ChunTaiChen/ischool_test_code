using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using FISCA.DSAUtil;
//using SmartSchool.SmartPlugIn.Student.AttendanceEditor;
using SmartSchool.Feature.Basic;

namespace SmartSchool.Evaluation.Configuration.MoralConductEditors
{
    public partial class PeriodAbsenceCalcRuleEditor : UserControl,IMoralConductInstance
    {
        private bool _SourceSetting = false;

        private string _BaseString;
        private XmlElement _Source;
        private bool _IsValidate = true;
        private Dictionary<Control, ErrorProvider> _errorProviderDictionary = new Dictionary<Control, ErrorProvider>();

        private static int SortPeriod(XmlElement period1,XmlElement period2)
        {
            int a=0, b=0;
            int.TryParse(period1.GetAttribute("Sort"), out a);
            int.TryParse(period2.GetAttribute("Sort"), out b);
            return a.CompareTo(b);
        }

        public PeriodAbsenceCalcRuleEditor()
        {
            InitializeComponent();
        }

        private void dataGridViewX1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
            DataGridViewCell cell = dataGridViewX1.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string message = "儲存格值：" + cell.Value + "。\n發生錯誤： " + e.Exception.Message + "。";
            if (cell.ErrorText != message)
            {
                cell.ErrorText = message;
                dataGridViewX1.UpdateCellErrorText(e.ColumnIndex, e.RowIndex);
            }
        }

        private void dataGridViewX1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            ValidateAll();
        }
        private void textBoxX1_TextChanged(object sender, EventArgs e)
        {
            ValidateAll();
        }

        private void ValidateAll()
        {
            bool isPass = true;
            decimal dec;
            if ( decimal.TryParse(textBoxX1.Text, out dec) )
                ResetErrorProvider(textBoxX1);
            else
            {
                SetErrorProvider(textBoxX1, "必須輸入分數。");
                isPass &= false;
            }
            foreach ( DataGridViewRow row in dataGridViewX1.Rows )
            {
                isPass &= ValidateRow(row);
            }
            IsValidate = isPass;
            if ( !_SourceSetting )
                this.IsDirty = ( _BaseString != this.GetSource().OuterXml );
        }

        private bool ValidateRow(DataGridViewRow row)
        {
            bool pass = true;
            decimal sub = 0;
            #region 驗證扣分欄必須填入數字
            if (!decimal.TryParse("" + row.Cells[3].Value, out sub))
            {
                pass &= false;
                row.Cells[3].ErrorText = "扣分欄必須填寫，若不影響成績計算請填入0。";
                dataGridViewX1.UpdateCellErrorText(3, row.Index);
            }
            else if (row.Cells[3].ErrorText == "扣分欄必須填寫，若不影響成績計算請填入0。")
            {
                row.Cells[3].ErrorText = "";
                dataGridViewX1.UpdateCellErrorText(3, row.Index);
            }
            if (sub < 0)
            {
                pass &= false;
                row.Cells[3].ErrorText = "扣分欄請填入正整數。";
                dataGridViewX1.UpdateCellErrorText(3, row.Index);
            }
            else if (row.Cells[3].ErrorText == "扣分欄請填入正整數。")
            {
                row.Cells[3].ErrorText = "";
                dataGridViewX1.UpdateCellErrorText(3, row.Index);
            }
            #endregion
            int aggregated = 0;
            #region 驗證累計單位，必須填入正整數，若減分為0則可以填入空白。
            if (!(sub == 0 && "" + row.Cells[2].Value =="")&&(!int.TryParse("" + row.Cells[2].Value, out aggregated) || aggregated <= 0))
            {
                pass &= false;
                row.Cells[2].ErrorText = "累計單位請填入正整數。";
                dataGridViewX1.UpdateCellErrorText(2, row.Index);
            }
            else if (row.Cells[2].ErrorText == "累計單位請填入正整數。")
            {
                row.Cells[2].ErrorText = "";
                dataGridViewX1.UpdateCellErrorText(2, row.Index);
            }
            #endregion
            return pass;
        }

        private void SetErrorProvider(Control control, string p)
        {
            if (!_errorProviderDictionary.ContainsKey(control))
            {
                ErrorProvider ep = new ErrorProvider();
                ep.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                ep.SetIconAlignment(control, ErrorIconAlignment.MiddleRight);
                ep.Icon = Properties.Resources.warning;
                ep.SetError(control, p);
                _errorProviderDictionary.Add(control, ep);
            }
        }

        private void ResetErrorProvider(Control control)
        {
            if (_errorProviderDictionary.ContainsKey(control))
            {
                _errorProviderDictionary[control].Clear();
                _errorProviderDictionary.Remove(control);
            }
        }

        #region IMoralConductInstance 成員

        public string XPath
        {
            get { return "PeriodAbsenceCalcRule"; }
        }
        public void SetSource(XmlElement source)
        {
            _SourceSetting = true;
            IsDirty = false;
            dataGridViewX1.Rows.Clear();
            #region 抓節次後排序
            DSResponse dsrsp = Config.GetPeriodList();
            List<XmlElement> PeriodInfoList = new List<XmlElement>();
            foreach (XmlElement element in dsrsp.GetContent().GetElements("Period"))
            {
                PeriodInfoList.Add(element);
            }
            PeriodInfoList.Sort(SortPeriod); 
            #endregion

            #region 抓假別
            dsrsp = SmartSchool.Feature.Basic.Config.GetAbsenceList();
            List<XmlElement> AbsenceInfoList = new List<XmlElement>();
            foreach (XmlElement element in dsrsp.GetContent().GetElements("Absence"))
            {
                AbsenceInfoList.Add(element);
            } 
            #endregion

            #region 填入節次假別
            List<string> _FilledPeriod = new List<string>();
            foreach (XmlElement period in PeriodInfoList)
            {
                if (!_FilledPeriod.Contains(period.GetAttribute("Type")))
                {
                    _FilledPeriod.Add(period.GetAttribute("Type"));
                    foreach (XmlElement absence in AbsenceInfoList)
                    {
                        dataGridViewX1.Rows.Add(period.GetAttribute("Type"), absence.GetAttribute("Name"));
                    }
                }
            } 
            #endregion
            _Source = source;
            if (source != null)
            {
                textBoxX1.Text = _Source.GetAttribute("NoAbsenceReward");
                foreach (DataGridViewRow row in dataGridViewX1.Rows)
                {
                    XmlElement ele=(XmlElement) source.SelectSingleNode("Rule[@Period='" + row.Cells[0].Value + "' and @Absence='" + row.Cells[1].Value + "']");
                    if (ele != null)
                    {
                        row.Cells[2].Value = ele.GetAttribute("Aggregated");
                        row.Cells[3].Value = ele.GetAttribute("Subtract");
                    }
                }
            }
            _SourceSetting = false;
            _BaseString = this.GetSource().OuterXml;
            ValidateAll();
        }
        public XmlElement GetSource()
        {
            if (_Source == null)
            {
                XmlDocument doc = new XmlDocument();
                _Source = doc.CreateElement("PeriodAbsenceCalcRule");
            }
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                XmlElement ele = (XmlElement)_Source.SelectSingleNode("Rule[@Period='" + row.Cells[0].Value + "' and @Absence='" + row.Cells[1].Value + "']");
                if (ele != null)
                {
                    ele.SetAttribute("Aggregated", "" + row.Cells[2].Value);
                    ele.SetAttribute("Subtract", "" + row.Cells[3].Value);
                }
                else
                {
                    XmlElement newRule = _Source.OwnerDocument.CreateElement("Rule");
                    newRule.SetAttribute("Period", "" + row.Cells[0].Value);
                    newRule.SetAttribute("Absence", "" + row.Cells[1].Value);
                    newRule.SetAttribute("Aggregated", "" + row.Cells[2].Value);
                    newRule.SetAttribute("Subtract", "" + row.Cells[3].Value);
                    _Source.AppendChild(newRule);
                }
            }
            _Source.SetAttribute("NoAbsenceReward", textBoxX1.Text);
            return _Source;
        }
        public bool IsValidate
        {
            get { return _IsValidate; }
            private set
            {
                if (_IsValidate != value)
                {
                    _IsValidate = value;
                    if (this.IsValidateChanged != null)
                        IsValidateChanged.Invoke(this, new EventArgs());
                }
            }
        }
        public event EventHandler IsValidateChanged;

        public void GetDependenceData()
        {
        }

        public void SetDependenceData()
        {
        }

        private bool _IsDirty = false;

        public bool IsDirty
        {
            get { return _IsDirty; }
            set
            {
                _IsDirty = value;
                if ( IsDirtyChanged != null )
                {
                    IsDirtyChanged.Invoke(this, new EventArgs());
                }
            }
        }

        public event EventHandler IsDirtyChanged;
        #endregion

        private void dataGridViewX1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dataGridViewX1.EndEdit();
            ValidateAll();
            dataGridViewX1.BeginEdit(false);
        }
    }
}

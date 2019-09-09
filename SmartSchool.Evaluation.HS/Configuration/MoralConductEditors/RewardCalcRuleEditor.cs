using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using DevComponents.DotNetBar.Controls;
using DevComponents.DotNetBar.Rendering;

namespace SmartSchool.Evaluation.Configuration.MoralConductEditors
{
    public partial class RewardCalcRuleEditor : UserControl,IMoralConductInstance
    {
        private bool _SourceSetting = false;

        private Dictionary<Control, ErrorProvider> _errorProviderDictionary = new Dictionary<Control, ErrorProvider>();

        private bool _IsValidate = true;

        private XmlElement _Source;

        private string _BaseString;

        private Dictionary<string, TextBoxX> _textBoxs=new Dictionary<string,TextBoxX>();

        private Dictionary<string, CheckBoxX> _checkBoxs = new Dictionary<string, CheckBoxX>();

        public RewardCalcRuleEditor()
        {
            InitializeComponent();
            FIllControls(this);
            #region 如果系統的Renderer是Office2007Renderer，同化_ClassTeacherView,_CategoryView的顏色
            if ( GlobalManager.Renderer is Office2007Renderer )
            {
                ( (Office2007Renderer)GlobalManager.Renderer ).ColorTableChanged += new EventHandler(ScoreCalcRuleEditor_ColorTableChanged);
                SetForeColor();
            }
            #endregion
        }

        void ScoreCalcRuleEditor_ColorTableChanged(object sender, EventArgs e)
        {
            SetForeColor();
        }

        private void SetForeColor()
        {
            radioButton1.ForeColor=radioButton2.ForeColor  = ( (Office2007Renderer)GlobalManager.Renderer ).ColorTable.CheckBoxItem.Default.Text;
        }

        private void FIllControls(Control control)
        {
            if (""+control.Tag != "")
            {
                if (control is TextBoxX)
                {
                    _textBoxs.Add("" + control.Tag, (TextBoxX)control);
                }
                if (control is CheckBoxX)
                    _checkBoxs.Add("" + control.Tag, (CheckBoxX)control);
            }
            foreach (Control var in control.Controls)
            {
                FIllControls(var);
            }
            if(control is TextBox)
                control.TextChanged += new EventHandler(CheckIsDirty);
            if(control is RadioButton )
                ( (RadioButton)control ).CheckedChanged += new EventHandler(CheckIsDirty);
            if ( control is CheckBox )
                ( (CheckBox)control ).CheckedChanged += new EventHandler(CheckIsDirty);
        }

        void CheckIsDirty(object sender, EventArgs e)
        {
            if ( !_SourceSetting )
                this.IsDirty = ( _BaseString != this.GetSource().OuterXml );
        }

        void control_TextChanged(object sender, EventArgs e)
        {

            GetCheckBox("" + ((TextBoxX)sender).Tag).Checked = ((TextBoxX)sender).Text != "";
        }

        #region IMoralConductInstance 成員

        public string XPath
        {
            get { return "RewardCalcRule"; }
        }

        public void SetSource(System.Xml.XmlElement source)
        {
            _SourceSetting = true;
            IsDirty = false;
            if ( source != null )
            {
                _Source = source;
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<RewardCalcRule/>");
                _Source = doc.DocumentElement;
            }
            foreach ( string ke in new string[] { "AwardA", "AwardB", "AwardC", "FaultA", "FaultB", "FaultC" } )
            {
                string key = ke + "1";
                GetTextBox(key).Text = _Source.GetAttribute(key);
                foreach ( string y in new string[] { "2", "3" } )
                {
                    key = ke + y;
                    if ( _Source.HasAttribute(key) )
                    {
                        GetCheckBox(key).Checked = true;
                        GetTextBox(key).Text = _Source.GetAttribute(key);
                    }
                    else
                    {
                        GetCheckBox(key).Checked = false;
                        GetTextBox(key).Text = "";
                    }
                }
            }
            bool calcCancel = false;
            if ( !bool.TryParse(_Source.GetAttribute("CalcCancel"), out calcCancel) )
                calcCancel = false;
            if ( calcCancel )
                radioButton1.Checked = true;
            else
                radioButton2.Checked = true;
            Val(null, null);
            _SourceSetting = false;
            _BaseString = this.GetSource().OuterXml;
        }

        public System.Xml.XmlElement GetSource()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<RewardCalcRule/>");
            _Source = doc.DocumentElement;
            foreach (string ke in new string[] { "AwardA", "AwardB", "AwardC", "FaultA", "FaultB", "FaultC" })
            {
                foreach (string y in new string[] {"1", "2", "3" })
                {
                    string key = ke + y;
                    if(GetCheckBox(key).Checked)
                    {
                        _Source.SetAttribute(key, GetTextBox(key).Text);
                    }
                }
            }
            _Source.SetAttribute("CalcCancel", ""+radioButton1.Checked);
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
                    IsDirtyChanged.Invoke(this,new EventArgs());
                }
            }
        }

        public event EventHandler IsDirtyChanged;
        #endregion

        private CheckBoxX GetCheckBox(string name)
        {
            return _checkBoxs[name];
        }

        private TextBox GetTextBox(string name)
        {
            return _textBoxs[name];
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

        private void Val(object sender, EventArgs e)
        {
            bool pass=true;
            foreach (string ke in new string[] { "AwardA", "AwardB", "AwardC", "FaultA", "FaultB", "FaultC" })
            {
                string key;
                foreach (string y in new string[] {"1", "2", "3" })
                {
                    key = ke + y;
                    decimal dec;
                    if (GetCheckBox(key).Checked && !decimal.TryParse(GetTextBox(key).Text, out dec))
                    {
                        pass &= false;
                        SetErrorProvider(GetTextBox(key), "必須輸加減分");
                    }
                    else
                    {
                        ResetErrorProvider(GetTextBox(key));
                    }
                    if (y != "1" && sender is TextBoxX&&key==""+((TextBoxX)sender).Tag)
                    {
                        GetCheckBox(key).Checked = (((TextBoxX)sender).Text != "");
                    }
                }
            }
            IsValidate = pass;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace SmartSchool.Evaluation.Configuration.MoralConductEditors
{
    public partial class BasicScoreEditor : UserControl,IMoralConductInstance
    {
        private bool _SourceSetting = false;

        private Dictionary<Control, ErrorProvider> _errorProviderDictionary = new Dictionary<Control, ErrorProvider>();

        private bool _IsValidate = true;

        private XmlElement _Source;

        private string _BaseString;

        public BasicScoreEditor()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            this.numericUpDown2.ValueChanged += new System.EventHandler(this.CheckIsDirty);
            this.textBoxX2.TextChanged += new System.EventHandler(this.CheckIsDirty);
            this.comboBox1.TextChanged += new System.EventHandler(this.CheckIsDirty);
            this.comboBox2.TextChanged += new System.EventHandler(this.CheckIsDirty);
            this.textBoxX1.TextChanged += new System.EventHandler(this.CheckIsDirty);
        }

        #region IMoralConductInstance 成員

        public string XPath
        {
            get { return "BasicScore"; }
        }

        public void SetSource(System.Xml.XmlElement source)
        {
            _SourceSetting = true;
            IsDirty = false;
            if (source != null)
            {
                _Source = source;
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<BasicScore/>");
                _Source = doc.DocumentElement;
            }
            this.textBoxX1.Text = _Source.GetAttribute("NormalScore");
            this.textBoxX2.Text = _Source.GetAttribute("UltimateAdmonitionScore");
            decimal dec = 2;
            if ( !decimal.TryParse(_Source.GetAttribute("Decimals"), out dec) ) 
                dec = 2;
            numericUpDown2.Value = dec;
            switch ( _Source.GetAttribute("DecimalType") )
            { 
                default:
                case "四捨五入":
                    comboBox1.SelectedIndex = 0;
                    break;
                case "無條件捨去":
                    comboBox1.SelectedIndex = 1;
                    break;
                case "無條件進位":
                    comboBox1.SelectedIndex = 2;
                    break;
            }
            switch ( _Source.GetAttribute("Over100") )
            {
                default:
                case "以實際分數計":
                    comboBox2.SelectedIndex = 0;
                    break;
                case "以100分計":
                    comboBox2.SelectedIndex = 1;
                    break;
            }
            TextChanged(null, null); 
            _SourceSetting = false;
            _BaseString = this.GetSource().OuterXml;
        }

        public System.Xml.XmlElement GetSource()
        {
            if ( _Source == null )
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<BasicScore/>");
                _Source = doc.DocumentElement;
            }
            _Source.SetAttribute("NormalScore", textBoxX1.Text);
            _Source.SetAttribute("UltimateAdmonitionScore", textBoxX2.Text);
            _Source.SetAttribute("Decimals", "" + numericUpDown2.Value);
            _Source.SetAttribute("DecimalType", "" + comboBox1.Text);
            _Source.SetAttribute("Over100", "" + comboBox2.Text);
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

        private void TextChanged(object sender, EventArgs e)
        {
            decimal dec;
            bool a, b;
            a = decimal.TryParse(this.textBoxX1.Text, out dec);
            if (a)
            {
                ResetErrorProvider(textBoxX1);
            }
            else
            {
                SetErrorProvider(textBoxX1, "請輸入數值。");
            }
            b = decimal.TryParse(this.textBoxX2.Text, out dec);
            if (b)
            {
                ResetErrorProvider(textBoxX2);
            }
            else
            {
                SetErrorProvider(textBoxX2, "請輸入數值。");
            }
            IsValidate = a & b;
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

        private void CheckIsDirty(object sender, EventArgs e)
        {
            if ( !_SourceSetting )
                this.IsDirty = ( _BaseString != this.GetSource().OuterXml );
        }
    }
}

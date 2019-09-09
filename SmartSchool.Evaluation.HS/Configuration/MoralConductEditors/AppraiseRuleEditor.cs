using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace SmartSchool.Evaluation.Configuration.MoralConductEditors
{
    public partial class AppraiseRuleEditor : UserControl,IMoralConductInstance
    {
        private bool _SourceSetting = false;

        private Dictionary<Control, ErrorProvider> _errorProviderDictionary = new Dictionary<Control, ErrorProvider>();

        private bool _IsValidate = true;

        private XmlElement _Source;

        private string _BaseString;

        private List<string> _Names = new List<string>();

        public AppraiseRuleEditor()
        {
            InitializeComponent();
            this.textBoxX1.TextChanged+=new EventHandler(CheckIsDirty);
        }

        #region IMoralConductInstance 成員

        public string XPath
        {
            get { return "TeacherAppraise"; }
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
                doc.LoadXml("<TeacherAppraise/>");
                _Source = doc.DocumentElement;
            }
            this.textBoxX1.Text = _Source.GetAttribute("Range");
            textBoxX1_TextChanged(null, null);
            _SourceSetting = false;
            _BaseString = this.GetSource().OuterXml; 
        }

        public System.Xml.XmlElement GetSource()
        {
            _Source.SetAttribute("Range", textBoxX1.Text);
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
            _Names = new List<string>();
            listView1.Items.Clear();
            foreach (XmlElement element in SmartSchool.Feature.Basic.Config.GetMoralDiffItemList().GetContent().GetElements("DiffItem"))
            {
                listView1.Items.Add(element.GetAttribute("Name")).ImageIndex=0;
                _Names.Add(element.GetAttribute("Name"));
            }
        }

        public void SetDependenceData()
        {
            List<string> names = new List<string>();
            foreach (ListViewItem var in listView1.Items)
            {
                names.Add(var.Text);
            }
            SmartSchool.Feature.Basic.Config.SetMoralDiffItemList(names.ToArray());
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

        private void textBoxX1_TextChanged(object sender, EventArgs e)
        {
            decimal dec;
            IsValidate = decimal.TryParse(this.textBoxX1.Text, out dec);
            if (IsValidate)
            {
                ResetErrorProvider(textBoxX1);
            }
            else
            {
                SetErrorProvider(textBoxX1, "請輸入數值。");
            }
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ListViewItem newItem = this.listView1.Items.Add("新增項目");
            newItem.ImageIndex=0;
            newItem.BeginEdit();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
                this.listView1.SelectedItems[0].BeginEdit();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                this.listView1.Items.Remove(item);
            }
            CheckIsDirty(null, null);
        }

        private void CheckIsDirty(object sender, EventArgs e)
        {
            if ( !_SourceSetting )
            {
                bool dirty;
                dirty = ( _BaseString != this.GetSource().OuterXml );
                if ( listView1.Items.Count == _Names.Count )
                {
                    foreach ( ListViewItem var in listView1.Items )
                    {
                        if ( !_Names.Contains(var.Text) )
                            dirty |= true;
                    }
                }
                else
                    dirty |= true;
                IsDirty = dirty;
            }
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if ( e.Label != null )
            {
                listView1.Items[e.Item].Text = e.Label;
            }
            else
                e.CancelEdit = true;
            CheckIsDirty(null, null);
        }
    }
}

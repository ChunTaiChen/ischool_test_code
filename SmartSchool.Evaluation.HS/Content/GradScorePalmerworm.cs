using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.AccessControl;
using SmartSchool.ApplicationLog;
using SmartSchool.Customization.Data;

namespace SmartSchool.Evaluation.Content
{
    [FeatureCode("Content0145")]
    public partial class GradScorePalmerworm : UserControl, SmartSchool.Customization.PlugIn.ExtendedContent.IContentItem
    {
        public GradScorePalmerworm()
        {
            InitializeComponent();
            EventHub.Instance.ScoreChanged += new EventHandler<ScoreChangedEventArgs>(Instance_ScoreChanged);
            _EntryTextBoxMapping = new Dictionary<string, TextBox>();
            _EntryTextBoxMapping.Add("學業", textBoxX1);
            _EntryTextBoxMapping.Add("德行", textBoxX2);
            _EntryTextBoxMapping.Add("體育", textBoxX3);
            _EntryTextBoxMapping.Add("國防通識", textBoxX4);
            _EntryTextBoxMapping.Add("健康與護理", textBoxX5);
            _EntryTextBoxMapping.Add("實習科目", textBoxX6);
            _Loader = new BackgroundWorker();
            _Loader.DoWork += new DoWorkEventHandler(loader_DoWork);
            _Loader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loader_RunWorkerCompleted);
            SaveButtonVisibleChanged += new EventHandler(GradScorePalmerworm_SaveButtonVisibleChanged);
        }

        private void GradScorePalmerworm_SaveButtonVisibleChanged(object sender, EventArgs e)
        {
            if (Attribute.IsDefined(GetType(), typeof(SmartSchool.AccessControl.FeatureCodeAttribute)))
            {
                try
                {
                    if (!CurrentUser.Acl[GetType()].Editable && SaveButtonVisible)
                    {
                        SaveButtonVisible = false;
                        ClearErrors();
                    }
                }
                catch (Exception) { }
            }
        }

        void Instance_ScoreChanged(object sender, ScoreChangedEventArgs e)
        {
            if (e.StudentIds.Contains(this._CurrentID))
                this.LoadContent(_CurrentID);
        }

        private void loader_DoWork(object sender, DoWorkEventArgs e)
        {
            string id = "" + e.Argument;
            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (XmlElement element in SmartSchool.Feature.QueryStudent.GetDetailList(new string[] { "GradScore" }, id).GetContent().GetElements("Student/GradScore/GradScore/EntryScore"))
            {
                values.Add(element.GetAttribute("Entry"), element.GetAttribute("Score"));
            }
            e.Result = values;
        }

        private void loader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_RunnngID != _CurrentID)
            {
                LoadContent(_CurrentID);
            }
            else
            {
                WaitingPicVisible = false;
                Dictionary<string, string> values = (Dictionary<string, string>)e.Result;
                foreach (string entry in _EntryTextBoxMapping.Keys)
                {
                    if (values.ContainsKey(entry))
                    {
                        _EntryTextBoxMapping[entry].Tag = values[entry];
                        _EntryTextBoxMapping[entry].Text = values[entry];
                    }
                    else
                    {
                        _EntryTextBoxMapping[entry].Tag = "";
                        _EntryTextBoxMapping[entry].Text = "";
                    }
                }
                CheckChanged();
            }
        }

        private Dictionary<string, TextBox> _EntryTextBoxMapping;

        private bool WaitingPicVisible { set { this.picWaiting.Visible = value; } }

        private string _RunnngID = "";

        string _CurrentID = "";

        private bool _SaveButtonVisible = false;

        private bool _CancelButtonVisible = false;

        private BackgroundWorker _Loader;

        #region IContentItem 成員

        public bool CancelButtonVisible
        {
            get { return _CancelButtonVisible; }
            private set
            {
                if (_CancelButtonVisible != value)
                {
                    _CancelButtonVisible = value;
                    if (CancelButtonVisibleChanged != null)
                        CancelButtonVisibleChanged.Invoke(this, new EventArgs());
                }
            }
        }

        public event EventHandler CancelButtonVisibleChanged;

        public Control DisplayControl
        {
            get { return this; }
        }

        public void LoadContent(string id)
        {
            ClearErrors();
            _CurrentID = id;
            if (!_Loader.IsBusy)
            {
                _RunnngID = _CurrentID;
                WaitingPicVisible = true;
                textBoxX1.Tag = textBoxX2.Tag = textBoxX3.Tag = textBoxX4.Tag = textBoxX5.Tag = textBoxX6.Tag = "";
                textBoxX1.Text = textBoxX2.Text = textBoxX3.Text = textBoxX4.Text = textBoxX5.Text = textBoxX6.Text = "";
                CheckChanged();
                _Loader.RunWorkerAsync(_RunnngID);
            }
        }

        private void ClearErrors()
        {
            foreach (ErrorProvider each in _errorProviderDictionary.Values)
                each.Clear();
            _errorProviderDictionary.Clear();
        }

        public void Save()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement gradeCalcScoreElement = doc.CreateElement("GradScore");
            foreach (string entry in _EntryTextBoxMapping.Keys)
            {
                if (_EntryTextBoxMapping[entry].Text != "")
                {
                    XmlElement entryScoreElement = doc.CreateElement("EntryScore");
                    entryScoreElement.SetAttribute("Entry", entry);
                    entryScoreElement.SetAttribute("Score", _EntryTextBoxMapping[entry].Text);
                    gradeCalcScoreElement.AppendChild(entryScoreElement);
                }
            }
            DSXmlHelper requesthelper = new DSXmlHelper("UpdateStudentList");
            requesthelper.AddElement("Student");
            requesthelper.AddElement("Student", "Field");
            requesthelper.AddElement("Student/Field", "GradScore");
            requesthelper.AddElement("Student/Field/GradScore", gradeCalcScoreElement);
            requesthelper.AddElement("Student", "Condition");
            requesthelper.AddElement("Student/Condition", "ID", _CurrentID);
            SmartSchool.Feature.EditStudent.Update(new DSRequest(requesthelper));


            #region 處理Log
            StringBuilder desc = new StringBuilder("");
            AccessHelper accessHelper = new AccessHelper();
            desc.AppendLine("學生姓名：" + (accessHelper.StudentHelper.GetStudents(_CurrentID).Count > 0 ? accessHelper.StudentHelper.GetStudents(_CurrentID)[0].StudentName : "未知"));
            //desc.AppendLine("學生姓名：" + SmartSchool.StudentRelated.Student.Instance.Items[_CurrentID].Name + " ");
            desc.AppendLine("畢業成績：");
            foreach (string entry in _EntryTextBoxMapping.Keys)
            {
                if (_EntryTextBoxMapping[entry].Text != "" + _EntryTextBoxMapping[entry].Tag)
                    desc.AppendLine("「" + entry + "成績」由「" + _EntryTextBoxMapping[entry].Tag + "」變更為「" + _EntryTextBoxMapping[entry].Text + "」");
                //修改原值設定為新值
                _EntryTextBoxMapping[entry].Tag = _EntryTextBoxMapping[entry].Text;
            }
            CurrentUser.Instance.AppLog.Write(EntityType.Student, EntityAction.Update, _CurrentID, desc.ToString(), this.Title, requesthelper.GetRawXml());
            #endregion
            CheckChanged();
        }

        public bool SaveButtonVisible
        {
            get { return _SaveButtonVisible; }
            private set
            {
                if (_SaveButtonVisible != value)
                {
                    _SaveButtonVisible = value;
                    if (SaveButtonVisibleChanged != null)
                        SaveButtonVisibleChanged.Invoke(this, new EventArgs());
                }
            }
        }

        public event EventHandler SaveButtonVisibleChanged;

        public string Title
        {
            get { return "畢業成績"; }
        }

        public void Undo()
        {
            LoadContent(_CurrentID);
        }

        #endregion

        #region ICloneable 成員

        public object Clone()
        {
            return new GradScorePalmerworm();
        }

        #endregion

        private void textBoxX1_TextChanged(object sender, EventArgs e)
        {
            CheckChanged();
        }

        private void CheckChanged()
        {
            bool changed = false;
            bool vailate = true;
            foreach (TextBox textbox in _EntryTextBoxMapping.Values)
            {
                if (textbox.Text != "" + textbox.Tag)
                {
                    changed = true;
                    decimal d;
                    bool pass = textbox.Text == "" || decimal.TryParse(textbox.Text, out d);
                    vailate &= pass;
                    if (pass)
                        ResetErrorProvider(textbox);
                    else
                        SetErrorProvider(textbox, "需輸入數字。");
                }
            }
            CancelButtonVisible = changed;
            SaveButtonVisible = changed & vailate;

            if (!SaveButtonVisible)
                ClearErrors();
        }

        private Dictionary<Control, ErrorProvider> _errorProviderDictionary = new Dictionary<Control, ErrorProvider>();

        private void SetErrorProvider(Control control, string p)
        {
            if (!_errorProviderDictionary.ContainsKey(control))
            {
                ErrorProvider ep = new ErrorProvider();
                ep.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                ep.SetIconAlignment(control, ErrorIconAlignment.MiddleRight);
                //ep.Icon = Properties.Resources.Info3D;
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
    }
}

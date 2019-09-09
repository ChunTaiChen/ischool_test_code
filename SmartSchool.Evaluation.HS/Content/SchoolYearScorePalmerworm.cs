using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.AccessControl;
using SmartSchool.ApplicationLog;
using SmartSchool.Common;
using SmartSchool.Feature.Score;
using SmartSchool.StudentRelated;

namespace SmartSchool.Evaluation.Content
{
    [FeatureCode("Content0090")]
    public partial class SchoolYearScorePalmerworm : UserControl, SmartSchool.Customization.PlugIn.ExtendedContent.IContentItem, IPreference
    {
        BackgroundWorker _bkwEntryLoader = new BackgroundWorker();

        BackgroundWorker _bkwSubjectLoader = new BackgroundWorker();

        string _RunningEntryID;

        string _RunningSubjectID;

        string _CurrentID;

        DSResponse _EntryResponse;

        DSResponse _SubjectResponse;

        bool Reload = false;

        private bool WaitingPicVisible { set { this.picWaiting.Visible = value; } }

        public static string FeatureCode = "";
        private FeatureAce _permission;

        public SchoolYearScorePalmerworm()
        {
            InitializeComponent();
            EventHub.Instance.ScoreChanged += new EventHandler<ScoreChangedEventArgs>(Instance_ScoreChanged);
            PreferenceUpdater.Instance.Items.Add(this);
            LoadPreference();
            lvColumnManager1.DenyColumns.AddRange(new ColumnHeader[] { colSchoolYear, colGradeYear });
            _bkwEntryLoader.DoWork += new DoWorkEventHandler(_bkwEntryLoader_DoWork);
            _bkwEntryLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bkwEntryLoader_RunWorkerCompleted);
            _bkwSubjectLoader.DoWork += new DoWorkEventHandler(_bkwSubjectLoader_DoWork);
            _bkwSubjectLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bkwSubjectLoader_RunWorkerCompleted);
            Application.Idle += new EventHandler(Application_Idle);

            //取得此 Class 定議的 FeatureCode。
            FeatureCodeAttribute code = Attribute.GetCustomAttribute(this.GetType(), typeof(FeatureCodeAttribute)) as FeatureCodeAttribute;
            FeatureCode = code.FeatureCode;

            _permission = CurrentUser.Acl[FeatureCode];

            btnAdd.Visible = _permission.Editable;
            btnDelete.Visible = _permission.Editable;
            btnModify.Visible = _permission.Editable;

            btnView.Visible = !_permission.Editable;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            if (Reload)
            {
                LoadContent(_CurrentID);
                Reload = false;
            }
        }

        private void Instance_ScoreChanged(object sender, ScoreChangedEventArgs e)
        {
            foreach (string var in e.StudentIds)
            {
                if (var == _CurrentID)
                {
                    Reload = true;
                }
            }
        }

        private void FillScore()
        {
            if (_EntryResponse == null) return;
            if (_SubjectResponse == null) return;

            WaitingPicVisible = false;
            Dictionary<string, string[]> EntryScore = new Dictionary<string, string[]>();
            Dictionary<string, decimal> Credit = new Dictionary<string, decimal>();
            foreach (XmlElement var in _SubjectResponse.GetContent().GetElements("SemesterSubjectScore"))
            {
                #region 統計取得學分數
                string schoolyear = var.SelectSingleNode("SchoolYear").InnerText;
                string gradeyear = var.SelectSingleNode("GradeYear").InnerText;
                decimal creditCount = 0;
                foreach (XmlNode cnode in var.SelectNodes("ScoreInfo/SemesterSubjectScoreInfo/Subject"))
                {
                    decimal credit = decimal.Parse(cnode.SelectSingleNode("@開課學分數").InnerText);
                    bool getCredit = cnode.SelectSingleNode("@是否取得學分").InnerText == "是";
                    bool notIncludedInCredit = cnode.SelectSingleNode("@不計學分").InnerText == "是";
                    if (getCredit && !notIncludedInCredit)
                    {
                        creditCount += credit;
                    }
                }
                if (Credit.ContainsKey(schoolyear + "_" + gradeyear))
                {
                    Credit[schoolyear + "_" + gradeyear] += creditCount;
                }
                else
                    Credit.Add(schoolyear + "_" + gradeyear, creditCount);
                #endregion
            }
            foreach (XmlElement var in _EntryResponse.GetContent().GetElements("SchoolYearEntryScore"))
            {
                #region 填入分項成績資料
                string schoolyear = var.SelectSingleNode("SchoolYear").InnerText;
                string gradeyear = var.SelectSingleNode("GradeYear").InnerText;
                string[] subItems;
                if (EntryScore.ContainsKey(schoolyear + "_" + gradeyear))
                {
                    subItems = EntryScore[schoolyear + "_" + gradeyear];
                }
                else
                {
                    subItems = new string[10];
                    subItems[0] = schoolyear;
                    subItems[1] = gradeyear;
                    EntryScore.Add(schoolyear + "_" + gradeyear, subItems);
                }
                foreach (XmlNode score in var.SelectNodes("ScoreInfo/SchoolYearEntryScore/Entry"))
                {
                    XmlElement element = (XmlElement)score;
                    #region 依分項填入格子
                    switch (element.GetAttribute("分項").Trim())
                    {
                        case "學業":
                            subItems[2] = element.GetAttribute("成績");
                            break;
                        case "實習科目":
                            subItems[3] = element.GetAttribute("成績");
                            break;
                        case "專業科目":
                            subItems[4] = element.GetAttribute("成績");
                            break;
                        case "體育":
                            subItems[5] = element.GetAttribute("成績");
                            break;
                        case "國防通識":
                            subItems[6] = element.GetAttribute("成績");
                            break;
                        case "健康與護理":
                            subItems[7] = element.GetAttribute("成績");
                            break;
                        case "德行":
                            subItems[8] = element.GetAttribute("成績");
                            break;
                        default:
                            //throw new Exception("拎唄謀洗鰓機雷分項： " + element.GetAttribute("分項"));
                            break;
                    }
                    #endregion
                }
                #endregion
            }
            #region 填入總學分數
            foreach (string var in Credit.Keys)
            {
                string[] subItems;
                if (EntryScore.ContainsKey(var))
                {
                    subItems = EntryScore[var];
                }
                else
                {
                    subItems = new string[10];
                    subItems[0] = var.Split("_".ToCharArray())[0];
                    subItems[1] = var.Split("_".ToCharArray())[1];
                    EntryScore.Add(var, subItems);
                }
                subItems[9] = Credit[var].ToString();
            }
            #endregion
            foreach (string[] subItems in EntryScore.Values)
            {
                ListViewItem item = new ListViewItem(subItems);
                listView1.Items.Add(item);
            }
            lvColumnManager1.Reflash();
        }

        void _bkwSubjectLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_RunningEntryID != _CurrentID)
            {
                _RunningEntryID = _CurrentID;
                _bkwSubjectLoader.RunWorkerAsync(_CurrentID);
                return;
            }
            _SubjectResponse = (DSResponse)e.Result;
            FillScore();
        }

        void _bkwSubjectLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = QueryScore.GetSemesterSubjectScore(e.Argument.ToString());
        }

        void _bkwEntryLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_RunningEntryID != _CurrentID)
            {
                _RunningEntryID = _CurrentID;
                _bkwEntryLoader.RunWorkerAsync(_CurrentID);
                return;
            }
            _EntryResponse = (DSResponse)e.Result;
            FillScore();
        }

        void _bkwEntryLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = QueryScore.GetSchoolYearEntryScore(e.Argument.ToString());
        }

        public void LoadContent(string id)
        {
            btnModify.Enabled = btnDelete.Enabled = false;
            listView1.Items.Clear();
            _CurrentID = id;
            if (!_bkwEntryLoader.IsBusy)
            {
                _RunningEntryID = id;
                _EntryResponse = null;
                _bkwEntryLoader.RunWorkerAsync(id);
                WaitingPicVisible = true;
            }
            if (!_bkwSubjectLoader.IsBusy)
            {
                _RunningSubjectID = id;
                _SubjectResponse = null;
                _bkwSubjectLoader.RunWorkerAsync(id);
                WaitingPicVisible = true;
            }
        }
        public void Save()
        {
            //base.Save();
        }
        public void Undo()
        {
            LoadContent(_CurrentID);
        }

        public void UpdatePreference()
        {
            if (this.IsDisposed)
                return;
            #region 紀錄欄位顯示
            #region 取得PreferenceElement
            XmlElement PreferenceElement = CurrentUser.Instance.Preference["SchoolYearScorePalmerworm"];
            if (PreferenceElement == null)
            {
                PreferenceElement = new XmlDocument().CreateElement("SchoolYearScorePalmerworm");
            }
            #endregion
            //紀錄顯示欄位
            PreferenceElement.SetAttribute("col健康與護理Visible", listView1.Columns.Contains(col健康與護理).ToString());
            PreferenceElement.SetAttribute("col國防通識Visible", listView1.Columns.Contains(col國防通識).ToString());
            PreferenceElement.SetAttribute("col實得學分Visible", listView1.Columns.Contains(col實得學分).ToString());
            PreferenceElement.SetAttribute("col實習科目Visible", listView1.Columns.Contains(col實習科目).ToString());
            PreferenceElement.SetAttribute("col德行成績Visible", listView1.Columns.Contains(col德行成績).ToString());
            PreferenceElement.SetAttribute("col學業成績Visible", listView1.Columns.Contains(col學業成績).ToString());
            PreferenceElement.SetAttribute("col體育成績Visible", listView1.Columns.Contains(col體育成績).ToString());
            PreferenceElement.SetAttribute("col專業科目Visible", listView1.Columns.Contains(col專業科目).ToString());
            CurrentUser.Instance.Preference["SchoolYearScorePalmerworm"] = PreferenceElement;
            #endregion
        }

        private void LoadPreference()
        {
            XmlElement PreferenceData = CurrentUser.Instance.Preference["SchoolYearScorePalmerworm"];
            if (PreferenceData != null)
            {
                string[] attributeList = new string[] { "col健康與護理Visible", "col國防通識Visible", "col實得學分Visible", "col實習科目Visible", "col德行成績Visible", "col學業成績Visible", "col體育成績Visible", " col專業科目Visible" };
                ColumnHeader[] columnList = new ColumnHeader[] { col健康與護理, col國防通識, col實得學分, col實習科目, col德行成績, col學業成績, col體育成績, col專業科目 };
                for (int i = 0; i < attributeList.Length; i++)
                {
                    if (PreferenceData.HasAttribute(attributeList[i]))
                    {
                        bool show = bool.Parse(PreferenceData.GetAttribute(attributeList[i]));
                        if (!show)
                            lvColumnManager1.HideColumn(columnList[i]);
                    }
                }
            }
            else
                //預設顯示欄位
                lvColumnManager1.HideColumn(new ColumnHeader[] { col體育成績, col國防通識, col健康與護理, col實習科目, col專業科目 });
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnView.Enabled = (listView1.SelectedIndices.Count == 1);
            btnModify.Enabled = (listView1.SelectedIndices.Count == 1 && _permission.Viewable);
            btnDelete.Enabled = (listView1.SelectedIndices.Count == 1 && _permission.Editable);
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            if (MsgBox.Show("您確定要刪除此學年成績？", "", MessageBoxButtons.YesNo) == DialogResult.No) return;

            foreach (XmlElement var in QueryScore.GetSchoolYearEntryScoreBySchoolYearGradeYear(int.Parse(listView1.SelectedItems[0].SubItems[0].Text), int.Parse(listView1.SelectedItems[0].SubItems[1].Text), _CurrentID).GetContent().GetElements("SchoolYearEntryScore"))
            {
                RemoveScore.DeleteSchoolYearEntityScore(var.SelectSingleNode("@ID").InnerText);
            }
            foreach (XmlElement var in QueryScore.GetSchoolYearSubjectScoreBySchoolYearGradeYear(int.Parse(listView1.SelectedItems[0].SubItems[0].Text), int.Parse(listView1.SelectedItems[0].SubItems[1].Text), _CurrentID).GetContent().GetElements("SchoolYearSubjectScore"))
            {
                RemoveScore.DeleteSchoolYearSubjectScore(var.SelectSingleNode("@ID").InnerText);
            }

            // 刪除學年成績 log
            StringBuilder deleteDesc = new StringBuilder("");
            deleteDesc.AppendLine("學生姓名：" + Student.Instance.Items[_CurrentID].Name + " ");
            deleteDesc.AppendLine("刪除 " + listView1.SelectedItems[0].SubItems[0].Text + " 學年度 學年成績");
            CurrentUser.Instance.AppLog.Write(EntityType.Student, EntityAction.Delete, _CurrentID, deleteDesc.ToString(), Title, "");

            EventHub.Instance.InvokScoreChanged(_CurrentID);
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            ScoreEditor.SchoolYearScoreEditor editor = new ScoreEditor.SchoolYearScoreEditor(_CurrentID);
            editor.FormClosed += new FormClosedEventHandler(editor_FormClosed);
            editor.ShowDialog();
        }

        void editor_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form form = (Form)sender;
            form.FormClosed -= new FormClosedEventHandler(editor_FormClosed);
            //LoadById(_CurrentID);
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            ScoreEditor.SchoolYearScoreEditor editor = new ScoreEditor.SchoolYearScoreEditor(listView1.SelectedItems[0].SubItems[0].Text, listView1.SelectedItems[0].SubItems[1].Text, _CurrentID);
            editor.FormClosed += new FormClosedEventHandler(editor_FormClosed);
            editor.ShowDialog();
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            ScoreEditor.SchoolYearScoreEditor editor = new ScoreEditor.SchoolYearScoreEditor(listView1.SelectedItems[0].SubItems[0].Text, listView1.SelectedItems[0].SubItems[1].Text, _CurrentID);
            editor.FormClosed += new FormClosedEventHandler(editor_FormClosed);
            editor.ShowDialog();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedIndices.Count == 1)
            {
                //MotherForm.SetWaitCursor();
                ScoreEditor.SchoolYearScoreEditor editor = new ScoreEditor.SchoolYearScoreEditor(listView1.SelectedItems[0].SubItems[0].Text, listView1.SelectedItems[0].SubItems[1].Text, _CurrentID);
                editor.FormClosed += new FormClosedEventHandler(editor_FormClosed);
                //MotherForm.ResetWaitCursor();
                editor.ShowDialog();
            }
        }

        #region IContentItem 成員

        public bool CancelButtonVisible
        {
            get { return false; }
        }

        public event EventHandler CancelButtonVisibleChanged;

        public Control DisplayControl
        {
            get { return this; }
        }

        public bool SaveButtonVisible
        {
            get { return false; }
        }

        public event EventHandler SaveButtonVisibleChanged;

        public string Title
        {
            get { return "學年成績"; }
        }

        #endregion

        #region ICloneable 成員

        public object Clone()
        {
            return new SchoolYearScorePalmerworm();
        }

        #endregion
    }
}

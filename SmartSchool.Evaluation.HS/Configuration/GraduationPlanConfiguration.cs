using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using FISCA.Presentation;
using SmartSchool.Common;
using SmartSchool.Customization.Data;
using SmartSchool.Evaluation.GraduationPlan;
using SmartSchool.Evaluation.GraduationPlan.Editor;
//using SmartSchool.ClassRelated;
using SmartSchool.Feature.GraduationPlan;

namespace SmartSchool.Evaluation.Configuration
{
    public partial class GraduationPlanConfiguration : SmartSchool.Customization.PlugIn.Configure.ConfigurationItem
    {
        private Dictionary<GraduationPlanInfo, ButtonItem> _InfoDictionary = new Dictionary<GraduationPlanInfo, ButtonItem>();

        private IGraduationPlanEditor _GraduationPlanEditor;

        private ButtonItem _SelectButton;

        private bool _DataLoading;

        private BackgroundWorker _BKWGraduationPlanLoader;

        private BackgroundWorker _BKWChecker;

        private Dictionary<GraduationPlanInfo, ButtonItem> _GPlanMapping;

        private AccessHelper _AccessHelper = new AccessHelper();  

        public GraduationPlanConfiguration()
        {
            InitializeComponent();
        }

        protected override void OnActive()
        {
            _BKWChecker = new BackgroundWorker();
            _BKWChecker.DoWork += new DoWorkEventHandler(_BKWChecker_DoWork);
            _BKWChecker.ProgressChanged += new ProgressChangedEventHandler(_BKWChecker_ProgressChanged);
            _BKWChecker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BKWChecker_RunWorkerCompleted);
            _BKWChecker.WorkerSupportsCancellation = true;
            _BKWChecker.WorkerReportsProgress = true;

            _GraduationPlanEditor = graduationPlanEditor1;
            _GraduationPlanEditor.IsDirtyChanged += new EventHandler(_GraduationPlanEditor_IsDirtyChanged);
            _BKWGraduationPlanLoader = new BackgroundWorker();
            _BKWGraduationPlanLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BKWGraduationPlanLoader_RunWorkerCompleted);
            _BKWGraduationPlanLoader.DoWork += new DoWorkEventHandler(_BKWGraduationPlanLoader_DoWork);

            LoadGraduationPlan(false);
            itemPanel1_SizeChanged(null, null);

            EventHub.Instance.GraduationPlanDeleted += new EventHandler<DeleteGraduationPlanEventArgs>(Instance_GraduationPlanDeleted);
            EventHub.Instance.GraduationPlanInserted += new EventHandler(Instance_GraduationPlanInserted);
            EventHub.Instance.GraduationPlanUpdated += new EventHandler<UpdateGraduationPlanEventArgs>(Instance_GraduationPlanUpdated);
        }


        void Instance_GraduationPlanUpdated(object sender, UpdateGraduationPlanEventArgs e)
        {
            if (_InfoDictionary.ContainsKey(e.OldInfo))
            {
                ButtonItem item = _InfoDictionary[e.OldInfo];
                if (e.NewInfo != null)
                {
                    item.Tag = e.NewInfo;
                    //item.Text = e.NewInfo.Name;

                    SmartSchool.Evaluation.GraduationPlan.Validate.ValidateGraduationPlanInfo validater = new SmartSchool.Evaluation.GraduationPlan.Validate.ValidateGraduationPlanInfo();
                    if (validater.Validate(e.NewInfo, null))
                    {
                        item.Tooltip = "";
                        item.ButtonStyle = eButtonStyle.TextOnlyAlways;
                        item.Image = null;
                        item.Refresh();
                    }
                    else
                    {
                        item.Tooltip = "驗證失敗，請檢查內容。\n否則使用此規劃表之學生將無法加入修課。";
                        item.Image = Properties.Resources.warning1;
                        item.ButtonStyle = eButtonStyle.TextOnlyAlways;
                        item.Refresh();
                    }
                }
                else
                {
                    itemPanel1.Items.Remove(_InfoDictionary[e.OldInfo]);
                    _InfoDictionary.Remove(e.OldInfo);
                }
            }
            else
                LoadGraduationPlan(false);
        }

        void Instance_GraduationPlanInserted(object sender, EventArgs e)
        {
            LoadGraduationPlan(false);
        }

        void Instance_GraduationPlanDeleted(object sender, DeleteGraduationPlanEventArgs e)
        {
            tabControl2.Visible = false;
            _SelectButton = null;
            foreach (GraduationPlanInfo var in _InfoDictionary.Keys)
            {
                if (var.ID == e.ID)
                {
                    itemPanel1.Items.Remove(_InfoDictionary[var]);
                    _InfoDictionary.Remove(var);
                    break;
                }
            }
        }

        private void StartCheck()
        {
            _GPlanMapping = new Dictionary<GraduationPlanInfo, ButtonItem>();
            List<GraduationPlanInfo> gplanList = new List<GraduationPlanInfo>();
            foreach (ButtonItem item in itemPanel1.Items)
            {
                GraduationPlanInfo gplan = (GraduationPlanInfo)item.Tag;
                _GPlanMapping.Add(gplan, item);
                gplanList.Add(gplan);
            }
            _BKWChecker.RunWorkerAsync(gplanList);
        }

        void _BKWChecker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                StartCheck();
            }
            else
            {
                MotherForm.SetStatusBarMessage("驗證課程規劃表完成");
            }
        }

        void _BKWChecker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MotherForm.SetStatusBarMessage("驗證課程規劃表", e.ProgressPercentage);
            if (!(bool)((object[])e.UserState)[0])
            {
                ButtonItem item;
                if (_GPlanMapping.TryGetValue((GraduationPlanInfo)((object[])e.UserState)[1], out item))
                {
                    item.ButtonStyle = eButtonStyle.ImageAndText;
                    item.Tooltip = "驗證失敗，請檢查內容。\n否則使用此規劃表之學生將無法加入修課。";
                    item.Image = Properties.Resources.warning1;
                    item.Refresh();
                }
            }
        }

        void _BKWChecker_DoWork(object sender, DoWorkEventArgs e)
        {
            SmartSchool.Evaluation.GraduationPlan.Validate.ValidateGraduationPlanInfo validater = new SmartSchool.Evaluation.GraduationPlan.Validate.ValidateGraduationPlanInfo();
            List<GraduationPlanInfo> gplanList = (List<GraduationPlanInfo>)e.Argument;
            for (int i = 0; i < gplanList.Count; i++)
            {
                if (_BKWChecker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                bool pass = validater.Validate(gplanList[i], null);
                _BKWChecker.ReportProgress((int)(((double)i * 100.0f) / gplanList.Count), new object[] { pass, gplanList[i] });
            }
        }

        void _GraduationPlanEditor_IsDirtyChanged(object sender, EventArgs e)
        {
            buttonX2.Enabled = _GraduationPlanEditor.IsDirty;
            labelX1.Text = labelX2.Text + (_GraduationPlanEditor.IsDirty ? " (<font color=\"Chocolate\">已變更</font>)" : "");
        }

        public void LoadGraduationPlan(bool reflash)
        {
            if (!_BKWGraduationPlanLoader.IsBusy)
            {
                foreach (BaseItem item in itemPanel1.Items)
                {
                    item.Click -= new EventHandler(item_Click);
                }
                _SelectButton = null;
                itemPanel1.Items.Clear();
                _InfoDictionary = new Dictionary<GraduationPlanInfo, ButtonItem>();
                setDataLoading();
                _BKWGraduationPlanLoader.RunWorkerAsync(reflash);
            }
        }

        private void _BKWGraduationPlanLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            if ((bool)e.Argument) SmartSchool.Evaluation.GraduationPlan.GraduationPlan.Instance.Reflash();
            e.Result = SmartSchool.Evaluation.GraduationPlan.GraduationPlan.Instance.Items;
        }

        private void _BKWGraduationPlanLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SmartSchool.Evaluation.GraduationPlan.GraduationPlanInfoCollection resp = (SmartSchool.Evaluation.GraduationPlan.GraduationPlanInfoCollection)e.Result;
            tabControl2.Visible = false;
            _SelectButton = null;
            foreach (GraduationPlanInfo gPlan in resp)
            {
                ButtonItem item = new ButtonItem(gPlan.ID, gPlan.Name);
                item.Tag = gPlan;
                itemPanel1.Items.Add(item);
                item.ImagePosition = eImagePosition.Left;
                item.ImageFixedSize = new Size(14, 14);
                item.Image = null;
                item.ButtonStyle = eButtonStyle.TextOnlyAlways;
                item.Click += new EventHandler(item_Click);
                _InfoDictionary.Add(gPlan, item);
            }
            itemPanel1.Refresh();
            resetDataLoading();

            if (_BKWChecker.IsBusy)
            {
                _BKWChecker.CancelAsync();
            }
            else
            {
                StartCheck();
            }
        }

        private void item_Click(object sender, EventArgs e)
        {
            if (_SelectButton != null)
                _SelectButton.Checked = false;
            ButtonItem item = (ButtonItem)sender;
            GraduationPlanInfo info = (GraduationPlanInfo)item.Tag;
            _SelectButton = item;
            item.Checked = true;

            tabControl2.Visible = true & (_SelectButton != null);
            tabControlPanel3.Visible = tabControlPanel2.Visible = tabItem2.Visible = tabItem1.Visible = true;
            tabControl2.SelectedTab = tabItem1;
            tabControl2.SelectedPanel = tabControlPanel2;

            labelX2.Text = labelX1.Text = item.Text;
            _GraduationPlanEditor.SetSource(info.GraduationPlanElement);
            listViewEx1.SuspendLayout();
            listViewEx1.Items.Clear();
            listViewEx1.Groups.Clear();
            Dictionary<ClassRecord, int> classCount = new Dictionary<ClassRecord, int>();
            List<StudentRecord> noClassStudents = new List<StudentRecord>();
            foreach ( StudentRecord stu in _AccessHelper .StudentHelper.GetAllStudent())
            {
                if ( GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(stu.StudentID) == info )
                {
                    if ( stu.RefClass != null )
                    {
                        if ( !classCount.ContainsKey(stu.RefClass) )
                            classCount.Add(stu.RefClass, 0);
                        classCount[stu.RefClass]++;
                    }
                    else
                    {
                        noClassStudents.Add(stu);
                    }
                }
            }
            foreach ( ClassRecord var in classCount.Keys )
            {
                string groupKey;
                int a;
                if ( int.TryParse(var.GradeYear, out a) )
                {
                    groupKey = var.GradeYear + "　年級";
                }
                else
                    groupKey = var.GradeYear;
                ListViewGroup group = listViewEx1.Groups[groupKey];
                if ( group == null )
                    group = listViewEx1.Groups.Add(groupKey, groupKey);
                listViewEx1.Items.Add(new ListViewItem(var.ClassName + "(" + classCount[var] + ")　", 0, group));
            }
            if ( noClassStudents.Count > 0 )
            {
                ListViewGroup group = listViewEx1.Groups["未分班"];
                if ( group == null )
                    group = listViewEx1.Groups.Add("未分班", "未分班");
                foreach ( StudentRecord stu in noClassStudents )
                {
                    listViewEx1.Items.Add(new ListViewItem(stu.StudentName + "[" + stu.StudentNumber + "] 　", 1, group));
                }
            }
            listViewEx1.ResumeLayout();
            tabControl2.Visible = true;
            buttonX3.Enabled = true;
        }
        /// <summary>
        /// 設定顯示執行中圖示
        /// </summary>
        private void setDataLoading()
        {
            _DataLoading = true;
            if (_WaitingPicture != null)
                _WaitingPicture.Visible = (_DataLoading); //& expandablePanel1.Expanded;
        }

        /// <summary>
        /// 設定隱藏執行中圖示
        /// </summary>
        private void resetDataLoading()
        {
            _DataLoading = false;
            if (_WaitingPicture != null)
                _WaitingPicture.Visible = (_DataLoading);// &expandablePanel1.Expanded;
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            if (_SelectButton != null)
            {
                if (!_GraduationPlanEditor.IsValidated)
                {
                    MsgBox.Show("課程資料表內容輸入錯誤，請檢查輸入資料。");
                    return;
                }

                EditGraduationPlan.Update(_SelectButton.Name, _GraduationPlanEditor.GetSource());
                //SmartSchool.GraduationPlanRelated.GraduationPlan.Instance.Reflash();
                //bool RemoveNode=true;
                //foreach (GraduationPlanInfo info in SmartSchool.GraduationPlanRelated.GraduationPlan.Instance.Items)
                //{
                //    if (info.ID == _SelectButton.Name)
                //    {
                //        _SelectButton.Tag = info;
                //        _SelectButton.Text = info.Name;
                //        SmartSchool.GraduationPlanRelated.Validate.ValidateGraduationPlanInfo validater = new SmartSchool.GraduationPlanRelated.Validate.ValidateGraduationPlanInfo();
                //        if (validater.Validate(info, null))
                //        {
                //            _SelectButton.Tooltip = "驗證失敗，請檢查內容。\n否則使用此規劃表之學生將無法加入修課。";
                //            _SelectButton.Image = Properties.Resources.warning1;
                //            _SelectButton.Refresh();
                //        }
                //        else
                //        {
                //            _SelectButton.Tooltip = "";
                //            _SelectButton.Image = null;
                //            _SelectButton.Refresh();
                //        }

                //        RemoveNode=false;
                //        item_Click(_SelectButton, new EventArgs());
                //        break;
                //    }
                //}
                //if (RemoveNode)
                //{
                //    itemPanel1.Items.Remove(_SelectButton);
                //    tabControl2.Visible = false;
                //}
                EventHub.Instance.InvokGraduationPlanUpdated(_SelectButton.Name);
            }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            new GraduationPlanCreator().ShowDialog();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            LoadGraduationPlan(true);
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            if (_SelectButton != null)
            {
                if (MsgBox.Show("確定要刪除 '" + _SelectButton.Text + "' 課程規劃表？", "確定", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    RemoveGraduationPlan.Delete(_SelectButton.Name);
                    EventHub.Instance.InvokGraduationPlanDeleted(_SelectButton.Name);
                }
            }
        }

        private void itemPanel1_MouseHover(object sender, EventArgs e)
        {
            if (!itemPanel1.ContainsFocus)
                itemPanel1.Focus();
        }

        private void listViewEx1_MouseHover(object sender, EventArgs e)
        {
            if ( this.TopLevelControl != null && this.TopLevelControl.ContainsFocus && !listViewEx1.ContainsFocus )
                listViewEx1.Focus();
        }

        private void itemPanel1_SizeChanged(object sender, EventArgs e)
        {
            _WaitingPicture.Location = new Point((itemPanel1.Width - _WaitingPicture.Width) / 2, (itemPanel1.Height - _WaitingPicture.Height) / 2);
        }
    }
}

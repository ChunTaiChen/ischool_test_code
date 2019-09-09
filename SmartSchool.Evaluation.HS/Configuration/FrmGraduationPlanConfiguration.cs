using DevComponents.AdvTree;
using DevComponents.DotNetBar;
using FISCA.Presentation;
using FISCA.Presentation.Controls;
using SmartSchool.Customization.Data;
using SmartSchool.Evaluation.GraduationPlan;
using SmartSchool.Evaluation.GraduationPlan.Editor;
using SmartSchool.Feature.GraduationPlan;
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
    public partial class FrmGraduationPlanConfiguration : BaseForm
    {
        private Dictionary<GraduationPlanInfo, Node> _InfoDictionary = new Dictionary<GraduationPlanInfo, Node>();
        private IGraduationPlanEditor _GraduationPlanEditor;
        private Node _SelectItem;
        private bool _DataLoading;
        private BackgroundWorker _BKWGraduationPlanLoader;
        private BackgroundWorker _BKWChecker;
        private Dictionary<GraduationPlanInfo, Node> _GPlanMapping;
        private AccessHelper _AccessHelper = new AccessHelper();
        public static Dictionary<string, bool> _AdvTreeExpandStatus = new Dictionary<string, bool>();

        public FrmGraduationPlanConfiguration()
        {
            InitializeComponent();

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

            // itemPanel1_SizeChanged(null, null);

            EventHub.Instance.GraduationPlanDeleted += new EventHandler<DeleteGraduationPlanEventArgs>(Instance_GraduationPlanDeleted);
            EventHub.Instance.GraduationPlanInserted += new EventHandler(Instance_GraduationPlanInserted);
            EventHub.Instance.GraduationPlanUpdated += new EventHandler<UpdateGraduationPlanEventArgs>(Instance_GraduationPlanUpdated);

            _AdvTreeExpandStatus.Clear();

            this.imageList1.Images.Clear();
            this.imageList1.Images.Add("0", Properties.Resources.classroom_64);
            this.imageList1.Images.Add("1", Properties.Resources.elementary_school_64);

        }

        private void FrmGraduationPlanConfiguration_Load(object sender, EventArgs e)
        {
            LoadGraduationPlan(false);

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

        void _BKWChecker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MotherForm.SetStatusBarMessage("驗證課程規劃表", e.ProgressPercentage);
            if (!(bool)((object[])e.UserState)[0])
            {
                Node item;
                if (_GPlanMapping.TryGetValue((GraduationPlanInfo)((object[])e.UserState)[1], out item))
                {
                    SetWarningNode(item, true);
                }
            }
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

        void _GraduationPlanEditor_IsDirtyChanged(object sender, EventArgs e)
        {
            btnUpdate.Enabled = _GraduationPlanEditor.IsDirty;
            labelX1.Text = labelX2.Text + (_GraduationPlanEditor.IsDirty ? " (<font color=\"Chocolate\">已變更</font>)" : "");
        }

        private void _BKWGraduationPlanLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SmartSchool.Evaluation.GraduationPlan.GraduationPlanInfoCollection resp = (SmartSchool.Evaluation.GraduationPlan.GraduationPlanInfoCollection)e.Result;
            tabControl2.Visible = false;

            FillAdvTreeItem(resp);

            setDataLoading(false);

            if (_BKWChecker.IsBusy)
            {
                _BKWChecker.CancelAsync();
            }
            else
            {
                StartCheck();
            }
        }

        private void _BKWGraduationPlanLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            if ((bool)e.Argument) SmartSchool.Evaluation.GraduationPlan.GraduationPlan.Instance.Reflash();
            e.Result = SmartSchool.Evaluation.GraduationPlan.GraduationPlan.Instance.Items;
        }

        void Instance_GraduationPlanDeleted(object sender, DeleteGraduationPlanEventArgs e)
        {
            tabControl2.Visible = false;
            _SelectItem = null;
            foreach (GraduationPlanInfo var in _InfoDictionary.Keys)
            {
                if (var.ID == e.ID)
                {
                    RemoveAdvTreeItem(e.ID);
                    _InfoDictionary.Remove(var);
                    break;
                }
            }
        }

        void Instance_GraduationPlanInserted(object sender, EventArgs e)
        {
            LoadGraduationPlan(false);
        }

        void Instance_GraduationPlanUpdated(object sender, UpdateGraduationPlanEventArgs e)
        {
            LoadGraduationPlan(false);
        }

        private void StartCheck()
        {
            _GPlanMapping = new Dictionary<GraduationPlanInfo, Node>();
            List<GraduationPlanInfo> gplanList = new List<GraduationPlanInfo>();
            foreach (Node node in advTree1.Nodes)
            {
                foreach (Node childNode in node.Nodes)
                {
                    GraduationPlanInfo gplan = (GraduationPlanInfo)childNode.Tag;
                    _GPlanMapping.Add(gplan, childNode);
                    gplanList.Add(gplan);
                }
            }
            _BKWChecker.RunWorkerAsync(gplanList);
        }

        private void listViewEx1_MouseHover(object sender, EventArgs e)
        {
            if (this.TopLevelControl != null && this.TopLevelControl.ContainsFocus && !listViewEx1.ContainsFocus)
                listViewEx1.Focus();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            SaveAdvTreeExpandStatus();
            if (new GraduationPlanCreator().ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _SelectItem = null;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_SelectItem == null) return;

            if (!_GraduationPlanEditor.IsValidated)
            {
                MsgBox.Show("課程資料表內容輸入錯誤，請檢查輸入資料。");
                return;
            }

            SaveAdvTreeExpandStatus();

            string schoolYear = _SelectItem.Parent.TagString;
            string id = _SelectItem.Name;
            string name = schoolYear + (_SelectItem.Tag as GraduationPlanInfo).TrimName;

            EditGraduationPlan.Update(_SelectItem.Name, _GraduationPlanEditor.GetSource(schoolYear));

            EventHub.Instance.InvokGraduationPlanUpdated(_SelectItem.Name);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_SelectItem == null) return;

            if (MsgBox.Show("確定要刪除 '" + GetNodeFullPath(_SelectItem) + "' 課程規劃表？", "確定", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                SaveAdvTreeExpandStatus();
                RemoveGraduationPlan.Delete(_SelectItem.Name);
                EventHub.Instance.InvokGraduationPlanDeleted(_SelectItem.Name);
                _SelectItem = null;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveAdvTreeExpandStatus();
            LoadGraduationPlan(true);
        }

        public void LoadGraduationPlan(bool reflash)
        {
            if (!_BKWGraduationPlanLoader.IsBusy)
            {
                _InfoDictionary = new Dictionary<GraduationPlanInfo, Node>();
                BtnEnabled(false);
                setDataLoading(true);
                _BKWGraduationPlanLoader.RunWorkerAsync(reflash);
            }
        }

        /// <summary>
        /// 設定顯示執行中圖示
        /// </summary>
        private void setDataLoading(bool enabled)
        {
            _DataLoading = enabled;
            if (_WaitingPicture != null)
                _WaitingPicture.Visible = (_DataLoading); //& expandablePanel1.Expanded;
        }

        private void advTree1_SizeChanged(object sender, EventArgs e)
        {
            _WaitingPicture.Location = new Point((advTree1.Width - _WaitingPicture.Width) / 2, (advTree1.Height - _WaitingPicture.Height) / 2);
        }

        private void BtnEnabled(bool enabled)
        {
            btnEditName.Enabled = enabled;
            btnUpdate.Enabled = enabled;
            btnDelete.Enabled = enabled;
        }

        #region 處理AdvTree
        /// <summary>
        /// 把資料填入AdvTree
        /// </summary>
        /// <param name="items"></param>
        private void FillAdvTreeItem(GraduationPlanInfoCollection items)
        {
            string noSchoolYear = "未分類";
            Dictionary<string, DevComponents.AdvTree.Node> itemNodes = new Dictionary<string, DevComponents.AdvTree.Node>();
            // 先根據學年度分類
            #region 根據學年度分類
            foreach (GraduationPlanInfo item in items)
            {
                string schoolYear = item.SchoolYear;
                if (string.IsNullOrEmpty(item.SchoolYear))
                {
                    schoolYear = noSchoolYear;
                }

                if (!itemNodes.ContainsKey(schoolYear))
                {
                    itemNodes.Add(schoolYear, new DevComponents.AdvTree.Node());
                    itemNodes[schoolYear].Text = (schoolYear == noSchoolYear) ? noSchoolYear : schoolYear + "學年度";
                    itemNodes[schoolYear].TagString = (schoolYear == noSchoolYear) ? "" : schoolYear;
                }

                DevComponents.AdvTree.Node childNode = new DevComponents.AdvTree.Node();

                childNode.Tag = item;
                childNode.Text = item.TrimName;
                childNode.Name = item.ID;

                if (_AdvTreeExpandStatus.ContainsKey(itemNodes[schoolYear].TagString))
                    itemNodes[schoolYear].Expanded = _AdvTreeExpandStatus[itemNodes[schoolYear].TagString];

                itemNodes[schoolYear].Nodes.Add(childNode);

                _InfoDictionary.Add(item, childNode);
            }
            #endregion 根據學年度分類

            // 針對學年度排序
            #region 針對學年度排序
            List<string> sortedKey = itemNodes.Keys.ToList<string>();
            sortedKey.Sort(delegate(string key1, string key2)
            {
                if (key1 == "未分類") return 1;
                if (key2 == "未分類") return -1;

                string sort1 = key1.PadLeft(10, '0');
                string sort2 = key2.PadLeft(10, '0');
                return sort2.CompareTo(sort1);
            });
            #endregion 針對學年度排序

            // 把結果填入畫面
            #region 把結果填入畫面
            advTree1.BeginUpdate();
            advTree1.Nodes.Clear();
            foreach (string key in sortedKey)
            {
                itemNodes[key].Nodes.Sort();
                advTree1.Nodes.Add(itemNodes[key]);
            }

            if (_AdvTreeExpandStatus.Count == 0)
            {
                if (advTree1.Nodes.Count > 0)
                    advTree1.Nodes[0].Expand();
            }

            advTree1.EndUpdate();
            #endregion 把結果填入畫面

            if (_SelectItem != null)
            {
                advTree1.SelectedNode = _SelectItem;
            }
        }

        /// <summary>
        /// Node被點擊
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void advTree1_NodeClick(object sender, TreeNodeMouseEventArgs e)
        {
            if (!(e.Node.Tag is GraduationPlanInfo))
            {
                // 假如使用者點到母節點, 清掉選擇選項, 以免有問題
                _SelectItem = null;
                BtnEnabled(false);
                return;
            }


            if (_SelectItem != null)
                _SelectItem.Checked = false;
            Node item = e.Node;

            GraduationPlanInfo info = (GraduationPlanInfo)item.Tag;
            _SelectItem = item;
            item.Checked = true;

            tabControl2.Visible = true & (_SelectItem != null);
            tabControlPanel3.Visible = tabControlPanel2.Visible = tabItem2.Visible = tabItem1.Visible = true;
            tabControl2.SelectedTab = tabItem1;
            tabControl2.SelectedPanel = tabControlPanel2;

            labelX2.Text = labelX1.Text = GetNodeFullPath(item);
            _GraduationPlanEditor.SetSource(info.GraduationPlanElement);
            listViewEx1.SuspendLayout();
            listViewEx1.Items.Clear();
            listViewEx1.Groups.Clear();
            Dictionary<ClassRecord, int> classCount = new Dictionary<ClassRecord, int>();
            List<StudentRecord> noClassStudents = new List<StudentRecord>();
            foreach (StudentRecord stu in _AccessHelper.StudentHelper.GetAllStudent())
            {
                if (GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(stu.StudentID) == info)
                {
                    if (stu.RefClass != null)
                    {
                        if (!classCount.ContainsKey(stu.RefClass))
                            classCount.Add(stu.RefClass, 0);
                        classCount[stu.RefClass]++;
                    }
                    else
                    {
                        noClassStudents.Add(stu);
                    }
                }
            }
            foreach (ClassRecord var in classCount.Keys)
            {
                string groupKey;
                int a;
                if (int.TryParse(var.GradeYear, out a))
                {
                    groupKey = var.GradeYear + "　年級";
                }
                else
                    groupKey = var.GradeYear;
                ListViewGroup group = listViewEx1.Groups[groupKey];
                if (group == null)
                    group = listViewEx1.Groups.Add(groupKey, groupKey);
                listViewEx1.Items.Add(new ListViewItem(var.ClassName + "(" + classCount[var] + ")　", 0, group));
            }
            if (noClassStudents.Count > 0)
            {
                ListViewGroup group = listViewEx1.Groups["未分班"];
                if (group == null)
                    group = listViewEx1.Groups.Add("未分班", "未分班");
                foreach (StudentRecord stu in noClassStudents)
                {
                    listViewEx1.Items.Add(new ListViewItem(stu.StudentName + "[" + stu.StudentNumber + "] 　", 1, group));
                }
            }
            listViewEx1.ResumeLayout();
            tabControl2.Visible = true;
            BtnEnabled(true);
        }

        /// <summary>
        /// 設定是否有錯誤訊息
        /// </summary>
        /// <param name="node"></param>
        /// <param name="isInserted"></param>
        private void SetWarningNode(Node node, bool isInserted)
        {
            if (isInserted == true)
            {
                ElementStyle style = new ElementStyle();
                style.TextColor = Color.Red;
                Node subNode = new Node();
                subNode.Text = "驗證失敗，請檢查內容。\n否則使用此規劃表之學生將無法加入修課。";
                subNode.Style = style;
                node.Nodes.Add(subNode);
                node.Expanded = true;
                node.Image = Properties.Resources.warning1;
            }
            else
            {
                if (node.Nodes.Count > 0)
                {
                    node.Nodes.Clear();
                }
                node.Image = null;
            }
        }

        /// <summary>
        /// 設定母節點是否展開
        /// </summary>
        /// <param name="schoolYear"></param>
        /// <param name="value"></param>
        public static void SetAdvTreeExpandStatus(string schoolYear, bool value)
        {
            if (!_AdvTreeExpandStatus.ContainsKey(schoolYear))
                _AdvTreeExpandStatus.Add(schoolYear, value);

            _AdvTreeExpandStatus[schoolYear] = value;
        }

        /// <summary>
        /// 儲存目前展開的狀態
        /// </summary>
        private void SaveAdvTreeExpandStatus()
        {
            _AdvTreeExpandStatus.Clear();
            foreach (DevComponents.AdvTree.Node node in advTree1.Nodes)
            {
                if (!_AdvTreeExpandStatus.ContainsKey(node.TagString))
                    _AdvTreeExpandStatus.Add(node.TagString, false);
                _AdvTreeExpandStatus[node.TagString] = node.Expanded;
            }
        }

        /// <summary>
        /// 取得目前node的full path (去除";" & "未分類")
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string GetNodeFullPath(Node node)
        {
            return (node.FullPath.Replace(";", "")).Replace("未分類", "");
        }

        /// <summary>
        /// 移除指定的Node
        /// </summary>
        /// <param name="id"></param>
        private void RemoveAdvTreeItem(string id)
        {
            bool isFound = false;
            advTree1.BeginUpdate();
            foreach (Node parentNode in advTree1.Nodes)
            {
                foreach (Node childNode in parentNode.Nodes)
                {
                    if (childNode.Name == id)
                    {
                        isFound = true;
                        childNode.Remove();
                        break;
                    }
                }

                if (isFound == true)
                {
                    if (parentNode.Nodes.Count == 0)
                    {
                        if (parentNode.NextNode != null)
                        {
                            parentNode.NextNode.Expand();
                        }

                        parentNode.Remove();
                    }
                    break;
                }
            }
            advTree1.EndUpdate(true);
        }

        /// <summary>
        /// 把node移動到指定的學年度
        /// </summary>
        /// <param name="schoolYear"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        //private Node MoveNodeToSchoolYear(string schoolYear, Node node)
        //{
        //    // Copy original node
        //    Node newNode = node.DeepCopy();
        //    advTree1.BeginUpdate();
        //    // find the position to insert
        //    bool isInserted = false;
        //    foreach (Node parentNode in advTree1.Nodes)
        //    {
        //        if (parentNode.TagString == schoolYear)
        //        {
        //            parentNode.Nodes.Add(newNode);
        //            parentNode.Expand();
        //            isInserted = true;
        //            break;
        //        }
        //    }

        //    if (isInserted == false)
        //    {
        //        Node parentNode = new Node();
        //        parentNode.Text = (schoolYear == "") ? "未分類" : schoolYear + "學年度";
        //        parentNode.TagString = (schoolYear == "") ? "" : schoolYear;
        //        parentNode.Nodes.Add(newNode);
        //        parentNode.Expand();
        //        advTree1.Nodes.Add(parentNode);
        //        //advTree1.Nodes.Sort();
        //    }
        //    advTree1.EndUpdate(true);
        //    return newNode;
        //}
        #endregion 處理AdvTree

        #region 處理名稱修改
        private void btnEditName_Click(object sender, EventArgs e)
        {
            if (_SelectItem == null) return;
            SaveAdvTreeExpandStatus();
            FrmReviseRuleName frm = new FrmReviseRuleName(
                                    "修改課程規劃表名稱",
                                    "新課程規劃表名稱：",
                                    (_SelectItem.Tag as GraduationPlanInfo).TrimName,
                                    (_SelectItem.Tag as GraduationPlanInfo).SchoolYear);
            frm.ValidateEvent += frm_ValidateEvent;
            frm.SaveEvent += frm_SaveEvent;
            frm.ShowDialog();
        }

        private void frm_ValidateEvent(object sender, FrmReviseRuleName.ReviseRuleNameEventArgs e)
        {
            if (_SelectItem == null) return;

            if (e.OldFullName == e.NewFullName)
            {
                e.Error = false;
                return;
            }

            if (CheckRuleNameDup(e.NewFullName))
            {
                e.ErrorString = "名稱不可重複。";
                e.Error = true;
            }
            else
            {
                e.ErrorString = "";
                e.Error = false;
            }
        }

        private void frm_SaveEvent(object sender, FrmReviseRuleName.ReviseRuleNameEventArgs e)
        {
            if (_SelectItem != null)
            {
                if (e.IsSame == true)
                {
                    return;
                }
                EditGraduationPlan.Update(_SelectItem.Name, e.NewFullName, _GraduationPlanEditor.GetSource(e.NewSchoolYear));
                SetAdvTreeExpandStatus(e.NewSchoolYear, true);
                EventHub.Instance.InvokGraduationPlanUpdated(_SelectItem.Name);
            }
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
        #endregion 處理名稱修改

    }
}

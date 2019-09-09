using FISCA.Presentation.Controls;
using SmartSchool.Evaluation.ScoreCalcRule;
using SmartSchool.Feature.ScoreCalcRule;
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
    public partial class FrmScoreCalcConfiguration : BaseForm
    {
        private BackgroundWorker _BGWScoreCalcRuleLoader;
        private DevComponents.AdvTree.Node _SelectItem;
        private bool _DataLoading;

        public static Dictionary<string, bool> _AdvTreeExpandStatus = new Dictionary<string, bool>();

        public FrmScoreCalcConfiguration()
        {
            InitializeComponent();

            _BGWScoreCalcRuleLoader = new BackgroundWorker();
            _BGWScoreCalcRuleLoader.DoWork += new DoWorkEventHandler(_BGWScoreCalcRuleLoader_DoWork);
            _BGWScoreCalcRuleLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BGWScoreCalcRuleLoader_RunWorkerCompleted);

            ScoreCalcRule.ScoreCalcRule.Instance.ScoreCalcRuleInserted += new EventHandler(Instance_ScoreCalcRuleInserted);
            ScoreCalcRule.ScoreCalcRule.Instance.ScoreCalcRuleUpdated += new EventHandler(Instance_ScoreCalcRuleUpdated);
            ScoreCalcRule.ScoreCalcRule.Instance.ScoreCalcRuleDeleted += new EventHandler<DeleteScoreCalcRuleEventArgs>(Instance_ScoreCalcRuleDeleted);

            _AdvTreeExpandStatus.Clear();
        }

        private void FrmScoreCalcConfiguration_Load(object sender, EventArgs e)
        {
            LoadScoreCalcRule(true);
        }

        void _BGWScoreCalcRuleLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            if ((bool)e.Argument) ScoreCalcRule.ScoreCalcRule.Instance.Reflash();
            e.Result = ScoreCalcRule.ScoreCalcRule.Instance.Items;
        }

        void _BGWScoreCalcRuleLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ScoreCalcRuleInfoCollection resp = (ScoreCalcRuleInfoCollection)e.Result;
            FillAdvTreeItem(resp);
            BtnEnabled(false);
            scoreCalcRuleEditor1.Visible = false;
            setDataLoading(false);
        }
        void Instance_ScoreCalcRuleDeleted(object sender, DeleteScoreCalcRuleEventArgs e)
        {
            LoadScoreCalcRule(false);
        }

        void Instance_ScoreCalcRuleUpdated(object sender, EventArgs e)
        {
            LoadScoreCalcRule(false);
        }

        void Instance_ScoreCalcRuleInserted(object sender, EventArgs e)
        {
            LoadScoreCalcRule(false);
        }

        public void LoadScoreCalcRule(bool reflash)
        {
            if (!_BGWScoreCalcRuleLoader.IsBusy)
            {
                BtnEnabled(false);
                setDataLoading(true);
                _BGWScoreCalcRuleLoader.RunWorkerAsync(reflash);
            }
        }

        private void setDataLoading(bool dataLoading)
        {
            _DataLoading = dataLoading;
            if (_WaitingPicture != null)
                _WaitingPicture.Visible = (_DataLoading);
        }

        private void btn_create_Click(object sender, EventArgs e)
        {
            SaveAdvTreeExpandStatus();
            if (new ScoreCalcRuleCreator().ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _SelectItem = null;
            }
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            if (_SelectItem == null) return;
            if (this.scoreCalcRuleEditor1.IsValidated)
            {
                SaveAdvTreeExpandStatus();
                string schoolYear = _SelectItem.Parent.TagString;
                string id = _SelectItem.Name;
                string name = schoolYear + (_SelectItem.Tag as ScoreCalcRuleInfo).TrimName;
                EditScoreCalcRule.Update(id, name, this.scoreCalcRuleEditor1.GetSource(schoolYear));
                ScoreCalcRule.ScoreCalcRule.Instance.Invok_ScoreCalcRuleUpdated();
            }
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            if (_SelectItem == null) return;

            SaveAdvTreeExpandStatus();
            if (MsgBox.Show("確定要刪除 '" + GetNodeFullPath(_SelectItem) + "' 成績計算規則？", "確定", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                string id = _SelectItem.Name;
                RemoveScoreCalcRule.Delete(id);
                _SelectItem = null;
                ScoreCalcRule.ScoreCalcRule.Instance.Invok_ScoreCalcRuleDeleted(id);
            }
        }

        private void advTree1_SizeChanged(object sender, EventArgs e)
        {
            _WaitingPicture.Location = new Point((advTree1.Width - _WaitingPicture.Width) / 2, (advTree1.Height - _WaitingPicture.Height) / 2);
        }

        private void BtnEnabled(bool enabled)
        {
            btn_edit_name.Enabled = enabled;
            btn_update.Enabled = enabled;
            btn_delete.Enabled = enabled;
        }

        #region 處理名稱修改
        private void btn_edit_name_Click(object sender, EventArgs e)
        {
            if (_SelectItem == null) return;
            SaveAdvTreeExpandStatus();
            FrmReviseRuleName frm = new FrmReviseRuleName(
                                    "修改成績計算規則名稱",
                                    "新成績計算規則名稱：",
                                    (_SelectItem.Tag as ScoreCalcRuleInfo).TrimName,
                                    (_SelectItem.Tag as ScoreCalcRuleInfo).SchoolYear);
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
                e.Error = true;
                e.ErrorString = "名稱不可重複。";
            }
            else
            {
                e.Error = false;
                e.ErrorString = "";
            }
        }

        private void frm_SaveEvent(object sender, FrmReviseRuleName.ReviseRuleNameEventArgs e)
        {
            if (_SelectItem == null) return;

            if (e.IsSame == true)
            {
                return;
            }

            EditScoreCalcRule.Update(_SelectItem.Name, e.NewFullName, this.scoreCalcRuleEditor1.GetSource(e.NewSchoolYear));
            SetAdvTreeExpandStatus(e.NewSchoolYear, true);
            ScoreCalcRule.ScoreCalcRule.Instance.Invok_ScoreCalcRuleUpdated();
        }
        
        /// <summary>
        /// 檢查名稱是否重複
        /// </summary>
        /// <param name="ruleName"></param>
        /// <returns></returns>
        private bool CheckRuleNameDup(string newRuleName)
        {
            foreach (DevComponents.AdvTree.Node parentNode in advTree1.Nodes)
            {
                foreach (DevComponents.AdvTree.Node childNode in parentNode.Nodes)
                {
                    string ruleName = (childNode.Tag as ScoreCalcRuleInfo).Name;
                    if (newRuleName == ruleName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion 處理名稱修改

        #region 處理AdvTree
        /// <summary>
        /// 把資料填入AdvTree
        /// </summary>
        /// <param name="items"></param>
        private void FillAdvTreeItem(ScoreCalcRuleInfoCollection items)
        {
            string noSchoolYear = "未分類";
            Dictionary<string, DevComponents.AdvTree.Node> itemNodes = new Dictionary<string, DevComponents.AdvTree.Node>();
            // 先根據學年度分類
            #region 根據學年度分類
            foreach (ScoreCalcRuleInfo item in items)
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
        private void advTree1_NodeClick(object sender, DevComponents.AdvTree.TreeNodeMouseEventArgs e)
        {
            if (!(e.Node.Tag is ScoreCalcRuleInfo))
            {
                // 假如使用者點到母節點, 清掉選擇選項, 以免有問題
                _SelectItem = null;
                BtnEnabled(false);
                return;
            }

            if (_SelectItem != null)
                _SelectItem.Checked = false;

            _SelectItem = e.Node;
            ScoreCalcRuleInfo info = (ScoreCalcRuleInfo)_SelectItem.Tag;
            _SelectItem.Checked = true;

            this.scoreCalcRuleEditor1.SetSource(info.ScoreCalcRuleElement);
            this.scoreCalcRuleEditor1.ScoreCalcRuleName = info.Name;
            BtnEnabled(true);
            scoreCalcRuleEditor1.Visible = true;
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
        private string GetNodeFullPath(DevComponents.AdvTree.Node node)
        {
            return (node.FullPath.Replace(";", "")).Replace("未分類", "");
        }
        #endregion 處理AdvTree

    }
}

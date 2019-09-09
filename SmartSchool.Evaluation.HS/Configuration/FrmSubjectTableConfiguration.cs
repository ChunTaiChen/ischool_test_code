using DevComponents.AdvTree;
using FISCA.Presentation.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SmartSchool.Evaluation.Configuration
{
    public partial class FrmSubjectTableConfiguration : BaseForm
    {
        private string _Catalog;

        private Node _SelectItem;
        public static Dictionary<string, bool> _AdvTreeExpandStatus = new Dictionary<string, bool>();

        public FrmSubjectTableConfiguration(string catalog)
        {
            InitializeComponent();

            this.expandablePanel1.TitleText = this.Text = _Catalog = catalog;
            if (catalog == "學程科目表")
            {
                this.subjectTableEditor1.ProgramTable = true;
            }
            else if (catalog == "核心科目表")
            {
                this.expandablePanel1.TitleText = "自訂畢業應修及格科目表";
                this.Text = "自訂畢業應修及格科目表";
            }

            _AdvTreeExpandStatus.Clear();
        }

        private void FrmSubjectTableConfiguration_Load(object sender, EventArgs e)
        {
            RefillSubjectTables();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_SelectItem == null) return;
            SaveAdvTreeExpandStatus();

            if (MsgBox.Show("確定要刪除 '" + GetNodeFullPath(_SelectItem) + "' ？", "確定", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                SmartSchool.Feature.SubjectTable.RemoveSubejctTable.Delete(_SelectItem.Name);
                SubjectTable.Items[_Catalog].Reflash();
                _SelectItem = null;
                RefillSubjectTables();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_SelectItem == null) return;
            
            if (subjectTableEditor1.IsValidated())
            {
                SaveAdvTreeExpandStatus();
                // 因為subjectTableEditor1取得的content一定不會有SchoolYear(Attribute), 所以必須要補上!!
                XmlElement content = ReviseXmlContent(this.subjectTableEditor1.Content, ((SubjectTableItem)_SelectItem.Tag).SchoolYear);
                SmartSchool.Feature.SubjectTable.EditSubejctTable.UpdateSubject(_SelectItem.Name, content);
                SubjectTable.Items[_Catalog].Reflash();
                RefillSubjectTables();
            }
            else
            {
                MsgBox.Show("輸入資料有誤，無法儲存。\n請檢查輸入資料。");
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            SaveAdvTreeExpandStatus();
            if (new SubjectTableCreator(_Catalog).ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _SelectItem = null;
                RefillSubjectTables();
            }
        }
        private void RefillSubjectTables()
        {
            BtnEnabled(false);
            this.subjectTableEditor1.Visible = false;
            FillAdvTreeItem(SubjectTable.Items[_Catalog]);
        }

        /// <summary>
        /// 修改XML的內容
        /// </summary>
        /// <param name="content"></param>
        /// <param name="schoolYear"></param>
        /// <returns></returns>
        private XmlElement ReviseXmlContent(XmlElement content, string schoolYear)
        {
            if (!string.IsNullOrEmpty(schoolYear))
            {
                if (content == null)
                {
                    content = new XmlDocument().CreateElement("SubjectTableContent");
                }

                content.RemoveAttribute("SchoolYear");
                content.SetAttribute("SchoolYear", schoolYear);
            }
            else
            {
                if (content != null)
                {
                    content.RemoveAttribute("SchoolYear");
                }
            }
            return content;
        }

        private void BtnEnabled(bool enabled)
        {
            btnEditName.Enabled = enabled;
            btnUpdate.Enabled = enabled;
            btnDelete.Enabled = enabled;
        }

        #region 處理名稱修改
        private void btnEditName_Click(object sender, EventArgs e)
        {
            if (_SelectItem == null) return;
            SaveAdvTreeExpandStatus();
            FrmReviseRuleName frm = new FrmReviseRuleName(
                                    "修改" + _Catalog + "名稱",
                                    "新" + _Catalog + "名稱：",
                                    (_SelectItem.Tag as SubjectTableItem).TrimName,
                                    (_SelectItem.Tag as SubjectTableItem).SchoolYear);
            frm.ValidateEvent += frm_ValidateEvent;
            frm.SaveEvent += frm_SaveEvent;
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                RefillSubjectTables();
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
            // 因為subjectTableEditor1取得的content一定不會有SchoolYear(Attribute), 所以必須要補上!!
            XmlElement content = ReviseXmlContent(this.subjectTableEditor1.Content, e.NewSchoolYear);
            SmartSchool.Feature.SubjectTable.EditSubejctTable.UpdateSubject(_SelectItem.Name, e.NewFullName, content);
            SubjectTable.Items[_Catalog].Reflash();
            SetAdvTreeExpandStatus(e.NewSchoolYear, true);
        }

        /// <summary>
        /// 檢查名稱是否重複
        /// </summary>
        /// <param name="ruleName"></param>
        /// <returns></returns>
        private bool CheckRuleNameDup(string newRuleName)
        {
            foreach (SubjectTableItem var in SubjectTable.Items[_Catalog])
            {
                if (var.Name == newRuleName)
                {
                    return true;
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
        private void FillAdvTreeItem(SubjectTableCollection items)
        {
            string noSchoolYear = "未分類";
            Dictionary<string, Node> itemNodes = new Dictionary<string, Node>();
            // 先根據學年度分類
            #region 根據學年度分類
            foreach (SubjectTableItem item in items)
            {
                string schoolYear = item.SchoolYear;
                if (string.IsNullOrEmpty(item.SchoolYear))
                {
                    schoolYear = noSchoolYear;
                }

                if (!itemNodes.ContainsKey(schoolYear))
                {
                    itemNodes.Add(schoolYear, new Node());
                    itemNodes[schoolYear].Text = (schoolYear == noSchoolYear) ? noSchoolYear : schoolYear + "學年度";
                    itemNodes[schoolYear].TagString = (schoolYear == noSchoolYear) ? "" : schoolYear;
                }

                Node childNode = new Node();

                childNode.Tag = item;
                childNode.Text = item.TrimName;
                childNode.Name = item.ID;

                if (_AdvTreeExpandStatus.ContainsKey(itemNodes[schoolYear].TagString))
                    itemNodes[schoolYear].Expanded = _AdvTreeExpandStatus[itemNodes[schoolYear].TagString];

                itemNodes[schoolYear].Nodes.Add(childNode);
            }
            #endregion 根據學年度分類

            // 排序
            #region 排序
            List<string> sortedKey = itemNodes.Keys.ToList<string>();
            sortedKey.Sort(delegate(string key1, string key2)
            {
                if (key1 == "未分類") return 1;
                if (key2 == "未分類") return -1;

                string sort1 = key1.PadLeft(10, '0');
                string sort2 = key2.PadLeft(10, '0');
                return sort2.CompareTo(sort1);
            });
            #endregion 排序

            // 把結果填入畫面
            #region 把結果填入畫面
            advTree1.BeginUpdate();
            advTree1.Nodes.Clear();
            foreach (string key in sortedKey)
            {
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
            
            if (!(e.Node.Tag is SubjectTableItem))
            {
                // 假如使用者點到母節點, 清掉選擇選項, 以免有問題
                _SelectItem = null;
                this.subjectTableEditor1.Visible = false;
                BtnEnabled(false);
                return;
            }

            if (_SelectItem != null)
                _SelectItem.Checked = false;

            _SelectItem = e.Node;
            subjectTableEditor1.Content = ((SubjectTableItem)_SelectItem.Tag).Content;
            _SelectItem.Checked = true;
            BtnEnabled(true);
            this.subjectTableEditor1.Visible = true;
            
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
            foreach (Node node in advTree1.Nodes)
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
        #endregion 處理AdvTree

    }
}

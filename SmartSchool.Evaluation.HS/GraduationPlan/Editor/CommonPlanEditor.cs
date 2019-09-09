using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SmartSchool.Evaluation.GraduationPlan.Editor
{
    public partial class CommonPlanEditor : UserControl, IGraduationPlanEditor
    {
        private List<DataGridViewCell> _DirtyCells = new List<DataGridViewCell>();

        private bool _IsDirty;

        private int _CreditStartIndex;

        private int _SubjectNameIndex;

        private int _StartLevelIndex;

        private int _CategoryIndex;

        private int _DomainIndex;

        private int _RequiredByIndex;

        private int _RequiredIndex;

        private int _EntryIndex;

        private int _NotIncludedInCreditIndex;

        private int _NotIncludedInCalcIndex;

        private Dictionary<DataGridViewCell, object> _defaultValues = new Dictionary<DataGridViewCell, object>();

        private Dictionary<DataGridViewRow, string> _RowSubject = new Dictionary<DataGridViewRow, string>();

        private int _SelectedRowIndex;

        public CommonPlanEditor()
        {
            InitializeComponent();
            _CategoryIndex = Column1.Index;
            _DomainIndex = Column2.Index;
            _RequiredByIndex = Column3.Index;
            _RequiredIndex = Column4.Index;
            _SubjectNameIndex = Column5.Index;
            _EntryIndex = Column13.Index;
            _StartLevelIndex = Column6.Index;
            _CreditStartIndex = Column7.Index;
            _NotIncludedInCreditIndex = Column17.Index;
            _NotIncludedInCalcIndex = Column16.Index;
            dataGridViewX1.Rows.Add();
            dataGridViewX1.Rows[0].DefaultCellStyle.BackColor = Color.AntiqueWhite;
            dataGridViewX1.Rows[0].Cells[_SubjectNameIndex].Value = "預設";
            dataGridViewX1.Rows[0].Cells[_CategoryIndex].Value = "";
            dataGridViewX1.Rows[0].Cells[_CreditStartIndex].Value = "0";
            dataGridViewX1.Rows[0].Cells[_DomainIndex].Value = "";
            dataGridViewX1.Rows[0].Cells[_EntryIndex].Value = "學業";
            dataGridViewX1.Rows[0].Cells[_RequiredIndex].Value = "選修";
            dataGridViewX1.Rows[0].Cells[_RequiredByIndex].Value = "校訂";
            dataGridViewX1.Rows[0].Cells[_SubjectNameIndex].ReadOnly = true;
            dataGridViewX1.Rows[0].Cells[_StartLevelIndex].Value = "";
            dataGridViewX1.Rows[0].Cells[_StartLevelIndex].ReadOnly = true;

            dataGridViewX1.CurrentCell = dataGridViewX1.FirstDisplayedCell;
            if(dataGridViewX1.CurrentCell !=null)
                dataGridViewX1.BeginEdit(true);
        }

        private string GetNumber(int p)
        {
            string levelNumber;
            switch (p)
            {
                #region 對應levelNumber
                case 1:
                    levelNumber = "I";
                    break;
                case 2:
                    levelNumber = "II";
                    break;
                case 3:
                    levelNumber = "III";
                    break;
                case 4:
                    levelNumber = "IV";
                    break;
                case 5:
                    levelNumber = "V";
                    break;
                case 6:
                    levelNumber = "VI";
                    break;
                case 7:
                    levelNumber = "VII";
                    break;
                case 8:
                    levelNumber = "VIII";
                    break;
                case 9:
                    levelNumber = "IX";
                    break;
                case 10:
                    levelNumber = "X";
                    break;
                default:
                    levelNumber = "" + (p);
                    break;
                #endregion
            }
            return levelNumber;
        }

        #region IGraduationPlanEditor 成員

        public void SetSource(System.Xml.XmlElement source)
        {
            _defaultValues = new Dictionary<DataGridViewCell, object>();
            _RowSubject = new Dictionary<DataGridViewRow, string>();
            _DirtyCells = new List<DataGridViewCell>();
            dataGridViewX1.Rows.Clear();
            dataGridViewX1.Rows.Add();
            dataGridViewX1.Rows[0].DefaultCellStyle.BackColor = Color.AntiqueWhite;
            dataGridViewX1.Rows[0].Cells[_SubjectNameIndex].Value = "預設";
            dataGridViewX1.Rows[0].Cells[_CategoryIndex].Value = "";
            dataGridViewX1.Rows[0].Cells[_CreditStartIndex].Value = "0";
            dataGridViewX1.Rows[0].Cells[_DomainIndex].Value = "";
            dataGridViewX1.Rows[0].Cells[_EntryIndex].Value = "學業";
            dataGridViewX1.Rows[0].Cells[_RequiredIndex].Value = "選修";
            dataGridViewX1.Rows[0].Cells[_RequiredByIndex].Value = "校訂";
            dataGridViewX1.Rows[0].Cells[_SubjectNameIndex].ReadOnly = true;
            dataGridViewX1.Rows[0].Cells[_StartLevelIndex].Value = "";
            dataGridViewX1.Rows[0].Cells[_StartLevelIndex].ReadOnly = true;
            _RowSubject.Add(dataGridViewX1.Rows[0], "預設");

            if (source != null)
            {
                foreach (XmlNode node in source.SelectNodes("Subject"))
                {
                    DataGridViewRow row;
                    XmlElement element = (XmlElement)node;
                    //檢查是否符合群組設定

                    #region 以該科別的開始級別做為開始級別
                    if (element.Attributes["SubjectName"].InnerText == "預設")
                    {
                        row = dataGridViewX1.Rows[0];
                    }
                    else
                    {
                        row = new DataGridViewRow();
                        row.CreateCells(dataGridViewX1, "", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
                        dataGridViewX1.Rows.Add(row);
                    }
                    row.Cells[_CategoryIndex].Value = element.Attributes["Category"].InnerText;
                    row.Cells[_DomainIndex].Value = element.Attributes["Domain"].InnerText;
                    row.Cells[_RequiredByIndex].Value = element.Attributes["RequiredBy"].InnerText;
                    row.Cells[_RequiredIndex].Value = element.Attributes["Required"].InnerText;
                    row.Cells[_SubjectNameIndex].Value = element.Attributes["SubjectName"].InnerText;
                    row.Cells[_NotIncludedInCreditIndex].Value = element.GetAttribute("NotIncludedInCredit") == "True" ? true : false;
                    row.Cells[_NotIncludedInCalcIndex].Value = element.GetAttribute("NotIncludedInCalc")== "True" ? true : false;
                    if (element.HasAttribute("Entry"))
                    {
                        switch (element.GetAttribute("Entry"))
                        {
                            default:
                            case "學業":
                                row.Cells[_EntryIndex].Value = "學業";
                                break;
                            case "體育":
                                row.Cells[_EntryIndex].Value = "體育";
                                break;
                            case "國防通識":
                                row.Cells[_EntryIndex].Value = "國防通識(軍訓)";
                                break;
                            case "健康與護理":
                                row.Cells[_EntryIndex].Value = "健康與護理";
                                break;
                            case "實習科目":
                                row.Cells[_EntryIndex].Value = "實習科目";
                                break;
                            case "專業科目":
                                row.Cells[_EntryIndex].Value = "專業科目";
                                break;
                        }
                    }
                    else
                        row.Cells[_EntryIndex].Value = "學業";

                    row.Cells[_StartLevelIndex].Value = element.Attributes["Level"].InnerText;
                    //填入學分數
                        row.Cells[_CreditStartIndex].Value = element.Attributes["Credit"].InnerText;
                    #endregion
                    //呼叫結束編輯處理函式
                    dataGridViewX1_CellEndEdit(this, new DataGridViewCellEventArgs(_SubjectNameIndex, row.Index));
                }
            }
            if (this.IsValidated) { }
            dataGridViewX1.CurrentCell = dataGridViewX1.FirstDisplayedCell;

            if(dataGridViewX1.CurrentCell !=null)
                dataGridViewX1.BeginEdit(true);
        }

        public System.Xml.XmlElement GetSource()
        {
            return GetSource(string.Empty);
        }
        public System.Xml.XmlElement GetSource(string schoolYear)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<GraduationPlan/>");
            if (!string.IsNullOrEmpty(schoolYear))
                doc.DocumentElement.SetAttribute("SchoolYear", "" + schoolYear);

            int rowIndex = 0;
            //掃每一列資料
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                //有表示科目欄有填寫
                if (_RowSubject.ContainsKey(row))
                {
                    rowIndex++;
                    //記錄每個級別所包含的學分數
                    List<decimal> Credits = new List<decimal>();
                    //記錄每個級別所包含的學期
                    List<int> Semesters = new List<int>();
                    //記錄每個級別所包含的學年度
                    List<int> GradeYears = new List<int>();
                    //未加級別前的科目名稱
                    string subjectName = _RowSubject[row];

                    int startLevel = 0;
                    //如果有填數開始級別則以開始級別開始算
                    if (row.Cells[_StartLevelIndex].Value != null)
                        int.TryParse("" + row.Cells[_StartLevelIndex].Value, out startLevel);
                    //這個row中包含的數個科目級別
                    XmlElement parentElement;
                    //全組資料(回填用)
                    XmlElement grouping;

                    //這個row中包含的數個科目級別
                    parentElement = doc.CreateElement("Subject");
                    //全組資料(回填用)
                    grouping = doc.CreateElement("Grouping");
                    grouping.SetAttribute("RowIndex", rowIndex.ToString());
                    grouping.SetAttribute("startLevel", row.Cells[_StartLevelIndex].Value == null ? "" : row.Cells[_StartLevelIndex].Value.ToString());
                    parentElement.AppendChild(grouping);
                    //建立新Element加入至doc
                    parentElement.SetAttribute("SubjectName", subjectName);
                    parentElement.SetAttribute("Category", row.Cells[_CategoryIndex].Value == null ? "" : row.Cells[_CategoryIndex].Value.ToString());
                    parentElement.SetAttribute("Domain", row.Cells[_DomainIndex].Value == null ? "" : row.Cells[_DomainIndex].Value.ToString());
                    parentElement.SetAttribute("RequiredBy", row.Cells[_RequiredByIndex].Value == null ? "" : row.Cells[_RequiredByIndex].Value.ToString());
                    parentElement.SetAttribute("Required", row.Cells[_RequiredIndex].Value == null ? "" : row.Cells[_RequiredIndex].Value.ToString());

                    parentElement.SetAttribute("Credit", ""+row.Cells[_CreditStartIndex].Value);
                    bool b;
                    bool.TryParse(row.Cells[_NotIncludedInCreditIndex].Value == null ? "false" : row.Cells[_NotIncludedInCreditIndex].Value.ToString(), out b);
                    parentElement.SetAttribute("NotIncludedInCredit", b.ToString());
                    bool.TryParse(row.Cells[_NotIncludedInCalcIndex].Value == null ? "false" : row.Cells[_NotIncludedInCalcIndex].Value.ToString(), out b);
                    parentElement.SetAttribute("NotIncludedInCalc", b.ToString());

                    switch ("" + row.Cells[_EntryIndex].Value)
                    {
                        default:
                        case "學業":
                            parentElement.SetAttribute("Entry", "學業");
                            break;
                        case "體育":
                            parentElement.SetAttribute("Entry", "體育");
                            break;
                        case "國防通識(軍訓)":
                            parentElement.SetAttribute("Entry", "國防通識");
                            break;
                        case "健康與護理":
                            parentElement.SetAttribute("Entry", "健康與護理");
                            break;
                        case "實習科目":
                            parentElement.SetAttribute("Entry", "實習科目");
                            break;
                        case "專業科目":
                            parentElement.SetAttribute("Entry", "專業科目");
                            break;
                    }


                    #region 計算科目級別
                    if (startLevel == 0)
                    {
                        XmlElement element = (XmlElement)parentElement.CloneNode(true);
                        element.SetAttribute("FullName", subjectName);
                        element.SetAttribute("Level", row.Cells[_StartLevelIndex].Value == null ? "" : row.Cells[_StartLevelIndex].Value.ToString());
                        doc.DocumentElement.AppendChild(element);
                    }
                    else
                    {
                        //填入開始級別
                        XmlElement element = (XmlElement)parentElement.CloneNode(true);
                        element.SetAttribute("FullName", subjectName + " " + GetNumber(startLevel));
                        element.SetAttribute("Level", startLevel.ToString());
                        doc.DocumentElement.AppendChild(element);
                    }
                    #endregion.
                }
            }
            return doc.DocumentElement;
        }

        public bool IsDirty
        {
            get { return _IsDirty; }
        }

        public event EventHandler IsDirtyChanged;

        public bool IsValidated
        {
            get
            {
                bool pass = true;
                dataGridViewX1.EndEdit();
                foreach (DataGridViewRow row in dataGridViewX1.Rows)
                {
                    if (!row.IsNewRow)
                        pass &= ValidateRow(row);
                }
                foreach (DataGridViewRow row in dataGridViewX1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            pass &= (cell.ErrorText== "");
                        }
                    }
                }
                return pass;
            }
        }

        private bool ValidateRow(DataGridViewRow row)
        {
            if (row.IsNewRow) return true;

            bool pass = true;
            DataGridViewCell cell ;
            decimal tryInt = 0;
            #region 檢查學分填寫正確
            cell = row.Cells[_CreditStartIndex];
            if ("" + cell.Value != "" && !decimal.TryParse("" + cell.Value, out tryInt))
            {
                cell.ErrorText = "必須填入數字。";
                dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
                pass &= false;
            }
            else
            {
                cell.ErrorText = "";
                dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);                
            }
            #endregion
            #region 檢查級別填寫正確
            int leval = 0;
            cell = row.Cells[_StartLevelIndex];
            if ("" + cell.Value != "" && !int.TryParse("" + cell.Value, out leval))
            {
                cell.ErrorText = "必須填入數字。";
                dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
                pass &= false;
            }
            else
            {
                cell.ErrorText = "";
                dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
            }
            #endregion            
            #region 檢查必填欄位
            foreach (int index in new int[]{_CreditStartIndex,_EntryIndex,_RequiredByIndex,_RequiredIndex,_SubjectNameIndex})
            {
                cell=row.Cells[index];
                if ("" + cell.Value == "")
                {
                    cell.ErrorText = "必須填寫。";
                    dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
                    pass &= false;
                }
                else
                {
                    if (cell.ErrorText == "必須填寫。")
                    {
                        cell.ErrorText = "";
                        dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
                    }
                }
            }
            #endregion
            return pass;
        }

        #endregion

        private void dataGridViewX1_MouseHover(object sender, EventArgs e)
        {
            if (!dataGridViewX1.ContainsFocus)
                dataGridViewX1.Focus();
        }

        private void dataGridViewX1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridViewRow row = dataGridViewX1.Rows[e.RowIndex];
            if (e.ColumnIndex == _SubjectNameIndex && _RowSubject.ContainsKey(row))
            {
                row.Cells[_SubjectNameIndex].Value = _RowSubject[row];
            }
            #region 把初值存起來做為之後判斷IsDirty用
            if (!_defaultValues.ContainsKey(row.Cells[e.ColumnIndex]))
                _defaultValues.Add(row.Cells[e.ColumnIndex], row.Cells[e.ColumnIndex].Value);
            #endregion
        }

        private void dataGridViewX1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dataGridViewX1.Rows[e.RowIndex];
            #region 做IsDirty檢查
            if (_defaultValues.ContainsKey(row.Cells[e.ColumnIndex]) &&
                    ("" + _defaultValues[row.Cells[e.ColumnIndex]]) != ("" + row.Cells[e.ColumnIndex].Value)//把值用都轉成字串來比對相等，用string+object相加來省去null.ToString()的麻煩
                    )
            {
                _DirtyCells.Add(row.Cells[e.ColumnIndex]);
            }
            else
            {
                if (_DirtyCells.Contains(row.Cells[e.ColumnIndex]))
                    _DirtyCells.Remove(row.Cells[e.ColumnIndex]);
            }
            if ((_DirtyCells.Count != 0) != _IsDirty)
            {
                _IsDirty = (_DirtyCells.Count != 0);
                if (IsDirtyChanged != null)
                    IsDirtyChanged.Invoke(this, new EventArgs());
            }
            #endregion
            //判斷是否為科目名稱欄加入至集合中
            #region 判斷是否為科目名稱欄加入至集合中
            if (e.ColumnIndex == _SubjectNameIndex)
            {
                if (row.Cells[_SubjectNameIndex].Value != null && "" + row.Cells[_SubjectNameIndex].Value != "")
                {
                    if (_RowSubject.ContainsKey(row))
                        _RowSubject[row] = row.Cells[_SubjectNameIndex].Value.ToString();
                    else
                        _RowSubject.Add(row, row.Cells[_SubjectNameIndex].Value.ToString());
                }
                else
                    if (_RowSubject.ContainsKey(row))
                        _RowSubject.Remove(row);
            }
            #endregion
            //如果有填入科目名稱則開始計算級別
            #region 如果有填入科目名稱則開始計算級別
            if (_RowSubject.ContainsKey(row))
            {
                string subjectName = _RowSubject[row];

                int startLevel = 0;
                //如果有填數開始級別則以開始級別開始算
                if (int.TryParse("" + row.Cells[_StartLevelIndex].Value, out startLevel))
                {
                    row.Cells[_SubjectNameIndex].Value = subjectName + "  ( " + subjectName +" "+ GetNumber(startLevel) + " ) ";
                }
                else
                    row.Cells[_SubjectNameIndex].Value = subjectName;
            }
            #endregion
            //編輯最後一列(新增資料那列前依列)
            #region 編輯最後一列(新增資料那列前依列)
            if (e.RowIndex == dataGridViewX1.Rows.Count - 2)
            {
                foreach (int index in new int[] { _CategoryIndex, _RequiredByIndex, _RequiredIndex, _EntryIndex })
                {
                    dataGridViewX1.Rows[e.RowIndex + 1].Cells[index].Value = dataGridViewX1.Rows[e.RowIndex].Cells[index].Value;
                }
            } 
            #endregion
        }

        private void dataGridViewX1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewX1.SelectedCells.Count == 1)
                dataGridViewX1.BeginEdit(true);
        }

        private void dataGridViewX1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex>0&&e.ColumnIndex < 0 && e.Button == MouseButtons.Right)
            {
                _SelectedRowIndex = e.RowIndex;
                foreach (DataGridViewRow var in dataGridViewX1.SelectedRows)
                {
                    if (var.Index != _SelectedRowIndex)
                        var.Selected = false;
                }
                dataGridViewX1.Rows[_SelectedRowIndex].Selected = true;
                contextMenuStrip1.Show(dataGridViewX1, dataGridViewX1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location);
            }
        }

        private void dataGridViewX1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            ValidateRow(dataGridViewX1.Rows[e.RowIndex]);
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

        private void dataGridViewX1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = 0; i < e.RowCount; i++)
            {
                //如果有前一筆資料則複製前筆資料
                if (i + e.RowIndex > 0)
                {
                    foreach (int index in new int[] { _CategoryIndex, _RequiredByIndex, _RequiredIndex, _EntryIndex })
                    {
                        dataGridViewX1.Rows[i + e.RowIndex].Cells[index].Value = dataGridViewX1.Rows[i + e.RowIndex - 1].Cells[index].Value;
                    }
                }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            dataGridViewX1.Rows.Insert(_SelectedRowIndex, new DataGridViewRow());
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (_SelectedRowIndex > 0 && dataGridViewX1.Rows.Count - 1 > _SelectedRowIndex)
                dataGridViewX1.Rows.RemoveAt(_SelectedRowIndex);
        }

        private void dataGridViewX1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dataGridViewX1.EndEdit();
        }
    }
}

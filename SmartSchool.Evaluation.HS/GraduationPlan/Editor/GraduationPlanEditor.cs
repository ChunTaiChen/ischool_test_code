using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using DevComponents.DotNetBar.Rendering;

namespace SmartSchool.Evaluation.GraduationPlan.Editor
{
    public partial class GraduationPlanEditor : UserControl,IGraduationPlanEditor
    {
        private List<DataGridViewCell> _DirtyCells=new List<DataGridViewCell>();

        private bool _RawDeleted;

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

        private Dictionary<DataGridViewCell, object> _defaultValues=new Dictionary<DataGridViewCell,object>();

        private Dictionary<DataGridViewRow, string> _RowSubject = new Dictionary<DataGridViewRow, string>();

        private int _SelectedRowIndex;

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
                if (row.Cells[_SubjectNameIndex].Value != null && ""+row.Cells[_SubjectNameIndex].Value != "")
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
                List<int> levels = ProcessLevels(row);
                if (levels.Count > 1)
                {
                    List<string> SubjectLevels = new List<string>();
                    int index = 0;
                    if (levels[index] == 0)
                    {
                        index++;
                        SubjectLevels.Add("　");
                    }
                    for (int i = index; i < levels.Count; i++)
                    {
                        SubjectLevels.Add(GetNumber(levels[i]));
                    }
                    string levelstring = "";
                    foreach (string var in SubjectLevels)
                    {
                        levelstring += (levelstring == "" ? " (" : "、") + var;
                    }
                    row.Cells[_SubjectNameIndex].Value = subjectName + levelstring + ")";
                }
                else
                {
                    if (levels[0] > 0)
                        row.Cells[_SubjectNameIndex].Value = subjectName + " (" + GetNumber(levels[0]) + ")";
                    else
                        row.Cells[_SubjectNameIndex].Value = subjectName;
                }
                #region OldWay
                //int countLevel = 0;
                ////掃描開課學期
                //for (int i = _CreditStartIndex; i < _CreditStartIndex+8; i++)
                //{
                //    if (row.Cells[i].Value != null)
                //    {
                //        //壘計課程級別
                //        countLevel++;
                //    }
                //}

                //int startLevel = 0;
                ////如果有填數開始級別則以開始級別開始算
                //if (row.Cells[_StartLevelIndex].Value != null)
                //    int.TryParse("" + row.Cells[_StartLevelIndex].Value, out startLevel);
                ////計算科目級別
                //if (countLevel > 1)
                //{
                //    #region 自動計算科目級別
                //    List<string> SubjectLevels = new List<string>();
                //    //如果開課超過一學期且沒有填入開始級別，開始級別從1開始
                //    if (startLevel == 0)
                //    {
                //        //沒填入開始級別則第一筆不加級別
                //        SubjectLevels.Add("　");
                //        startLevel = 1;
                //        //填入開始級別第二筆開始會從2開始算級別
                //        for (int i = 1; i < countLevel; i++)
                //        {
                //            string levelNumber = "";
                //            levelNumber = GetNumber(i + startLevel);
                //            SubjectLevels.Add(levelNumber);
                //        }
                //    }
                //    else
                //    {
                //        //填入開始級別
                //        for (int i = 0; i < countLevel; i++)
                //        {
                //            string levelNumber = "";
                //            levelNumber = GetNumber(i + startLevel);
                //            SubjectLevels.Add(levelNumber);
                //        }
                //    }
                //    string levelstring = "";
                //    foreach (string var in SubjectLevels)
                //    {
                //        levelstring += (levelstring == "" ? " (" : "、") + var;
                //    }
                //    row.Cells[_SubjectNameIndex].Value = subjectName + levelstring + ")";
                //    #endregion
                //}
                //else
                //{
                //    //有填入開始級別，但沒有開課或只開課一學期
                //    if (startLevel > 0)
                //        row.Cells[_SubjectNameIndex].Value = subjectName + " (" + GetNumber(startLevel) + ")";
                //    else
                //        row.Cells[_SubjectNameIndex].Value = subjectName;
                //} 
	            #endregion
            }
            #endregion
            //編輯最後一列(新增資料那列前依列)
            if (e.RowIndex == dataGridViewX1.Rows.Count - 2)
            {
                foreach (int index in new int[] { _CategoryIndex, _RequiredByIndex, _RequiredIndex,_EntryIndex})
                {
                    dataGridViewX1.Rows[e.RowIndex + 1].Cells[index].Value = dataGridViewX1.Rows[e.RowIndex].Cells[index].Value;
                }
            }
        }

        private List<int> ProcessLevels(DataGridViewRow row)
        {
            List<int> list= new List<int>();
            int countLevel = 0;
            //掃描開課學期
            for (int i = _CreditStartIndex; i < _CreditStartIndex + 8; i++)
            {
                if (row.Cells[i].Value != null)
                {
                    //壘計課程級別
                    countLevel++;
                }
            }
            int startLevel = 0;
            //如果有填數開始級別則以開始級別開始算
            if (row.Cells[_StartLevelIndex].Value != null)
                int.TryParse("" + row.Cells[_StartLevelIndex].Value, out startLevel);
            //計算科目級別
            if (countLevel > 1)
            {
                #region 自動計算科目級別
                //如果開課超過一學期且沒有填入開始級別，開始級別從1開始
                if (startLevel == 0)
                {
                    //沒填入開始級別則第一筆不加級別
                    list.Add(0);
                    startLevel = 1;
                    //填入開始級別第二筆開始會從2開始算級別
                    for (int i = 1; i < countLevel; i++)
                    {
                        list.Add(i + startLevel);
                    }
                }
                else
                {
                    //填入開始級別
                    for (int i = 0; i < countLevel; i++)
                    {
                        list.Add(i + startLevel);
                    }
                }
                #endregion
            }
            else
            {
                //有填入開始級別，但沒有開課或只開課一學期
                list.Add(startLevel);
            }
            return list;
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

        private void dataGridViewX1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridViewCell cell = dataGridViewX1.Rows[e.RowIndex].Cells[e.ColumnIndex];
            cell.ErrorText = "";
            #region 數字欄位輸入格式檢察
            if ((e.ColumnIndex == _StartLevelIndex || (e.ColumnIndex >= _CreditStartIndex && e.ColumnIndex < _CreditStartIndex + 8)) && "" + e.FormattedValue!= "")
            {
                decimal i = 0;
                if (!decimal.TryParse(e.FormattedValue.ToString(), out i))
                {
                    cell.ErrorText = "必須輸入學分數";
                }
            }
            #endregion
            #region 檢查科目名稱頭尾不可有空白
            if ( e.ColumnIndex == _SubjectNameIndex && ( "" + e.FormattedValue != ( "" + e.FormattedValue ).Trim() ) )
                cell.ErrorText = "科目名稱頭尾不可有空白字元";
            #endregion
            dataGridViewX1.UpdateCellErrorText(e.ColumnIndex, e.RowIndex); 

            #region 學分數檢察
            if ("" + dataGridViewX1.Rows[e.RowIndex].Cells[_SubjectNameIndex].FormattedValue != "")
            {
                bool pass = false;
                #region 若是正輸入的欄位則用驗證值檢查否則用欄位上的值檢察
                for (int i = 0; i < 8; i++)
                {
                    decimal x = 0;
                    if (i + _CreditStartIndex == e.ColumnIndex && (decimal.TryParse(e.FormattedValue.ToString(), out x)))
                    {
                        pass = true;
                        break;
                    }
                    else if (decimal.TryParse("" + dataGridViewX1.Rows[e.RowIndex].Cells[i + _CreditStartIndex].FormattedValue, out x))
                    {
                        pass = true;
                        break;
                    }
                } 
                #endregion
                if (!pass)
                {
                    dataGridViewX1.Rows[e.RowIndex].Cells[_SubjectNameIndex].ErrorText = "必須輸入學分數";
                    dataGridViewX1.UpdateCellErrorText(_SubjectNameIndex, e.RowIndex);
                }
                else if (dataGridViewX1.Rows[e.RowIndex].Cells[_SubjectNameIndex].ErrorText == "必須輸入學分數")
                {
                    dataGridViewX1.Rows[e.RowIndex].Cells[_SubjectNameIndex].ErrorText = "";
                    dataGridViewX1.UpdateCellErrorText(_SubjectNameIndex, e.RowIndex);
                }
            }
            #endregion
        }

        private void dataGridViewX1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 && e.Button == MouseButtons.Right)
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

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            dataGridViewX1.Rows.Insert(_SelectedRowIndex, new DataGridViewRow());
            _IsDirty =true;
            if (IsDirtyChanged != null)
                IsDirtyChanged.Invoke(this, new EventArgs());
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (dataGridViewX1.Rows.Count - 1 > _SelectedRowIndex)
            {
                dataGridViewX1.Rows.RemoveAt(_SelectedRowIndex);
            }
        }

        private void dataGridViewX1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = 0; i < e.RowCount; i++)
            {
                //如果有前一筆資料則複製前筆資料
                if (i + e.RowIndex > 0)
                {
                    foreach (int index in new int[] { _CategoryIndex, _RequiredByIndex, _RequiredIndex,_EntryIndex })
                    {
                        dataGridViewX1.Rows[i + e.RowIndex].Cells[index].Value = dataGridViewX1.Rows[i + e.RowIndex - 1].Cells[index].Value;
                    }
                }
            }
        }

        private void dataGridViewX1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewX1.SelectedCells.Count == 1)
                dataGridViewX1.BeginEdit(true);
        }

        public GraduationPlanEditor()
        {
            InitializeComponent();

            if ( GlobalManager.Renderer is Office2007Renderer )
            {
                ( GlobalManager.Renderer as Office2007Renderer ).ColorTableChanged += delegate { this.dataGridViewX1.AlternatingRowsDefaultCellStyle.BackColor = ( GlobalManager.Renderer as Office2007Renderer ).ColorTable.RibbonBar.MouseOver.TopBackground.End; };
                this.dataGridViewX1.AlternatingRowsDefaultCellStyle.BackColor = ( GlobalManager.Renderer as Office2007Renderer ).ColorTable.RibbonBar.MouseOver.TopBackground.End;
            }
            
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

            dataGridViewX1.CurrentCell = dataGridViewX1.FirstDisplayedCell;
            if (dataGridViewX1.CurrentCell !=null)
                dataGridViewX1.BeginEdit(true);

        }

        #region IGraduationPlanEditor 成員

        public void SetSource(System.Xml.XmlElement source)
        {
            _defaultValues = new Dictionary<DataGridViewCell, object>();
            _RowSubject = new Dictionary<DataGridViewRow, string>();
            _DirtyCells = new List<DataGridViewCell>();
            dataGridViewX1.Rows.Clear();
            _RawDeleted =_IsDirty= false;
            Dictionary<string, DataGridViewRow> rowDictionary = new Dictionary<string, DataGridViewRow>();
            if (source != null)
            {
                foreach (XmlNode node in source.SelectNodes("Subject"))
                {
                    DataGridViewRow row;
                    XmlElement element = (XmlElement)node;
                    XmlNode groupNode = element.SelectSingleNode("Grouping");
                    //檢查是否符合群組設定
                    if (groupNode != null && groupNode.SelectSingleNode("@RowIndex") != null && groupNode.SelectSingleNode("@startLevel") != null)
                    {
                        #region 以第一筆資料為主填入各級別學年度學期學分數
                        XmlElement groupElement = (XmlElement)groupNode;
                        if (!rowDictionary.ContainsKey(groupElement.Attributes["RowIndex"].InnerText))
                        {
                            row = new DataGridViewRow();
                            row.CreateCells(dataGridViewX1, "", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
                            dataGridViewX1.Rows.Add(row);
                            row.Cells[_CategoryIndex].Value = element.Attributes["Category"].InnerText;
                            row.Cells[_DomainIndex].Value = element.Attributes["Domain"].InnerText;
                            row.Cells[_RequiredByIndex].Value = element.Attributes["RequiredBy"].InnerText;
                            row.Cells[_RequiredIndex].Value = element.Attributes["Required"].InnerText;
                            row.Cells[_SubjectNameIndex].Value = element.Attributes["SubjectName"].InnerText;

                            //舊版沒有下面這兩個 Attributes
                            if (element.HasAttribute("NotIncludedInCredit") && element.HasAttribute("NotIncludedInCalc") && element.Attributes["NotIncludedInCredit"].Value != "" && element.Attributes["NotIncludedInCalc"].Value != "")
                            {
                                row.Cells[_NotIncludedInCreditIndex].Value = bool.Parse(element.Attributes["NotIncludedInCredit"].Value);
                                row.Cells[_NotIncludedInCalcIndex].Value = bool.Parse(element.Attributes["NotIncludedInCalc"].Value);
                            }
                            
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
                            {
                                row.Cells[_EntryIndex].Value = "學業";
                            }
                            row.Cells[_StartLevelIndex].Value = element.Attributes["Level"].InnerText;
                            rowDictionary.Add(groupElement.Attributes["RowIndex"].InnerText, row);
                            //呼叫結束編輯處理函式
                            dataGridViewX1_CellEndEdit(this, new DataGridViewCellEventArgs(_SubjectNameIndex, row.Index));
                        }
                        else
                            row = rowDictionary[groupElement.Attributes["RowIndex"].InnerText];
                        //填入自動開課資料(年級學期學分數)
                        int gradeyear = 0;
                        int semester = 0;
                        if (int.TryParse(element.Attributes["GradeYear"].InnerText, out gradeyear) && int.TryParse(element.Attributes["Semester"].InnerText, out semester))
                            row.Cells[(gradeyear - 1) * 2 + semester + _CreditStartIndex-1].Value = element.Attributes["Credit"].InnerText;
                        //呼叫結束編輯處理函式
                        dataGridViewX1_CellEndEdit(this, new DataGridViewCellEventArgs((gradeyear - 1) * 2 + semester + _CreditStartIndex-1, row.Index));
                        #endregion

                    }
                    else
                    {
                        #region 以該科別的開始級別做為開始級別
                        row = new DataGridViewRow();
                        row.CreateCells(dataGridViewX1, "", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
                        dataGridViewX1.Rows.Add(row);
                        row.Cells[_CategoryIndex].Value = element.Attributes["Category"].InnerText;
                        row.Cells[_DomainIndex].Value = element.Attributes["Domain"].InnerText;
                        row.Cells[_RequiredByIndex].Value = element.Attributes["RequiredBy"].InnerText;
                        row.Cells[_RequiredIndex].Value = element.Attributes["Required"].InnerText;
                        row.Cells[_SubjectNameIndex].Value = element.Attributes["SubjectName"].InnerText;
                        row.Cells[_NotIncludedInCreditIndex].Value = element.Attributes["NotIncludedInCredit"].InnerText == "True" ? true : false;
                        row.Cells[_NotIncludedInCalcIndex].Value = element.Attributes["NotIncludedInCalc"].InnerText == "True" ? true : false;
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
                        //填入自動開課資料(年級學期學分數)
                        int gradeyear = 0;
                        int semester = 0;
                        if (int.TryParse(element.Attributes["GradeYear"].InnerText, out gradeyear) && int.TryParse(element.Attributes["Semester"].InnerText, out semester))
                            row.Cells[(gradeyear - 1) * 2 + semester + _CreditStartIndex-1].Value = element.Attributes["Credit"].InnerText;
                        #endregion
                        //呼叫結束編輯處理函式
                        dataGridViewX1_CellEndEdit(this, new DataGridViewCellEventArgs(_SubjectNameIndex, row.Index));

                    }
                }
            }
            if (this.IsValidated) {}
            dataGridViewX1.CurrentCell = dataGridViewX1.FirstDisplayedCell;
            if(dataGridViewX1.CurrentCell !=null)
                dataGridViewX1.BeginEdit(true);
            ValidateSameSubjectSameLevel();
        }
        /// <summary>
        /// 取得設定資料
        /// </summary>
        /// <returns></returns>
        public System.Xml.XmlElement GetSource()
        {
            return GetSource(string.Empty);
        }
        /// <summary>
        /// 取得設定資料
        /// </summary>
        /// <returns></returns>
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
                    //int countLevel = 0;
                    //掃描開課學期
                    for (int i = _CreditStartIndex; i < _CreditStartIndex+8; i++)
                    {
                        if (row.Cells[i].Value != null)
                        {
                            ////壘計課程級別並記錄學分數學期年級
                            //countLevel++;
                            Credits.Add(decimal.Parse("" + row.Cells[i].Value));
                            Semesters.Add((i - _CreditStartIndex+2) % 2 + 1);
                            GradeYears.Add((i - _CreditStartIndex+2) / 2);
                        }
                    }

                    //int startLevel = 0;
                    ////如果有填數開始級別則以開始級別開始算
                    //if (row.Cells[_StartLevelIndex].Value != null)
                    //    int.TryParse("" + row.Cells[_StartLevelIndex].Value, out startLevel);
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

                    bool b;
                    bool.TryParse(row.Cells[_NotIncludedInCreditIndex].Value == null ? "false" : row.Cells[_NotIncludedInCreditIndex].Value.ToString(), out b);
                    parentElement.SetAttribute("NotIncludedInCredit", b.ToString());
                    bool.TryParse(row.Cells[_NotIncludedInCalcIndex].Value == null ? "false" : row.Cells[_NotIncludedInCalcIndex].Value.ToString(), out b);
                    parentElement.SetAttribute("NotIncludedInCalc", b.ToString());

                    #region 填入分項類別
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
	                #endregion


                    #region 計算科目級別
                    List<int> levelList = ProcessLevels(row);
                    if(levelList.Count==0)
                        throw new Exception("輸入資料無法計算學分數");
                    if (levelList.Count == 1 && levelList[0] == 0)
                    {
                        XmlElement element = (XmlElement)parentElement.CloneNode(true);
                        element.SetAttribute("FullName", subjectName);
                        element.SetAttribute("Level", row.Cells[_StartLevelIndex].Value == null ? "" : row.Cells[_StartLevelIndex].Value.ToString());

                        element.SetAttribute("Credit", Credits[0].ToString());
                        element.SetAttribute("GradeYear", GradeYears[0].ToString());
                        element.SetAttribute("Semester", Semesters[0].ToString());

                        doc.DocumentElement.AppendChild(element);
                    }
                    else
                    {
                        int index = 0;
                        //沒輸入開始級別，第一筆沒有級別，第二筆以後從2開始
                            XmlElement element = (XmlElement)parentElement.CloneNode(true);
                        if (levelList[0] == 0)
                        {
                            #region 單獨處理第一筆

                            element.SetAttribute("FullName", subjectName);
                            element.SetAttribute("Level", "");

                            element.SetAttribute("Credit", Credits[0].ToString());
                            element.SetAttribute("GradeYear", GradeYears[0].ToString());
                            element.SetAttribute("Semester", Semesters[0].ToString());

                            doc.DocumentElement.AppendChild(element);
                            #endregion
                            //接下來從第二筆開始
                            index = 1;
                        }
                        for (int i = index; i < levelList.Count; i++)
                        {
                            element = (XmlElement)parentElement.CloneNode(true);

                            element.SetAttribute("FullName", subjectName + " " + GetNumber(levelList[i]));
                            element.SetAttribute("Level", (levelList[i]).ToString());

                            element.SetAttribute("Credit", Credits[i].ToString());
                            element.SetAttribute("GradeYear", GradeYears[i].ToString());
                            element.SetAttribute("Semester", Semesters[i].ToString());

                            doc.DocumentElement.AppendChild(element);
                        }
                    }
                    #region OldWay
                    //if (countLevel == 0)
                    //    throw new Exception("輸入資料無法計算學分數");
                    //if (countLevel == 1 && startLevel == 0)
                    //{
                    //    XmlElement element = (XmlElement)parentElement.CloneNode(true);
                    //    element.SetAttribute("FullName", subjectName);
                    //    element.SetAttribute("Level", row.Cells[_StartLevelIndex].Value == null ? "" : row.Cells[_StartLevelIndex].Value.ToString());

                    //    element.SetAttribute("Credit", Credits[0].ToString());
                    //    element.SetAttribute("GradeYear", GradeYears[0].ToString());
                    //    element.SetAttribute("Semester", Semesters[0].ToString());

                    //    doc.DocumentElement.AppendChild(element);
                    //}
                    //else//countLevel>1
                    //{
                    //    //沒輸入開始級別，第一筆沒有級別，第二筆以後從2開始
                    //    if (startLevel == 0)
                    //    {
                    //        #region 單獨處理第一筆
                    //        XmlElement element = (XmlElement)parentElement.CloneNode(true);

                    //        element.SetAttribute("FullName", subjectName);
                    //        element.SetAttribute("Level", "");

                    //        element.SetAttribute("Credit", Credits[0].ToString());
                    //        element.SetAttribute("GradeYear", GradeYears[0].ToString());
                    //        element.SetAttribute("Semester", Semesters[0].ToString());

                    //        doc.DocumentElement.AppendChild(element); 
                    //        #endregion
                    //        //填入開始級別
                    //        startLevel = 1;
                    //        for (int i = 1; i < countLevel; i++)
                    //        {
                    //            #region 第二筆之後
                    //            element = (XmlElement)parentElement.CloneNode(true);

                    //            element.SetAttribute("FullName", subjectName + " " + GetNumber(i + startLevel));
                    //            element.SetAttribute("Level", (i + startLevel).ToString());

                    //            element.SetAttribute("Credit", Credits[i].ToString());
                    //            element.SetAttribute("GradeYear", GradeYears[i].ToString());
                    //            element.SetAttribute("Semester", Semesters[i].ToString());

                    //            doc.DocumentElement.AppendChild(element); 
                    //            #endregion
                    //        }
                    //    }
                    //    else
                    //    {
                    //        //填入開始級別
                    //        for (int i = 0; i < countLevel; i++)
                    //        {
                    //            XmlElement element = (XmlElement)parentElement.CloneNode(true);

                    //            element.SetAttribute("FullName", subjectName + " " + GetNumber(i + startLevel));
                    //            element.SetAttribute("Level", (i + startLevel).ToString());

                    //            element.SetAttribute("Credit", Credits[i].ToString());
                    //            element.SetAttribute("GradeYear", GradeYears[i].ToString());
                    //            element.SetAttribute("Semester", Semesters[i].ToString());

                    //            doc.DocumentElement.AppendChild(element);
                    //        }
                    //    }
                    //} 
                    #endregion
                    #endregion.
                }
            }
            return doc.DocumentElement;
        }

        public bool IsDirty
        {
            get { return _IsDirty | _RawDeleted; }
        }

        public event EventHandler IsDirtyChanged;

       public  bool IsValidated
        {
            get
            {
                dataGridViewX1.EndEdit();
                foreach (DataGridViewRow row in dataGridViewX1.Rows)
                {
                    #region 學分數檢察
                    if (row.Cells[_SubjectNameIndex].FormattedValue.ToString() != "")
                    {
                        bool pass = false;
                        #region 若是正輸入的欄位則用驗證值檢查否則用欄位上的值檢察
                        for (int i = 0; i < 8; i++)
                        {
                            decimal x = 0;
                            if (decimal.TryParse("" + row.Cells[i + _CreditStartIndex].FormattedValue, out x))
                            {
                                pass = true;
                                break;
                            }
                        }
                        #endregion
                        if (!pass)
                        {
                            row.Cells[_SubjectNameIndex].ErrorText = "必須輸入學分數";
                            dataGridViewX1.UpdateCellErrorText(_SubjectNameIndex, row.Index);
                        }
                        else if (row.Cells[_SubjectNameIndex].ErrorText == "必須輸入學分數")
                        {
                            row.Cells[_SubjectNameIndex].ErrorText = "";
                            dataGridViewX1.UpdateCellErrorText(_SubjectNameIndex, row.Index);
                        }
                    }

                    #region 檢查科目名稱頭尾不可有空白
                    if ( _RowSubject.ContainsKey(row) && ( _RowSubject[row] != (  _RowSubject[row] ).Trim() ) )
                    {
                        row.Cells[_SubjectNameIndex].ErrorText = "科目名稱頭尾不可有空白字元";
                        dataGridViewX1.UpdateCellErrorText(_SubjectNameIndex, row.Index);
                    }
                    #endregion
                    #endregion
                }
                if (ValidateSameSubjectSameLevel())
                {
                    foreach (DataGridViewRow row in dataGridViewX1.Rows)
                    {
                        if (row.ErrorText != "")
                            return false;
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell.ErrorText != "")
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        private bool  ValidateSameSubjectSameLevel()
        {
            bool allPsss = true;
            List<string> subjectWithLevel = new List<string>();
            foreach (DataGridViewRow row in _RowSubject.Keys)
            {
                bool passed=true;
                foreach (int level in ProcessLevels(row))
                {
                    string key = _RowSubject[row] + "_" + level;
                    if (subjectWithLevel.Contains(key))
                    {
                        passed = false;
                        break;
                    }
                    else
                    {
                        subjectWithLevel.Add(key);
                    }
                }
                if (passed)
                {
                    if (row.Cells[_SubjectNameIndex].ErrorText == "已經有相同科目相同級別的資料存在，\n請檢察科目級別。")
                    {
                        row.Cells[_SubjectNameIndex].ErrorText = "";
                        dataGridViewX1.UpdateCellErrorText(_SubjectNameIndex, row.Index);
                    }
                }
                else
                {
                    if (row.Cells[_SubjectNameIndex].ErrorText == "")
                    {
                        row.Cells[_SubjectNameIndex].ErrorText = "已經有相同科目相同級別的資料存在，\n請檢察科目級別。";
                        dataGridViewX1.UpdateCellErrorText(_SubjectNameIndex, row.Index);
                    }
                }
                allPsss &= passed;
            }
            return allPsss;
        }

        #endregion

        private void dataGridViewX1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
            DataGridViewCell cell = dataGridViewX1.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string message = "儲存格值：" + cell.Value + "。\n發生錯誤： " + e.Exception.Message+"。";
            if (cell.ErrorText != message)
            {
                cell.ErrorText = message;
                dataGridViewX1.UpdateCellErrorText(e.ColumnIndex, e.RowIndex);
            }
        }

        private void dataGridViewX1_MouseHover(object sender, EventArgs e)
        {
            if(!dataGridViewX1.ContainsFocus)
            dataGridViewX1.Focus();
        }

        private void dataGridViewX1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            List<DataGridViewRow> deletedList=new List<DataGridViewRow>();
            foreach (DataGridViewRow row in _RowSubject.Keys)
            {
                if (!dataGridViewX1.Rows.Contains(row))
                    deletedList.Add(row);
            }
            if (deletedList.Count > 0)
            {
                foreach (DataGridViewRow row in deletedList)
                {
                    _RowSubject.Remove(row);
                }
                _RawDeleted = _IsDirty = true;
                if (IsDirtyChanged != null)
                    IsDirtyChanged.Invoke(this, new EventArgs());
            }
        }

        private void dataGridViewX1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dataGridViewX1.EndEdit();
        }
    }
}

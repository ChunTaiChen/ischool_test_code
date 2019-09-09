using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace SmartSchool.Evaluation.Configuration
{
    public partial class SubjectTableEditor : UserControl
    {
        private int _SelectedRowIndex;

        private bool _ProgramTable = false;

        public SubjectTableEditor()
        {
            InitializeComponent();

            textBoxX1.TextChanged += (sender,e) =>
            {
                errorProvider1.Clear();
                decimal integer;
                if (!decimal.TryParse(textBoxX1.Text, out integer))
                    errorProvider1.SetError(textBoxX1, "請輸入數字!");
            };

            textBoxX2.TextChanged += (sender, e) =>
            {
                errorProvider2.Clear();
                decimal integer;
                if (!decimal.TryParse(textBoxX2.Text, out integer))
                    errorProvider2.SetError(textBoxX2, "請輸入數字!");
            };
        }

        public XmlElement Content
        {
            set
            {
                textBoxX1.Text = string.Empty;
                textBoxX2.Text = string.Empty;

                dataGridViewX1.Rows.Clear();
                XmlElement element = (XmlElement)value.SelectSingleNode("SubjectTableContent");
                if ( element != null )
                {
                     decimal integer;
                     decimal CoreInteger;

                    //學程科目表 (2011/4/21修改)
                    if (_ProgramTable) 
                    {
                        //2011/4/21將integerInput改為textBox，解決dataGrid在編輯模式時無法切換輸入
                        //if (int.TryParse(element.GetAttribute("CreditCount"), out integer))
                        //    integerInput1.Value = integer;
                        //if (int.TryParse(element.GetAttribute("CoreCount"), out integer))
                        //    integerInput2.Value = integer;

                        if (decimal.TryParse(element.GetAttribute("CreditCount"), out integer))
                            textBoxX1.Text = ""+integer;
                        if (decimal.TryParse(element.GetAttribute("CoreCount"), out CoreInteger))
                             textBoxX2.Text = "" + CoreInteger;
                    }
                    //核心科目表 (2011/4/21 修改)
                    else 
                    {
                        //2011/4/21將integerInput改為textBox，解決dataGrid在編輯模式時無法切換輸入
                        //if (int.TryParse(element.GetAttribute("AttendCount"), out integer))
                        //    integerInput1.Value = integer;
                        //if (int.TryParse(element.GetAttribute("CreditCount"), out integer))
                        //    integerInput2.Value = integer;

                         if (decimal.TryParse(element.GetAttribute("AttendCount"), out CoreInteger))
                              textBoxX1.Text = "" + CoreInteger;
                        if (decimal.TryParse(element.GetAttribute("CreditCount"), out integer))
                            textBoxX2.Text = ""+integer;
                    }

                    foreach ( XmlElement subjectNode in element.SelectNodes("Subject") )
                    {
                        string name;
                        name =subjectNode.GetAttribute("Name");
                        List<int> levels = new List<int>();
                        foreach ( XmlNode levelNode in subjectNode.SelectNodes("Level") )
                        {
                            levels.Add(int.Parse(levelNode.InnerText));
                        }
                        DataGridViewRow row = new DataGridViewRow();
                        if (_ProgramTable) //學程科目表
                        {
                            bool iscore = false;
                            bool.TryParse(subjectNode.GetAttribute("IsCore"), out iscore);
                            row.CreateCells(dataGridViewX1, name, "", iscore);
                        }
                        else //核心科目表
                        {
                            row.CreateCells(dataGridViewX1, name, "",false);
                        }
                        row.Cells[0].Tag = name;
                        row.Tag = levels;

                        if ( levels.Count != 0 )
                        {
                            string cellString = "" + levels[0];
                            string levelString = "(" +GetNumber( levels[0]);
                            for ( int i = 1 ; i < levels.Count ; i++ )
                            {
                                levelString += "、" + GetNumber(levels[i]);
                                cellString += "," + levels[i];
                            }
                            levelString += ")";
                            row.Cells[0].Value = "" + row.Cells[0].Tag + levelString;
                            row.Cells[1].Value = cellString;
                        }



                        this.dataGridViewX1.Rows.Add(row);
                    }
                }
            }
            get
            {
                XmlElement element = new XmlDocument().CreateElement("SubjectTableContent");
                
                if (_ProgramTable) //學程科目表
                {
                    //2011/4/21將integerInput改為textBox，解決dataGrid在編輯模式時無法切換輸入
                    //element.SetAttribute("CreditCount", "" + integerInput1.Value);
                    //element.SetAttribute("CoreCount", "" + integerInput2.Value);

                    element.SetAttribute("CreditCount", "" + textBoxX1.Text);
                    element.SetAttribute("CoreCount", "" + textBoxX2.Text);
                }
                else //核心科目表(2011/3/29 - wei修改)
                {
                    //2011/4/21將integerInput改為textBox，解決dataGrid在編輯模式時無法切換輸入
                    //element.SetAttribute("AttendCount", "" + integerInput1.Value);
                    //element.SetAttribute("CreditCount", "" + integerInput2.Value);

                    element.SetAttribute("AttendCount", "" + textBoxX1.Text);
                    element.SetAttribute("CreditCount", "" + textBoxX2.Text);
                }

                foreach ( DataGridViewRow row in dataGridViewX1.Rows )
                {
                    #region 科目
                    if ( row.IsNewRow ) continue;
                    XmlElement subjectElement = (XmlElement)element.AppendChild(element.OwnerDocument.CreateElement("Subject"));
                    subjectElement.SetAttribute("Name", "" + row.Cells[0].Tag);
                    if ( _ProgramTable )
                    {
                        subjectElement.SetAttribute("IsCore", "" + row.Cells[2].Value);
                    }
                    if ( row.Tag is List <int> )
                    {
                        #region 級別
                        foreach ( int level in (List<int>)row.Tag )
                        {
                            XmlElement levelElement = (XmlElement)subjectElement.AppendChild(element.OwnerDocument.CreateElement("Level"));
                            levelElement.InnerText = ""+level;
                        } 
                        #endregion
                    } 
                    #endregion
                }
                return element;
            }
        }

        public bool IsValidated()
        {
            bool pass = true;
            foreach ( DataGridViewRow row in dataGridViewX1.Rows )
            {
                if ( row.IsNewRow ) continue;
                if ( row.Cells[1].ErrorText != "" )
                    pass &= false;
            }

            if (!string.IsNullOrEmpty(errorProvider1.GetError(textBoxX1)))
                return false;

            if (!string.IsNullOrEmpty(errorProvider2.GetError(textBoxX2)))
                return false;

            return pass;
        }

        public bool ProgramTable
        {
            get
            {
                return _ProgramTable;
            }
            set
            {
                _ProgramTable = value;
                if (_ProgramTable) //學程科目表
                {
                    labelX1.Text = "需修習下列科目達";
                    labelX2.Text = "  學分、含核心科目";
                    labelX3.Text = "  學分。";
                    colCore.Visible = true;
                }
                else //核心科目表(2011/3/29 - wei修改)
                {
                    labelX1.Text = "下方列舉科目應修";
                    labelX2.Text = "  學分，且";
                    labelX3.Text = "  學分為成績及格。";
                }
            }
        }

        private void dataGridViewX1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //編輯科目行時先還原成只有科目名稱
            if ( e.ColumnIndex == 0 && e.RowIndex >= 0 )
                dataGridViewX1.Rows[e.RowIndex].Cells[0].Value = "" + dataGridViewX1.Rows[e.RowIndex].Cells[0].Tag;
        }

        private void dataGridViewX1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if ( e.ColumnIndex == 0 && e.RowIndex >= 0 )
            {
                if ( dataGridViewX1.Rows[e.RowIndex].Tag is List<int> )
                {
                    List<int> levels = (List<int>)dataGridViewX1.Rows[e.RowIndex].Tag;
                    if ( levels.Count != 0 )
                    {
                        string levelString = "(" + GetNumber(levels[0]);
                        for ( int i = 1 ; i < levels.Count ; i++ )
                        {
                            levelString += "、" + GetNumber(levels[i]);
                        }
                        levelString += ")";
                        dataGridViewX1.Rows[e.RowIndex].Cells[0].Value = "" + dataGridViewX1.Rows[e.RowIndex].Cells[0].Tag + levelString;
                    }
                    else
                        dataGridViewX1.Rows[e.RowIndex].Cells[0].Value = "" + dataGridViewX1.Rows[e.RowIndex].Cells[0].Tag;
                }
                else
                {
                    dataGridViewX1.Rows[e.RowIndex].Cells[0].Value = "" + dataGridViewX1.Rows[e.RowIndex].Cells[0].Tag;
                }
            }
        }

        private void dataGridViewX1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            DataGridViewCell cell = dataGridViewX1.CurrentCell;
            //如果是編輯科目欄則將科目名稱存入 tag以供合併級還原
            if ( cell.ColumnIndex == 0 )
            {
                dataGridViewX1.Rows[cell.RowIndex].Cells[0].Tag = dataGridViewX1.Rows[cell.RowIndex].Cells[0].Value;
            }
            //如果是編輯級別欄位，整理輸入的級別資料成為級別清單
            if ( cell.ColumnIndex == 1 )
            {
                List<int> levels = new List<int>();
                try
                {
                    foreach ( string levelrange in ( "" + cell.Value as string ).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) )
                    {
                        string[] levelsp=levelrange.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if ( levelsp.Length == 2 )
                        { 
                            int end=int.Parse(levelsp[1]);
                            for ( int i = int.Parse(levelsp[0]) ; i <=end ; i++ )
                            {
                                levels.Add(i);    
                            }
                        }
                        else
                            levels.Add(int.Parse(levelrange));
                    }
                    cell.ErrorText = "";
                }
                catch
                {
                    cell.ErrorText = "輸入級別無法轉換。\n允許的輸入方式為：\n　1,2,3\n或1-3";
                    levels.Clear();
                }
                dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
                dataGridViewX1.Rows[cell.RowIndex].Tag = levels;
            }
            //重新合併科目跟級別
            if ( dataGridViewX1.Rows[cell.RowIndex].Tag is List<int > )
            {
                List<int> levels = (List<int>)dataGridViewX1.Rows[cell.RowIndex].Tag;
                if ( levels.Count != 0 )
                {
                    string levelString = "(" + GetNumber(levels[0]);
                    for ( int i = 1 ; i < levels.Count ; i++ )
                    {
                        levelString += "、" + GetNumber(levels[i]);
                    }
                    levelString += ")";
                    dataGridViewX1.Rows[cell.RowIndex].Cells[0].Value = "" + dataGridViewX1.Rows[cell.RowIndex].Cells[0].Tag + levelString;
                }
                else
                    dataGridViewX1.Rows[cell.RowIndex].Cells[0].Value = "" + dataGridViewX1.Rows[cell.RowIndex].Cells[0].Tag;
            }
            else
            {
                dataGridViewX1.Rows[cell.RowIndex].Cells[0].Value = "" + dataGridViewX1.Rows[cell.RowIndex].Cells[0].Tag;
            }


            cell.Value = cell.EditedFormattedValue;
            dataGridViewX1.EndEdit();
            dataGridViewX1.BeginEdit(false);
        }

        private void dataGridViewX1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewX1.SelectedCells.Count == 1)
                dataGridViewX1.BeginEdit(true);
        }

        private void dataGridViewX1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if ( e.RowIndex > 0 && e.ColumnIndex < 0 && e.Button == MouseButtons.Right )
            {
                _SelectedRowIndex = e.RowIndex;
                foreach ( DataGridViewRow var in dataGridViewX1.SelectedRows )
                {
                    if ( var.Index != _SelectedRowIndex )
                        var.Selected = false;
                }
                dataGridViewX1.Rows[_SelectedRowIndex].Selected = true;
                contextMenuStrip1.Show(dataGridViewX1, dataGridViewX1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if ( _SelectedRowIndex > 0 && dataGridViewX1.Rows.Count - 1 > _SelectedRowIndex )
                dataGridViewX1.Rows.RemoveAt(_SelectedRowIndex);
        }

        private string GetNumber(int p)
        {
            string levelNumber;
            switch ( p )
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
                    levelNumber = "" + ( p );
                    break;
                #endregion
            }
            return levelNumber;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using DevComponents.DotNetBar.Controls;
using DevComponents.DotNetBar.Rendering;

namespace SmartSchool.Evaluation.Configuration
{
    public partial class ScoreCalcRuleEditor : UserControl
    {
        //private string _scrID;
        //private string _scrName;
        //private XmlElement _scrContent;
        //Control[] checkedList = new Control[] { radioButton16, radioButton9, radioButton4, radioButton8, radioButton12, checkBoxX1, checkBoxX2, checkBoxX3, checkBoxX4, radioButton10, radioButton31, radioButton20, radioButton21, radioButton13, radioButton28 };
        //Control[] uncheckedList = new Control[] { radioButton15, radioButton1, radioButton5, radioButton2, radioButton3, radioButton6, radioButton7, radioButton30, radioButton29, checkBoxX8, checkBoxX11, checkBoxX13, checkBoxX15, radioButton11, radioButton25, radioButton19, radioButton22, radioButton14, radioButton26, radioButton27 };
        private List<Control> checkedList = new List<Control>();
        private List<Control> uncheckedList = new List<Control>();
        private bool reseting = false;
        private int _SelectedRowIndex;

        public ScoreCalcRuleEditor()
        {
            InitializeComponent();
            try
            {
                #region 如果系統的Renderer是Office2007Renderer，同化_ClassTeacherView,_CategoryView的顏色
                if (GlobalManager.Renderer is Office2007Renderer)
                {
                    ((Office2007Renderer)GlobalManager.Renderer).ColorTableChanged += new EventHandler(ScoreCalcRuleEditor_ColorTableChanged);
                    SetForeColor(this);
                }
                #endregion

                SetupControl(this);
                SetDefault();
            }
            catch { }
        }

        #region UI相關
        private void SetupControl(Control control)
        {
            if (control is RadioButton)
            {
                RadioButton ctr = ((RadioButton)control);
                ctr.CheckedChanged += new EventHandler(ScoreCalcRuleEditor_CheckedChanged);
                if (ctr.Checked)
                {
                    checkedList.Add(ctr);
                }
                else
                    uncheckedList.Add(ctr);
            }
            else if (control is CheckBoxX)
            {
                CheckBoxX ctr = ((CheckBoxX)control);
                ctr.CheckedChanged += new EventHandler(ScoreCalcRuleEditor_CheckedChanged);
                if (ctr.Checked)
                {
                    checkedList.Add(ctr);
                }
                else
                    uncheckedList.Add(ctr);
            }
            else
                control.TextChanged += new EventHandler(control_TextChanged);
            foreach (Control var in control.Controls)
            {
                SetupControl(var);
            }
        }

        void ScoreCalcRuleEditor_CheckedChanged(object sender, EventArgs e)
        {
            if (!reseting)
            {

            }
        }

        void control_TextChanged(object sender, EventArgs e)
        {
            if (!reseting)
            {

            }
        }

        //public ScoreCalcRuleEditor(string name):this()
        //{
        //    InitializeComponent();
        //    textBoxX1.Text = name;
        //    comboBoxEx2.SelectedIndex = 0;
        //    #region 如果系統的Renderer是Office2007Renderer，同化_ClassTeacherView,_CategoryView的顏色
        //    if (GlobalManager.Renderer is Office2007Renderer)
        //    {
        //        ((Office2007Renderer)GlobalManager.Renderer).ColorTableChanged += new EventHandler(ScoreCalcRuleEditor_ColorTableChanged);
        //        SetForeColor(this);
        //    }
        //    #endregion
        //}

        //public ScoreCalcRuleEditor(string name, XmlElement content)
        //    : this(name)
        //{
        //    SetSource(content);
        //}

        //public ScoreCalcRuleEditor(ScoreCalcRuleInfo scrInfo)
        //    : this(scrInfo.Name, scrInfo.ScoreCalcRuleElement)
        //{
        //    _scrID = scrInfo.ID;
        //}

        void ScoreCalcRuleEditor_ColorTableChanged(object sender, EventArgs e)
        {
            SetForeColor(this);
        }

        private void SetForeColor(Control parent)
        {
            foreach (Control var in parent.Controls)
            {
                if (var is RadioButton)
                    var.ForeColor = ((Office2007Renderer)GlobalManager.Renderer).ColorTable.CheckBoxItem.Default.Text;
                SetForeColor(var);
            }
        }
        #endregion

        private string _Name = "";

        public string ScoreCalcRuleName
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }
        /// <summary>
        /// 設為預設內容
        /// </summary>
        private void SetDefault()
        {
            reseting = true;
            #region 取消選取項目
            foreach (Control var in uncheckedList)
            {
                if (var is RadioButton)
                {
                    ((RadioButton)var).Checked = false;
                }
                if (var is CheckBoxX)
                {
                    ((CheckBoxX)var).Checked = false;
                }
            }
            #endregion
            #region 選取項目
            foreach (Control var in checkedList)
            {
                if (var is RadioButton)
                {
                    ((RadioButton)var).Checked = true;
                }
                if (var is CheckBoxX)
                {
                    ((CheckBoxX)var).Checked = true;
                }
            }
            #endregion
            #region 及格標準預設值
            dataGridViewX1.Rows.Clear();
            dataGridViewX1.Rows.Add();
            dataGridViewX1.Rows[0].DefaultCellStyle.BackColor = Color.AntiqueWhite;
            dataGridViewX1.Rows[0].Cells[0].Value = "預設";
            dataGridViewX1.Rows[0].Cells[0].ReadOnly = true;
            dataGridViewX1.Rows[0].Cells[1].Value = "60";
            dataGridViewX1.Rows[0].Cells[2].Value = "60";
            dataGridViewX1.Rows[0].Cells[3].Value = "60";
            dataGridViewX1.Rows[0].Cells[4].Value = "60";
            dataGridViewX1.Rows[0].Cells[5].Value = "40";
            dataGridViewX1.Rows[0].Cells[6].Value = "40";
            dataGridViewX1.Rows[0].Cells[7].Value = "40";
            dataGridViewX1.Rows[0].Cells[8].Value = "40";

            dataGridViewX1.CurrentCell = dataGridViewX1.FirstDisplayedCell;
            #endregion
            #region 學期科目成績計算至學年科目成績計算規則預設值(2010/6/24 騉翔新增)

            checkBoxX5.Checked = false;
            #endregion

            #region 新增學年學業成績及格 與 德行畢業判斷規則(功過相抵未滿三大過)(2011/3/29 - wei)

            checkBoxX7.Checked = false; //學年學業成績及格
            checkBoxX6.Checked = false; //功過相抵未滿三大過

            #endregion

            #region 新增 分項成績計算項目，併入學期學業成績 預設都勾起, 2014/3/20,ChenCT
            checkBoxX10.Checked = true; // 專業科目
            checkBoxX15.Checked = true; // 實習科目
            checkBoxX8.Checked = true; // 體育
            checkBoxX13.Checked = true; // 健康與護理
            checkBoxX11.Checked = true; // 國防通識
            #endregion

            #region 修改 學年調整成績預設值，不使用學年調整成績(不登錄成績、不修改取得學分),因使用者需求調整 2014/3/26,ChenCT
            radioButton27.Checked = true; // 不使用學年調整成績(不登錄成績、不修改取得學分)
            #endregion

            numericUpDown5.Value = 4;
            numericUpDown1.Value = numericUpDown2.Value = numericUpDown3.Value = numericUpDown4.Value = 2;
            comboBoxEx2.SelectedIndex = 1;
            comboBoxEx1.SelectedIndex = 0;
            //清空畢業學分數設定值
            textBoxX2.Text = textBoxX6.Text = textBoxX1.Text = textBoxX7.Text = textBoxX11.Text = textBoxX8.Text = textBoxX10.Text = "";



            #region 新增 計算採計學年科目成績欄位 其 預設值 (2017/1/12 穎驊新增)
            //原始成績
            checkBoxX25.Checked = true;
            //補考成績
            checkBoxX24.Checked = true;
            //重修成績
            checkBoxX23.Checked = true;
            //手動調整成績
            checkBoxX22.Checked = true;
            //學年調整成績
            //2017/1/12 穎驊筆記，恩正說 此項 學年調整成績 就邏輯上不會出現在"計算採計學年科目成績欄位" 故預設 為 false ，甚至 其在UI功能也 disable 掉
            checkBoxX21.Checked = false; 
            #endregion


            reseting = false;
        }
        /// <summary>
        /// 填入Xml內容
        /// </summary>
        /// <param name="source"></param>
        public void SetSource(XmlElement source)
        {
            SetDefault();
            XmlElement _scrContent = source == null ? new XmlDocument().CreateElement("ScoreCalcRule") : source;
            #region 學期科目成績屬性採計方式
            XmlElement element = (XmlElement)_scrContent.SelectSingleNode("學期科目成績屬性採計方式");
            bool tryParseBool = false;
            if (element != null)
            {
                if (element.InnerText == "以課程規劃表內容為準")
                    radioButton16.Checked = true;
                if (element.InnerText == "以實際學期科目成績內容為準")
                    radioButton15.Checked = true;
            }
            #endregion

            #region 必需修滿課程規劃表中所列必修課程
            //if (radioButton16.Checked)
            //{
            element = (XmlElement)_scrContent.SelectSingleNode("修滿所有必修課程");
            if (element != null && bool.TryParse(element.InnerText, out tryParseBool))
            {
                checkBoxX9.Tag = checkBoxX9.Checked = tryParseBool;
            }
            //}
            //else
            //{
            //    checkBoxX9.Tag = checkBoxX9.Checked = false;
            //}
            #endregion
            #region 各項成績計算位數
            if (_scrContent.SelectSingleNode("各項成績計算位數") != null)
            {
                #region 科目成績計算位數
                element = (XmlElement)_scrContent.SelectSingleNode("各項成績計算位數/科目成績計算位數");
                if (element != null)
                {
                    numericUpDown1.Value = decimal.Parse(element.GetAttribute("位數"));
                    if (bool.TryParse(element.GetAttribute("無條件進位"), out tryParseBool) && tryParseBool)
                        radioButton1.Checked = true;
                    if (bool.TryParse(element.GetAttribute("無條件捨去"), out tryParseBool) && tryParseBool)
                        radioButton5.Checked = true;
                    if (bool.TryParse(element.GetAttribute("四捨五入"), out tryParseBool) && tryParseBool)
                        radioButton9.Checked = true;
                }
                #endregion
                #region 學期分項成績計算位數
                element = (XmlElement)_scrContent.SelectSingleNode("各項成績計算位數/學期分項成績計算位數");
                if (element != null)
                {
                    numericUpDown2.Value = decimal.Parse(element.GetAttribute("位數"));
                    if (bool.TryParse(element.GetAttribute("無條件進位"), out tryParseBool) && tryParseBool)
                        radioButton2.Checked = true;
                    if (bool.TryParse(element.GetAttribute("無條件捨去"), out tryParseBool) && tryParseBool)
                        radioButton3.Checked = true;
                    if (bool.TryParse(element.GetAttribute("四捨五入"), out tryParseBool) && tryParseBool)
                        radioButton4.Checked = true;
                }
                #endregion
                #region 學年科目成績計算位數
                element = (XmlElement)_scrContent.SelectSingleNode("各項成績計算位數/學年科目成績計算位數");
                if (element != null)
                {
                    numericUpDown3.Value = decimal.Parse(element.GetAttribute("位數"));
                    if (bool.TryParse(element.GetAttribute("無條件進位"), out tryParseBool) && tryParseBool)
                        radioButton6.Checked = true;
                    if (bool.TryParse(element.GetAttribute("無條件捨去"), out tryParseBool) && tryParseBool)
                        radioButton7.Checked = true;
                    if (bool.TryParse(element.GetAttribute("四捨五入"), out tryParseBool) && tryParseBool)
                        radioButton8.Checked = true;
                }
                #endregion
                #region 學年分項成績計算位數
                element = (XmlElement)_scrContent.SelectSingleNode("各項成績計算位數/學年分項成績計算位數");
                if (element != null)
                {
                    numericUpDown6.Value = decimal.Parse(element.GetAttribute("位數"));
                    if (bool.TryParse(element.GetAttribute("無條件進位"), out tryParseBool) && tryParseBool)
                        radioButton10.Checked = true;
                    if (bool.TryParse(element.GetAttribute("無條件捨去"), out tryParseBool) && tryParseBool)
                        radioButton11.Checked = true;
                    if (bool.TryParse(element.GetAttribute("四捨五入"), out tryParseBool) && tryParseBool)
                        radioButton21.Checked = true;
                }
                #endregion
                #region 畢業成績計算位數
                element = (XmlElement)_scrContent.SelectSingleNode("各項成績計算位數/畢業成績計算位數");
                if (element != null)
                {
                    numericUpDown4.Value = decimal.Parse(element.GetAttribute("位數"));
                    if (bool.TryParse(element.GetAttribute("無條件進位"), out tryParseBool) && tryParseBool)
                        radioButton30.Checked = true;
                    if (bool.TryParse(element.GetAttribute("無條件捨去"), out tryParseBool) && tryParseBool)
                        radioButton29.Checked = true;
                    if (bool.TryParse(element.GetAttribute("四捨五入"), out tryParseBool) && tryParseBool)
                        radioButton12.Checked = true;
                }
                #endregion
            }
            #endregion
            #region 分項成績計算項目
            if (_scrContent.SelectSingleNode("分項成績計算項目") != null)
            {
                #region 體育
                element = (XmlElement)_scrContent.SelectSingleNode("分項成績計算項目/體育");
                if (element != null)
                {
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("計算成績"), out tryParseBool);
                    checkBoxX1.Checked = tryParseBool;
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("併入學期學業成績"), out tryParseBool);
                    checkBoxX8.Checked = tryParseBool;
                }
                #endregion
                #region 國防通識
                element = (XmlElement)_scrContent.SelectSingleNode("分項成績計算項目/國防通識");
                if (element != null)
                {
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("計算成績"), out tryParseBool);
                    checkBoxX2.Checked = tryParseBool;
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("併入學期學業成績"), out tryParseBool);
                    checkBoxX11.Checked = tryParseBool;
                }
                #endregion
                #region 健康與護理
                element = (XmlElement)_scrContent.SelectSingleNode("分項成績計算項目/健康與護理");
                if (element != null)
                {
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("計算成績"), out tryParseBool);
                    checkBoxX3.Checked = tryParseBool;
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("併入學期學業成績"), out tryParseBool);
                    checkBoxX13.Checked = tryParseBool;
                }
                #endregion
                #region 實習科目
                element = (XmlElement)_scrContent.SelectSingleNode("分項成績計算項目/實習科目");
                if (element != null)
                {
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("計算成績"), out tryParseBool);
                    checkBoxX4.Checked = tryParseBool;
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("併入學期學業成績"), out tryParseBool);
                    checkBoxX15.Checked = tryParseBool;
                }
                #endregion
                #region 專業科目
                element = (XmlElement)_scrContent.SelectSingleNode("分項成績計算項目/專業科目");
                if (element != null)
                {
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("計算成績"), out tryParseBool);
                    checkBoxX12.Checked = tryParseBool;
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("併入學期學業成績"), out tryParseBool);
                    checkBoxX10.Checked = tryParseBool;
                }
                else
                {
                    checkBoxX12.Checked = true;
                    checkBoxX10.Checked = false;
                }
                #endregion
            }
            #endregion
            #region 分項成績計算採計成績欄位
            element = (XmlElement)_scrContent.SelectSingleNode("分項成績計算採計成績欄位");
            if (element != null)
            {
                #region 原始成績
                tryParseBool = true;
                bool.TryParse(element.GetAttribute("原始成績"), out tryParseBool);
                checkBoxX16.Checked = tryParseBool;
                #endregion
                #region 補考成績
                tryParseBool = true;
                bool.TryParse(element.GetAttribute("補考成績"), out tryParseBool);
                checkBoxX17.Checked = tryParseBool;
                #endregion
                #region 重修成績
                tryParseBool = true;
                bool.TryParse(element.GetAttribute("重修成績"), out tryParseBool);
                checkBoxX19.Checked = tryParseBool;
                #endregion
                #region 擇優採計成績
                tryParseBool = true;
                bool.TryParse(element.GetAttribute("擇優採計成績"), out tryParseBool);
                checkBoxX14.Checked = tryParseBool;
                #endregion
                #region 學年調整成績
                tryParseBool = true;
                bool.TryParse(element.GetAttribute("學年調整成績"), out tryParseBool);
                checkBoxX18.Checked = tryParseBool;
                #endregion
            }
            #endregion

            #region 學年成績計算採計成績欄位
            element = (XmlElement)_scrContent.SelectSingleNode("學年成績計算採計成績欄位");
            if (element != null)
            {
                #region 原始成績
                tryParseBool = true;
                bool.TryParse(element.GetAttribute("原始成績"), out tryParseBool);
                checkBoxX25.Checked = tryParseBool;
                #endregion
                #region 補考成績
                tryParseBool = true;
                bool.TryParse(element.GetAttribute("補考成績"), out tryParseBool);
                checkBoxX24.Checked = tryParseBool;
                #endregion
                #region 重修成績
                tryParseBool = true;
                bool.TryParse(element.GetAttribute("重修成績"), out tryParseBool);
                checkBoxX23.Checked = tryParseBool;
                #endregion
                #region 擇優採計成績
                tryParseBool = true;
                bool.TryParse(element.GetAttribute("擇優採計成績"), out tryParseBool);
                checkBoxX22.Checked = tryParseBool;
                #endregion
                #region 學年調整成績
                tryParseBool = true;
                bool.TryParse(element.GetAttribute("學年調整成績"), out tryParseBool);
                checkBoxX21.Checked = tryParseBool;
                #endregion
            }
            #endregion


            #region 延修及重讀成績處理規則
            if (_scrContent.SelectSingleNode("延修及重讀成績處理規則") != null)
            {
                #region 延修成績
                element = (XmlElement)_scrContent.SelectSingleNode("延修及重讀成績處理規則/延修成績");
                if (element != null)
                {
                    decimal d = 4;
                    decimal.TryParse(element.GetAttribute("開始年級"), out d);
                    numericUpDown5.Value = d;
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("延修成績登錄至各修課學年度學期"), out tryParseBool);
                    radioButton20.Checked = tryParseBool;
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("所有延修成績合併登錄至同一延修學期"), out tryParseBool);
                    radioButton19.Checked = tryParseBool;
                }
                #endregion
                #region 重讀成績
                element = (XmlElement)_scrContent.SelectSingleNode("延修及重讀成績處理規則/重讀成績");
                if (element != null)
                {
                    tryParseBool = true;
                    bool.TryParse(element.GetAttribute("擇優採計成績"), out tryParseBool);
                    comboBoxEx2.SelectedIndex = (tryParseBool ? 1 : 0);
                    tryParseBool = true;
                }
                #endregion
            }
            #endregion
            #region 學年調整成績
            element = (XmlElement)_scrContent.SelectSingleNode("學年調整成績");
            tryParseBool = false;
            if (element != null)
            {
                if (bool.TryParse(element.GetAttribute("以六十分登錄"), out tryParseBool) && tryParseBool)
                    radioButton13.Checked = true;
                if (bool.TryParse(element.GetAttribute("以學生及格標準登錄"), out tryParseBool) && tryParseBool)
                    radioButton14.Checked = true;
                if (bool.TryParse(element.GetAttribute("不登錄學年調整成績"), out tryParseBool) && tryParseBool)
                    radioButton26.Checked = true;
                if (bool.TryParse(element.GetAttribute("不使用學年調整成績"), out tryParseBool) && tryParseBool)
                    radioButton27.Checked = true;
                //if ( bool.TryParse(element.GetAttribute("重新計算學期分項成績"), out tryParseBool) && tryParseBool )
                //    radioButton28.Checked = true;
                //if ( bool.TryParse(element.GetAttribute("不重新計算學期分項成績"), out tryParseBool) && tryParseBool )
                //    radioButton27.Checked = true;
            }
            #endregion



            #region 重修成績
            element = (XmlElement)_scrContent.SelectSingleNode("重修成績");
            tryParseBool = false;
            if (element != null)
            {
                bool.TryParse(element.GetAttribute("登錄至原學期"), out tryParseBool);
            }
            comboBoxEx1.SelectedIndex = (tryParseBool ? 1 : 0);
            #endregion
            #region 及格標準
            if (_scrContent.SelectSingleNode("及格標準") != null)
            {
                #region 類別
                foreach (XmlNode node in _scrContent.SelectNodes("及格標準/學生類別"))
                {
                    if (node != null)
                    {
                        element = (XmlElement)node;
                        if (element.GetAttribute("類別") == "預設")
                        {
                            dataGridViewX1.Rows[0].Cells[1].Value = element.GetAttribute("一年級及格標準");
                            dataGridViewX1.Rows[0].Cells[2].Value = element.GetAttribute("二年級及格標準");
                            dataGridViewX1.Rows[0].Cells[3].Value = element.GetAttribute("三年級及格標準");
                            dataGridViewX1.Rows[0].Cells[4].Value = element.GetAttribute("四年級及格標準");
                            dataGridViewX1.Rows[0].Cells[5].Value = element.GetAttribute("一年級補考標準");
                            dataGridViewX1.Rows[0].Cells[6].Value = element.GetAttribute("二年級補考標準");
                            dataGridViewX1.Rows[0].Cells[7].Value = element.GetAttribute("三年級補考標準");
                            dataGridViewX1.Rows[0].Cells[8].Value = element.GetAttribute("四年級補考標準");
                        }
                        else
                        {
                            dataGridViewX1.Rows.Add(element.GetAttribute("類別"),
                                element.GetAttribute("一年級及格標準"),
                                element.GetAttribute("二年級及格標準"),
                                element.GetAttribute("三年級及格標準"),
                                element.GetAttribute("四年級及格標準"),
                                element.GetAttribute("一年級補考標準"),
                                element.GetAttribute("二年級補考標準"),
                                element.GetAttribute("三年級補考標準"),
                                element.GetAttribute("四年級補考標準"));
                        }
                    }
                }
                #endregion
            }
            #endregion
            #region 畢業成績計算規則(2011/3/29 - wei)
            element = (XmlElement)_scrContent.SelectSingleNode("畢業成績計算規則");
            if (element != null)
            {
                if (element.InnerText == "學期科目成績加權")
                    radioButton23.Checked = true;
                else
                    radioButton24.Checked = true;

                //學年學業成績及格(new)
                if (element.GetAttribute("學年學業成績及格") == "true")
                    checkBoxX7.Checked = true;
                else
                    checkBoxX7.Checked = false; //預設
            }
            #endregion
            #region 德行成績畢業判斷規則(2011/3/29 - wei)
            element = (XmlElement)_scrContent.SelectSingleNode("德行成績畢業判斷規則");
            if (element != null)
            {
                if (element.GetAttribute("功過相抵未滿三大過") == "true")
                    checkBoxX6.Checked = true;
                else
                    checkBoxX6.Checked = false; //預設
            }
            #endregion
            #region 畢業學分數
            //學科累計總學分數
            element = (XmlElement)_scrContent.SelectSingleNode("畢業學分數/學科累計總學分數");
            if (element != null)
            {
                textBoxX6.Text = element.InnerText;
            }
            //專業及實習總學分數
            element = (XmlElement)_scrContent.SelectSingleNode("畢業學分數/專業及實習總學分數");
            if (element != null)
            {
                textBoxX2.Text = element.InnerText;
            }
            //必修學分數
            element = (XmlElement)_scrContent.SelectSingleNode("畢業學分數/必修學分數");
            if (element != null)
            {
                textBoxX1.Text = element.InnerText;
            }
            //部訂必修學分數
            element = (XmlElement)_scrContent.SelectSingleNode("畢業學分數/部訂必修學分數");
            if (element != null)
            {
                textBoxX7.Text = element.InnerText;
            }
            //實習學分數
            element = (XmlElement)_scrContent.SelectSingleNode("畢業學分數/實習學分數");
            if (element != null)
            {
                textBoxX11.Text = element.InnerText;
            }
            //選修學分數
            element = (XmlElement)_scrContent.SelectSingleNode("畢業學分數/選修學分數");
            if (element != null)
            {
                textBoxX8.Text = element.InnerText;
            }
            //校訂必修學分數
            element = (XmlElement)_scrContent.SelectSingleNode("畢業學分數/校訂必修學分數");
            if (element != null)
            {
                textBoxX10.Text = element.InnerText;
            }
            element = (XmlElement)_scrContent.SelectSingleNode("畢業學分數/同一科目級別不重複採計");
            if (element != null)
            {
                if (element.InnerText == "FALSE")
                    checkBoxX20.Checked = false;
                else
                    checkBoxX20.Checked = true;
            }
            #endregion
            #region 核心科目表
            //在成績計算規則中已選取的核心科目表
            List<string> checkedList = new List<string>();
            foreach (XmlNode var in _scrContent.SelectNodes("核心科目表/科目表"))
            {
                checkedList.Add(var.InnerText);
            }
            //取得所有的核心科目表，並判斷是否有選取
            listView1.Items.Clear();
            foreach (SubjectTableItem subjectTable in SubjectTable.Items["核心科目表"])
            {
                ListViewItem item = listView1.Items.Add(subjectTable.Name);
                if (checkedList.Contains(subjectTable.Name))
                    item.Checked = true;
            }
            #endregion
            #region 學期科目成績計算至學年科目成績計算規則(2010/6/24 騉翔新增)
            element = _scrContent.SelectSingleNode("學期科目成績計算至學年科目成績規則") as XmlElement;

            if (element != null)
                if (element.GetAttribute("進校上下學期及格規則").Equals("True"))
                    checkBoxX5.Checked = true;
            #endregion
            #region 專業科目表(2010/5/3 騉翔新增)
            //取得目前成績計算規則中的專業科目表，若節點不存在則設為空字串
            XmlNode Node = _scrContent.SelectSingleNode("核心科目表/專業科目表");
            string strProfessionalTableName = Node != null ? Node.InnerText : string.Empty;

            //先將ComboBox清空
            cmbProfessionalSubjectTable.Items.Clear();

            //將空白加入到第一個選項
            cmbProfessionalSubjectTable.Items.Add("");

            //將目前系統所有的核心科目表加入到ComboBox中
            foreach (SubjectTableItem subjectTable in SubjectTable.Items["核心科目表"])
                cmbProfessionalSubjectTable.Items.Add(subjectTable.Name);

            //針對ComboBox每個項目判斷是否等於專業科目表，若是的話則選取
            for (int i = 0; i < cmbProfessionalSubjectTable.Items.Count; i++)
            {
                string strSubjectTableName = "" + cmbProfessionalSubjectTable.Items[i];

                if (strSubjectTableName.Equals(strProfessionalTableName))
                {
                    cmbProfessionalSubjectTable.SelectedIndex = i;
                    break;
                }
            }
            #endregion
        }
        /// <summary>
        /// 取得設定資料
        /// </summary>
        /// <returns></returns>
        public XmlElement GetSource()
        {
            return GetSource(string.Empty);
        }
        /// <summary>
        /// 取得設定資料
        /// </summary>
        /// <returns></returns>
        public XmlElement GetSource(string schoolYear)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<ScoreCalcRule/>");
            if (!string.IsNullOrEmpty(schoolYear))
                doc.DocumentElement.SetAttribute("SchoolYear", "" + schoolYear);

            #region 學期科目成績屬性採計方式
            XmlElement element = doc.CreateElement("學期科目成績屬性採計方式");
            element.InnerText = radioButton16.Checked ? "以課程規劃表內容為準" : "以實際學期科目成績內容為準";
            doc.DocumentElement.AppendChild(element);
            #endregion
            #region 必需修滿課程規劃表中所列必修課程
            element = doc.CreateElement("修滿所有必修課程");
            element.InnerText = "" + checkBoxX9.Checked;
            doc.DocumentElement.AppendChild(element);
            #endregion
            #region 各項成績計算位數
            XmlElement parentelement = doc.CreateElement("各項成績計算位數");
            doc.DocumentElement.AppendChild(parentelement);
            #region 科目成績計算位數
            element = doc.CreateElement("科目成績計算位數");
            element.SetAttribute("位數", numericUpDown1.Value.ToString());
            element.SetAttribute("無條件進位", radioButton1.Checked.ToString());
            element.SetAttribute("無條件捨去", radioButton5.Checked.ToString());
            element.SetAttribute("四捨五入", radioButton9.Checked.ToString());
            parentelement.AppendChild(element);
            #endregion
            #region 學期分項成績計算位數
            element = doc.CreateElement("學期分項成績計算位數");
            element.SetAttribute("位數", numericUpDown2.Value.ToString());
            element.SetAttribute("無條件進位", radioButton2.Checked.ToString());
            element.SetAttribute("無條件捨去", radioButton3.Checked.ToString());
            element.SetAttribute("四捨五入", radioButton4.Checked.ToString());
            parentelement.AppendChild(element);
            #endregion
            #region 學年科目成績計算位數
            element = doc.CreateElement("學年科目成績計算位數");
            element.SetAttribute("位數", numericUpDown3.Value.ToString());
            element.SetAttribute("無條件進位", radioButton6.Checked.ToString());
            element.SetAttribute("無條件捨去", radioButton7.Checked.ToString());
            element.SetAttribute("四捨五入", radioButton8.Checked.ToString());
            parentelement.AppendChild(element);
            #endregion
            #region 學年分項成績計算位數
            element = doc.CreateElement("學年分項成績計算位數");
            element.SetAttribute("位數", numericUpDown6.Value.ToString());
            element.SetAttribute("無條件進位", radioButton10.Checked.ToString());
            element.SetAttribute("無條件捨去", radioButton11.Checked.ToString());
            element.SetAttribute("四捨五入", radioButton21.Checked.ToString());
            parentelement.AppendChild(element);
            #endregion
            #region 畢業成績計算位數
            element = doc.CreateElement("畢業成績計算位數");
            element.SetAttribute("位數", numericUpDown4.Value.ToString());
            element.SetAttribute("無條件進位", radioButton30.Checked.ToString());
            element.SetAttribute("無條件捨去", radioButton29.Checked.ToString());
            element.SetAttribute("四捨五入", radioButton12.Checked.ToString());
            parentelement.AppendChild(element);
            #endregion
            #endregion
            #region 分項成績計算項目
            parentelement = doc.CreateElement("分項成績計算項目");
            #region 體育
            element = doc.CreateElement("體育");
            element.SetAttribute("計算成績", checkBoxX1.Checked.ToString());
            element.SetAttribute("併入學期學業成績", checkBoxX8.Checked.ToString());
            parentelement.AppendChild(element);
            #endregion
            #region 國防通識
            element = doc.CreateElement("國防通識");
            element.SetAttribute("計算成績", checkBoxX2.Checked.ToString());
            element.SetAttribute("併入學期學業成績", checkBoxX11.Checked.ToString());
            parentelement.AppendChild(element);
            #endregion
            #region 健康與護理
            element = doc.CreateElement("健康與護理");
            element.SetAttribute("計算成績", checkBoxX3.Checked.ToString());
            element.SetAttribute("併入學期學業成績", checkBoxX13.Checked.ToString());
            parentelement.AppendChild(element);
            #endregion
            #region 實習科目
            element = doc.CreateElement("實習科目");
            element.SetAttribute("計算成績", checkBoxX4.Checked.ToString());
            element.SetAttribute("併入學期學業成績", checkBoxX15.Checked.ToString());
            parentelement.AppendChild(element);
            #endregion
            #region 專業科目
            element = doc.CreateElement("專業科目");
            element.SetAttribute("計算成績", checkBoxX12.Checked.ToString());
            element.SetAttribute("併入學期學業成績", checkBoxX10.Checked.ToString());
            parentelement.AppendChild(element);
            #endregion
            doc.DocumentElement.AppendChild(parentelement);
            #endregion
            #region 分項成績計算採計成績欄位
            parentelement = doc.CreateElement("分項成績計算採計成績欄位");
            parentelement.SetAttribute("原始成績", checkBoxX16.Checked.ToString());
            parentelement.SetAttribute("補考成績", checkBoxX17.Checked.ToString());
            parentelement.SetAttribute("重修成績", checkBoxX19.Checked.ToString());
            parentelement.SetAttribute("擇優採計成績", checkBoxX14.Checked.ToString());
            parentelement.SetAttribute("學年調整成績", checkBoxX18.Checked.ToString());
            doc.DocumentElement.AppendChild(parentelement);
            #endregion

            #region 學年成績計算採計成績欄位
            parentelement = doc.CreateElement("學年成績計算採計成績欄位");
            parentelement.SetAttribute("原始成績", checkBoxX25.Checked.ToString());
            parentelement.SetAttribute("補考成績", checkBoxX24.Checked.ToString());
            parentelement.SetAttribute("重修成績", checkBoxX23.Checked.ToString());
            parentelement.SetAttribute("擇優採計成績", checkBoxX22.Checked.ToString());
            parentelement.SetAttribute("學年調整成績", checkBoxX21.Checked.ToString());
            doc.DocumentElement.AppendChild(parentelement);
            #endregion


            #region 延修及重讀成績處理規則
            parentelement = doc.CreateElement("延修及重讀成績處理規則");
            {
                #region 延修成績
                element = doc.CreateElement("延修成績");
                element.SetAttribute("開始年級", numericUpDown5.Value.ToString());
                element.SetAttribute("所有延修成績合併登錄至同一延修學期", radioButton19.Checked.ToString());
                element.SetAttribute("延修成績登錄至各修課學年度學期", radioButton20.Checked.ToString());
                parentelement.AppendChild(element);
                #endregion
                #region 重讀成績
                element = doc.CreateElement("重讀成績");
                element.SetAttribute("擇優採計成績", (comboBoxEx2.SelectedIndex == 1).ToString());
                parentelement.AppendChild(element);
                #endregion
            }
            doc.DocumentElement.AppendChild(parentelement);
            #endregion
            #region 學年調整成績
            element = doc.CreateElement("學年調整成績");
            element.SetAttribute("以六十分登錄", radioButton13.Checked.ToString());
            element.SetAttribute("以學生及格標準登錄", radioButton14.Checked.ToString());
            element.SetAttribute("不登錄學年調整成績", radioButton26.Checked.ToString());
            element.SetAttribute("不使用學年調整成績", radioButton27.Checked.ToString());
            //element.SetAttribute("重新計算學期分項成績", radioButton28.Checked.ToString());
            //element.SetAttribute("不重新計算學期分項成績", radioButton27.Checked.ToString());
            doc.DocumentElement.AppendChild(element);
            #endregion


            #region 重修成績
            element = doc.CreateElement("重修成績");
            element.SetAttribute("登錄至原學期", "" + (comboBoxEx1.SelectedIndex == 1));
            doc.DocumentElement.AppendChild(element);
            #endregion
            #region 及格標準
            parentelement = doc.CreateElement("及格標準");
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow) break;

                element = doc.CreateElement("學生類別");
                element.SetAttribute("類別", "" + row.Cells[0].Value);

                element.SetAttribute("一年級及格標準", "" + row.Cells[1].Value);
                element.SetAttribute("二年級及格標準", "" + row.Cells[2].Value);
                element.SetAttribute("三年級及格標準", "" + row.Cells[3].Value);
                element.SetAttribute("四年級及格標準", "" + row.Cells[4].Value);

                element.SetAttribute("一年級補考標準", "" + row.Cells[5].Value);
                element.SetAttribute("二年級補考標準", "" + row.Cells[6].Value);
                element.SetAttribute("三年級補考標準", "" + row.Cells[7].Value);
                element.SetAttribute("四年級補考標準", "" + row.Cells[8].Value);
                parentelement.AppendChild(element);
            }
            doc.DocumentElement.AppendChild(parentelement);
            #endregion
            #region 畢業成績計算規則
            parentelement = doc.CreateElement("畢業成績計算規則");
            parentelement.InnerText = radioButton24.Checked ? "學期分項成績平均" : "學期科目成績加權";
            if (checkBoxX7.Checked)
                parentelement.SetAttribute("學年學業成績及格", "true");
            else
                parentelement.SetAttribute("學年學業成績及格", "false");
            doc.DocumentElement.AppendChild(parentelement);
            #endregion
            #region 德行成績畢業判斷規則(2011/3/29 - wei)
            parentelement = doc.CreateElement("德行成績畢業判斷規則");
            parentelement.InnerText = "每學年德行成績均及格";
            if (checkBoxX6.Checked)
                parentelement.SetAttribute("功過相抵未滿三大過", "true");
            else
                parentelement.SetAttribute("功過相抵未滿三大過", "false");
            doc.DocumentElement.AppendChild(parentelement);
            #endregion
            #region 畢業學分數
            parentelement = doc.CreateElement("畢業學分數");
            //學科累計總學分數
            if (ValidateCredit(textBoxX6) && textBoxX6.Text != "")
            {
                element = doc.CreateElement("學科累計總學分數");
                element.InnerText = textBoxX6.Text;
                parentelement.AppendChild(element);
            }
            //專業及實習總學分數
            if (ValidateCredit(textBoxX2) && textBoxX2.Text != "")
            {
                element = doc.CreateElement("專業及實習總學分數");
                element.InnerText = textBoxX2.Text;
                parentelement.AppendChild(element);
            }
            //必修學分數
            if (ValidateCredit(textBoxX1) && textBoxX1.Text != "")
            {
                element = doc.CreateElement("必修學分數");
                element.InnerText = textBoxX1.Text;
                parentelement.AppendChild(element);
            }
            //部訂必修學分數
            if (ValidateCredit(textBoxX7) && textBoxX7.Text != "")
            {
                element = doc.CreateElement("部訂必修學分數");
                element.InnerText = textBoxX7.Text;
                parentelement.AppendChild(element);
            }
            //實習學分數
            if (ValidateCredit(textBoxX11) && textBoxX11.Text != "")
            {
                element = doc.CreateElement("實習學分數");
                element.InnerText = textBoxX11.Text;
                parentelement.AppendChild(element);
            }
            //選修學分數
            if (ValidateCredit(textBoxX8) && textBoxX8.Text != "")
            {
                element = doc.CreateElement("選修學分數");
                element.InnerText = textBoxX8.Text;
                parentelement.AppendChild(element);
            }
            //校訂必修學分數
            if (ValidateCredit(textBoxX10) && textBoxX10.Text != "")
            {
                element = doc.CreateElement("校訂必修學分數");
                element.InnerText = textBoxX10.Text;
                parentelement.AppendChild(element);
            }
            //同一科目級別不重複採計
            element = doc.CreateElement("同一科目級別不重複採計");
            element.InnerText = checkBoxX20.Checked ? "TRUE" : "FALSE";
            parentelement.AppendChild(element);

            doc.DocumentElement.AppendChild(parentelement);
            #endregion
            #region 核心科目表
            parentelement = doc.CreateElement("核心科目表");
            foreach (ListViewItem var in listView1.CheckedItems)
            {
                element = doc.CreateElement("科目表");
                element.InnerText = var.Text;
                parentelement.AppendChild(element);
            }

            string strProfessionalSubjectTableName = "" + cmbProfessionalSubjectTable.SelectedItem;

            element = doc.CreateElement("專業科目表");
            element.InnerText = strProfessionalSubjectTableName;
            parentelement.AppendChild(element);

            doc.DocumentElement.AppendChild(parentelement);
            #endregion
            #region 學期科目成績計算至學年科目成績計算規則(2010/6/24 騉翔新增)
            parentelement = doc.CreateElement("學期科目成績計算至學年科目成績規則");

            parentelement.SetAttribute("進校上下學期及格規則", "" + checkBoxX5.Checked);

            doc.DocumentElement.AppendChild(parentelement);
            #endregion
            return doc.DocumentElement;
        }

        #region 及格補考標準資料
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

            dataGridViewX1.Rows.Insert(_SelectedRowIndex, new DataGridViewRow());
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

            if (_SelectedRowIndex > 0 && dataGridViewX1.Rows.Count - 1 > _SelectedRowIndex)
                dataGridViewX1.Rows.RemoveAt(_SelectedRowIndex);
        }

        private void dataGridViewX1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {

            if (dataGridViewX1.SelectedCells.Count == 1)
                dataGridViewX1.BeginEdit(true);
        }

        private void dataGridViewX1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            if (e.RowIndex > 0 && e.ColumnIndex < 0 && e.Button == MouseButtons.Right)
            {
                dataGridViewX1.CurrentCell = null;
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

        }

        private bool ValidateRow(DataGridViewRow row)
        {
            if (row.IsNewRow) return true;

            bool pass = true;
            DataGridViewCell cell;
            int tryInt = 0;
            #region 檢查學生類別
            cell = row.Cells[0];
            if ("" + cell.Value == "")
            {
                cell.ErrorText = "不得空白。";
                dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
                pass &= false;
            }
            else if (cell.ErrorText == "不得空白。")
            {
                cell.ErrorText = "";
                dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
            }
            #endregion
            #region 檢查123年級及格標準不得空白
            for (int i = 0; i < 3; i++)
            {
                cell = row.Cells[1 + i];
                if ("" + cell.Value == "")
                {
                    cell.ErrorText = "不得空白。";
                    dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
                    pass &= false;
                }
                else if (cell.ErrorText == "不得空白。")
                {
                    cell.ErrorText = "";
                    dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
                }
            }
            #endregion
            #region 檢查123年級補考標準不得空白
            for (int i = 0; i < 3; i++)
            {
                cell = row.Cells[5 + i];
                if ("" + cell.Value == "")
                {
                    cell.ErrorText = "不得空白。";
                    dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
                    pass &= false;
                }
                else if (cell.ErrorText == "不得空白。")
                {
                    cell.ErrorText = "";
                    dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
                }
            }
            #endregion
            #region 檢查數字欄填寫正確
            double tryParse = 0;
            for (int i = 1; i < 8; i++)
            {
                cell = row.Cells[i];
                if ("" + cell.Value != "" && !double.TryParse("" + cell.Value, out tryParse))
                {
                    cell.ErrorText = "必須填入數字。";
                    dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
                    pass &= false;
                }
                else if (cell.ErrorText == "必須填入數字。")
                {
                    cell.ErrorText = "";
                    dataGridViewX1.UpdateCellErrorText(cell.ColumnIndex, cell.RowIndex);
                }
            }
            #endregion
            return pass;
        }

        private bool ValidateAll()
        {
            bool pass1 = true;
            bool pass2 = true;
            #region 驗證第一頁資料正確
            dataGridViewX1.EndEdit();
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (!row.IsNewRow)
                    pass1 &= ValidateRow(row);
            }
            if (!pass1)
                tabItem4.Icon = Properties.Resources.warning;
            else
                tabItem4.Icon = null;
            #endregion
            #region 驗證第二頁資料正確
            foreach (TextBox tbox in new TextBox[] { textBoxX6, textBoxX2, textBoxX7, textBoxX8, textBoxX10, textBoxX11, textBoxX1 })
            {
                pass2 &= ValidateCredit(tbox);
            }
            if (!pass2)
                tabItem6.Icon = Properties.Resources.warning;
            else
                tabItem6.Icon = null;
            #endregion
            return pass1 & pass2;
        }

        private bool ValidateCredit(TextBox txtCredit)
        {

            bool pass = true;
            #region 判斷輸入資料，沒輸入或輸入學分數或輸入百分比
            if (txtCredit.Text.Trim() != "")
            {
                //%結尾
                if (txtCredit.Text.EndsWith("%"))
                {
                    if (!radioButton16.Checked)
                    {
                        //部是由課程規畫表取得學分採計方式就不能使用百分比
                        pass &= false;
                        SetErrorProvider(txtCredit, "學分級修課資訊採計方式必需選擇\"由課程規畫表取得\"才能支援使用百分比判斷。");
                    }
                    decimal d;
                    if (decimal.TryParse(txtCredit.Text.TrimEnd("%".ToCharArray()), out d))
                    {
                        //是數字
                        if (d > 100 || d < 0)
                        {
                            //爆了啦
                            pass &= false;
                            SetErrorProvider(txtCredit, "數字必須介於0-100%之間");
                        }
                    }
                    else
                    {
                        //輸入不是數字
                        pass &= false;
                        SetErrorProvider(txtCredit, "必須輸入百分比。");
                    }
                }
                else
                {
                    decimal i;
                    if (decimal.TryParse(txtCredit.Text, out i))
                    {
                        //輸入是數字
                        if (i < 0)
                        {
                            //輸入數字小於0
                            pass &= false;
                            SetErrorProvider(txtCredit, "學分數不能小於0。");
                        }
                    }
                    else
                    {
                        //輸入不是數字
                        pass &= false;
                        SetErrorProvider(txtCredit, "必須輸入學分數。");
                    }
                }
            }
            #endregion
            if (pass)
                ResetErrorProvider(txtCredit);
            return pass;
        }

        private void textBoxX6_TextChanged(object sender, EventArgs e)
        {
            ValidateCredit((TextBox)sender);
        }

        private Dictionary<Control, ErrorProvider> _errorProviderDictionary = new Dictionary<Control, ErrorProvider>();

        private void SetErrorProvider(Control control, string p)
        {
            if (!_errorProviderDictionary.ContainsKey(control))
            {
                ErrorProvider ep = new ErrorProvider();
                ep.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                ep.SetIconAlignment(control, ErrorIconAlignment.MiddleRight);
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

        public bool IsValidated
        {
            get { return ValidateAll(); }
        }
        #endregion

        private void radioButton16_CheckedChanged(object sender, EventArgs e)
        {
            //if (checkBoxX9.Enabled != radioButton16.Checked)
            //{
            //    checkBoxX9.Enabled = radioButton16.Checked;
            //    if (checkBoxX9.Enabled)
            //    {
            //        checkBoxX9.Checked = (bool)checkBoxX9.Tag;
            //        checkBoxX9.Text = "必需修滿課程規劃表中所列必修課程";
            //    }
            //    else
            //    {
            //        checkBoxX9.Tag = checkBoxX9.Checked;
            //        checkBoxX9.Checked = false;
            //        checkBoxX9.Text = "必需修滿課程規劃表中所列必修課程(需成績屬性需選用課程規劃表判斷)";
            //    }
            //}
            foreach (TextBox tbox in new TextBox[] { textBoxX6, textBoxX2, textBoxX7, textBoxX8, textBoxX10, textBoxX11, textBoxX1 })
            {
                ValidateCredit(tbox);
            }
        }

        //private void buttonX1_Click(object sender, EventArgs e)
        //{
        //    if (textBoxX1.Text != "")
        //    {

        //        if (_scrID == null)
        //        {
        //            AddScoreCalcRule.Insert(textBoxX1.Text, GetSource());
        //            SmartSchool.ScoreCalcRuleRelated.ScoreCalcRule.Instance.Invok_ScoreCalcRuleInserted();
        //        }
        //        else
        //        {
        //            EditScoreCalcRule.Update(_scrID, textBoxX1.Text, GetSource());
        //            SmartSchool.ScoreCalcRuleRelated.ScoreCalcRule.Instance.Invok_ScoreCalcRuleUpdated();
        //        }

        //        this.Close();
        //    }
        //}

        //private void buttonX2_Click(object sender, EventArgs e)
        //{
        //    this.Close();
        //}
    }
}
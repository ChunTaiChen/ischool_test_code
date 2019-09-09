using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.ApplicationLog;
using SmartSchool.Common;
using SmartSchool.Feature.Score;
using SmartSchool.StudentRelated;

namespace SmartSchool.Evaluation.Content.ScoreEditor
{
    public partial class SchoolYearScoreEditor : BaseForm
    {
        string _SubjectScoreID = "";
        string _EntryScoreID1 = "";
        string _EntryScoreID2 = "";
        string _StudentID;

        //Log用來比較前後資料差異的變數
        Dictionary<string, string> beforeData = new Dictionary<string, string>();
        Dictionary<string, string> afterData = new Dictionary<string, string>();
        DSXmlHelper beforeXml = new DSXmlHelper("BeforeXml");
        DSXmlHelper afterXml = new DSXmlHelper("AfterXml");
        EntityAction entityAction = EntityAction.Insert;

        //排名相關物件
        private SubjectScoreToolTipProvider _subject_rating; //各科目排名
        private EntryScoreToolTipProvider _score_rating, _moral_rating; //學業、德行成績排名。
        //科目名稱欄 Index
        private const int SubjectColumn = 0;

        public SchoolYearScoreEditor(string refStudentID)
        {
            InitializeComponent();
            _StudentID = refStudentID;
            beforeXml.AddElement("SubjectGradeYear");
            afterXml.AddElement("SubjectGradeYear");
            beforeXml.AddElement("SubjectCollection");
            beforeXml.AddElement("EntryCollection");
            afterXml.AddElement("SubjectCollection");
            afterXml.AddElement("EntryCollection");
            comboBoxEx3.Items.AddRange(new object[] { "1", "2", "3", "4" });
            labelX10.Text = Student.Instance.Items[_StudentID].IDNumber + "  " + Student.Instance.Items[_StudentID].Name;
            for (int s = 3; s > 0; s--)
            {
                comboBoxEx1.Items.Add(CurrentUser.Instance.SchoolYear - s);
            }
            ValidateAll();
        }

        public SchoolYearScoreEditor(string schoolYear, string gradeyear, string refStudentID)
        {
            InitializeComponent();
            _StudentID = refStudentID;
            beforeXml.AddElement("SubjectGradeYear");
            afterXml.AddElement("SubjectGradeYear");
            beforeXml.AddElement("SubjectCollection");
            beforeXml.AddElement("EntryCollection");
            afterXml.AddElement("SubjectCollection");
            afterXml.AddElement("EntryCollection");
            entityAction = EntityAction.Update;
            comboBoxEx3.Items.AddRange(new object[] { "1", "2", "3", "4" });
            labelX10.Text = Student.Instance.Items[_StudentID].IDNumber + "  " + Student.Instance.Items[_StudentID].Name;
            this.comboBoxEx1.Text = schoolYear;
            this.comboBoxEx3.Text = gradeyear;
            comboBoxEx1.Enabled = false;
            ReLoad(null, null);
            ValidateAll();

            btnSave.Visible = CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
            dataGridViewX1.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
            comboBoxEx1.Enabled = CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
            comboBoxEx3.Enabled = CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
            textBoxX1.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
            textBoxX2.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
            textBoxX3.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
            textBoxX4.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
            textBoxX5.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
            textBoxX6.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
            textBoxX7.ReadOnly = !CurrentUser.Acl[SemesterScorePalmerworm.FeatureCode].Editable;
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ReLoad(object sender, EventArgs e)
        {
            _SubjectScoreID = "";
            _EntryScoreID1 = "";
            _EntryScoreID2 = "";

            if (!ValidateSchoolYear())
                return;

            #region 科目成績
            dataGridViewX1.Rows.Clear();
            DSXmlHelper _SubjectResponse = QueryScore.GetSchoolYearSubjectScore(true, comboBoxEx1.Text, _StudentID);
            if (_SubjectResponse.GetElement("SchoolYearSubjectScore") != null)
            {
                if (comboBoxEx3.Text == "")
                {
                    comboBoxEx3.Text = _SubjectResponse.GetElement("SchoolYearSubjectScore/GradeYear").InnerText;
                }
                else
                {
                    if (comboBoxEx3.Text != _SubjectResponse.GetElement("SchoolYearSubjectScore/GradeYear").InnerText)
                    {
                        MsgBox.Show("發現同一學年度出現兩筆不同成績年級之資料。");
                    }
                }

                #region 建立排名相關物件
                _subject_rating = new SubjectScoreToolTipProvider(_SubjectResponse.GetElement("SchoolYearSubjectScore/ClassRating"),
                    _SubjectResponse.GetElement("SchoolYearSubjectScore/DeptRating"),
                    _SubjectResponse.GetElement("SchoolYearSubjectScore/YearRating"));
                #endregion

                beforeXml.AddElement("SubjectGradeYear", "GradeYear", comboBoxEx3.Text);

                _SubjectScoreID = _SubjectResponse.GetElement("SchoolYearSubjectScore").GetAttribute("ID");
                foreach (XmlElement var in _SubjectResponse.GetElements("SchoolYearSubjectScore/ScoreInfo/SchoolYearSubjectScore/Subject"))
                {
                    beforeXml.AddElement("SubjectCollection", var);

                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(dataGridViewX1,
                        var.GetAttribute("科目"),
                        var.GetAttribute("學年成績"),
                        var.GetAttribute("結算成績") == "" ? var.GetAttribute("學年成績") : var.GetAttribute("結算成績"),
                        var.GetAttribute("補考成績"),
                        var.GetAttribute("重修成績")
                        );
                    row.Cells[0].ToolTipText = _subject_rating.GetTooltip(row);
                    dataGridViewX1.Rows.Add(row);
                }
            }
            #endregion
            #region 分項成績
            textBoxX1.Text = "";
            textBoxX2.Text = "";
            textBoxX3.Text = "";
            textBoxX4.Text = "";
            textBoxX5.Text = "";
            textBoxX6.Text = "";
            textBoxX7.Text = "";
            DSXmlHelper _EntryResponse = QueryScore.GetSchoolYearEntryScore(true, comboBoxEx1.Text, null, _StudentID);
            foreach (XmlElement var in _EntryResponse.GetElements("SchoolYearEntryScore"))
            {
                if (comboBoxEx3.Text == "")
                {
                    comboBoxEx3.Text = var.SelectSingleNode("GradeYear").InnerText;
                }
                else
                {
                    if (comboBoxEx3.Text != var.SelectSingleNode("GradeYear").InnerText)
                    {
                        MsgBox.Show("發現同一學年度出現兩筆不同成績年級之資料。");
                    }
                }
                string id = var.GetAttribute("ID");
                switch (var.SelectSingleNode("EntryGroup").InnerText)
                {
                    case "學習":
                        _EntryScoreID1 = id;
                        _score_rating = new EntryScoreToolTipProvider("學業", var);
                        break;
                    case "行為":
                        _EntryScoreID2 = id;
                        _moral_rating = new EntryScoreToolTipProvider("德行", var);
                        break;
                    default:
                        throw new Exception("拎唄謀洗鰓機雷EntryGroup： 　\"" + var.SelectSingleNode("EntryGroup").InnerText + "\"");
                }
                foreach (XmlNode score in var.SelectNodes("ScoreInfo/SchoolYearEntryScore/Entry"))
                {
                    XmlElement element = (XmlElement)score;
                    beforeXml.AddElement("EntryCollection", element);

                    #region 依分項填入格子
                    switch (element.GetAttribute("分項"))
                    {
                        case "學業":
                            textBoxX1.Text = element.GetAttribute("成績");

                            _score_rating.SetTooltip(textBoxX1);
                            _score_rating.SetTooltip(labelX3);
                            break;
                        case "體育":
                            textBoxX3.Text = element.GetAttribute("成績");
                            break;
                        case "國防通識":
                            textBoxX4.Text = element.GetAttribute("成績");
                            break;
                        case "健康與護理":
                            textBoxX5.Text = element.GetAttribute("成績");
                            break;
                        case "實習科目":
                            textBoxX6.Text = element.GetAttribute("成績");
                            break;
                        case "專業科目":
                            textBoxX7.Text = element.GetAttribute("成績");
                            break;
                        case "德行":
                            textBoxX2.Text = element.GetAttribute("成績");
                            _moral_rating.SetTooltip(textBoxX2);
                            _moral_rating.SetTooltip(labelX4);

                            break;
                        default:
                            //throw new Exception("拎唄謀洗鰓機雷分項： " + element.GetAttribute("分項"));
                            break;
                    }
                    #endregion
                }
            }
            #endregion
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

        private void dataGridViewX1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewX1.SelectedCells.Count == 1)
                dataGridViewX1.BeginEdit(true);
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            dataGridViewX1.EndEdit();
            if (!ValidateAll()) return;
            #region 新增修改科目成績資料
            XmlDocument doc = new XmlDocument();
            XmlElement subjectScoreInfo = doc.CreateElement("SchoolYearSubjectScore");

            afterXml.AddElement("SubjectGradeYear", "GradeYear", comboBoxEx3.Text);

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow)
                    break;
                XmlElement subjectElement = doc.CreateElement("Subject");
                subjectElement.SetAttribute("科目", "" + row.Cells[0].Value);
                subjectElement.SetAttribute("學年成績", "" + row.Cells[1].Value);
                subjectElement.SetAttribute("結算成績", "" + row.Cells[2].Value);
                subjectElement.SetAttribute("補考成績", "" + row.Cells[3].Value);
                subjectElement.SetAttribute("重修成績", "" + row.Cells[4].Value);
                subjectScoreInfo.AppendChild(subjectElement);

                afterXml.AddElement("SubjectCollection", subjectElement);
            }
            if (_SubjectScoreID != "")
            {
                if (subjectScoreInfo.SelectNodes("Subject").Count == 0)
                    RemoveScore.DeleteSchoolYearSubjectScore(_SubjectScoreID);
                else
                    EditScore.UpdateSchoolYearSubjectScore(_SubjectScoreID, comboBoxEx3.Text, subjectScoreInfo);
            }
            else
                if (subjectScoreInfo.SelectNodes("Subject").Count > 0)
                    AddScore.InsertSchoolYearSubjectScore(_StudentID, comboBoxEx1.Text, comboBoxEx3.Text, subjectScoreInfo);
            #endregion

            #region 新增修改分項成績資料

            XmlElement entryScoreInfo;
            XmlElement entryElement;
            double test = 0;
            #region 行為類
            if (double.TryParse(textBoxX2.Text, out test))
            {
                entryScoreInfo = doc.CreateElement("SchoolYearEntryScore");
                entryElement = doc.CreateElement("Entry");
                entryElement.SetAttribute("分項", "德行");
                entryElement.SetAttribute("成績", textBoxX2.Text);
                entryScoreInfo.AppendChild(entryElement);

                afterXml.AddElement("EntryCollection", entryElement);

                if (_EntryScoreID2 != "")
                {
                    EditScore.UpdateSchoolYearEntryScore(_EntryScoreID2, comboBoxEx3.Text, entryScoreInfo);
                }
                else
                    AddScore.InsertSchoolYearEntryScore(_StudentID, comboBoxEx1.Text, comboBoxEx3.Text, "行為", entryScoreInfo);
            }
            else
            {
                if (_EntryScoreID2 != "")
                    RemoveScore.DeleteSchoolYearEntityScore(_EntryScoreID2);
            }
            #endregion
            #region 學習類
            if (double.TryParse(textBoxX1.Text, out test)
                || double.TryParse(textBoxX3.Text, out test)
                || double.TryParse(textBoxX5.Text, out test)
                || double.TryParse(textBoxX4.Text, out test)
                || double.TryParse(textBoxX6.Text, out test)
                || double.TryParse(textBoxX7.Text, out test))
            {
                entryScoreInfo = doc.CreateElement("SchoolYearEntryScore");
                if (double.TryParse(textBoxX1.Text, out test))
                {
                    entryElement = doc.CreateElement("Entry");
                    entryElement.SetAttribute("分項", "學業");
                    entryElement.SetAttribute("成績", textBoxX1.Text);
                    entryScoreInfo.AppendChild(entryElement);
                    afterXml.AddElement("EntryCollection", entryElement);
                }
                if (double.TryParse(textBoxX3.Text, out test))
                {
                    entryElement = doc.CreateElement("Entry");
                    entryElement.SetAttribute("分項", "體育");
                    entryElement.SetAttribute("成績", textBoxX3.Text);
                    entryScoreInfo.AppendChild(entryElement);
                    afterXml.AddElement("EntryCollection", entryElement);
                }
                if (double.TryParse(textBoxX5.Text, out test))
                {
                    entryElement = doc.CreateElement("Entry");
                    entryElement.SetAttribute("分項", "健康與護理");
                    entryElement.SetAttribute("成績", textBoxX5.Text);
                    entryScoreInfo.AppendChild(entryElement);
                    afterXml.AddElement("EntryCollection", entryElement);
                }
                if (double.TryParse(textBoxX4.Text, out test))
                {
                    entryElement = doc.CreateElement("Entry");
                    entryElement.SetAttribute("分項", "國防通識");
                    entryElement.SetAttribute("成績", textBoxX4.Text);
                    entryScoreInfo.AppendChild(entryElement);
                    afterXml.AddElement("EntryCollection", entryElement);
                }
                if (double.TryParse(textBoxX6.Text, out test))
                {
                    entryElement = doc.CreateElement("Entry");
                    entryElement.SetAttribute("分項", "實習科目");
                    entryElement.SetAttribute("成績", textBoxX6.Text);
                    entryScoreInfo.AppendChild(entryElement);
                    afterXml.AddElement("EntryCollection", entryElement);
                }
                if (double.TryParse(textBoxX7.Text, out test))
                {
                    entryElement = doc.CreateElement("Entry");
                    entryElement.SetAttribute("分項", "專業科目");
                    entryElement.SetAttribute("成績", textBoxX7.Text);
                    entryScoreInfo.AppendChild(entryElement);
                    afterXml.AddElement("EntryCollection", entryElement);
                }
                if (_EntryScoreID1 != "")
                {
                    EditScore.UpdateSchoolYearEntryScore(_EntryScoreID1, comboBoxEx3.Text, entryScoreInfo);
                }
                else
                    AddScore.InsertSchoolYearEntryScore(_StudentID, comboBoxEx1.Text, comboBoxEx3.Text, "學習", entryScoreInfo);
            }
            else
            {
                if (_EntryScoreID1 != "")
                    RemoveScore.DeleteSchoolYearEntityScore(_EntryScoreID1);
            }
            #endregion
            #endregion

            #region 處理Log

            StringBuilder desc = new StringBuilder("");
            desc.AppendLine("學生姓名：" + Student.Instance.Items[_StudentID].Name + " ");
            desc.AppendLine(comboBoxEx1.Text + " 學年度 ");

            #region 處理成績年級log
            if (beforeXml.GetText("SubjectGradeYear/GradeYear") != afterXml.GetText("SubjectGradeYear/GradeYear"))
                desc.AppendLine("成績年級由「" + beforeXml.GetText("SubjectGradeYear/GradeYear") + "」變更為「" + afterXml.GetText("SubjectGradeYear/GradeYear") + "」");
            #endregion

            #region 處理科目成績log

            foreach (XmlElement var in beforeXml.GetElements("SubjectCollection/Subject"))
            {
                if (!beforeData.ContainsKey(var.GetAttribute("科目")))
                    beforeData.Add(var.GetAttribute("科目"), var.GetAttribute("結算成績") == "" ? var.GetAttribute("學年成績") : var.GetAttribute("結算成績"));
            }

            foreach (XmlElement var in afterXml.GetElements("SubjectCollection/Subject"))
            {
                if (!afterData.ContainsKey(var.GetAttribute("科目")))
                    afterData.Add(var.GetAttribute("科目"), var.GetAttribute("結算成績"));
            }

            desc.AppendLine("學年科目成績：");

            foreach (string var in beforeData.Keys)
            {
                if (!afterData.ContainsKey(var))
                {
                    desc.AppendLine("刪除科目「" + var + "」");
                }
                else
                {
                    if (beforeData[var] != afterData[var])
                    {
                        desc.AppendLine("科目「" + var + "」的「結算成績」由「" + beforeData[var] + "」變更為「" + afterData[var] + "」");
                    }
                    foreach (string key in new string[] { "補考成績", "重修成績" })
                    {
                        string bk = beforeXml.GetElement("SubjectCollection/Subject[@科目='" + var + "']").GetAttribute(key);
                        string ak = afterXml.GetElement("SubjectCollection/Subject[@科目='" + var + "']").GetAttribute(key);
                        if (bk != ak)
                        {
                            desc.AppendLine("科目「" + var + "」的「" + key + "」由「" + bk + "」變更為「" + ak + "」");
                        }
                    }
                    afterData.Remove(var);
                }
            }

            foreach (string var in afterData.Keys)
            {
                desc.AppendLine("新增科目「" + var + "」");

                if (afterData[var] != "")
                    desc.AppendLine("科目「" + var + "」的「學年成績」由「」變更為「" + afterData[var] + "」");
                foreach (string key in new string[] { "補考成績", "重修成績" })
                {
                    string ak = afterXml.GetElement("SubjectCollection/Subject[@科目='" + var + "']").GetAttribute(key);
                    if (ak != "")
                    {
                        desc.AppendLine("科目「" + var + "」的「" + key + "」由「」變更為「" + ak + "」");
                    }
                }
            }

            beforeData.Clear();
            afterData.Clear();

            #endregion

            #region 處理分項成績log

            foreach (XmlElement var in beforeXml.GetElements("EntryCollection/Entry"))
            {
                if (!beforeData.ContainsKey(var.GetAttribute("分項")))
                    beforeData.Add(var.GetAttribute("分項"), var.GetAttribute("成績"));
                if (!afterData.ContainsKey(var.GetAttribute("分項")))
                    afterData.Add(var.GetAttribute("分項"), "");
            }

            foreach (XmlElement var in afterXml.GetElements("EntryCollection/Entry"))
            {
                if (!afterData.ContainsKey(var.GetAttribute("分項")))
                    afterData.Add(var.GetAttribute("分項"), var.GetAttribute("成績"));
                else
                    afterData[var.GetAttribute("分項")] = var.GetAttribute("成績");
                if (!beforeData.ContainsKey(var.GetAttribute("分項")))
                    beforeData.Add(var.GetAttribute("分項"), "");
            }

            desc.AppendLine("學年分項成績：");

            foreach (string var in afterData.Keys)
            {
                if (beforeData[var] != afterData[var])
                    desc.AppendLine("「" + var + "成績」由「" + beforeData[var] + "」變更為「" + afterData[var] + "」");
            }

            #endregion

            CurrentUser.Instance.AppLog.Write(EntityType.Student, entityAction, _StudentID, desc.ToString(), Text, afterXml.GetRawXml());

            #endregion

            EventHub.Instance.InvokScoreChanged(_StudentID);
            this.Close();
        }

        #region 資料驗證相關
        private bool ValidateAll()
        {
            bool validatePass = true;
            validatePass &= ValidateSchoolYear();
            validatePass &= ValidateGradeYear();
            #region 檢查DataGridView資料正確
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow)
                    break;
                validatePass &= ValidateRow(row.Index);
            }
            #endregion
            return validatePass;
        }

        private bool ValidateRow(int p)
        {
            bool validatePass = true;
            DataGridViewRow row = dataGridViewX1.Rows[p];
            if (row.IsNewRow) return true;

            bool hasScore = false;
            decimal score = decimal.MinValue;
            foreach (int i in new int[] { 2, 3, 4 })
            {
                row.Cells[i].ErrorText = "";
                decimal x = 0;
                if ("" + row.Cells[i].Value != "" && !decimal.TryParse("" + row.Cells[i].Value, out x))
                {
                    validatePass &= false;
                    row.Cells[i].ErrorText = "必須輸入數字";
                }
                if ("" + row.Cells[i].Value != "")
                {
                    hasScore = true;
                    if (score < x)
                        score = x;
                }
                dataGridViewX1.UpdateCellErrorText(i, row.Index);
            }
            if (hasScore)
            {
                foreach (int i in new int[] { 1 })
                {
                    row.Cells[i].Value = score;
                    row.Cells[i].ErrorText = "";
                    dataGridViewX1.UpdateCellErrorText(i, row.Index);
                }
            }
            else
            {
                foreach (int i in new int[] { 1 })
                {
                    validatePass &= false;
                    row.Cells[i].Value = "";
                    row.Cells[i].ErrorText = "必須有成績";
                    dataGridViewX1.UpdateCellErrorText(i, row.Index);
                }
            }
            foreach (int i in new int[] { 0 })
            {
                row.Cells[i].ErrorText = "";
                if ("" + row.Cells[i].Value == "")
                {
                    validatePass &= false;
                    row.Cells[i].ErrorText = "此為必填欄位";
                }
                dataGridViewX1.UpdateCellErrorText(i, row.Index);
            }
            foreach (DataGridViewCell cell in row.Cells)
            {
                validatePass &= (cell.ErrorText == "");
            }
            return validatePass;
        }

        private bool ValidateSchoolYear()
        {
            bool validatePass = true;
            #region 檢查輸入欄位值
            int s = 0;
            if (!int.TryParse(comboBoxEx1.Text, out s))
            {
                validatePass &= false;
                errorSchoolYear.Icon = Properties.Resources.error;
                errorSchoolYear.SetError(comboBoxEx1, "必須輸入數字");
            }
            else
                errorSchoolYear.Clear();
            #endregion
            #region 檢查空值
            if (comboBoxEx1.Text == "")
            {
                validatePass &= false;
                errorSchoolYear.Icon = Properties.Resources.error;
                errorSchoolYear.SetError(comboBoxEx1, "必須填寫");
            }
            else
                errorSchoolYear.Clear();
            #endregion
            return validatePass;
        }
        private bool ValidateGradeYear()
        {
            bool validatePass = true;
            #region 檢查輸入欄位值
            int s = 0;
            if (!int.TryParse(comboBoxEx3.Text, out s))
            {
                validatePass &= false;
                errorGradeYear.Icon = Properties.Resources.error;
                errorGradeYear.SetError(comboBoxEx3, "必須輸入數字");
            }
            else
                errorGradeYear.Clear();
            #endregion
            #region 檢查空值
            if (!int.TryParse(comboBoxEx3.Text, out s))
            {
                validatePass &= false;
                errorGradeYear.Icon = Properties.Resources.error;
                errorGradeYear.SetError(comboBoxEx3, "成績年級必須填寫");
            }
            else
                errorGradeYear.Clear();
            #endregion
            return validatePass;
        }

        private void dataGridViewX1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            ValidateRow(e.RowIndex);
        }
        #endregion

        private void comboBoxEx3_TextChanged(object sender, EventArgs e)
        {
            ValidateGradeYear();
        }

        private void dataGridViewX1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dataGridViewX1.EndEdit();
        }

        class SubjectScoreToolTipProvider
        {
            private Dictionary<string, ScorePlace> _class_rating;
            private Dictionary<string, ScorePlace> _dept_rating;
            private Dictionary<string, ScorePlace> _year_rating;

            public SubjectScoreToolTipProvider(XmlElement classRating, XmlElement deptRating, XmlElement yearRating)
            {
                _class_rating = new Dictionary<string, ScorePlace>();
                _dept_rating = new Dictionary<string, ScorePlace>();
                _year_rating = new Dictionary<string, ScorePlace>();

                CreatePlaceObjects(classRating, _class_rating);
                CreatePlaceObjects(deptRating, _dept_rating);
                CreatePlaceObjects(yearRating, _year_rating);
            }

            private void CreatePlaceObjects(XmlElement ratingData, Dictionary<string, ScorePlace> target)
            {
                DSXmlHelper temp;
                foreach (XmlElement eachPlace in ratingData.SelectNodes("Rating/Item"))
                {
                    temp = new DSXmlHelper(eachPlace);
                    string key = temp.GetText("@科目");

                    if (target.ContainsKey(key)) continue;

                    target.Add(key, new ScorePlace(eachPlace, temp.GetText("../@範圍人數")));
                }
            }

            public string GetTooltip(DataGridViewRow row)
            {
                //第1欄是科目名稱，第2欄是級別，如果改了的話....。
                string key = row.Cells[SubjectColumn].Value + string.Empty;
                ScorePlace temp;
                StringBuilder tooltip = new StringBuilder();

                if (_class_rating.TryGetValue(key, out temp))
                    tooltip.AppendFormat("班排名：{0}\n", temp.Place);

                if (_dept_rating.TryGetValue(key, out temp))
                    tooltip.AppendFormat("科排名：{0}\n", temp.Place);

                if (_year_rating.TryGetValue(key, out temp))
                    tooltip.AppendFormat("年排名：{0}\n", temp.Place);

                return tooltip.ToString();
            }

            class ScorePlace
            {
                public ScorePlace(XmlElement placeData, string ratingBase)
                {
                    DSXmlHelper hlpData = new DSXmlHelper(placeData);

                    _score = hlpData.GetText("@成績");
                    _rating_base = ratingBase;
                    _actual_base = hlpData.GetText("@成績人數");
                    _place = hlpData.GetText("@排名");
                }

                private string _score;
                public string Score
                {
                    get { return _score; }
                }

                private string _actual_base;
                public string ActualBase
                {
                    get { return _actual_base; }
                }

                private string _rating_base;
                public string RatingBase
                {
                    get { return _rating_base; }
                }

                private string _place;
                public string Place
                {
                    get { return _place; }
                }
            }
        }

        class EntryScoreToolTipProvider
        {
            private System.Windows.Forms.ToolTip _obj_tooltip = new System.Windows.Forms.ToolTip();

            public EntryScoreToolTipProvider(string entryGroup, XmlElement semesterEntryScore)
            {
                XmlElement classRating, deptRating, yearRating;
                DSXmlHelper hlpEntryScore = new DSXmlHelper(semesterEntryScore);
                classRating = hlpEntryScore.GetElement("ClassRating");
                deptRating = hlpEntryScore.GetElement("DeptRating");
                yearRating = hlpEntryScore.GetElement("YearRating");

                StringBuilder tooltip = new StringBuilder();
                string path = string.Format("Rating/Item[@分項='{0}']", entryGroup);

                DSXmlHelper temp = new DSXmlHelper(classRating);
                if (temp.PathExist(path))
                    tooltip.AppendFormat("班排名：{0}\n", temp.GetText(path + "/@排名"));

                temp = new DSXmlHelper(deptRating);
                if (temp.PathExist(path))
                    tooltip.AppendFormat("科排名：{0}\n", temp.GetText(path + "/@排名"));

                temp = new DSXmlHelper(yearRating);
                if (temp.PathExist(path))
                    tooltip.AppendFormat("年排名：{0}\n", temp.GetText(path + "/@排名"));

                _score_tooltip = tooltip.ToString();
            }

            private string _score_tooltip;
            public string ScoreToolTip
            {
                get { return _score_tooltip; }
            }

            public void SetTooltip(Control ctl)
            {
                if (_obj_tooltip.CanExtend(ctl))
                    _obj_tooltip.SetToolTip(ctl, ScoreToolTip);
            }
        }

        private void dataGridViewX1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dataGridViewX1.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

    }
}
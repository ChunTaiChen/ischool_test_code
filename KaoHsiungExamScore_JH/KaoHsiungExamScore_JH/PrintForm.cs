using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using K12.Data;
using JHSchool.Data;
using System.IO;
using KaoHsiung.JHEvaluation.Data;
using Aspose.Words;
using JHSchool.Evaluation.Calculation;
using Aspose.Words.Reporting;
using Aspose.Words.Tables;

namespace KaoHsiungExamScore_JH
{
    public partial class PrintForm : BaseForm, IFieldMergingCallback
    {
        private FISCA.UDT.AccessHelper _AccessHelper = new FISCA.UDT.AccessHelper();

        //private Dictionary<string, List<string>> _ExamSubjects = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _ExamSubjectFull = new Dictionary<string, List<string>>();
        List<string> _StudentIDList;
        // 缺曠區間統計
        Dictionary<string, Dictionary<string, int>> _AttendanceDict = new Dictionary<string, Dictionary<string, int>>();

        private List<string> typeList = new List<string>();
        private List<string> absenceList = new List<string>();
        private List<string> _SelSubjNameList = new List<string>();
        private List<string> _SelAttendanceList = new List<string>();

        private BackgroundWorker _bgWorkReport;
        private DocumentBuilder _builder;
        BackgroundWorker bkw;

        // 錯誤訊息
        List<string> _ErrorList = new List<string>();

        // 領域錯誤訊息
        List<string> _ErrorDomainNameList = new List<string>();

        // 樣板內有科目名稱
        List<string> _TemplateSubjectNameList = new List<string>();

        // 存檔路徑
        string pathW = "";

        // 樣板設定檔
        private List<Configure> _ConfigureList = new List<Configure>();

        // 開始日期
        private DateTime _BeginDate;
        // 結束日期
        private DateTime _EndDate;

        // 處理程度對照
        EffortMapper _EffortMapper = new EffortMapper();
        // 成績校正日期字串
        private string _ScoreEditDate = "";

        private string _DefalutSchoolYear = "";
        private string _DefaultSemester = "";

        private int _SelSchoolYear;
        private int _SelSemester;
        private string _SelExamName = "";
        private string _SelExamID = "";
        private string _SelNotRankedFilter = "";

        private List<ExamRecord> _exams = new List<ExamRecord>();

        private Dictionary<string, List<string>> _StudTagDict = new Dictionary<string, List<string>>();

        // 紀錄樣板設定
        List<DAO.UDT_ScoreConfig> _UDTConfigList;

        public PrintForm(List<string> StudIDList)
        {
            InitializeComponent();
            _StudentIDList = StudIDList;
            bkw = new BackgroundWorker();
            bkw.DoWork += new DoWorkEventHandler(bkw_DoWork);
            bkw.ProgressChanged += new ProgressChangedEventHandler(bkw_ProgressChanged);
            bkw.WorkerReportsProgress = true;
            bkw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bkw_RunWorkerCompleted);
            _bgWorkReport = new BackgroundWorker();
            _bgWorkReport.DoWork += new DoWorkEventHandler(_bgWorkReport_DoWork);
            _bgWorkReport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bgWorkReport_RunWorkerCompleted);
            _bgWorkReport.WorkerReportsProgress = true;
            _bgWorkReport.ProgressChanged += new ProgressChangedEventHandler(_bgWorkReport_ProgressChanged);
        }

        void bkw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            circularProgress1.Value = e.ProgressPercentage;
        }

        void bkw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnbSelect();

            _DefalutSchoolYear = K12.Data.School.DefaultSchoolYear;
            _DefaultSemester = K12.Data.School.DefaultSemester;

            if (_Configure == null)
                _Configure = new Configure();

            cboConfigure.Items.Clear();
            foreach (var item in _ConfigureList)
            {
                cboConfigure.Items.Add(item);
            }
            cboConfigure.Items.Add(new Configure() { Name = "新增" });
            int i;

            if (int.TryParse(_DefalutSchoolYear, out i))
            {
                for (int j = 5; j > 0; j--)
                {
                    cboSchoolYear.Items.Add("" + (i - j));
                }
                for (int j = 0; j < 3; j++)
                {
                    cboSchoolYear.Items.Add("" + (i + j));
                }

            }

            cboSemester.Items.Add("1");
            cboSemester.Items.Add("2");
            cboExam.Items.Clear();
            foreach (ExamRecord exName in _exams)
                cboExam.Items.Add(exName.Name);

            circularProgress1.Hide();
            if (_ConfigureList.Count > 0)
            {
                cboConfigure.SelectedIndex = 0;
            }
            else
            {
                cboConfigure.SelectedIndex = -1;
            }

            if (_Configure.PrintAttendanceList == null)
                _Configure.PrintAttendanceList = new List<string>();

            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
            colName.HeaderText = "節次分類";
            colName.MinimumWidth = 70;
            colName.Name = "colName";
            colName.ReadOnly = true;
            colName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colName.Width = 70;
            this.dgAttendanceData.Columns.Add(colName);

            List<string> colNameList = new List<string>();
            foreach (string absence in absenceList)
            {
                System.Windows.Forms.DataGridViewCheckBoxColumn newCol = new DataGridViewCheckBoxColumn();
                newCol.HeaderText = absence;
                newCol.Width = 55;
                newCol.ReadOnly = false;
                newCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                newCol.Tag = absence;
                colNameList.Add(absence);
                newCol.ValueType = typeof(bool);
                this.dgAttendanceData.Columns.Add(newCol);
            }

            foreach (string str in typeList)
            {
                int rowIdx = dgAttendanceData.Rows.Add();
                dgAttendanceData.Rows[rowIdx].Tag = str;
                dgAttendanceData.Rows[rowIdx].Cells[0].Value = str;
                int colIdx = 1;

                foreach (string str1 in colNameList)
                {
                    string key = str + "_" + str1;
                    DataGridViewCheckBoxCell cell = new DataGridViewCheckBoxCell();
                    cell.Tag = key;
                    cell.Value = false;
                    if (_Configure.PrintAttendanceList.Contains(key))
                        cell.Value = true;

                    dgAttendanceData.Rows[rowIdx].Cells[colIdx] = cell;
                    colIdx++;
                }
            }

            string userSelectConfigName = "";
            // 檢查畫面上是否有使用者選的
            foreach (DAO.UDT_ScoreConfig conf in _UDTConfigList)
                if (conf.Type == Global._UserConfTypeName)
                {
                    userSelectConfigName = conf.Name;
                    break;
                }

            if (!string.IsNullOrEmpty(_Configure.SelSetConfigName))
                cboConfigure.Text = userSelectConfigName;

            // 不排名學生類別項目放入
            cboNotRankedFilter.Items.Add("");
            foreach (string name in _StudTagDict.Keys)
                cboNotRankedFilter.Items.Add(name);

            if (!string.IsNullOrEmpty(_Configure.NotRankedTagNameFilter))
            {
                if (cboNotRankedFilter.Items.Contains(_Configure.NotRankedTagNameFilter))
                    cboNotRankedFilter.Text = _Configure.NotRankedTagNameFilter;
            }
            btnSaveConfig.Enabled = btnPrint.Enabled = true;
        }

        void bkw_DoWork(object sender, DoWorkEventArgs e)
        {
            bkw.ReportProgress(1);
            //試別清單
            _exams.Clear();
            _exams = K12.Data.Exam.SelectAll();

            // 檢查預設樣板是否存在
            _UDTConfigList = DAO.UDTTransfer.GetDefaultConfigNameListByTableName(Global._UDTTableName);

            // 沒有設定檔，建立預設設定檔
            if (_UDTConfigList.Count < 2) 
            {
                bkw.ReportProgress(10);
                foreach (string name in Global.DefaultConfigNameList())
                {
                    Configure cn = new Configure();
                    cn.Name = name;
                    cn.SchoolYear = K12.Data.School.DefaultSchoolYear;
                    cn.Semester = K12.Data.School.DefaultSemester;
                    DAO.UDT_ScoreConfig conf = new DAO.UDT_ScoreConfig();
                    conf.Name = name;
                    conf.UDTTableName = Global._UDTTableName;
                    conf.ProjectName = Global._ProjectName;
                    conf.Type = Global._DefaultConfTypeName;
                    _UDTConfigList.Add(conf);

                    // 設預設樣板
                    switch (name)
                    {   // [ischoolkingdom] Vicky新增，個人評量成績單建議能增加可列印數量(高雄國中)，預設改為 領域成績單(預設)、科目成績單(預設)兩張。
                        case "領域成績單(預設)":
                            cn.Template = new Document(new MemoryStream(Properties.Resources.領域成績單_預設_));
                            break;

                        case "科目成績單(預設)":
                            cn.Template = new Document(new MemoryStream(Properties.Resources.科目成績單_預設_));
                            break;

                            //case "領域成績單":
                            //    cn.Template = new Document(new MemoryStream(Properties.Resources.高雄_領域成績單));
                            //    break;

                            //case "領域成績單(含平時成績)":
                            //    cn.Template = new Document(new MemoryStream(Properties.Resources.高雄_領域成績單_含平時成績));
                            //    break;

                            //case "科目成績單":
                            //    cn.Template = new Document(new MemoryStream(Properties.Resources.高雄_科目成績單));
                            //    break;
                            //case "科目成績單(含平時成績)":
                            //    cn.Template = new Document(new MemoryStream(Properties.Resources.高雄_科目成績單_含平時成績));
                            //    break;
                            //case "科目及領域成績單":
                            //    cn.Template = new Document(new MemoryStream(Properties.Resources.高雄_科目及領域成績單));
                            //    break;
                            //case "科目及領域成績單(含平時成績)":
                            //    cn.Template = new Document(new MemoryStream(Properties.Resources.高雄_科目及領域成績單_含平時成績));
                            //    break;
                    }

                    if (cn.Template == null)
                        cn.Template = new Document(new MemoryStream(Properties.Resources.高雄評量成績通知單樣板));
                    cn.Encode();
                    cn.Save();
                }
                if (_UDTConfigList.Count > 0)
                    DAO.UDTTransfer.InsertConfigData(_UDTConfigList);
            }
            bkw.ReportProgress(20);
            // 取的設定資料
            _ConfigureList = _AccessHelper.Select<Configure>();

            bkw.ReportProgress(40);
            // 缺曠資料
            foreach (JHPeriodMappingInfo info in JHPeriodMapping.SelectAll())
            {
                if (!typeList.Contains(info.Type))
                    typeList.Add(info.Type);
            }

            bkw.ReportProgress(70);
            foreach (JHAbsenceMappingInfo info in JHAbsenceMapping.SelectAll())
            {
                if (!absenceList.Contains(info.Name))
                    absenceList.Add(info.Name);
            }
            bkw.ReportProgress(80);
            // 所有有學生類別
            _StudTagDict = Utility.GetStudentTagRefDict();

            bkw.ReportProgress(100);
        }

        void _bgWorkReport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FISCA.Presentation.MotherForm.SetStatusBarMessage("評量成績報表產生中...", e.ProgressPercentage);
        }

        void _bgWorkReport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                btnSaveConfig.Enabled = true;
                btnPrint.Enabled = true;

                if (_ErrorList.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    //sb.AppendLine("樣板內科目合併欄位不足，請新增：");
                    //sb.AppendLine(string.Join(",",_ErrorList.ToArray()));
                    sb.AppendLine("1.樣板內科目合併欄位不足，請檢查樣板。");
                    sb.AppendLine("2.如果使用只有領域樣板，請忽略此訊息。");
                    if (_ErrorDomainNameList.Count > 0)
                        sb.AppendLine(string.Join(",", _ErrorDomainNameList.ToArray()));

                    FISCA.Presentation.Controls.MsgBox.Show(sb.ToString(), "樣板內科目合併欄位不足", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

                }

                FISCA.Presentation.MotherForm.SetStatusBarMessage("評量成績報表產生完成");
                System.Diagnostics.Process.Start(pathW);
            }
            catch (Exception ex)
            {
                FISCA.Presentation.Controls.MsgBox.Show("產生過程發生錯誤" + ex.Message);
            }
        }

        // 載入學生所屬學年度學習的試別，科目，並排序
        private void LoadExamSubject()
        {
            // 取得該學年度學期所有學生的試別修課科目
            _SelSchoolYear = _SelSemester = 0;
            int ss, sc;
            if (int.TryParse(cboSchoolYear.Text, out ss))
                _SelSchoolYear = ss;

            if (int.TryParse(cboSemester.Text, out sc))
                _SelSemester = sc;

            _ExamSubjectFull = Utility.GetExamSubjecList(_StudentIDList, _SelSchoolYear, _SelSemester);

            foreach (var list in _ExamSubjectFull.Values)
            {
                #region 排序
                list.Sort(new StringComparer("國文"
                                , "英文"
                                , "數學"
                                , "理化"
                                , "生物"
                                , "社會"
                                , "物理"
                                , "化學"
                                , "歷史"
                                , "地理"
                                , "公民"));
                #endregion
            }
        }


        void _bgWorkReport_DoWork(object sender, DoWorkEventArgs e)
        {
            #region 資料讀取
            _bgWorkReport.ReportProgress(1);


            // 處理樣板，每次合併後放入，最後再合成一張
            Document docTemplate = _Configure.Template;
            if (docTemplate == null)
                docTemplate = new Document(new MemoryStream(Properties.Resources.高雄評量成績通知單樣板));

            _ErrorList.Clear();
            _ErrorDomainNameList.Clear();
            _TemplateSubjectNameList.Clear();

            // 取得樣板內科目名稱
            foreach (string name in docTemplate.MailMerge.GetFieldNames())
            {
                if (name.Contains("科目名稱"))
                    _TemplateSubjectNameList.Add(name);
            }
        
            // 校名
            string SchoolName = K12.Data.School.ChineseName;
            // 校長
            string ChancellorChineseName = JHSchool.Data.JHSchoolInfo.ChancellorChineseName;
            // 教務主任
            string EduDirectorName = JHSchool.Data.JHSchoolInfo.EduDirectorName;

            // 班級
            Dictionary<string, ClassRecord> ClassDict = new Dictionary<string, ClassRecord>();
            foreach (ClassRecord cr in Class.SelectAll())
                ClassDict.Add(cr.ID, cr);
            // 教師
            Dictionary<string, TeacherRecord> TeacherDict = new Dictionary<string, TeacherRecord>();
            foreach (TeacherRecord tr in Teacher.SelectAll())
            {
                if (tr.Status == TeacherRecord.TeacherStatus.一般)
                    TeacherDict.Add(tr.ID, tr);
            }

            // 不排名學生ID
            List<string> notRankStudIDList = new List<string>();
            if (_StudTagDict.ContainsKey(_SelNotRankedFilter))
                notRankStudIDList.AddRange(_StudTagDict[_SelNotRankedFilter].ToArray());

            // 移除所選學生不排名
            foreach (string id in notRankStudIDList)
                _StudentIDList.Remove(id);

            // 所選學生資料
            List<StudentRecord> StudRecList = Student.SelectByIDs(_StudentIDList);

            _bgWorkReport.ReportProgress(5);
            // 班級年級區分,沒有年級不處理
            Dictionary<int, List<StudentRecord>> studGradeDict = new Dictionary<int, List<StudentRecord>>();
            List<string> studIDAllList = new List<string>();
            foreach (StudentRecord studRec in Student.SelectAll())
            {
                // 不排名學生ID
                if (notRankStudIDList.Contains(studRec.ID))
                    continue;

                if (studRec.Status == StudentRecord.StudentStatus.一般)
                {
                    if (ClassDict.ContainsKey(studRec.RefClassID))
                    {
                        if (ClassDict[studRec.RefClassID].GradeYear.HasValue)
                        {
                            int gr = ClassDict[studRec.RefClassID].GradeYear.Value;

                            if (!studGradeDict.ContainsKey(gr))
                                studGradeDict.Add(gr, new List<StudentRecord>());

                            studIDAllList.Add(studRec.ID);
                            studGradeDict[gr].Add(studRec);
                        }
                    }

                }
            }
            _bgWorkReport.ReportProgress(15);

            #region 取得學生成績計算規則
            ScoreCalculator defaultScoreCalculator = new ScoreCalculator(null);

            //key: ScoreCalcRuleID
            Dictionary<string, ScoreCalculator> calcCache = new Dictionary<string, ScoreCalculator>();
            //key: StudentID, val: ScoreCalcRuleID
            Dictionary<string, string> calcIDCache = new Dictionary<string, string>();
            List<string> scoreCalcRuleIDList = new List<string>();
            foreach (StudentRecord student in StudRecList)
            {
                //calcCache.Add(student.ID, new ScoreCalculator(student.ScoreCalcRule));
                string calcID = string.Empty;
                if (!string.IsNullOrEmpty(student.OverrideScoreCalcRuleID))
                    calcID = student.OverrideScoreCalcRuleID;
                else if (student.Class != null && !string.IsNullOrEmpty(student.Class.RefScoreCalcRuleID))
                    calcID = student.Class.RefScoreCalcRuleID;

                if (!string.IsNullOrEmpty(calcID))
                    calcIDCache.Add(student.ID, calcID);
            }
            foreach (JHScoreCalcRuleRecord record in JHScoreCalcRule.SelectByIDs(calcIDCache.Values))
            {
                if (!calcCache.ContainsKey(record.ID))
                    calcCache.Add(record.ID, new ScoreCalculator(record));
            }

            #endregion



            // 課程資料
            Dictionary<string, JHCourseRecord> CourseDict = new Dictionary<string, JHCourseRecord>();

            foreach (JHCourseRecord co in JHCourse.SelectBySchoolYearAndSemester(_SelSchoolYear, _SelSemester))
            {
                if (_SelSubjNameList.Contains(co.Subject))
                    CourseDict.Add(co.ID, co);
            }

            // 取得修課資料，主要取得平時成績 StudentID,CourseID
            Dictionary<string, decimal?> AssignmentScoreDict = new Dictionary<string, decimal?>();
            // 分課程取資料
            foreach (JHSCAttendRecord rec in JHSCAttend.SelectByCourseIDs(CourseDict.Keys.ToArray()))
            {
                if (studIDAllList.Contains(rec.RefStudentID))
                    if (!AssignmentScoreDict.ContainsKey(rec.ID))
                        AssignmentScoreDict.Add(rec.ID, rec.OrdinarilyScore);
            }

            _bgWorkReport.ReportProgress(20);
            // 取評量成績
            Dictionary<string, List<KH.JHSCETakeRecord>> Score1Dict = new Dictionary<string, List<KH.JHSCETakeRecord>>();
            foreach (JHSCETakeRecord record in JHSCETake.SelectByCourseAndExam(CourseDict.Keys.ToList(), _SelExamID))
            {
                if (studIDAllList.Contains(record.RefStudentID))
                {
                    if (!Score1Dict.ContainsKey(record.RefStudentID))
                        Score1Dict.Add(record.RefStudentID, new List<KH.JHSCETakeRecord>());

                    Score1Dict[record.RefStudentID].Add(new KH.JHSCETakeRecord(record));
                }
            }
            _bgWorkReport.ReportProgress(30);
            // 取得這次該修課程
            Dictionary<string, Dictionary<string, DAO.SubjectDomainName>> StudCourseDict = Utility.GetStudentSCAttendCourse(_StudentIDList, CourseDict.Keys.ToList(), _SelExamID);


            // 處理評量成績科目
            Dictionary<string, DAO.StudExamScore> studExamScoreDict = new Dictionary<string, DAO.StudExamScore>();
            foreach (string studID in _StudentIDList)
            {
                // 成績計算規則
                ScoreCalculator studentCalculator = defaultScoreCalculator;
                if (calcIDCache.ContainsKey(studID) && calcCache.ContainsKey(calcIDCache[studID]))
                    studentCalculator = calcCache[calcIDCache[studID]];

                if (Score1Dict.ContainsKey(studID))
                {
                    if (!studExamScoreDict.ContainsKey(studID))
                        studExamScoreDict.Add(studID, new DAO.StudExamScore(studentCalculator));

                    foreach (KH.JHSCETakeRecord rec in Score1Dict[studID])
                    {
                        if (rec.RefExamID == _SelExamID && CourseDict.ContainsKey(rec.RefCourseID))
                        {
                            JHCourseRecord cr = CourseDict[rec.RefCourseID];
                            // 濾過社團
                            if (string.IsNullOrEmpty(cr.Subject))
                                continue;

                            string SubjecName = cr.Subject;

                            // 勾選科目
                            if (_SelSubjNameList.Contains(SubjecName))
                            {
                                if (!studExamScoreDict[studID]._ExamSubjectScoreDict.ContainsKey(SubjecName))
                                {
                                    DAO.ExamSubjectScore ess = new DAO.ExamSubjectScore();
                                    ess.DomainName = cr.Domain;
                                    ess.SubjectName = SubjecName;
                                    // 努力程度                                     
                                    ess.Effort = rec.Effort;
                                    // 評量分數
                                    ess.Score = rec.Score;
                                    // 平時成績
                                    if (AssignmentScoreDict.ContainsKey(rec.RefSCAttendID))
                                        ess.AssignmentScore = AssignmentScoreDict[rec.RefSCAttendID];
                                    // 文字描述
                                    ess.Text = rec.Text;
                                    ess.Credit = cr.Credit;
                                    if (StudCourseDict.ContainsKey(studID))
                                    {
                                        // 有修課才加入
                                        if (StudCourseDict[studID].ContainsKey(SubjecName))
                                        {
                                            // 依科目成績計算規則四捨五入
                                            if (ess.AssignmentScore.HasValue)
                                                ess.AssignmentScore = studentCalculator.ParseSubjectScore(ess.AssignmentScore.Value);

                                            if (ess.Score.HasValue)
                                                ess.Score = studentCalculator.ParseSubjectScore(ess.Score.Value);

                                            studExamScoreDict[studID]._ExamSubjectScoreDict.Add(SubjecName, ess);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (StudCourseDict.ContainsKey(studID))
                {
                    if (!studExamScoreDict.ContainsKey(studID))
                        studExamScoreDict.Add(studID, new DAO.StudExamScore(studentCalculator));
                    if (studExamScoreDict[studID]._ExamSubjectScoreDict == null)
                    {
                        studExamScoreDict[studID]._ExamSubjectScoreDict = new Dictionary<string, DAO.ExamSubjectScore>();
                        studExamScoreDict[studID]._ExamDomainScoreDict = new Dictionary<string, DAO.ExamDomainScore>();
                    }
                    foreach (KeyValuePair<string, DAO.SubjectDomainName> data in StudCourseDict[studID])
                    {
                        // 濾過社團
                        if (string.IsNullOrEmpty(data.Key))
                            continue;

                        // 畫面上沒有勾選跳過
                        if (!_SelSubjNameList.Contains(data.Key))
                            continue;

                        // 加入有修課沒有成績空科目
                        if (!studExamScoreDict[studID]._ExamSubjectScoreDict.ContainsKey(data.Key))
                        {
                            DAO.ExamSubjectScore ess = new DAO.ExamSubjectScore();
                            ess.SubjectName = data.Key;
                            ess.DomainName = data.Value.DomainName;
                            ess.Credit = data.Value.Credit;
                            studExamScoreDict[studID]._ExamSubjectScoreDict.Add(data.Key, ess);
                        }
                    }
                }

                // 計算領域成績
                if (Score1Dict.ContainsKey(studID))
                {
                    studExamScoreDict[studID].CalcSubjectToDomain();
                }
            }

            #region 領域成績區間處理
            // 處理領域成績區間
            // 班級
            Dictionary<string, Dictionary<string, DAO.DomainRangeCount>> dmRClasssDict = new Dictionary<string, Dictionary<string, DAO.DomainRangeCount>>();
            // 年級
            Dictionary<int, Dictionary<string, DAO.DomainRangeCount>> dmGradeDict = new Dictionary<int, Dictionary<string, DAO.DomainRangeCount>>();

            List<string> dmClassDNList = new List<string>();
            List<string> dmGradeDNList = new List<string>();
            _bgWorkReport.ReportProgress(40);
            // 處理評量成績科目
            Dictionary<string, DAO.StudExamScore> studAllExamScoreDict = new Dictionary<string, DAO.StudExamScore>();
            // 全年級學生
            foreach (int gr in studGradeDict.Keys)
            {
                foreach (StudentRecord studR in studGradeDict[gr])
                {
                    if (Score1Dict.ContainsKey(studR.ID))
                    {
                        // 成績計算規則
                        ScoreCalculator studentCalculator = defaultScoreCalculator;
                        if (calcIDCache.ContainsKey(studR.ID) && calcCache.ContainsKey(calcIDCache[studR.ID]))
                            studentCalculator = calcCache[calcIDCache[studR.ID]];

                        if (!studAllExamScoreDict.ContainsKey(studR.ID))
                            studAllExamScoreDict.Add(studR.ID, new DAO.StudExamScore(studentCalculator));

                        studAllExamScoreDict[studR.ID].GradeYear = gr;
                        studAllExamScoreDict[studR.ID].ClassID = studR.RefClassID;

                        foreach (KH.JHSCETakeRecord rec in Score1Dict[studR.ID])
                        {

                            if (rec.RefExamID == _SelExamID && CourseDict.ContainsKey(rec.RefCourseID))
                            {
                                JHCourseRecord cr = CourseDict[rec.RefCourseID];
                                // 濾過社團
                                if (string.IsNullOrEmpty(cr.Subject))
                                    continue;

                                // 畫面有勾選才計算
                                if (_SelSubjNameList.Contains(cr.Subject))
                                {
                                    string dmName = cr.Domain;

                                    if (!studAllExamScoreDict[studR.ID]._ExamSubjectScoreDict.ContainsKey(cr.Subject))
                                    {
                                        DAO.ExamSubjectScore ess = new DAO.ExamSubjectScore();
                                        ess.DomainName = dmName;
                                        ess.SubjectName = cr.Subject;
                                        // 評量分數
                                        ess.Score = rec.Score;
                                        // 平時成績
                                        if (AssignmentScoreDict.ContainsKey(rec.RefSCAttendID))
                                            ess.AssignmentScore = AssignmentScoreDict[rec.RefSCAttendID];
                                        // 文字描述
                                        ess.Text = rec.Text;
                                        ess.Credit = cr.Credit;
                                        studAllExamScoreDict[studR.ID]._ExamSubjectScoreDict.Add(cr.Subject, ess);
                                    }
                                }
                            }
                        }
                        // 計算領域成績
                        studAllExamScoreDict[studR.ID].CalcSubjectToDomain();
                    }
                }
            }
            _bgWorkReport.ReportProgress(50);
            // 處理區間
            foreach (DAO.StudExamScore ses in studAllExamScoreDict.Values)
            {
                foreach (DAO.ExamDomainScore eds in ses._ExamDomainScoreDict.Values)
                {
                    string dmName = eds.DomainName;
                    //if (!dmClassDNList.Contains(dmName))
                    //    dmClassDNList.Add(dmName);

                    //if (!dmGradeDNList.Contains(dmName))
                    //    dmGradeDNList.Add(dmName);

                    if (!dmRClasssDict.ContainsKey(ses.ClassID))
                        dmRClasssDict.Add(ses.ClassID, new Dictionary<string, DAO.DomainRangeCount>());

                    if (!dmGradeDict.ContainsKey(ses.GradeYear))
                        dmGradeDict.Add(ses.GradeYear, new Dictionary<string, DAO.DomainRangeCount>());

                    // 班級
                    if (!dmRClasssDict[ses.ClassID].ContainsKey(dmName))
                    {
                        DAO.DomainRangeCount drc = new DAO.DomainRangeCount();
                        drc.Name = dmName;
                        dmRClasssDict[ses.ClassID].Add(dmName, drc);
                    }

                    // 年級
                    if (!dmGradeDict[ses.GradeYear].ContainsKey(dmName))
                    {
                        DAO.DomainRangeCount drc = new DAO.DomainRangeCount();
                        drc.Name = dmName;
                        dmGradeDict[ses.GradeYear].Add(dmName, drc);
                    }

                    // 放入成績
                    dmRClasssDict[ses.ClassID][dmName].AddScore(eds.Score);
                    dmGradeDict[ses.GradeYear][dmName].AddScore(eds.Score);


                }
            }


            #endregion

            #region 科目成績區間處理
            // 處理領域成績區間
            // 班級
            Dictionary<string, Dictionary<string, DAO.SubjectRangeCount>> subjRClasssDict = new Dictionary<string, Dictionary<string, DAO.SubjectRangeCount>>();
            // 年級
            Dictionary<int, Dictionary<string, DAO.SubjectRangeCount>> subjGradeDict = new Dictionary<int, Dictionary<string, DAO.SubjectRangeCount>>();

            List<string> subjClassDNList = new List<string>();
            List<string> subjGradeDNList = new List<string>();

            // 處理評量成績科目
            Dictionary<string, DAO.StudExamScore> studSubjAllExamScoreDict = new Dictionary<string, DAO.StudExamScore>();
            // 全年級學生
            foreach (int gr in studGradeDict.Keys)
            {
                foreach (StudentRecord studR in studGradeDict[gr])
                {
                    if (Score1Dict.ContainsKey(studR.ID))
                    {
                        // 成績計算規則
                        ScoreCalculator studentCalculator = defaultScoreCalculator;
                        if (calcIDCache.ContainsKey(studR.ID) && calcCache.ContainsKey(calcIDCache[studR.ID]))
                            studentCalculator = calcCache[calcIDCache[studR.ID]];

                        if (!studSubjAllExamScoreDict.ContainsKey(studR.ID))
                            studSubjAllExamScoreDict.Add(studR.ID, new DAO.StudExamScore(studentCalculator));

                        studSubjAllExamScoreDict[studR.ID].GradeYear = gr;
                        studSubjAllExamScoreDict[studR.ID].ClassID = studR.RefClassID;

                        foreach (KH.JHSCETakeRecord rec in Score1Dict[studR.ID])
                        {
                            if (rec.RefExamID == _SelExamID && CourseDict.ContainsKey(rec.RefCourseID))
                            {
                                JHCourseRecord cr = CourseDict[rec.RefCourseID];

                                // 畫面有勾選才計算
                                if (_SelSubjNameList.Contains(cr.Subject))
                                {

                                    if (!studSubjAllExamScoreDict[studR.ID]._ExamSubjectScoreDict.ContainsKey(cr.Subject))
                                    {
                                        DAO.ExamSubjectScore ess = new DAO.ExamSubjectScore();
                                        ess.SubjectName = cr.Subject;
                                        //ess.AssignmentScore = rec
                                        ess.Score = rec.Score;

                                        studSubjAllExamScoreDict[studR.ID]._ExamSubjectScoreDict.Add(cr.Subject, ess);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 處理科目區間
            foreach (DAO.StudExamScore ses in studSubjAllExamScoreDict.Values)
            {
                foreach (DAO.ExamSubjectScore ess in ses._ExamSubjectScoreDict.Values)
                {
                    string subjName = ess.SubjectName;
                    //if (!subjClassDNList.Contains(subjName))
                    //    subjClassDNList.Add(subjName);

                    //if (!subjGradeDNList.Contains(subjName))
                    //    subjGradeDNList.Add(subjName);

                    if (!subjRClasssDict.ContainsKey(ses.ClassID))
                        subjRClasssDict.Add(ses.ClassID, new Dictionary<string, DAO.SubjectRangeCount>());

                    if (!subjGradeDict.ContainsKey(ses.GradeYear))
                        subjGradeDict.Add(ses.GradeYear, new Dictionary<string, DAO.SubjectRangeCount>());

                    // 班級
                    if (!subjRClasssDict[ses.ClassID].ContainsKey(subjName))
                    {
                        DAO.SubjectRangeCount src = new DAO.SubjectRangeCount();
                        src.Name = subjName;
                        subjRClasssDict[ses.ClassID].Add(subjName, src);
                    }

                    // 年級
                    if (!subjGradeDict[ses.GradeYear].ContainsKey(subjName))
                    {
                        DAO.SubjectRangeCount src = new DAO.SubjectRangeCount();
                        src.Name = subjName;
                        subjGradeDict[ses.GradeYear].Add(subjName, src);
                    }

                    // 放入成績(總成績)
                    subjRClasssDict[ses.ClassID][subjName].AddScore(ess.Score);
                    subjGradeDict[ses.GradeYear][subjName].AddScore(ess.Score);
                }
            }
            #endregion
            _bgWorkReport.ReportProgress(60);

            // 缺曠資料區間統計
            _AttendanceDict = Utility.GetAttendanceCountByDate(StudRecList, _BeginDate, _EndDate);

            // 獎懲資料
            Dictionary<string, Dictionary<string, int>> DisciplineCountDict = Utility.GetDisciplineCountByDate(_StudentIDList, _BeginDate, _EndDate);

            // 服務學習
            Dictionary<string, decimal> ServiceLearningDict = Utility.GetServiceLearningDetailByDate(_StudentIDList, _BeginDate, _EndDate);

            // 取得父母監護人資訊
            Dictionary<string, JHParentRecord> ParentRecordDict = new Dictionary<string, JHParentRecord>();
            foreach (JHParentRecord rec in JHParent.SelectByStudentIDs(_StudentIDList))
            {
                if (!ParentRecordDict.ContainsKey(rec.RefStudentID))
                    ParentRecordDict.Add(rec.RefStudentID, rec);
            }

            // 取得地址相關資訊
            Dictionary<string, JHAddressRecord> AddressRecordDict = new Dictionary<string, JHAddressRecord>();
            foreach (JHAddressRecord rec in JHAddress.SelectByStudentIDs(_StudentIDList))
            {
                if (!AddressRecordDict.ContainsKey(rec.RefStudentID))
                    AddressRecordDict.Add(rec.RefStudentID, rec);
            }

            // 領域組距
            List<string> li2 = new List<string>();
            li2.Add("R100_u");
            li2.Add("R90_99");
            li2.Add("R80_89");
            li2.Add("R70_79");
            li2.Add("R60_69");
            li2.Add("R50_59");
            li2.Add("R40_49");
            li2.Add("R30_39");
            li2.Add("R20_29");
            li2.Add("R10_19");
            li2.Add("R0_9");

            #endregion


            _bgWorkReport.ReportProgress(75);

            List<string> domainLi = new List<string>();

            List<string> subjLi = new List<string>();
            subjLi.Add("科目名稱");
            subjLi.Add("科目權數");
            subjLi.Add("科目努力程度");
            subjLi.Add("科目評量分數");
            subjLi.Add("科目平時成績");
            subjLi.Add("科目文字描述");


            List<string> subjColList = new List<string>();
            foreach (string dName in Global.DomainNameList)
            {
                for (int i = 1; i <= 10; i++)
                {
                    foreach (string sName in subjLi)
                    {
                        string key = dName + "_" + sName + i;
                        subjColList.Add(key);
                    }
                }
            }


            //foreach(string sid in studExamScoreDict.Keys)
            //{
            //    int sub = 1;
            //    foreach (DAO.ExamSubjectScore ess in studExamScoreDict[sid]._ExamSubjectScoreDict.Values)
            //    {                    
            //        foreach (string item in subjLi)
            //        {
            //            // 科目領域
            //            if (!domainLi.Contains(ess.DomainName))
            //                domainLi.Add(ess.DomainName);

            //            // 科目名稱
            //            string key = ess.DomainName + "_" + item + sub;
            //            if (!subjColList.Contains(key))
            //                subjColList.Add(key);
            //        }
            //        sub++;
            //    }                    
            //}

            // 學生筆學期歷程
            Dictionary<string, SemesterHistoryItem> StudShiDict = new Dictionary<string, SemesterHistoryItem>();
            // 取得學期歷程，給班級、座號、班導師使用，條件：所選學生
            List<SemesterHistoryRecord> SemesterHistoryRecordList = SemesterHistory.SelectByStudentIDs(_StudentIDList);
            foreach (SemesterHistoryRecord shr in SemesterHistoryRecordList)
            {
                foreach (SemesterHistoryItem shi in shr.SemesterHistoryItems)
                    if (shi.SchoolYear == _SelSchoolYear && shi.Semester == _SelSemester)
                    {
                        if (!StudShiDict.ContainsKey(shi.RefStudentID))
                            StudShiDict.Add(shi.RefStudentID, shi);
                    }
            }

            #region 取得目前班級、座號、班導師
            Dictionary<string, DAO.StudCST> StudCSTDict = Utility.GetStudentCSTByStudentIDList(_StudentIDList);

            #endregion

            #region 處理合併 DataTable 相關資料
            // 儲存資料用 Data Table
            DataTable dt = new DataTable();

            //// 取得欄位
            //foreach (string colName in Global.DTColumnsList())
            //    dt.Columns.Add(colName);

            //// 組距欄位
            //// 班級組距            
            //foreach (string n1 in dmClassDNList)
            //{
            //    foreach (string n2 in li2)
            //    {
            //        string colName = "班級_" + n1 + "_" + n2;
            //        dt.Columns.Add(colName);
            //    }
            //}
            //// 年級
            //foreach (string n1 in dmGradeDNList)
            //{
            //    foreach (string n2 in li2)
            //    {
            //        string colName = "年級_" + n1 + "_" + n2;
            //        dt.Columns.Add(colName);
            //    }
            //}

            //// 新增科目成績欄位
            //foreach (string colName in subjColList)
            //    dt.Columns.Add(colName);

            //// 新增領域成績欄位
            //foreach (string dName in domainLi)
            //    dt.Columns.Add(dName + "_領域加權平均");

            Document doc = new Document();
            DataTable dtAtt = new DataTable();
            List<Document> docList = new List<Document>();
            // 填值
            foreach (StudentRecord StudRec in StudRecList)
            {
                DataRow row = dt.NewRow();
                dtAtt.Columns.Clear();
                dtAtt.Clear();
                dt.Clear();
                dt.Columns.Clear();

                dtAtt.Columns.Add("缺曠紀錄");
                DataRow rowT = dtAtt.NewRow();


                // 
                // 取得欄位
                foreach (string colName in Global.DTColumnsList())
                    dt.Columns.Add(colName);

                // 取得年級
                int grYear = 0;

                if (ClassDict.ContainsKey(StudRec.RefClassID))
                {
                    if (ClassDict[StudRec.RefClassID].GradeYear.HasValue)
                        grYear = ClassDict[StudRec.RefClassID].GradeYear.Value;
                }


                // 組距欄位
                // 組距欄位
                // 領域班級組距
                dmClassDNList.Clear();
                dmGradeDNList.Clear();

                if (dmRClasssDict.ContainsKey(StudRec.RefClassID))
                {
                    foreach (string dnName in dmRClasssDict[StudRec.RefClassID].Keys)
                        dmClassDNList.Add(dnName);
                }

                if (dmGradeDict.ContainsKey(grYear))
                {
                    foreach (string dnName in dmGradeDict[grYear].Keys)
                        dmGradeDNList.Add(dnName);
                }

                dmClassDNList.Sort();
                dmGradeDNList.Sort();
                foreach (string n1 in dmClassDNList)
                {
                    foreach (string n2 in li2)
                    {
                        string colName = "班級_" + n1 + "_" + n2;
                        dt.Columns.Add(colName);
                    }
                }
                // 年級
                foreach (string n1 in dmGradeDNList)
                {
                    foreach (string n2 in li2)
                    {
                        string colName = "年級_" + n1 + "_" + n2;
                        dt.Columns.Add(colName);
                    }
                }

                // 科目
                // 領域班級組距
                subjClassDNList.Clear();
                if (subjRClasssDict.ContainsKey(StudRec.RefClassID))
                {
                    foreach (string subjName in subjRClasssDict[StudRec.RefClassID].Keys)
                        subjClassDNList.Add(subjName);
                }
                subjClassDNList.Sort(new StringComparer("國文"
                                , "英文"
                                , "數學"
                                , "理化"
                                , "生物"
                                , "社會"
                                , "物理"
                                , "化學"
                                , "歷史"
                                , "地理"
                                , "公民"));

                subjGradeDNList.Clear();
                if (subjGradeDict.ContainsKey(grYear))
                {
                    foreach (string subjName in subjGradeDict[grYear].Keys)
                        subjGradeDNList.Add(subjName);
                }

                subjGradeDNList.Sort(new StringComparer("國文"
                                , "英文"
                                , "數學"
                                , "理化"
                                , "生物"
                                , "社會"
                                , "物理"
                                , "化學"
                                , "歷史"
                                , "地理"
                                , "公民"));

                // 科目
                // 領域班級組距
                Dictionary<string, string> colSubjMapDict = new Dictionary<string, string>();
                int c1 = 1, g1 = 1;
                foreach (string n1 in subjClassDNList)
                {
                    string sName1 = "s班級_科目名稱" + n1;
                    string sName2 = "s班級_科目名稱" + c1;
                    dt.Columns.Add(sName2);
                    // 放入科目名稱
                    colSubjMapDict.Add(sName1, sName2);
                    foreach (string n2 in li2)
                    {
                        string colName = "s班級_" + n1 + "_" + n2;
                        string colVal = "s班級_" + "科目" + c1 + "_" + n2;
                        colSubjMapDict.Add(colName, colVal);
                        dt.Columns.Add(colVal);
                    }
                    c1++;
                }
                // 年級
                foreach (string n1 in subjGradeDNList)
                {
                    string gName1 = "s年級_科目名稱" + n1;
                    string gName2 = "s年級_科目名稱" + g1;
                    dt.Columns.Add(gName2);
                    colSubjMapDict.Add(gName1, gName2);
                    foreach (string n2 in li2)
                    {
                        string colName = "s年級_" + n1 + "_" + n2;
                        string colVal = "s年級_" + "科目" + g1 + "_" + n2;
                        colSubjMapDict.Add(colName, colVal);
                        dt.Columns.Add(colVal);
                    }
                    g1++;
                }

                // 新增科目成績欄位
                foreach (string colName in subjColList)
                    dt.Columns.Add(colName);

                // 新增領域成績欄位
                foreach (string dName in Global.DomainNameList)
                {
                    dt.Columns.Add(dName + "_領域加權平均");
                    dt.Columns.Add(dName + "_領域權數");
                    dt.Columns.Add(dName + "_領域平時加權平均");
                    dt.Columns.Add(dName + "_領域努力程度");
                }

                dt.TableName = StudRec.ID;
                row["StudentID"] = StudRec.ID;
                row["學校名稱"] = SchoolName;
                row["學年度"] = _SelSchoolYear;
                row["學期"] = _SelSemester;
                row["試別名稱"] = _SelExamName;

                if (ParentRecordDict.ContainsKey(StudRec.ID))
                {
                    row["監護人姓名"] = ParentRecordDict[StudRec.ID].CustodianName;
                    row["父親姓名"] = ParentRecordDict[StudRec.ID].FatherName;
                    row["母親姓名"] = ParentRecordDict[StudRec.ID].MotherName;
                }
                if (AddressRecordDict.ContainsKey(StudRec.ID))
                {
                    row["戶籍地址"] = AddressRecordDict[StudRec.ID].PermanentAddress;
                    row["聯絡地址"] = AddressRecordDict[StudRec.ID].MailingAddress;
                    row["其他地址"] = AddressRecordDict[StudRec.ID].Address1Address;
                }

                //if (ClassDict.ContainsKey(StudRec.RefClassID))
                //{
                //    row["班級"] = ClassDict[StudRec.RefClassID].Name;

                //    if (TeacherDict.ContainsKey(ClassDict[StudRec.RefClassID].RefTeacherID))
                //    {
                //        row["班導師"] = TeacherDict[ClassDict[StudRec.RefClassID].RefTeacherID].Name;
                //    }
                //}
                //else
                //{
                //    row["班級"] = "";
                //    row["班導師"] = "";
                //}

                row["學號"] = StudRec.StudentNumber;
                //if (StudRec.SeatNo.HasValue)
                //    row["座號"] = StudRec.SeatNo.Value.ToString();
                //else
                //    row["座號"] = "";

                // 預設先使用目前班級、座號、班導師 StudCSTDict
                if (StudCSTDict.ContainsKey(StudRec.ID))
                {
                    row["班級"] = StudCSTDict[StudRec.ID].ClassName;
                    row["座號"] = StudCSTDict[StudRec.ID].SeatNo;
                    row["班導師"] = StudCSTDict[StudRec.ID].TeacherName;
                }

                // 使用學期歷程內的班級、座號、班導師
                if (StudShiDict.ContainsKey(StudRec.ID))
                {
                    row["班級"] = StudShiDict[StudRec.ID].ClassName;
                    row["座號"] = StudShiDict[StudRec.ID].SeatNo;
                    row["班導師"] = StudShiDict[StudRec.ID].Teacher;
                }

                row["姓名"] = StudRec.Name;


                // 傳入 ID當 Key
                // row["缺曠紀錄"] = StudRec.ID;
                rowT["缺曠紀錄"] = StudRec.ID;
                // 獎懲區間統計值
                if (DisciplineCountDict.ContainsKey(StudRec.ID))
                {
                    foreach (string str in Global.GetDisciplineNameList())
                    {
                        string key = str + "區間統計";
                        if (DisciplineCountDict[StudRec.ID].ContainsKey(str))
                            row[key] = DisciplineCountDict[StudRec.ID][str];
                    }
                }

                // 處理成績套印
                if (studExamScoreDict.ContainsKey(StudRec.ID))
                {
                    // 科目
                    int subj = 1;
                    Dictionary<string, int> dNameDict = new Dictionary<string, int>();
                    foreach (string name in Global.DomainNameList)
                        dNameDict.Add(name, 1);

                    foreach (DAO.ExamSubjectScore ess in studExamScoreDict[StudRec.ID]._ExamSubjectScoreDict.Values)
                    {
                        string ddname = ess.DomainName;
                        if (ddname == "")
                            ddname = "彈性課程";

                        // 檢查不在固定領域內跳過不處理
                        if (!Global.DomainNameList.Contains(ddname))
                        {
                            string errMsg = ddname + "領域相關成績，無法處理。";
                            if (!_ErrorDomainNameList.Contains(errMsg))
                                _ErrorDomainNameList.Add(errMsg);

                            continue;
                        }
                        if (dNameDict.ContainsKey(ddname))
                        {
                            subj = dNameDict[ddname];
                            dNameDict[ddname]++;
                        }
                        foreach (string item in subjLi)
                        {
                            string key = ddname + "_" + item + subj;

                            switch (item)
                            {
                                case "科目名稱":
                                    row[key] = ess.SubjectName;
                                    if (!_TemplateSubjectNameList.Contains(key))
                                    {
                                        if (!_ErrorList.Contains(key))
                                            _ErrorList.Add(key);
                                    }
                                    break;
                                case "科目權數":
                                    if (ess.Credit.HasValue)
                                        row[key] = ess.Credit.Value;
                                    break;
                                case "科目努力程度":
                                    if (ess.Effort.HasValue)
                                        row[key] = _EffortMapper.GetTextByCode(ess.Effort.Value);
                                    break;
                                case "科目評量分數":
                                    if (ess.Score.HasValue)
                                        row[key] = ess.Score.Value;
                                    break;
                                case "科目平時成績":
                                    if (ess.AssignmentScore.HasValue)
                                        row[key] = ess.AssignmentScore.Value;
                                    break;
                                case "科目文字描述":
                                    row[key] = ess.Text;
                                    break;
                            }
                        }
                    }

                    // 領域
                    foreach (DAO.ExamDomainScore eds in studExamScoreDict[StudRec.ID]._ExamDomainScoreDict.Values)
                    {
                        // 檢查不再固定領域內跳過
                        if (!Global.DomainNameList.Contains(eds.DomainName))
                        {
                            string errMsg = eds.DomainName + "領域相關成績，無法處理。";
                            if (!_ErrorDomainNameList.Contains(errMsg))
                                _ErrorDomainNameList.Add(errMsg);
                            continue;
                        }
                        string key = eds.DomainName + "_領域加權平均";
                        string keyc = eds.DomainName + "_領域權數";
                        string keya = eds.DomainName + "_領域平時加權平均";
                        string keye = eds.DomainName + "_領域努力程度";

                        //評量分數
                        if (eds.Score.HasValue)
                        {
                            row[key] = eds.Score.Value;
                            row[keye] = _EffortMapper.GetTextByScore(eds.Score.Value);
                        }
                        // 權數
                        if (eds.Credit.HasValue)
                            row[keyc] = eds.Credit.Value;
                        // 平時
                        if (eds.AssignmentScore.HasValue)
                            row[keya] = eds.AssignmentScore.Value;

                    }
                }

                if (studExamScoreDict.ContainsKey(StudRec.ID))
                {
                    if (studExamScoreDict[StudRec.ID].GetDomainScoreA().HasValue)
                        row["領域成績加權平均"] = studExamScoreDict[StudRec.ID].GetDomainScoreA().Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreAA().HasValue)
                        row["科目平時成績加權平均"] = studExamScoreDict[StudRec.ID].GetSubjectScoreAA().Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreAF().HasValue)
                        row["科目評量分數加權平均"] = studExamScoreDict[StudRec.ID].GetSubjectScoreAF().Value;

                }

                // 處理領域組距相關
                // 班級
                string kClassKey = "";

                List<DAO.DomainRangeCount.DomainRangeType> dtypeList = new List<DAO.DomainRangeCount.DomainRangeType>();
                dtypeList.Add(DAO.DomainRangeCount.DomainRangeType.R100_u);
                dtypeList.Add(DAO.DomainRangeCount.DomainRangeType.R90_99);
                dtypeList.Add(DAO.DomainRangeCount.DomainRangeType.R80_89);
                dtypeList.Add(DAO.DomainRangeCount.DomainRangeType.R70_79);
                dtypeList.Add(DAO.DomainRangeCount.DomainRangeType.R60_69);
                dtypeList.Add(DAO.DomainRangeCount.DomainRangeType.R50_59);
                dtypeList.Add(DAO.DomainRangeCount.DomainRangeType.R40_49);
                dtypeList.Add(DAO.DomainRangeCount.DomainRangeType.R30_39);
                dtypeList.Add(DAO.DomainRangeCount.DomainRangeType.R20_29);
                dtypeList.Add(DAO.DomainRangeCount.DomainRangeType.R10_19);
                dtypeList.Add(DAO.DomainRangeCount.DomainRangeType.R0_9);

                if (dmRClasssDict.ContainsKey(StudRec.RefClassID))
                {
                    foreach (KeyValuePair<string, DAO.DomainRangeCount> data in dmRClasssDict[StudRec.RefClassID])
                    {
                        foreach (DAO.DomainRangeCount.DomainRangeType dtType in dtypeList)
                        {
                            kClassKey = "班級_" + data.Key + "_" + dtType.ToString();
                            row[kClassKey] = data.Value.GetRankCount(dtType);
                        }
                    }
                }

                // 年級
                int grY = 0;
                if (ClassDict.ContainsKey(StudRec.RefClassID))
                    if (ClassDict[StudRec.RefClassID].GradeYear.HasValue)
                        grY = ClassDict[StudRec.RefClassID].GradeYear.Value;

                string kGradeKey = "";
                if (dmGradeDict.ContainsKey(grY))
                {
                    foreach (KeyValuePair<string, DAO.DomainRangeCount> data in dmGradeDict[grY])
                    {
                        foreach (DAO.DomainRangeCount.DomainRangeType dtType in dtypeList)
                        {
                            kGradeKey = "年級_" + data.Key + "_" + dtType.ToString();
                            row[kGradeKey] = data.Value.GetRankCount(dtType);
                        }
                    }
                }

                // 處理科目組距相關
                // 班級
                string sClassKey = "";

                List<DAO.SubjectRangeCount.SubjectRangeType> stypeList = new List<DAO.SubjectRangeCount.SubjectRangeType>();
                stypeList.Add(DAO.SubjectRangeCount.SubjectRangeType.R100_u);
                stypeList.Add(DAO.SubjectRangeCount.SubjectRangeType.R90_99);
                stypeList.Add(DAO.SubjectRangeCount.SubjectRangeType.R80_89);
                stypeList.Add(DAO.SubjectRangeCount.SubjectRangeType.R70_79);
                stypeList.Add(DAO.SubjectRangeCount.SubjectRangeType.R60_69);
                stypeList.Add(DAO.SubjectRangeCount.SubjectRangeType.R50_59);
                stypeList.Add(DAO.SubjectRangeCount.SubjectRangeType.R40_49);
                stypeList.Add(DAO.SubjectRangeCount.SubjectRangeType.R30_39);
                stypeList.Add(DAO.SubjectRangeCount.SubjectRangeType.R20_29);
                stypeList.Add(DAO.SubjectRangeCount.SubjectRangeType.R10_19);
                stypeList.Add(DAO.SubjectRangeCount.SubjectRangeType.R0_9);

                if (subjRClasssDict.ContainsKey(StudRec.RefClassID))
                {
                    foreach (KeyValuePair<string, DAO.SubjectRangeCount> data in subjRClasssDict[StudRec.RefClassID])
                    {
                        string ssKey = "s班級_科目名稱" + data.Key;
                        if (colSubjMapDict.ContainsKey(ssKey))
                            row[colSubjMapDict[ssKey]] = data.Key;

                        foreach (DAO.SubjectRangeCount.SubjectRangeType dtType in stypeList)
                        {
                            sClassKey = "s班級_" + data.Key + "_" + dtType.ToString();
                            if (colSubjMapDict.ContainsKey(sClassKey))
                            {
                                //   row[sClassKey] = data.Value.GetRankCount(dtType);
                                row[colSubjMapDict[sClassKey]] = data.Value.GetRankCount(dtType);
                            }
                        }
                    }
                }

                // 年級
                int sgrY = 0;
                if (ClassDict.ContainsKey(StudRec.RefClassID))
                    if (ClassDict[StudRec.RefClassID].GradeYear.HasValue)
                        sgrY = ClassDict[StudRec.RefClassID].GradeYear.Value;

                string sGradeKey = "";
                if (subjGradeDict.ContainsKey(sgrY))
                {
                    foreach (KeyValuePair<string, DAO.SubjectRangeCount> data in subjGradeDict[sgrY])
                    {
                        string ssKey = "s年級_科目名稱" + data.Key;
                        if (colSubjMapDict.ContainsKey(ssKey))
                            row[colSubjMapDict[ssKey]] = data.Key;


                        foreach (DAO.SubjectRangeCount.SubjectRangeType dtType in stypeList)
                        {
                            sGradeKey = "s年級_" + data.Key + "_" + dtType.ToString();
                            if (colSubjMapDict.ContainsKey(sGradeKey))
                            {
                                //   row[sGradeKey] = data.Value.GetRankCount(dtType);
                                row[colSubjMapDict[sGradeKey]] = data.Value.GetRankCount(dtType);
                            }
                        }
                    }
                }

                row["服務學習時數"] = "";
                if (ServiceLearningDict.ContainsKey(StudRec.ID))
                    row["服務學習時數"] = ServiceLearningDict[StudRec.ID];

                row["校長"] = ChancellorChineseName;
                row["教務主任"] = EduDirectorName;
                row["區間開始日期"] = _BeginDate.ToShortDateString();
                row["區間結束日期"] = _EndDate.ToShortDateString();
                row["成績校正日期"] = _ScoreEditDate;

                dt.Rows.Add(row);
                dtAtt.Rows.Add(rowT);




                // 處理固定欄位對應
                Document doc1 = new Document();
                doc1.Sections.Clear();

                // 處理動態處理(缺曠)
                Document docAtt = new Document();
                docAtt.Sections.Clear();
                docAtt.Sections.Add(docAtt.ImportNode(docTemplate.Sections[0], true));

                _builder = new DocumentBuilder(docAtt);
                //docAtt.MailMerge.MergeField += new Aspose.Words.Reporting.MergeFieldEventHandler(MailMerge_MergeField);
                docAtt.MailMerge.FieldMergingCallback = this;
                docAtt.MailMerge.Execute(dtAtt);

                doc1.Sections.Add(doc1.ImportNode(docAtt.Sections[0], true));
                doc1.MailMerge.Execute(dt);
                doc1.MailMerge.RemoveEmptyParagraphs = true;
                doc1.MailMerge.DeleteFields();
                docList.Add(doc1);

            }

            _bgWorkReport.ReportProgress(90);
            //// debug            
            //string ssStr = Application.StartupPath + "\\dt_debug.xml";
            //dt.WriteXml(ssStr);


            #endregion

            #region Word 合併列印

            doc.Sections.Clear();
            foreach (Document doc1 in docList)
                doc.Sections.Add(doc.ImportNode(doc1.Sections[0], true));

            string reportNameW = "高雄評量成績單";
            pathW = Path.Combine(System.Windows.Forms.Application.StartupPath + "\\Reports", "");
            if (!Directory.Exists(pathW))
                Directory.CreateDirectory(pathW);
            pathW = Path.Combine(pathW, reportNameW + ".doc");

            if (File.Exists(pathW))
            {
                int i = 1;
                while (true)
                {
                    string newPathW = Path.GetDirectoryName(pathW) + "\\" + Path.GetFileNameWithoutExtension(pathW) + (i++) + Path.GetExtension(pathW);
                    if (!File.Exists(newPathW))
                    {
                        pathW = newPathW;
                        break;
                    }
                }
            }

            try
            {
                doc.Save(pathW, Aspose.Words.SaveFormat.Doc);

            }
            catch (Exception exow)
            {

            }
            doc = null;
            docList.Clear();

            GC.Collect();
            #endregion
            _bgWorkReport.ReportProgress(100);
        }

        //void MailMerge_MergeField(object sender, Aspose.Words.Reporting.MergeFieldEventArgs e)
        //{
        //    if (e.FieldName == "缺曠紀錄")
        //    {
        //        if (_builder.MoveToMergeField(e.FieldName))
        //        {
        //            string sid = e.FieldValue.ToString();

        //            Dictionary<string, int> dataDict = new Dictionary<string, int>();                   

        //                List<string> colNameList = new List<string>();
        //                if (_AttendanceDict.ContainsKey(sid))
        //                    dataDict = _AttendanceDict[sid];
        //                //dataDict.Keys

        //                foreach (string name in _SelAttendanceList)
        //                    colNameList.Add(name.Replace("_",""));

        //                //colNameList.Sort();
        //                int colCount=colNameList.Count;

        //                if (colCount > 0)
        //                {
        //                    Cell cell = _builder.CurrentParagraph.ParentNode as Cell;
        //                    cell.CellFormat.LeftPadding = 0;
        //                    cell.CellFormat.RightPadding = 0;
        //                    double width = cell.CellFormat.Width;
        //                    int columnCount = colCount;
        //                    double miniUnitWitdh = width / (double)columnCount;

        //                    Table table = _builder.StartTable();

        //                    //(table.ParentNode.ParentNode as Row).RowFormat.LeftIndent = 0;
        //                    double p = _builder.RowFormat.LeftIndent;
        //                    _builder.RowFormat.HeightRule = HeightRule.Exactly;
        //                    _builder.RowFormat.Height = 18.0;
        //                    _builder.RowFormat.LeftIndent = 0;

        //                    // 缺曠名稱
        //                    foreach (string name in colNameList)
        //                    {
        //                        Cell c1 = _builder.InsertCell();
        //                        c1.CellFormat.Width = miniUnitWitdh;
        //                        c1.CellFormat.WrapText = true;
        //                        _builder.Write(name);                            
        //                    }
        //                    _builder.EndRow();

        //                    // 缺曠統計
        //                    foreach (string name in colNameList)
        //                    {
        //                        Cell c1 = _builder.InsertCell();
        //                        c1.CellFormat.Width = miniUnitWitdh;
        //                        c1.CellFormat.WrapText = true;
        //                        if (dataDict.ContainsKey(name)) 
        //                            _builder.Write(dataDict[name].ToString()); 
        //                        else
        //                            _builder.Write("");
        //                    }
        //                    _builder.EndRow();                            

        //                    _builder.EndTable();

        //                    //去除表格四邊的線
        //                    foreach (Cell c in table.FirstRow.Cells)
        //                        c.CellFormat.Borders.Top.LineStyle = LineStyle.None;

        //                    foreach (Cell c in table.LastRow.Cells)
        //                        c.CellFormat.Borders.Bottom.LineStyle = LineStyle.None;

        //                    foreach (Row r in table.Rows)
        //                    {
        //                        r.FirstCell.CellFormat.Borders.Left.LineStyle = LineStyle.None;
        //                        r.LastCell.CellFormat.Borders.Right.LineStyle = LineStyle.None;
        //                    }

        //                    _builder.RowFormat.LeftIndent = p;
        //                }

        //        }
        //    }            
        //}

        private void PrintForm_Load(object sender, EventArgs e)
        {
            DisSelect();
            _SelSchoolYear = int.Parse(K12.Data.School.DefaultSchoolYear);
            _SelSemester = int.Parse(K12.Data.School.DefaultSemester);

            bkw.RunWorkerAsync();
        }

        private void LoadSubject()
        {
            lvSubject.Items.Clear();
            string ExamID = "";
            foreach (ExamRecord ex in _exams)
            {
                if (ex.Name == cboExam.Text)
                {
                    ExamID = ex.ID;
                    break;
                }
            }

            if (_ExamSubjectFull.ContainsKey(ExamID))
            {
                foreach (string subjName in _ExamSubjectFull[ExamID])
                    lvSubject.Items.Add(subjName);
            }

        }


        private void lnkCopyConfig_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_Configure == null) return;
            CloneConfigure dialog = new CloneConfigure() { ParentName = _Configure.Name };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Configure conf = new Configure();
                conf.Name = dialog.NewConfigureName;
                conf.ExamRecord = _Configure.ExamRecord;
                conf.PrintSubjectList.AddRange(_Configure.PrintSubjectList);
                conf.SchoolYear = _Configure.SchoolYear;
                conf.Semester = _Configure.Semester;
                conf.SubjectLimit = _Configure.SubjectLimit;
                conf.Template = _Configure.Template;
                conf.BeginDate = _Configure.BeginDate;
                conf.EndDate = _Configure.EndDate;
                conf.ScoreEditDate = _Configure.ScoreEditDate;
                if (conf.PrintAttendanceList == null)
                    conf.PrintAttendanceList = new List<string>();
                conf.PrintAttendanceList.AddRange(_Configure.PrintAttendanceList);
                conf.Encode();
                conf.Save();
                _ConfigureList.Add(conf);
                cboConfigure.Items.Insert(cboConfigure.Items.Count - 1, conf);
                cboConfigure.SelectedIndex = cboConfigure.Items.Count - 2;
            }
        }

        public Configure _Configure { get; private set; }

        private void lnkDelConfig_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_Configure == null) return;

            // 檢查是否是預設設定檔名稱，如果是無法刪除
            if (Global.DefaultConfigNameList().Contains(_Configure.Name))
            {
                FISCA.Presentation.Controls.MsgBox.Show("系統預設設定檔案無法刪除");
                return;
            }

            if (MessageBox.Show("樣板刪除後將無法回復，確定刪除樣板?", "刪除樣板", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK)
            {
                _ConfigureList.Remove(_Configure);
                if (_Configure.UID != "")
                {
                    _Configure.Deleted = true;
                    _Configure.Save();
                }
                var conf = _Configure;
                cboConfigure.SelectedIndex = -1;
                cboConfigure.Items.Remove(conf);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 停用可選功能
        private void DisSelect()
        {
            cboConfigure.Enabled = false;
            cboExam.Enabled = false;
            cboSchoolYear.Enabled = false;
            cboSemester.Enabled = false;
            btnSaveConfig.Enabled = false;
            btnPrint.Enabled = false;
        }

        // 啟用可選功能
        private void EnbSelect()
        {
            cboConfigure.Enabled = true;
            cboExam.Enabled = true;
            cboSchoolYear.Enabled = true;
            cboSemester.Enabled = true;
            btnSaveConfig.Enabled = true;
            btnPrint.Enabled = true;
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (dtBegin.IsEmpty || dtEnd.IsEmpty)
            {
                FISCA.Presentation.Controls.MsgBox.Show("日期區間必須輸入!");
                return;
            }

            if (dtBegin.Value > dtEnd.Value)
            {
                FISCA.Presentation.Controls.MsgBox.Show("開始日期必須小於或等於結束日期!!");
                return;
            }

            int sc, ss;
            if (int.TryParse(cboSchoolYear.Text, out sc))
            {
                _SelSchoolYear = sc;
            }
            else
            {
                FISCA.Presentation.Controls.MsgBox.Show("學年度必填!");
                return;
            }

            if (int.TryParse(cboSemester.Text, out ss))
            {
                _SelSemester = ss;
            }
            else
            {
                FISCA.Presentation.Controls.MsgBox.Show("學期必填!");
                return;
            }

            if (string.IsNullOrEmpty(cboExam.Text))
            {
                FISCA.Presentation.Controls.MsgBox.Show("請選擇試別!");
                return;
            }
            else
            {
                bool isEr = true;
                foreach (ExamRecord ex in _exams)
                    if (ex.Name == cboExam.Text)
                    {
                        _SelExamID = ex.ID;
                        _SelExamName = ex.Name;
                        isEr = false;
                        break;
                    }

                if (isEr)
                {
                    FISCA.Presentation.Controls.MsgBox.Show("試別錯誤，請重新選擇!");
                    return;
                }
            }
            _SelNotRankedFilter = cboNotRankedFilter.Text;
            _SelSubjNameList.Clear();

            SaveTemplate(null, null);

            // 使用者勾選科目
            foreach (string name in _Configure.PrintSubjectList)
                _SelSubjNameList.Add(name);

            // 缺曠
            foreach (string name in _Configure.PrintAttendanceList)
                _SelAttendanceList.Add(name);


            _BeginDate = dtBegin.Value;
            _EndDate = dtEnd.Value;

            if (dtScoreEdit.IsEmpty)
                _ScoreEditDate = "";
            else
                _ScoreEditDate = dtScoreEdit.Value.ToShortDateString();

            btnSaveConfig.Enabled = false;
            btnSaveConfig.Enabled = false;

            SetDomainList();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            // 執行報表
            _bgWorkReport.RunWorkerAsync();
        }

        private void SetDomainList()
        {
            Global._SelSchoolYear = _SelSchoolYear;
            Global._SelSemester = _SelSemester;
            Global._SelStudentIDList = _StudentIDList;
            Global._SelExamID = _SelExamID;
            Global.SetDomainList();
        }

        // 儲存樣板
        private void SaveTemplate(object sender, EventArgs e)
        {
            if (_Configure == null) return;
            _Configure.SchoolYear = cboSchoolYear.Text;
            _Configure.Semester = cboSemester.Text;
            _Configure.SelSetConfigName = cboConfigure.Text;
            _Configure.NotRankedTagNameFilter = _SelNotRankedFilter;
            foreach (ExamRecord exm in _exams)
            {
                if (exm.Name == cboExam.Text)
                {
                    _Configure.ExamRecord = exm;
                    break;
                }
            }

            // 科目
            foreach (ListViewItem item in lvSubject.Items)
            {
                if (item.Checked)
                {
                    if (!_Configure.PrintSubjectList.Contains(item.Text))
                        _Configure.PrintSubjectList.Add(item.Text);
                }
                else
                {
                    if (_Configure.PrintSubjectList.Contains(item.Text))
                        _Configure.PrintSubjectList.Remove(item.Text);
                }
            }

            if (_Configure.PrintAttendanceList == null)
                _Configure.PrintAttendanceList = new List<string>();

            _Configure.PrintAttendanceList.Clear();
            // 儲存缺曠選項
            foreach (DataGridViewRow drv in dgAttendanceData.Rows)
            {
                foreach (DataGridViewCell cell in drv.Cells)
                {
                    bool bl;
                    if (bool.TryParse(cell.Value.ToString(), out bl))
                    {
                        if (bl)
                            _Configure.PrintAttendanceList.Add(cell.Tag.ToString());
                    }
                }
            }


            foreach (ListViewItem item in lvSubject.Items)
            {
                if (item.Checked)
                {
                    if (!_Configure.PrintSubjectList.Contains(item.Text))
                        _Configure.PrintSubjectList.Add(item.Text);
                }
                else
                {
                    if (_Configure.PrintSubjectList.Contains(item.Text))
                        _Configure.PrintSubjectList.Remove(item.Text);
                }
            }



            // 儲存開始與結束日期
            _Configure.BeginDate = dtBegin.Value.ToShortDateString();
            _Configure.EndDate = dtEnd.Value.ToShortDateString();
            if (dtScoreEdit.IsEmpty)
                _Configure.ScoreEditDate = "";
            else
                _Configure.ScoreEditDate = dtScoreEdit.Value.ToShortDateString();

            _Configure.Encode();
            _Configure.Save();

            #region 樣板設定檔記錄用

            // 記錄使用這選的專案            
            List<DAO.UDT_ScoreConfig> uList = new List<DAO.UDT_ScoreConfig>();
            foreach (DAO.UDT_ScoreConfig conf in _UDTConfigList)
                if (conf.Type == Global._UserConfTypeName)
                {
                    conf.Name = cboConfigure.Text;
                    uList.Add(conf);
                    break;
                }

            if (uList.Count > 0)
            {
                DAO.UDTTransfer.UpdateConfigData(uList);
            }
            else
            {
                // 新增
                List<DAO.UDT_ScoreConfig> iList = new List<DAO.UDT_ScoreConfig>();
                DAO.UDT_ScoreConfig conf = new DAO.UDT_ScoreConfig();
                conf.Name = cboConfigure.Text;
                conf.ProjectName = Global._ProjectName;
                conf.Type = Global._UserConfTypeName;
                conf.UDTTableName = Global._UDTTableName;
                iList.Add(conf);
                DAO.UDTTransfer.InsertConfigData(iList);
            }
            #endregion
        }

        private void cboSchoolYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisSelect();
            LoadExamSubject();
            LoadSubject();
            EnbSelect();
        }

        private void cboSemester_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisSelect();
            LoadExamSubject();
            LoadSubject();
            EnbSelect();
        }

        private void cboExam_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSubject();
        }

        private void cboConfigure_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboConfigure.SelectedIndex == cboConfigure.Items.Count - 1)
            {
                //新增
                btnSaveConfig.Enabled = btnPrint.Enabled = false;
                NewConfigure dialog = new NewConfigure();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    _Configure = new Configure();
                    _Configure.Name = dialog.ConfigName;
                    _Configure.Template = dialog.Template;
                    _Configure.SubjectLimit = dialog.SubjectLimit;
                    _Configure.SchoolYear = _DefalutSchoolYear;
                    _Configure.Semester = _DefaultSemester;
                    if (_Configure.PrintAttendanceList == null)
                        _Configure.PrintAttendanceList = new List<string>();
                    if (_Configure.PrintSubjectList == null)
                        _Configure.PrintSubjectList = new List<string>();

                    if (cboExam.Items.Count > 0)
                    {
                        string exName = cboExam.Items[0].ToString();
                        foreach (ExamRecord rec in _exams)
                        {
                            if (exName == rec.Name)
                            {
                                _Configure.ExamRecord = rec;
                                break;
                            }
                        }

                    }
                    _ConfigureList.Add(_Configure);
                    cboConfigure.Items.Insert(cboConfigure.SelectedIndex, _Configure);
                    cboConfigure.SelectedIndex = cboConfigure.SelectedIndex - 1;
                    _Configure.Encode();
                    _Configure.Save();
                }
                else
                {
                    cboConfigure.SelectedIndex = -1;
                }
            }
            else
            {
                if (cboConfigure.SelectedIndex >= 0)
                {
                    btnSaveConfig.Enabled = btnPrint.Enabled = true;
                    _Configure = _ConfigureList[cboConfigure.SelectedIndex];
                    if (_Configure.Template == null)
                        _Configure.Decode();
                    if (!cboSchoolYear.Items.Contains(_Configure.SchoolYear))
                        cboSchoolYear.Items.Add(_Configure.SchoolYear);
                    cboSchoolYear.Text = _Configure.SchoolYear;
                    cboSemester.Text = _Configure.Semester;
                    if (_Configure.ExamRecord != null)
                    {
                        int idx = 0;
                        foreach (string sitm in cboExam.Items)
                        {
                            if (sitm == _Configure.ExamRecord.Name)
                            {
                                cboExam.SelectedIndex = idx;
                                break;
                            }
                            idx++;
                        }

                    }

                    // 解析科目
                    foreach (ListViewItem lvi in lvSubject.Items)
                    {
                        if (_Configure.PrintSubjectList.Contains(lvi.Text))
                        {
                            lvi.Checked = true;
                        }
                    }

                    // 解析缺曠
                    foreach (DataGridViewRow drv in dgAttendanceData.Rows)
                    {
                        foreach (DataGridViewCell cell in drv.Cells)
                        {
                            if (cell.Tag == null)
                                continue;

                            string key = cell.Tag.ToString();
                            cell.Value = false;
                            if (_Configure.PrintAttendanceList.Contains(key))
                            {
                                cell.Value = true;
                            }
                        }
                    }


                    // 開始與結束日期
                    DateTime dtb, dte, dtee;
                    if (DateTime.TryParse(_Configure.BeginDate, out dtb))
                        dtBegin.Value = dtb;
                    else
                        dtBegin.Value = DateTime.Now;

                    if (DateTime.TryParse(_Configure.EndDate, out dte))
                        dtEnd.Value = dte;
                    else
                        dtEnd.Value = DateTime.Now;

                    // 成績校正日期
                    if (DateTime.TryParse(_Configure.ScoreEditDate, out dtee))
                        dtScoreEdit.Value = dtee;
                    else
                        dtScoreEdit.IsEmpty = true;

                }
                else
                {
                    _Configure = null;
                    cboSchoolYear.SelectedIndex = -1;
                    cboSemester.SelectedIndex = -1;
                    cboExam.SelectedIndex = -1;

                    // 開始與結束日期沒有預設值時給當天
                    dtBegin.Value = dtEnd.Value = DateTime.Now;
                }
            }
        }

        private void lnkViewTemplate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // 當沒有設定檔
            if (_Configure == null) return;

            lnkViewTemplate.Enabled = false;
            #region 儲存檔案

            string reportName = "高雄評量成績單樣板(" + _Configure.Name + ").doc";

            string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".doc");

            if (File.Exists(path))
            {
                int i = 1;
                while (true)
                {
                    string newPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + (i++) + Path.GetExtension(path);
                    if (!File.Exists(newPath))
                    {
                        path = newPath;
                        break;
                    }
                }
            }

            try
            {
                System.IO.FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                _Configure.Template.Save(stream, Aspose.Words.SaveFormat.Doc);

                stream.Flush();
                stream.Close();
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                System.Windows.Forms.SaveFileDialog sd = new System.Windows.Forms.SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".doc";
                sd.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
                if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        System.IO.FileStream stream = new FileStream(sd.FileName, FileMode.Create, FileAccess.Write);
                        stream.Write(Properties.Resources.高雄評量成績通知單樣板, 0, Properties.Resources.高雄評量成績通知單樣板.Length);
                        stream.Flush();
                        stream.Close();

                    }
                    catch
                    {
                        FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            lnkViewTemplate.Enabled = true;
            #endregion
        }

        private void lnkChangeTemplate_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            if (_Configure == null) return;
            lnkChangeTemplate.Enabled = false;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "上傳樣板";
            dialog.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    _Configure.Template = new Aspose.Words.Document(dialog.FileName);
                    List<string> fields = new List<string>(_Configure.Template.MailMerge.GetFieldNames());
                    _Configure.SubjectLimit = 0;
                    while (fields.Contains("科目名稱" + (_Configure.SubjectLimit + 1)))
                    {
                        _Configure.SubjectLimit++;
                    }

                }
                catch
                {
                    MessageBox.Show("樣板開啟失敗");
                }
            }
            lnkChangeTemplate.Enabled = true;
        }

        private void lnkViewMapColumns_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            lnkViewMapColumns.Enabled = false;

            int sc, ss;
            if (int.TryParse(cboSchoolYear.Text, out sc))
            {
                _SelSchoolYear = sc;
            }
            else
            {
                FISCA.Presentation.Controls.MsgBox.Show("學年度必填!");
                return;
            }

            if (int.TryParse(cboSemester.Text, out ss))
            {
                _SelSemester = ss;
            }
            else
            {
                FISCA.Presentation.Controls.MsgBox.Show("學期必填!");
                return;
            }

            if (string.IsNullOrEmpty(cboExam.Text))
            {
                FISCA.Presentation.Controls.MsgBox.Show("請選擇試別!");
                return;
            }
            else
            {
                bool isEr = true;
                foreach (ExamRecord ex in _exams)
                    if (ex.Name == cboExam.Text)
                    {
                        _SelExamID = ex.ID;
                        _SelExamName = ex.Name;
                        isEr = false;
                        break;
                    }

                if (isEr)
                {
                    FISCA.Presentation.Controls.MsgBox.Show("試別錯誤，請重新選擇!");
                    return;
                }
            }


            SetDomainList();

            Global.ExportMappingFieldWord();
            lnkViewMapColumns.Enabled = true;
        }

        private void chkSubjSelAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvSubject.Items)
            {
                lvi.Checked = chkSubjSelAll.Checked;
            }
        }

        private void chkAttendSelAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow drv in dgAttendanceData.Rows)
            {
                foreach (DataGridViewCell cell in drv.Cells)
                {
                    if (cell.ColumnIndex != 0)
                        cell.Value = chkAttendSelAll.Checked;
                }
            }
        }

        void IFieldMergingCallback.FieldMerging(FieldMergingArgs e)
        {
            if (e.FieldName == "缺曠紀錄")
            {
                e.Text = "";
                _builder.MoveToField(e.Field, false);
                string sid = e.FieldValue.ToString();

                Dictionary<string, int> dataDict = new Dictionary<string, int>();

                List<string> colNameList = new List<string>();
                if (_AttendanceDict.ContainsKey(sid))
                    dataDict = _AttendanceDict[sid];
                //dataDict.Keys

                foreach (string name in _SelAttendanceList)
                    colNameList.Add(name.Replace("_", ""));

                //colNameList.Sort();
                int colCount = colNameList.Count;

                if (colCount > 0)
                {
                    Cell cell = _builder.CurrentParagraph.ParentNode as Cell;
                    cell.CellFormat.LeftPadding = 0;
                    cell.CellFormat.RightPadding = 0;
                    double width = cell.CellFormat.Width;
                    int columnCount = colCount;
                    double miniUnitWitdh = width / (double)columnCount;

                    Table table = _builder.StartTable();

                    //(table.ParentNode.ParentNode as Row).RowFormat.LeftIndent = 0;
                    double p = _builder.RowFormat.LeftIndent;
                    _builder.RowFormat.HeightRule = HeightRule.Auto; // [ischoolkingdom] Vicky新增，個人評量成績單建議能增加可列印數量(高雄國中)，調整 缺曠紀錄表格外觀
                    _builder.RowFormat.Height = 34.0;
                    _builder.RowFormat.LeftIndent = 0;

                    // 缺曠名稱
                    foreach (string name in colNameList)
                    {
                        Cell c1 = _builder.InsertCell();
                        c1.CellFormat.Width = miniUnitWitdh;
                        c1.CellFormat.WrapText = true;
                        _builder.Write(name);
                    }
                    _builder.EndRow();

                    // 缺曠統計
                    foreach (string name in colNameList)
                    {
                        Cell c1 = _builder.InsertCell();
                        c1.CellFormat.Width = miniUnitWitdh;
                        c1.CellFormat.WrapText = true;
                        if (dataDict.ContainsKey(name))
                            _builder.Write(dataDict[name].ToString());
                        else
                            _builder.Write("");
                    }
                    _builder.EndRow();

                    _builder.EndTable();

                    //去除表格四邊的線
                    foreach (Cell c in table.FirstRow.Cells)
                        c.CellFormat.Borders.Top.LineStyle = LineStyle.None;

                    foreach (Cell c in table.LastRow.Cells)
                        c.CellFormat.Borders.Bottom.LineStyle = LineStyle.None;

                    foreach (Row r in table.Rows)
                    {
                        r.FirstCell.CellFormat.Borders.Left.LineStyle = LineStyle.None;
                        r.LastCell.CellFormat.Borders.Right.LineStyle = LineStyle.None;
                    }
                    _builder.RowFormat.LeftIndent = p;
                }
            }
        }

        void IFieldMergingCallback.ImageFieldMerging(ImageFieldMergingArgs args)
        {
            //do nothing...
        }
    }

}

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
using HsinChu.JHEvaluation.Data;
using Aspose.Words;
using JHSchool.Evaluation.Calculation;
using Aspose.Words.Reporting;
using Aspose.Words.Tables;
using FISCA.Data;
using Campus.ePaperCloud;

namespace HsinChuExamScore_JH
{
    public partial class PrintForm : BaseForm, IFieldMergingCallback
    {
        private FISCA.UDT.AccessHelper _AccessHelper = new FISCA.UDT.AccessHelper();

        private Dictionary<string, List<string>> _ExamSubjects = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _ExamSubjectFull = new Dictionary<string, List<string>>();
        List<string> _StudentIDList;
        // 缺曠區間統計
        Dictionary<string, Dictionary<string, int>> _AttendanceDict = new Dictionary<string, Dictionary<string, int>>();

        //領域加權排名資料 字典，存放、最後合併列印使用 (studentID,<MergeFieldkey,RankValue>)
        Dictionary<string, Dictionary<string, string>> _DomainWeightRankDataDict = new Dictionary<string, Dictionary<string, string>>();

        //科目排名資料 字典，存放、最後合併列印使用 (studentID,<subjectName +Key,RankValue>)
        Dictionary<string, Dictionary<string, string>> _subjectRankDataDict = new Dictionary<string, Dictionary<string, string>>();

        //科目排名資料 字典，存放、最後合併列印使用 (studentID,<domainName +Key,RankValue>)
        Dictionary<string, Dictionary<string, string>> _domainRankDataDict = new Dictionary<string, Dictionary<string, string>>();


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
                    {
                        case "領域成績單":
                            cn.Template = new Document(new MemoryStream(Properties.Resources.新竹_領域成績單));
                            break;

                        case "科目成績單":
                            cn.Template = new Document(new MemoryStream(Properties.Resources.新竹_科目成績單));
                            break;

                        case "科目及領域成績單_領域組距":
                            cn.Template = new Document(new MemoryStream(Properties.Resources.新竹_科目及領域成績單_領域組距));
                            break;
                        case "科目及領域成績單_科目組距":
                            cn.Template = new Document(new MemoryStream(Properties.Resources.新竹_科目及領域成績單_科目組距));
                            break;
                    }

                    if (cn.Template == null)
                        cn.Template = new Document(new MemoryStream(Properties.Resources.新竹_評量成績通知單樣板));
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
                object[] objArray = (object[])e.Result;
                MemoryStream memoryStream = (MemoryStream)objArray[0];
                string reportNameW = "" + _SelSchoolYear + "學年度第" + _SelSemester + "學期" + _SelExamName + "" + objArray[1];
                ePaperCloud ePaperCloud = new ePaperCloud();
                ePaperCloud.upload_ePaper(_SelSchoolYear, _SelSemester, reportNameW, "", memoryStream, ePaperCloud.ViewerType.Student, ePaperCloud.FormatType.Docx);

                btnSaveConfig.Enabled = true;
                btnPrint.Enabled = true;

                if (_ErrorList.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    //sb.AppendLine("樣板內科目合併欄位不足，請新增：");
                    //sb.AppendLine(string.Join(",", _ErrorList.ToArray()));
                    sb.AppendLine("1.樣板內科目合併欄位不足，請檢查樣板。");
                    sb.AppendLine("2.如果使用只有領域樣板，請忽略此訊息。");
                    if (_ErrorDomainNameList.Count > 0)
                        sb.AppendLine(string.Join(",", _ErrorDomainNameList.ToArray()));

                    FISCA.Presentation.Controls.MsgBox.Show(sb.ToString(), "樣板內科目合併欄位不足", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }

                FISCA.Presentation.MotherForm.SetStatusBarMessage("評量成績報表產生完成");
            }
            catch (Exception ex)
            {
                FISCA.Presentation.Controls.MsgBox.Show("產生過程發生錯誤," + ex.Message);
            }
        }

        void _bgWorkReport_DoWork(object sender, DoWorkEventArgs e)
        {
            #region 資料讀取
            _bgWorkReport.ReportProgress(1);

            // 每次合併後放入，最後再合成一張
            Document docTemplate = _Configure.Template;
            if (docTemplate == null)
                docTemplate = new Document(new MemoryStream(Properties.Resources.新竹_評量成績通知單樣板));

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
            //// 教師
            //Dictionary<string, TeacherRecord> TeacherDict = new Dictionary<string, TeacherRecord>();
            //foreach (TeacherRecord tr in Teacher.SelectAll())
            //{
            //    if (tr.Status == TeacherRecord.TeacherStatus.一般)
            //        TeacherDict.Add(tr.ID, tr);
            //}

            // 不排名學生ID
            List<string> notRankStudIDList = new List<string>();
            if (_StudTagDict.ContainsKey(_SelNotRankedFilter))
                notRankStudIDList.AddRange(_StudTagDict[_SelNotRankedFilter].ToArray());

            // 移除所選學生不排名
            foreach (string id in notRankStudIDList)
                _StudentIDList.Remove(id);

            // 所選學生資料
            List<StudentRecord> StudRecList = Student.SelectByIDs(_StudentIDList);

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
                CourseDict.Add(co.ID, co);
            }

            // 取評量成績
            Dictionary<string, List<HC.JHSCETakeRecord>> Score1Dict = new Dictionary<string, List<HC.JHSCETakeRecord>>();
            foreach (JHSCETakeRecord record in JHSCETake.SelectByStudentAndCourse(studIDAllList, CourseDict.Keys.ToList()))
            {
                if (record.RefExamID == _SelExamID)
                {

                    if (!Score1Dict.ContainsKey(record.RefStudentID))
                        Score1Dict.Add(record.RefStudentID, new List<HC.JHSCETakeRecord>());

                    Score1Dict[record.RefStudentID].Add(new HC.JHSCETakeRecord(record));
                }
            }

            // 取得這次該修課程
            Dictionary<string, Dictionary<string, DAO.SubjectDomainName>> StudCourseDict = Utility.GetStudentSCAttendCourse(_StudentIDList, CourseDict.Keys.ToList(), _SelExamID);

            // 取得評量設定比例
            Dictionary<string, decimal> ScorePercentageHSDict = Utility.GetScorePercentageHS();

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

                    foreach (HC.JHSCETakeRecord rec in Score1Dict[studID])
                    {
                        if (rec.RefExamID == _SelExamID && CourseDict.ContainsKey(rec.RefCourseID))
                        {
                            JHCourseRecord cr = CourseDict[rec.RefCourseID];

                            string SubjecName = cr.Subject;

                            // 勾選科目
                            if (_SelSubjNameList.Contains(SubjecName))
                            {
                                if (!studExamScoreDict[studID]._ExamSubjectScoreDict.ContainsKey(SubjecName))
                                {
                                    DAO.ExamSubjectScore ess = new DAO.ExamSubjectScore();
                                    ess.DomainName = cr.Domain;
                                    ess.SubjectName = SubjecName;
                                    ess.ScoreA = rec.AssignmentScore;
                                    ess.ScoreF = rec.Score;

                                    if (ess.ScoreA.HasValue && ess.ScoreF.HasValue)
                                    {
                                        if (ScorePercentageHSDict.ContainsKey(cr.RefAssessmentSetupID))
                                        {
                                            // 取得定期，評量由100-定期
                                            decimal f = ScorePercentageHSDict[cr.RefAssessmentSetupID] * 0.01M;
                                            decimal a = (100 - ScorePercentageHSDict[cr.RefAssessmentSetupID]) * 0.01M;
                                            ess.ScoreT = ess.ScoreA.Value * a + ess.ScoreF.Value * f;
                                        }
                                        else
                                            ess.ScoreT = ess.ScoreA.Value * 0.5M + ess.ScoreF.Value * 0.5M; // 沒有設定預設50,50

                                        // 原本
                                        //ess.ScoreT = (ess.ScoreA.Value + ess.ScoreF.Value) / 2;
                                    }
                                    if (ess.ScoreA.HasValue && ess.ScoreF.HasValue == false)
                                        ess.ScoreT = ess.ScoreA.Value;

                                    if (ess.ScoreA.HasValue == false && ess.ScoreF.HasValue)
                                        ess.ScoreT = ess.ScoreF.Value;

                                    // 依照成績計算規則科目方式處理進位，只有總成績。
                                    // 平時
                                    //if(ess.ScoreA.HasValue)
                                    //    ess.ScoreA = studentCalculator.ParseSubjectScore(ess.ScoreA.Value);

                                    // 定期
                                    //if (ess.ScoreF.HasValue)
                                    //    ess.ScoreF = studentCalculator.ParseSubjectScore(ess.ScoreF.Value);

                                    if (ess.ScoreT.HasValue)
                                        ess.ScoreT = studentCalculator.ParseSubjectScore(ess.ScoreT.Value);

                                    ess.Text = rec.Text;
                                    ess.Credit = cr.Credit;
                                    studExamScoreDict[studID]._ExamSubjectScoreDict.Add(SubjecName, ess);
                                }
                            }
                        }
                    }
                    // 計算領域成績
                    studExamScoreDict[studID].CalcSubjectToDomain();
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
                        // 沒有勾選不加入
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

            }
            _bgWorkReport.ReportProgress(30);
            #region 領域成績區間處理
            // 處理領域成績區間
            // 班級
            Dictionary<string, Dictionary<string, DAO.DomainRangeCount>> dmRClasssDict = new Dictionary<string, Dictionary<string, DAO.DomainRangeCount>>();
            // 年級
            Dictionary<int, Dictionary<string, DAO.DomainRangeCount>> dmGradeDict = new Dictionary<int, Dictionary<string, DAO.DomainRangeCount>>();

            List<string> dmClassDNList = new List<string>();
            List<string> dmGradeDNList = new List<string>();

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

                        foreach (HC.JHSCETakeRecord rec in Score1Dict[studR.ID])
                        {
                            if (rec.RefExamID == _SelExamID && CourseDict.ContainsKey(rec.RefCourseID))
                            {
                                JHCourseRecord cr = CourseDict[rec.RefCourseID];

                                // 畫面有勾選才計算
                                if (_SelSubjNameList.Contains(cr.Subject))
                                {
                                    string dmName = cr.Domain;

                                    // 處理語文
                                    if (dmName == "語文")
                                    {
                                        if (cr.Subject.Contains("國"))
                                            dmName = "語文_國語文";

                                        if (cr.Subject.Contains("英"))
                                            dmName = "語文_英文";
                                    }

                                    if (!studAllExamScoreDict[studR.ID]._ExamSubjectScoreDict.ContainsKey(cr.Subject))
                                    {
                                        DAO.ExamSubjectScore ess = new DAO.ExamSubjectScore();
                                        ess.DomainName = dmName;
                                        ess.SubjectName = cr.Subject;
                                        ess.ScoreA = rec.AssignmentScore;
                                        ess.ScoreF = rec.Score;

                                        if (ess.ScoreA.HasValue && ess.ScoreF.HasValue)
                                            ess.ScoreT = (ess.ScoreA.Value + ess.ScoreF.Value) / 2;

                                        if (ess.ScoreA.HasValue && ess.ScoreF.HasValue == false)
                                            ess.ScoreT = ess.ScoreA.Value;

                                        if (ess.ScoreA.HasValue == false && ess.ScoreF.HasValue)
                                            ess.ScoreT = ess.ScoreF.Value;

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

            // 處理區間
            foreach (DAO.StudExamScore ses in studAllExamScoreDict.Values)
            {
                foreach (DAO.ExamDomainScore eds in ses._ExamDomainScoreDict.Values)
                {
                    string dmName = eds.DomainName;
                    string dmNameF = eds.DomainName + "F";
                    string dmNameA = eds.DomainName + "A";
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

                    // 班級F
                    if (!dmRClasssDict[ses.ClassID].ContainsKey(dmNameF))
                    {
                        DAO.DomainRangeCount drc = new DAO.DomainRangeCount();
                        drc.Name = dmNameF;
                        dmRClasssDict[ses.ClassID].Add(dmNameF, drc);
                    }

                    // 班級A
                    if (!dmRClasssDict[ses.ClassID].ContainsKey(dmNameA))
                    {
                        DAO.DomainRangeCount drc = new DAO.DomainRangeCount();
                        drc.Name = dmNameA;
                        dmRClasssDict[ses.ClassID].Add(dmNameA, drc);
                    }

                    // 年級
                    if (!dmGradeDict[ses.GradeYear].ContainsKey(dmName))
                    {
                        DAO.DomainRangeCount drc = new DAO.DomainRangeCount();
                        drc.Name = dmName;
                        dmGradeDict[ses.GradeYear].Add(dmName, drc);
                    }

                    // 年級F
                    if (!dmGradeDict[ses.GradeYear].ContainsKey(dmNameF))
                    {
                        DAO.DomainRangeCount drc = new DAO.DomainRangeCount();
                        drc.Name = dmNameF;
                        dmGradeDict[ses.GradeYear].Add(dmNameF, drc);
                    }

                    // 年級A
                    if (!dmGradeDict[ses.GradeYear].ContainsKey(dmNameA))
                    {
                        DAO.DomainRangeCount drc = new DAO.DomainRangeCount();
                        drc.Name = dmNameA;
                        dmGradeDict[ses.GradeYear].Add(dmNameA, drc);
                    }

                    // 放入成績(總成績)
                    dmRClasssDict[ses.ClassID][dmName].AddScore(eds.ScoreT);
                    dmGradeDict[ses.GradeYear][dmName].AddScore(eds.ScoreT);

                    // 放入成績F
                    dmRClasssDict[ses.ClassID][dmNameF].AddScore(eds.ScoreF);
                    dmGradeDict[ses.GradeYear][dmNameF].AddScore(eds.ScoreF);

                    // 放入成績A
                    dmRClasssDict[ses.ClassID][dmNameA].AddScore(eds.ScoreA);
                    dmGradeDict[ses.GradeYear][dmNameA].AddScore(eds.ScoreA);
                }
            }
            #endregion
            _bgWorkReport.ReportProgress(45);
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

                        foreach (HC.JHSCETakeRecord rec in Score1Dict[studR.ID])
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
                                        ess.ScoreA = rec.AssignmentScore;
                                        ess.ScoreF = rec.Score;

                                        if (ess.ScoreA.HasValue && ess.ScoreF.HasValue)
                                            ess.ScoreT = (ess.ScoreA.Value + ess.ScoreF.Value) / 2;

                                        if (ess.ScoreA.HasValue && ess.ScoreF.HasValue == false)
                                            ess.ScoreT = ess.ScoreA.Value;

                                        if (ess.ScoreA.HasValue == false && ess.ScoreF.HasValue)
                                            ess.ScoreT = ess.ScoreF.Value;

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
                    string subjNameF = ess.SubjectName + "F";
                    string subjNameA = ess.SubjectName + "A";
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

                    // 班級F
                    if (!subjRClasssDict[ses.ClassID].ContainsKey(subjNameF))
                    {
                        DAO.SubjectRangeCount src = new DAO.SubjectRangeCount();
                        src.Name = subjNameF;
                        subjRClasssDict[ses.ClassID].Add(subjNameF, src);
                    }

                    // 班級A
                    if (!subjRClasssDict[ses.ClassID].ContainsKey(subjNameA))
                    {
                        DAO.SubjectRangeCount src = new DAO.SubjectRangeCount();
                        src.Name = subjNameA;
                        subjRClasssDict[ses.ClassID].Add(subjNameA, src);
                    }

                    // 年級
                    if (!subjGradeDict[ses.GradeYear].ContainsKey(subjName))
                    {
                        DAO.SubjectRangeCount src = new DAO.SubjectRangeCount();
                        src.Name = subjName;
                        subjGradeDict[ses.GradeYear].Add(subjName, src);
                    }

                    // 年級F
                    if (!subjGradeDict[ses.GradeYear].ContainsKey(subjNameF))
                    {
                        DAO.SubjectRangeCount src = new DAO.SubjectRangeCount();
                        src.Name = subjNameF;
                        subjGradeDict[ses.GradeYear].Add(subjNameF, src);
                    }

                    // 年級A
                    if (!subjGradeDict[ses.GradeYear].ContainsKey(subjNameA))
                    {
                        DAO.SubjectRangeCount src = new DAO.SubjectRangeCount();
                        src.Name = subjNameA;
                        subjGradeDict[ses.GradeYear].Add(subjNameA, src);
                    }

                    // 放入成績(總成績)
                    subjRClasssDict[ses.ClassID][subjName].AddScore(ess.ScoreT);
                    subjGradeDict[ses.GradeYear][subjName].AddScore(ess.ScoreT);

                    // 放入成績F
                    subjRClasssDict[ses.ClassID][subjNameF].AddScore(ess.ScoreF);
                    subjGradeDict[ses.GradeYear][subjNameF].AddScore(ess.ScoreF);

                    // 放入成績A
                    subjRClasssDict[ses.ClassID][subjNameA].AddScore(ess.ScoreA);
                    subjGradeDict[ses.GradeYear][subjNameA].AddScore(ess.ScoreA);
                }
            }
            #endregion


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

            //抓取排名資料
            string sql = @"
SELECT 
	rank_matrix.id AS rank_matrix_id
	, rank_matrix.school_year
	, rank_matrix.semester
	, rank_matrix.grade_year
	, rank_matrix.item_type
	, rank_matrix.ref_exam_id
	, rank_matrix.item_name
	, rank_matrix.rank_type
	, rank_matrix.rank_name
	, class.class_name
	, student.seat_no
	, student.student_number
	, student.name
	, rank_detail.ref_student_id
	, rank_detail.rank
	, rank_detail.pr
	, rank_detail.percentile
    , rank_matrix.avg_top_25
    , rank_matrix.avg_top_50
    , rank_matrix.avg
    , rank_matrix.avg_bottom_50
    , rank_matrix.avg_bottom_25
FROM 
	rank_matrix
	LEFT OUTER JOIN rank_detail
		ON rank_detail.ref_matrix_id = rank_matrix.id
	LEFT OUTER JOIN student
		ON student.id = rank_detail.ref_student_id
	LEFT OUTER JOIN class
		ON class.id = student.ref_class_id
WHERE
	rank_matrix.is_alive = true
	AND rank_matrix.school_year = '" + _SelSchoolYear + @"'
    AND rank_matrix.semester = '" + _SelSemester + @"'
	AND rank_matrix.item_type like '定期評量%'
	AND rank_matrix.ref_exam_id = '" + _SelExamID + @"'
    AND ref_student_id IN ('" + string.Join("','", _StudentIDList) + @"') 
ORDER BY 
	rank_matrix.id
	, rank_detail.rank
	, class.grade_year
	, class.display_order
	, class.class_name
	, student.seat_no
	, student.id";

            QueryHelper qh = new QueryHelper();
            DataTable datatable = qh.Select(sql);

            foreach (DataRow dr in datatable.Rows)
            {
                if (!_DomainWeightRankDataDict.ContainsKey("" + dr["ref_student_id"]))
                {
                    _DomainWeightRankDataDict.Add("" + dr["ref_student_id"], new Dictionary<string, string>());

                    if ("" + dr["item_type"] == "定期評量/總計成績" && "" + dr["item_name"] == "加權平均")
                    {
                        string rankType = "" + dr["rank_type"];

                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "名次"))
                        {
                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "名次", "" + dr["rank"]);
                        }
                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "PR值"))
                        {
                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "PR值", "" + dr["pr"]);
                        }
                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "百分比"))
                        {
                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "百分比", "" + dr["percentile"]);
                        }
                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "母體平均"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "母體平均", "" + d);
                        }

                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "母體頂標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_top_25"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "母體頂標", "" + d);
                        }

                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "母體前標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_top_50"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "母體前標", "" + d);
                        }

                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "母體後標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_bottom_50"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "母體後標", "" + d);
                        }

                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "母體底標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_bottom_25"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "母體底標", "" + d);
                        }

                    }
                }
                else
                {
                    if ("" + dr["item_type"] == "定期評量/總計成績" && "" + dr["item_name"] == "加權平均")
                    {
                        string rankType = "" + dr["rank_type"];

                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "名次"))
                        {
                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "名次", "" + dr["rank"]);
                        }
                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "PR值"))
                        {
                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "PR值", "" + dr["pr"]);
                        }
                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "百分比"))
                        {
                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "百分比", "" + dr["percentile"]);
                        }
                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "母體平均"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "母體平均", "" + d);
                        }

                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "母體頂標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_top_25"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "母體頂標", "" + d);
                        }

                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "母體前標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_top_50"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "母體前標", "" + d);
                        }

                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "母體後標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_bottom_50"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "母體後標", "" + d);
                        }

                        if (!_DomainWeightRankDataDict["" + dr["ref_student_id"]].ContainsKey("領域成績加權平均" + rankType + "母體底標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_bottom_25"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _DomainWeightRankDataDict["" + dr["ref_student_id"]].Add("領域成績加權平均" + rankType + "母體底標", "" + d);
                        }
                    }
                }

                if (!_subjectRankDataDict.ContainsKey("" + dr["ref_student_id"]))
                {
                    _subjectRankDataDict.Add("" + dr["ref_student_id"], new Dictionary<string, string>());

                    if ("" + dr["item_type"] == "定期評量/科目成績")
                    {
                        string subjectName = "" + dr["item_name"];
                        string rankType = "" + dr["rank_type"];

                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "名次"))
                        {
                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "名次", "" + dr["rank"]);
                        }
                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "PR值"))
                        {
                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "PR值", "" + dr["pr"]);
                        }
                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "百分比"))
                        {
                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "百分比", "" + dr["percentile"]);
                        }
                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "母體平均"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "母體平均", "" + d);
                        }

                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "母體頂標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_top_25"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "母體頂標", "" + d);
                        }

                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "母體前標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_top_50"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "母體前標", "" + d);
                        }

                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "母體後標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_bottom_50"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "母體後標", "" + d);
                        }

                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "母體底標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_bottom_25"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "母體底標", "" + d);
                        }

                    }
                }
                else
                {
                    if ("" + dr["item_type"] == "定期評量/科目成績")
                    {
                        string subjectName = "" + dr["item_name"];
                        string rankType = "" + dr["rank_type"];

                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "名次"))
                        {
                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "名次", "" + dr["rank"]);
                        }
                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "PR值"))
                        {
                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "PR值", "" + dr["pr"]);
                        }
                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "百分比"))
                        {
                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "百分比", "" + dr["percentile"]);
                        }
                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "母體平均"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "母體平均", "" + d);
                        }

                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "母體頂標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_top_25"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "母體頂標", "" + d);
                        }

                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "母體前標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_top_50"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "母體前標", "" + d);
                        }

                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "母體後標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_bottom_50"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "母體後標", "" + d);
                        }

                        if (!_subjectRankDataDict["" + dr["ref_student_id"]].ContainsKey(subjectName + "_科目" + rankType + "母體底標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_bottom_25"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _subjectRankDataDict["" + dr["ref_student_id"]].Add(subjectName + "_科目" + rankType + "母體底標", "" + d);
                        }
                    }

                }
                if (!_domainRankDataDict.ContainsKey("" + dr["ref_student_id"]))
                {
                    _domainRankDataDict.Add("" + dr["ref_student_id"], new Dictionary<string, string>());

                    if ("" + dr["item_type"] == "定期評量/領域成績")
                    {
                        string domainName = "" + dr["item_name"];
                        string rankType = "" + dr["rank_type"];

                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "名次"))
                        {
                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "名次", "" + dr["rank"]);
                        }
                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "PR值"))
                        {
                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "PR值", "" + dr["pr"]);
                        }
                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "百分比"))
                        {
                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "百分比", "" + dr["percentile"]);
                        }
                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "母體平均"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "母體平均", "" + d);
                        }

                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "母體頂標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_top_25"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "母體頂標", "" + d);
                        }

                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "母體前標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_top_50"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "母體前標", "" + d);
                        }

                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "母體後標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_bottom_50"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "母體後標", "" + d);
                        }

                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "母體底標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_bottom_25"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "母體底標", "" + d);
                        }
                    }
                }
                else
                {
                    if ("" + dr["item_type"] == "定期評量/領域成績")
                    {
                        string domainName = "" + dr["item_name"];
                        string rankType = "" + dr["rank_type"];

                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "名次"))
                        {
                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "名次", "" + dr["rank"]);
                        }
                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "PR值"))
                        {
                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "PR值", "" + dr["pr"]);
                        }
                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "百分比"))
                        {
                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "百分比", "" + dr["percentile"]);
                        }
                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "母體平均"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "母體平均", "" + d);
                        }

                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "母體頂標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_top_25"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "母體頂標", "" + d);
                        }

                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "母體前標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_top_50"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "母體前標", "" + d);
                        }

                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "母體後標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_bottom_50"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "母體後標", "" + d);
                        }

                        if (!_domainRankDataDict["" + dr["ref_student_id"]].ContainsKey(domainName + "_領域" + rankType + "母體底標"))
                        {
                            decimal d;

                            decimal.TryParse("" + dr["avg_bottom_25"], out d);

                            // 四捨五入標準作法
                            d = Math.Round(d, 2, MidpointRounding.AwayFromZero); // 四捨五入到第二位

                            _domainRankDataDict["" + dr["ref_student_id"]].Add(domainName + "_領域" + rankType + "母體底標", "" + d);
                        }
                    }
                }
            }



            #endregion


            _bgWorkReport.ReportProgress(60);

            List<string> domainLi = new List<string>();

            List<string> subjLi = new List<string>();
            subjLi.Add("科目名稱");
            subjLi.Add("科目權數");
            subjLi.Add("科目定期評量");
            subjLi.Add("科目平時評量");
            subjLi.Add("科目總成績");
            subjLi.Add("科目文字評量");

            // 新增排名資訊
            subjLi.Add("科目班排名名次");
            subjLi.Add("科目班排名PR值");
            subjLi.Add("科目班排名百分比");
            subjLi.Add("科目班排名母體頂標");
            subjLi.Add("科目班排名母體前標");
            subjLi.Add("科目班排名母體平均");
            subjLi.Add("科目班排名母體後標");
            subjLi.Add("科目班排名母體底標");
            subjLi.Add("科目年排名名次");
            subjLi.Add("科目年排名PR值");
            subjLi.Add("科目年排名百分比");
            subjLi.Add("科目年排名母體頂標");
            subjLi.Add("科目年排名母體前標");
            subjLi.Add("科目年排名母體平均");
            subjLi.Add("科目年排名母體後標");
            subjLi.Add("科目年排名母體底標");
            subjLi.Add("科目類別1排名名次");
            subjLi.Add("科目類別1排名PR值");
            subjLi.Add("科目類別1排名百分比");
            subjLi.Add("科目類別1排名母體頂標");
            subjLi.Add("科目類別1排名母體前標");
            subjLi.Add("科目類別1排名母體平均");
            subjLi.Add("科目類別1排名母體後標");
            subjLi.Add("科目類別1排名母體底標");
            subjLi.Add("科目類別2排名名次");
            subjLi.Add("科目類別2排名PR值");
            subjLi.Add("科目類別2排名百分比");
            subjLi.Add("科目類別2排名母體頂標");
            subjLi.Add("科目類別2排名母體前標");
            subjLi.Add("科目類別2排名母體平均");
            subjLi.Add("科目類別2排名母體後標");
            subjLi.Add("科目類別2排名母體底標");


            List<string> subjColList = new List<string>();
            foreach (string dName in Global.DomainNameList)
            {
                for (int i = 1; i <= 12; i++)
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
                bool thisTimeHaveSemesterHistoryItem = false;

                foreach (SemesterHistoryItem shi in shr.SemesterHistoryItems)
                {
                    if (shi.SchoolYear == _SelSchoolYear && shi.Semester == _SelSemester)
                    {
                        if (!StudShiDict.ContainsKey(shi.RefStudentID))
                            StudShiDict.Add(shi.RefStudentID, shi);
                        thisTimeHaveSemesterHistoryItem = true;

                    }

                }
                if (!thisTimeHaveSemesterHistoryItem)
                {

                    if (!StudShiDict.ContainsKey(shr.RefStudentID))
                    {

                        SemesterHistoryItem New_shi = new SemesterHistoryItem();

                        New_shi.ClassName = shr.Student.Class != null ? shr.Student.Class.Name : "";

                        New_shi.SeatNo = shr.Student.SeatNo;

                        New_shi.Teacher = shr.Student.Class != null ? shr.Student.Class.Teacher.Name : "";

                        StudShiDict.Add(shr.RefStudentID, New_shi);

                    }


                }

            }




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

                // 取得年級
                int grYear = 0;

                if (ClassDict.ContainsKey(StudRec.RefClassID))
                {
                    if (ClassDict[StudRec.RefClassID].GradeYear.HasValue)
                        grYear = ClassDict[StudRec.RefClassID].GradeYear.Value;
                }
                // 
                // 取得欄位
                foreach (string colName in Global.DTColumnsList())
                    dt.Columns.Add(colName);

                // 缺曠欄位
                foreach (var type in typeList)
                {
                    foreach (var absence in absenceList)
                    {
                        dt.Columns.Add(type + absence);
                    }
                }

                //foreach (var studentID in _AttendanceDict.Keys)
                //{
                //    foreach (var absentKey in _AttendanceDict[studentID].Keys)
                //    {
                //        if (!dt.Columns.Contains(absentKey))
                //        {
                //            dt.Columns.Add(absentKey);
                //        }
                //    }
                //}

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
                Dictionary<string, string> colSubjMapDict = new Dictionary<string, string>();
                Dictionary<string, string> colSubjMapDictF = new Dictionary<string, string>();
                Dictionary<string, string> colSubjMapDictA = new Dictionary<string, string>();
                List<string> listF = new List<string>();
                List<string> listA = new List<string>();

                int c1 = 1, g1 = 1;
                foreach (string n1 in subjClassDNList)
                {
                    if (n1.Contains("F"))
                    {
                        string name = n1.Remove(n1.Length - 1);
                        listF.Add(name);
                        continue;
                    }
                    if (n1.Contains("A"))
                    {
                        string name = n1.Remove(n1.Length - 1);
                        listA.Add(name);
                        continue;
                    }

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

                c1 = 1;
                foreach (string n1 in listF)
                {
                    string sName1 = "sf班級_科目名稱" + n1;
                    string sName2 = "sf班級_科目名稱" + c1;
                    dt.Columns.Add(sName2);
                    // 放入科目名稱
                    colSubjMapDictF.Add(sName1, sName2);
                    foreach (string n2 in li2)
                    {
                        string colName = "sf班級_" + n1 + "_" + n2;
                        string colVal = "sf班級_" + "科目" + c1 + "_" + n2;
                        colSubjMapDictF.Add(colName, colVal);
                        dt.Columns.Add(colVal);
                    }
                    c1++;
                }

                c1 = 1;
                foreach (string n1 in listA)
                {
                    string sName1 = "sa班級_科目名稱" + n1;
                    string sName2 = "sa班級_科目名稱" + c1;
                    dt.Columns.Add(sName2);
                    // 放入科目名稱
                    colSubjMapDictA.Add(sName1, sName2);
                    foreach (string n2 in li2)
                    {
                        string colName = "sa班級_" + n1 + "_" + n2;
                        string colVal = "sa班級_" + "科目" + c1 + "_" + n2;
                        colSubjMapDictA.Add(colName, colVal);
                        dt.Columns.Add(colVal);
                    }
                    c1++;
                }

                // 年級
                listF.Clear();
                listA.Clear();
                foreach (string n1 in subjGradeDNList)
                {
                    if (n1.Contains("F"))
                    {
                        string name = n1.Remove(n1.Length - 1);
                        listF.Add(name);
                        continue;
                    }
                    if (n1.Contains("A"))
                    {
                        string name = n1.Remove(n1.Length - 1);
                        listA.Add(name);
                        continue;
                    }

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

                g1 = 1;
                foreach (string n1 in listF)
                {
                    string gName1 = "sf年級_科目名稱" + n1;
                    string gName2 = "sf年級_科目名稱" + g1;
                    dt.Columns.Add(gName2);
                    colSubjMapDictF.Add(gName1, gName2);
                    foreach (string n2 in li2)
                    {
                        string colName = "sf年級_" + n1 + "_" + n2;
                        string colVal = "sf年級_" + "科目" + g1 + "_" + n2;
                        colSubjMapDictF.Add(colName, colVal);
                        dt.Columns.Add(colVal);
                    }
                    g1++;
                }

                g1 = 1;
                foreach (string n1 in listA)
                {
                    string gName1 = "sa年級_科目名稱" + n1;
                    string gName2 = "sa年級_科目名稱" + g1;
                    dt.Columns.Add(gName2);
                    colSubjMapDictA.Add(gName1, gName2);
                    foreach (string n2 in li2)
                    {
                        string colName = "sa年級_" + n1 + "_" + n2;
                        string colVal = "sa年級_" + "科目" + g1 + "_" + n2;
                        colSubjMapDictA.Add(colName, colVal);
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
                    dt.Columns.Add(dName + "_領域定期加權平均");
                    dt.Columns.Add(dName + "_領域平時加權平均");

                    //領域排名欄位
                    dt.Columns.Add(dName + "_領域班排名名次");
                    dt.Columns.Add(dName + "_領域班排名PR值");
                    dt.Columns.Add(dName + "_領域班排名百分比");
                    dt.Columns.Add(dName + "_領域班排名母體頂標");
                    dt.Columns.Add(dName + "_領域班排名母體前標");
                    dt.Columns.Add(dName + "_領域班排名母體平均");
                    dt.Columns.Add(dName + "_領域班排名母體後標");
                    dt.Columns.Add(dName + "_領域班排名母體底標");
                    dt.Columns.Add(dName + "_領域年排名名次");
                    dt.Columns.Add(dName + "_領域年排名PR值");
                    dt.Columns.Add(dName + "_領域年排名百分比");
                    dt.Columns.Add(dName + "_領域年排名母體頂標");
                    dt.Columns.Add(dName + "_領域年排名母體前標");
                    dt.Columns.Add(dName + "_領域年排名母體平均");
                    dt.Columns.Add(dName + "_領域年排名母體後標");
                    dt.Columns.Add(dName + "_領域年排名母體底標");
                    dt.Columns.Add(dName + "_領域類別1排名名次");
                    dt.Columns.Add(dName + "_領域類別1排名PR值");
                    dt.Columns.Add(dName + "_領域類別1排名百分比");
                    dt.Columns.Add(dName + "_領域類別1排名母體頂標");
                    dt.Columns.Add(dName + "_領域類別1排名母體前標");
                    dt.Columns.Add(dName + "_領域類別1排名母體平均");
                    dt.Columns.Add(dName + "_領域類別1排名母體後標");
                    dt.Columns.Add(dName + "_領域類別1排名母體底標");
                    dt.Columns.Add(dName + "_領域類別2排名名次");
                    dt.Columns.Add(dName + "_領域類別2排名PR值");
                    dt.Columns.Add(dName + "_領域類別2排名百分比");
                    dt.Columns.Add(dName + "_領域類別2排名母體頂標");
                    dt.Columns.Add(dName + "_領域類別2排名母體前標");
                    dt.Columns.Add(dName + "_領域類別2排名母體平均");
                    dt.Columns.Add(dName + "_領域類別2排名母體後標");
                    dt.Columns.Add(dName + "_領域類別2排名母體底標");

                }

                //領域加權排名欄位
                dt.Columns.Add("領域成績加權平均班排名名次");
                dt.Columns.Add("領域成績加權平均班排名PR值");
                dt.Columns.Add("領域成績加權平均班排名百分比");
                dt.Columns.Add("領域成績加權平均班排名母體頂標");
                dt.Columns.Add("領域成績加權平均班排名母體前標");
                dt.Columns.Add("領域成績加權平均班排名母體平均");
                dt.Columns.Add("領域成績加權平均班排名母體後標");
                dt.Columns.Add("領域成績加權平均班排名母體底標");
                dt.Columns.Add("領域成績加權平均年排名名次");
                dt.Columns.Add("領域成績加權平均年排名PR值");
                dt.Columns.Add("領域成績加權平均年排名百分比");
                dt.Columns.Add("領域成績加權平均年排名母體頂標");
                dt.Columns.Add("領域成績加權平均年排名母體前標");
                dt.Columns.Add("領域成績加權平均年排名母體平均");
                dt.Columns.Add("領域成績加權平均年排名母體後標");
                dt.Columns.Add("領域成績加權平均年排名母體底標");
                dt.Columns.Add("領域成績加權平均類別1排名名次");
                dt.Columns.Add("領域成績加權平均類別1排名PR值");
                dt.Columns.Add("領域成績加權平均類別1排名百分比");
                dt.Columns.Add("領域成績加權平均類別1排名母體頂標");
                dt.Columns.Add("領域成績加權平均類別1排名母體前標");
                dt.Columns.Add("領域成績加權平均類別1排名母體平均");
                dt.Columns.Add("領域成績加權平均類別1排名母體後標");
                dt.Columns.Add("領域成績加權平均類別1排名母體底標");
                dt.Columns.Add("領域成績加權平均類別2排名名次");
                dt.Columns.Add("領域成績加權平均類別2排名PR值");
                dt.Columns.Add("領域成績加權平均類別2排名百分比");
                dt.Columns.Add("領域成績加權平均類別2排名母體頂標");
                dt.Columns.Add("領域成績加權平均類別2排名母體前標");
                dt.Columns.Add("領域成績加權平均類別2排名母體平均");
                dt.Columns.Add("領域成績加權平均類別2排名母體後標");
                dt.Columns.Add("領域成績加權平均類別2排名母體底標");

                dt.TableName = StudRec.ID;
                row["系統編號"] = "系統編號{" + StudRec.ID + "}";
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
                //if (StudRec.SeatNo.HasValue)
                //    row["座號"] = StudRec.SeatNo.Value.ToString();
                //else
                //    row["座號"] = "";

                // 班級、座號、班導師 使用學期歷程內
                if (StudShiDict.ContainsKey(StudRec.ID))
                {
                    row["班級"] = StudShiDict[StudRec.ID].ClassName;
                    row["座號"] = StudShiDict[StudRec.ID].SeatNo;
                    row["班導師"] = StudShiDict[StudRec.ID].Teacher;
                }

                row["學號"] = StudRec.StudentNumber;
                row["姓名"] = StudRec.Name;


                // 傳入 ID當 Key
                // row["缺曠紀錄"] = StudRec.ID;
                rowT["缺曠紀錄"] = StudRec.ID;
                //缺曠套印
                foreach (var type in typeList)
                {
                    foreach (var absence in absenceList)
                    {
                        row[type + absence] = "0";
                    }
                }
                if (_AttendanceDict.ContainsKey(StudRec.ID))
                {
                    foreach (var absentKey in _AttendanceDict[StudRec.ID].Keys)
                    {
                        row[absentKey] = _AttendanceDict[StudRec.ID][absentKey];
                    }
                }
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

                        // 當領域非固定領域，無法處理
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
                                case "科目定期評量":
                                    if (ess.ScoreF.HasValue)
                                        row[key] = ess.ScoreF.Value;
                                    break;
                                case "科目平時評量":
                                    if (ess.ScoreA.HasValue)
                                        row[key] = ess.ScoreA.Value;
                                    break;
                                case "科目總成績":
                                    if (ess.ScoreT.HasValue)
                                        row[key] = ess.ScoreT.Value;
                                    break;
                                case "科目文字評量":
                                    row[key] = ess.Text;
                                    break;

                                case "科目班排名名次":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目班排名PR值":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目班排名百分比":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目班排名母體頂標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目班排名母體前標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目班排名母體平均":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目班排名母體後標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目班排名母體底標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目年排名名次":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目年排名PR值":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目年排名百分比":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目年排名母體頂標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目年排名母體前標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目年排名母體平均":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目年排名母體後標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目年排名母體底標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別1排名名次":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別1排名PR值":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別1排名百分比":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別1排名母體頂標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別1排名母體前標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別1排名母體平均":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別1排名母體後標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別1排名母體底標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別2排名名次":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別2排名PR值":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別2排名百分比":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別2排名母體頂標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別2排名母體前標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別2排名母體平均":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別2排名母體後標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                                case "科目類別2排名母體底標":
                                    if (!_subjectRankDataDict.ContainsKey(StudRec.ID))
                                    {
                                        continue; // 沒有排名資料 就跳出
                                    }
                                    row[key] = _subjectRankDataDict[StudRec.ID].ContainsKey(ess.SubjectName + "_" + item) ? _subjectRankDataDict[StudRec.ID][ess.SubjectName + "_" + item] : "";
                                    break;
                            }


                        }

                    }

                    // 領域
                    foreach (DAO.ExamDomainScore eds in studExamScoreDict[StudRec.ID]._ExamDomainScoreDict.Values)
                    {
                        // 當領域非固定領域，無法處理
                        if (!Global.DomainNameList.Contains(eds.DomainName))
                        {
                            string errMsg = eds.DomainName + "領域相關成績，無法處理。";
                            if (!_ErrorDomainNameList.Contains(errMsg))
                                _ErrorDomainNameList.Add(errMsg);
                            continue;
                        }

                        string key = eds.DomainName + "_領域加權平均";
                        string keya = eds.DomainName + "_領域平時加權平均";
                        string keyf = eds.DomainName + "_領域定期加權平均";
                        string keyc = eds.DomainName + "_領域權數";

                        string keyRank1 = eds.DomainName + "_領域班排名名次";
                        string keyRank2 = eds.DomainName + "_領域班排名PR值";
                        string keyRank3 = eds.DomainName + "_領域班排名百分比";
                        string keyRank4 = eds.DomainName + "_領域班排名母體頂標";
                        string keyRank5 = eds.DomainName + "_領域班排名母體前標";
                        string keyRank6 = eds.DomainName + "_領域班排名母體平均";
                        string keyRank7 = eds.DomainName + "_領域班排名母體後標";
                        string keyRank8 = eds.DomainName + "_領域班排名母體底標";
                        string keyRank9 = eds.DomainName + "_領域年排名名次";
                        string keyRank10 = eds.DomainName + "_領域年排名PR值";
                        string keyRank11 = eds.DomainName + "_領域年排名百分比";
                        string keyRank12 = eds.DomainName + "_領域年排名母體頂標";
                        string keyRank13 = eds.DomainName + "_領域年排名母體前標";
                        string keyRank14 = eds.DomainName + "_領域年排名母體平均";
                        string keyRank15 = eds.DomainName + "_領域年排名母體後標";
                        string keyRank16 = eds.DomainName + "_領域年排名母體底標";
                        string keyRank17 = eds.DomainName + "_領域類別1排名名次";
                        string keyRank18 = eds.DomainName + "_領域類別1排名PR值";
                        string keyRank19 = eds.DomainName + "_領域類別1排名百分比";
                        string keyRank20 = eds.DomainName + "_領域類別1排名母體頂標";
                        string keyRank21 = eds.DomainName + "_領域類別1排名母體前標";
                        string keyRank22 = eds.DomainName + "_領域類別1排名母體平均";
                        string keyRank23 = eds.DomainName + "_領域類別1排名母體後標";
                        string keyRank24 = eds.DomainName + "_領域類別1排名母體底標";
                        string keyRank25 = eds.DomainName + "_領域類別2排名名次";
                        string keyRank26 = eds.DomainName + "_領域類別2排名PR值";
                        string keyRank27 = eds.DomainName + "_領域類別2排名百分比";
                        string keyRank28 = eds.DomainName + "_領域類別2排名母體頂標";
                        string keyRank29 = eds.DomainName + "_領域類別2排名母體前標";
                        string keyRank30 = eds.DomainName + "_領域類別2排名母體平均";
                        string keyRank31 = eds.DomainName + "_領域類別2排名母體後標";
                        string keyRank32 = eds.DomainName + "_領域類別2排名母體底標";

                        // 總成績
                        if (eds.ScoreT.HasValue)
                            row[key] = eds.ScoreT.Value;

                        // 學分
                        if (eds.Credit1.HasValue)
                            row[keyc] = eds.Credit1.Value;

                        // 平時加權
                        if (eds.ScoreA.HasValue)
                            row[keya] = eds.ScoreA.Value;

                        // 定期加權
                        if (eds.ScoreF.HasValue)
                            row[keyf] = eds.ScoreF.Value;

                        // 領域排名資料

                        if (!_domainRankDataDict.ContainsKey(StudRec.ID))
                        {
                            continue; // 沒有排名資料 就跳出
                        }
                        row[keyRank1] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank1) ? _domainRankDataDict[StudRec.ID][keyRank1] : "";
                        row[keyRank2] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank2) ? _domainRankDataDict[StudRec.ID][keyRank2] : "";
                        row[keyRank3] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank3) ? _domainRankDataDict[StudRec.ID][keyRank3] : "";
                        row[keyRank4] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank4) ? _domainRankDataDict[StudRec.ID][keyRank4] : "";
                        row[keyRank5] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank5) ? _domainRankDataDict[StudRec.ID][keyRank5] : "";
                        row[keyRank6] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank6) ? _domainRankDataDict[StudRec.ID][keyRank6] : "";
                        row[keyRank7] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank7) ? _domainRankDataDict[StudRec.ID][keyRank7] : "";
                        row[keyRank8] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank8) ? _domainRankDataDict[StudRec.ID][keyRank8] : "";
                        row[keyRank9] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank9) ? _domainRankDataDict[StudRec.ID][keyRank9] : "";
                        row[keyRank10] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank10) ? _domainRankDataDict[StudRec.ID][keyRank10] : "";
                        row[keyRank11] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank11) ? _domainRankDataDict[StudRec.ID][keyRank11] : "";
                        row[keyRank12] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank12) ? _domainRankDataDict[StudRec.ID][keyRank12] : "";
                        row[keyRank13] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank13) ? _domainRankDataDict[StudRec.ID][keyRank13] : "";
                        row[keyRank14] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank14) ? _domainRankDataDict[StudRec.ID][keyRank14] : "";
                        row[keyRank15] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank15) ? _domainRankDataDict[StudRec.ID][keyRank15] : "";
                        row[keyRank16] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank17] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank18] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank19] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank20] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank21] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank22] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank23] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank24] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank25] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank26] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank27] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank28] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank29] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank30] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank31] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";
                        row[keyRank32] = _domainRankDataDict[StudRec.ID].ContainsKey(keyRank16) ? _domainRankDataDict[StudRec.ID][keyRank16] : "";

                    }
                }

                // 加權平均
                if (studExamScoreDict.ContainsKey(StudRec.ID))
                {
                    if (studExamScoreDict[StudRec.ID].GetDomainScoreA(true).HasValue)
                        row["領域成績加權平均"] = studExamScoreDict[StudRec.ID].GetDomainScoreA(true).Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreAA(true).HasValue)
                        row["科目平時評量加權平均"] = studExamScoreDict[StudRec.ID].GetSubjectScoreAA(true).Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreAF(true).HasValue)
                        row["科目定期評量加權平均"] = studExamScoreDict[StudRec.ID].GetSubjectScoreAF(true).Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreAT(true).HasValue)
                        row["科目總成績加權平均"] = studExamScoreDict[StudRec.ID].GetSubjectScoreAT(true).Value;


                    if (studExamScoreDict[StudRec.ID].GetDomainScoreA(false).HasValue)
                        row["領域成績加權平均(不含彈性)"] = studExamScoreDict[StudRec.ID].GetDomainScoreA(false).Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreAA(false).HasValue)
                        row["科目平時評量加權平均(不含彈性)"] = studExamScoreDict[StudRec.ID].GetSubjectScoreAA(false).Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreAF(false).HasValue)
                        row["科目定期評量加權平均(不含彈性)"] = studExamScoreDict[StudRec.ID].GetSubjectScoreAF(false).Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreAT(false).HasValue)
                        row["科目總成績加權平均(不含彈性)"] = studExamScoreDict[StudRec.ID].GetSubjectScoreAT(false).Value;
                }

                // 加權總分
                if (studExamScoreDict.ContainsKey(StudRec.ID))
                {
                    if (studExamScoreDict[StudRec.ID].GetDomainScoreS(true).HasValue)
                        row["領域成績加權總分"] = studExamScoreDict[StudRec.ID].GetDomainScoreS(true).Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreSA(true).HasValue)
                        row["科目平時評量加權總分"] = studExamScoreDict[StudRec.ID].GetSubjectScoreSA(true).Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreSF(true).HasValue)
                        row["科目定期評量加權總分"] = studExamScoreDict[StudRec.ID].GetSubjectScoreSF(true).Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreST(true).HasValue)
                        row["科目總成績加權總分"] = studExamScoreDict[StudRec.ID].GetSubjectScoreST(true).Value;


                    if (studExamScoreDict[StudRec.ID].GetDomainScoreS(false).HasValue)
                        row["領域成績加權總分(不含彈性)"] = studExamScoreDict[StudRec.ID].GetDomainScoreS(false).Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreSA(false).HasValue)
                        row["科目平時評量加權總分(不含彈性)"] = studExamScoreDict[StudRec.ID].GetSubjectScoreSA(false).Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreSF(false).HasValue)
                        row["科目定期評量加權總分(不含彈性)"] = studExamScoreDict[StudRec.ID].GetSubjectScoreSF(false).Value;

                    if (studExamScoreDict[StudRec.ID].GetSubjectScoreST(false).HasValue)
                        row["科目總成績加權總分(不含彈性)"] = studExamScoreDict[StudRec.ID].GetSubjectScoreST(false).Value;
                }


                // 領域加權排名資料
                if (_DomainWeightRankDataDict.ContainsKey(StudRec.ID))
                {
                    foreach (string key in _DomainWeightRankDataDict[StudRec.ID].Keys)
                    {
                        row[key] = _DomainWeightRankDataDict[StudRec.ID][key];
                    }
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
                        string ssKey = "";
                        string name = "";
                        if (data.Key.Contains("F"))
                        {
                            name = data.Key.Remove(data.Key.Length - 1);
                            ssKey = "sf班級_科目名稱" + name;
                        }
                        else if (data.Key.Contains("A"))
                        {
                            name = data.Key.Remove(data.Key.Length - 1);
                            ssKey = "sa班級_科目名稱" + name;
                        }
                        else
                        {
                            name = data.Key;
                            ssKey = "s班級_科目名稱" + name;
                        }

                        if (colSubjMapDict.ContainsKey(ssKey))
                            row[colSubjMapDict[ssKey]] = name;

                        if (colSubjMapDictF.ContainsKey(ssKey))
                            row[colSubjMapDictF[ssKey]] = name;

                        if (colSubjMapDictA.ContainsKey(ssKey))
                            row[colSubjMapDictA[ssKey]] = name;

                        foreach (DAO.SubjectRangeCount.SubjectRangeType dtType in stypeList)
                        {
                            if (data.Key.Contains("F"))
                            {
                                sClassKey = "sf班級_" + name + "_" + dtType.ToString();
                            }
                            else if (data.Key.Contains("A"))
                            {
                                sClassKey = "sa班級_" + name + "_" + dtType.ToString();
                            }
                            else
                            {
                                sClassKey = "s班級_" + name + "_" + dtType.ToString();
                            }

                            if (colSubjMapDict.ContainsKey(sClassKey))
                            {
                                row[colSubjMapDict[sClassKey]] = data.Value.GetRankCount(dtType);
                            }

                            if (colSubjMapDictF.ContainsKey(sClassKey))
                            {
                                row[colSubjMapDictF[sClassKey]] = data.Value.GetRankCount(dtType);
                            }

                            if (colSubjMapDictA.ContainsKey(sClassKey))
                            {
                                row[colSubjMapDictA[sClassKey]] = data.Value.GetRankCount(dtType);
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
                        string ssKey = "";
                        string name = "";

                        if (data.Key.Contains("F"))
                        {
                            name = data.Key.Remove(data.Key.Length - 1);
                            ssKey = "sf年級_科目名稱" + name;
                        }
                        else if (data.Key.Contains("A"))
                        {
                            name = data.Key.Remove(data.Key.Length - 1);
                            ssKey = "sa年級_科目名稱" + name;
                        }
                        else
                        {
                            name = data.Key;
                            ssKey = "s年級_科目名稱" + name;
                        }


                        if (colSubjMapDict.ContainsKey(ssKey))
                            row[colSubjMapDict[ssKey]] = name;

                        if (colSubjMapDictF.ContainsKey(ssKey))
                            row[colSubjMapDictF[ssKey]] = name;

                        if (colSubjMapDictA.ContainsKey(ssKey))
                            row[colSubjMapDictA[ssKey]] = name;


                        foreach (DAO.SubjectRangeCount.SubjectRangeType dtType in stypeList)
                        {
                            if (data.Key.Contains("F"))
                            {
                                sGradeKey = "sf年級_" + name + "_" + dtType.ToString();
                            }
                            else if (data.Key.Contains("A"))
                            {
                                sGradeKey = "sa年級_" + name + "_" + dtType.ToString();
                            }
                            else
                            {
                                sGradeKey = "s年級_" + name + "_" + dtType.ToString();
                            }

                            if (colSubjMapDict.ContainsKey(sGradeKey))
                            {
                                row[colSubjMapDict[sGradeKey]] = data.Value.GetRankCount(dtType);
                            }

                            if (colSubjMapDictF.ContainsKey(sGradeKey))
                            {
                                row[colSubjMapDictF[sGradeKey]] = data.Value.GetRankCount(dtType);
                            }

                            if (colSubjMapDictA.ContainsKey(sGradeKey))
                            {
                                row[colSubjMapDictA[sGradeKey]] = data.Value.GetRankCount(dtType);
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

                //_builder = new DocumentBuilder(docAtt);
                //                docAtt.MailMerge.MergeField += new Aspose.Words.Reporting.MergeFieldEventHandler(MailMerge_MergeField);
                docAtt.MailMerge.FieldMergingCallback = this;
                docAtt.MailMerge.Execute(dtAtt);

                doc1.Sections.Add(doc1.ImportNode(docAtt.Sections[0], true));
                doc1.MailMerge.Execute(dt);
                doc1.MailMerge.RemoveEmptyParagraphs = true;
                doc1.MailMerge.DeleteFields();
                docList.Add(doc1);

            }

            _bgWorkReport.ReportProgress(80);
            //// debug 用           
            //string ssStr = Application.StartupPath + "\\dt_debug.xml";
            //dt.WriteXml(ssStr);


            #endregion

            #region Word 合併列印

            doc.Sections.Clear();
            foreach (Document doc1 in docList)
                doc.Sections.Add(doc.ImportNode(doc1.Sections[0], true));

            string reportNameW = "新竹評量成績單";
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
                //2019/4/19 俊緯更新 完成[190417-01][03] 電子報表及推播功能，聽恩正建議將Save功能移到Complete執行
                MemoryStream memoryStream = new MemoryStream();
                doc.Save(memoryStream, SaveFormat.Docx);
                e.Result = new object[] { memoryStream, reportNameW };

            }
            catch (Exception exow)
            {
                throw exow;
            }
            doc = null;
            docList.Clear();

            GC.Collect();
            #endregion
            _bgWorkReport.ReportProgress(100);
        }

        //void MailMerge_MergeField(object sender, Aspose.Words.Reporting.MergeFieldEventArgs e)
        //{

        //}


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

            string reportName = "新竹評量成績單樣板(" + _Configure.Name + ").doc";

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
                        stream.Write(Properties.Resources.新竹_評量成績通知單樣板, 0, Properties.Resources.新竹_評量成績通知單樣板.Length);
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

        private void SetDomainList()
        {
            Global._SelSchoolYear = _SelSchoolYear;
            Global._SelSemester = _SelSemester;
            Global._SelStudentIDList = _StudentIDList;
            Global._SelExamID = _SelExamID;
            Global.SetDomainList();
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
                var _builder = new DocumentBuilder(e.Document);
                if (_builder.MoveToMergeField(e.FieldName))
                {
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
                        _builder.RowFormat.HeightRule = HeightRule.Exactly;
                        _builder.RowFormat.Height = 18.0;
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

        }

        void IFieldMergingCallback.ImageFieldMerging(ImageFieldMergingArgs args)
        {
            throw new NotImplementedException();
        }
    }
}

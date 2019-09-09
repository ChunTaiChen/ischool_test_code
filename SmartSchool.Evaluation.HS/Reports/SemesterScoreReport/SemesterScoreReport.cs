using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Customization.PlugIn.Report;
using System.ComponentModel;
using Aspose.Words;
using System.IO;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using System.Windows.Forms;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.Common;
using SHSchool.Data;

namespace SmartSchool.Evaluation.Reports
{
    class SemesterScoreReport
    {
        private ButtonAdapter button, button2;
        private BackgroundWorker _BGWSemesterScoreReport;
        private Dictionary<string, decimal> _degreeList = null; //等第List

        // 錯誤訊息
        StringBuilder _ErrorMessage;
        private enum Entity { Student, Class }

        public SemesterScoreReport()
        {
            string reportName = "學期成績單";
            string path = "成績相關報表";

            _ErrorMessage = new StringBuilder();

            button = new SecureButtonAdapter("Report0050");
            button.Text = reportName;
            button.Path = path;
            button.OnClick += new EventHandler(button_OnClick);
            StudentReport.AddReport(button);

            button2 = new SecureButtonAdapter("Report0150");
            button2.Text = reportName;
            button2.Path = path;
            button2.OnClick += new EventHandler(button2_OnClick);
            ClassReport.AddReport(button2);
        }

        #region Common Function

        public int SortBySemesterSubjectScoreInfo(SemesterSubjectScoreInfo a, SemesterSubjectScoreInfo b)
        {
            return SortBySubjectName(a.Subject, b.Subject);
        }

        private int SortBySubjectName(string a, string b)
        {
            string a1 = a.Length > 0 ? a.Substring(0, 1) : "";
            string b1 = b.Length > 0 ? b.Substring(0, 1) : "";
            #region 第一個字一樣的時候
            if (a1 == b1)
            {
                if (a.Length > 1 && b.Length > 1)
                    return SortBySubjectName(a.Substring(1), b.Substring(1));
                else
                    return a.Length.CompareTo(b.Length);
            }
            #endregion
            #region 第一個字不同，分別取得在設定順序中的數字，如果都不在設定順序中就用單純字串比較
            int ai = getIntForSubject(a1), bi = getIntForSubject(b1);
            if (ai > 0 || bi > 0)
                return ai.CompareTo(bi);
            else
                return a1.CompareTo(b1);
            #endregion
        }

        private int getIntForSubject(string a1)
        {
            List<string> list = new List<string>();
            list.AddRange(new string[] { "國", "英", "數", "物", "化", "生", "基", "歷", "地", "公", "文", "礎", "世" });

            int x = list.IndexOf(a1);
            if (x < 0)
                return list.Count;
            else
                return x;
        }

        private int SortByEntryName(string a, string b)
        {
            int ai = getIntForEntry(a), bi = getIntForEntry(b);
            if (ai > 0 || bi > 0)
                return ai.CompareTo(bi);
            else
                return a.CompareTo(b);
        }

        private int getIntForEntry(string a1)
        {
            List<string> list = new List<string>();
            list.AddRange(new string[] { "學業", "學業成績名次", "實習科目", "體育", "國防通識", "健康與護理", "德行", "學年德行成績" });

            int x = list.IndexOf(a1);
            if (x < 0)
                return list.Count;
            else
                return x;
        }

        //科目級別轉換
        private string GetNumber(int p)
        {
            List<string> list = new List<string>(new string[] { "", "Ⅰ", "Ⅱ", "Ⅲ", "Ⅳ", "Ⅴ", "Ⅵ", "Ⅶ", "Ⅷ", "Ⅸ", "Ⅹ" });

            if (p < list.Count)
                return list[p];
            else
                return "" + p;
        }

        //德行成績 -> 等第
        private string ParseLevel(decimal score)
        {
            if (_degreeList == null)
            {
                _degreeList = new Dictionary<string, decimal>();
                DSResponse dsrsp = SmartSchool.Feature.Basic.Config.GetDegreeList();
                DSXmlHelper helper = dsrsp.GetContent();
                foreach (XmlElement element in helper.GetElements("Degree"))
                {
                    decimal low = decimal.MinValue;
                    if (!decimal.TryParse(element.GetAttribute("Low"), out low))
                        low = decimal.MinValue;
                    _degreeList.Add(element.GetAttribute("Name"), low);
                }
            }

            foreach (string var in _degreeList.Keys)
            {
                if (_degreeList[var] <= score)
                    return var;
            }
            return "";
        }

        //報表產生完成後，儲存並且開啟
        private void Completed(string inputReportName, Document inputDoc)
        {
            string reportName = inputReportName;

            string path = Path.Combine(Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".doc");

            Aspose.Words.Document doc = inputDoc;

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
                doc.Save(path, Aspose.Words.SaveFormat.Doc);
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                SaveFileDialog sd = new SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".doc";
                sd.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
                if (sd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        doc.Save(sd.FileName, Aspose.Words.SaveFormat.Doc);

                    }
                    catch
                    {
                        MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
        }

        //填入學生資料
        private void FillStudentData(AccessHelper helper, List<StudentRecord> students)
        {
            helper.StudentHelper.FillAttendance(students);
            helper.StudentHelper.FillReward(students);
            helper.StudentHelper.FillParentInfo(students);
            helper.StudentHelper.FillContactInfo(students);
            //helper.StudentHelper.FillSchoolYearEntryScore(true, students);
            //helper.StudentHelper.FillSchoolYearSubjectScore(true, students);
            helper.StudentHelper.FillSemesterSubjectScore(true, students);
            helper.StudentHelper.FillSemesterEntryScore(true, students);
            helper.StudentHelper.FillField("SemesterEntryClassRating", students); //學期分項班排名。
            helper.StudentHelper.FillField("補考標準", students);
            helper.StudentHelper.FillSchoolYearEntryScore(true, students);
            helper.StudentHelper.FillSemesterMoralScore(true, students);
        }

        #endregion

        void button_OnClick(object sender, EventArgs e)
        {
            SemesterScoreReportForm form = new SemesterScoreReportForm();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _ErrorMessage.Clear();
                _BGWSemesterScoreReport = new BackgroundWorker();
                _BGWSemesterScoreReport.WorkerReportsProgress = true;
                _BGWSemesterScoreReport.DoWork += new DoWorkEventHandler(_BGWSemesterScoreReport_DoWork);
                _BGWSemesterScoreReport.ProgressChanged += new ProgressChangedEventHandler(_BGWSemesterScoreReport_ProgressChanged);
                _BGWSemesterScoreReport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BGWSemesterScoreReport_RunWorkerCompleted);
                _BGWSemesterScoreReport.RunWorkerAsync(new object[] { form.SchoolYear, form.Semester, form.UserDefinedType, form.Template, form.Receiver, form.Address, form.ResitSign, form.RepeatSign, Entity.Student, form.AllowMoralScoreOver100 });
            }
        }

        void button2_OnClick(object sender, EventArgs e)
        {
            SemesterScoreReportForm form = new SemesterScoreReportForm();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _ErrorMessage.Clear();
                _BGWSemesterScoreReport = new BackgroundWorker();
                _BGWSemesterScoreReport.WorkerReportsProgress = true;
                _BGWSemesterScoreReport.DoWork += new DoWorkEventHandler(_BGWSemesterScoreReport_DoWork);
                _BGWSemesterScoreReport.ProgressChanged += new ProgressChangedEventHandler(_BGWSemesterScoreReport_ProgressChanged);
                _BGWSemesterScoreReport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BGWSemesterScoreReport_RunWorkerCompleted);
                _BGWSemesterScoreReport.RunWorkerAsync(new object[] { form.SchoolYear, form.Semester, form.UserDefinedType, form.Template, form.Receiver, form.Address, form.ResitSign, form.RepeatSign, Entity.Class, form.AllowMoralScoreOver100 });
            }
        }

        void _BGWSemesterScoreReport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // 當有錯誤訊息
            if (_ErrorMessage.Length > 0)
            {
                FISCA.Presentation.Controls.MsgBox.Show(_ErrorMessage.ToString());
            }
            button.SetBarMessage("學期成績單產生完成");
            Completed("學期成績單", (Document)e.Result);
        }

        void _BGWSemesterScoreReport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            button.SetBarMessage("學期成績單產生中...", e.ProgressPercentage);
        }

        void _BGWSemesterScoreReport_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] objectValue = (object[])e.Argument;

            int schoolyear = (int)objectValue[0];
            int semester = (int)objectValue[1];
            Dictionary<string, List<string>> userType = (Dictionary<string, List<string>>)objectValue[2];
            MemoryStream template = (MemoryStream)objectValue[3];
            int receiver = (int)objectValue[4];
            int address = (int)objectValue[5];
            string resitSign = (string)objectValue[6];
            string repeatSign = (string)objectValue[7];
            Entity entity = (Entity)objectValue[8];
            bool over100 = (bool)objectValue[9];

            _BGWSemesterScoreReport.ReportProgress(0);

            #region 取得資料

            GetPeriodType();

            AccessHelper helper = new AccessHelper();

            List<StudentRecord> allStudent = new List<StudentRecord>();

            if (entity == Entity.Student)
            {
                allStudent = helper.StudentHelper.GetSelectedStudent();
                FillStudentData(helper, allStudent);
            }
            else if (entity == Entity.Class)
            {
                foreach (ClassRecord aClass in helper.ClassHelper.GetSelectedClass())
                {
                    FillStudentData(helper, aClass.Students);
                    allStudent.AddRange(aClass.Students);
                }
            }

            int currentStudent = 1;
            int totalStudent = allStudent.Count;

            //WearyDogComputer computer = new WearyDogComputer();
            //computer.FillSemesterSubjectScoreInfoWithResit(helper, true, allStudent);

            #endregion

            #region 產生報表並填入資料

            Document doc = new Document();
            doc.Sections.Clear();

            foreach (StudentRecord var in allStudent)
            {
                Document eachStudentDoc = new Document(template, "", LoadFormat.Doc, "");

                Dictionary<string, object> mergeKeyValue = new Dictionary<string, object>();

                #region 學校基本資料
                mergeKeyValue.Add("學校名稱", SmartSchool.Customization.Data.SystemInformation.SchoolChineseName);
                mergeKeyValue.Add("學校地址", SmartSchool.Customization.Data.SystemInformation.Address);
                mergeKeyValue.Add("學校電話", SmartSchool.Customization.Data.SystemInformation.Telephone);
                #endregion

                #region 收件人姓名與地址
                if (receiver == 0)
                    mergeKeyValue.Add("收件人", var.ParentInfo.CustodianName);
                else if (receiver == 1)
                    mergeKeyValue.Add("收件人", var.ParentInfo.FatherName);
                else if (receiver == 2)
                    mergeKeyValue.Add("收件人", var.ParentInfo.MotherName);
                else if (receiver == 3)
                    mergeKeyValue.Add("收件人", var.StudentName);

                if (address == 0)
                {
                    mergeKeyValue.Add("收件人地址", var.ContactInfo.PermanentAddress.FullAddress);
                }
                else if (address == 1)
                {
                    mergeKeyValue.Add("收件人地址", var.ContactInfo.MailingAddress.FullAddress);
                }
                #endregion

                #region 學生基本資料
                mergeKeyValue.Add("學年度", schoolyear.ToString());
                mergeKeyValue.Add("學期", semester.ToString());
                mergeKeyValue.Add("班級科別名稱", (var.RefClass != null) ? var.RefClass.Department : "");
                mergeKeyValue.Add("班級", (var.RefClass != null) ? var.RefClass.ClassName : "");
                mergeKeyValue.Add("學號", var.StudentNumber);
                mergeKeyValue.Add("姓名", var.StudentName);
                mergeKeyValue.Add("座號", var.SeatNo);
                #endregion

                #region 導師與評語
                if (var.RefClass != null && var.RefClass.RefTeacher != null)
                {
                    mergeKeyValue.Add("班導師", var.RefClass.RefTeacher.TeacherName);
                }
                mergeKeyValue.Add("評語", "");
                if (var.SemesterMoralScoreList.Count > 0)
                {
                    foreach (SemesterMoralScoreInfo info in var.SemesterMoralScoreList)
                    {
                        if (info.SchoolYear == schoolyear && info.Semester == semester)
                            mergeKeyValue["評語"] = info.SupervisedByComment;
                    }
                }
                #endregion

                #region 獎懲紀錄
                int awardA = 0;
                int awardB = 0;
                int awardC = 0;
                int faultA = 0;
                int faultB = 0;
                int faultC = 0;
                bool ua = false; //留校察看
                foreach (RewardInfo info in var.RewardList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester)
                    {
                        awardA += info.AwardA;
                        awardB += info.AwardB;
                        awardC += info.AwardC;

                        if (!info.Cleared)
                        {
                            faultA += info.FaultA;
                            faultB += info.FaultB;
                            faultC += info.FaultC;
                        }

                        if (info.UltimateAdmonition)
                            ua = true;
                    }
                }
                mergeKeyValue.Add("大功", awardA.ToString());
                mergeKeyValue.Add("小功", awardB.ToString());
                mergeKeyValue.Add("嘉獎", awardC.ToString());
                mergeKeyValue.Add("大過", faultA.ToString());
                mergeKeyValue.Add("小過", faultB.ToString());
                mergeKeyValue.Add("警告", faultC.ToString());
                if (ua)
                    mergeKeyValue.Add("留校察看", "ˇ");
                else
                    mergeKeyValue.Add("留校察看", "");

                #endregion

                #region 科目成績

                Dictionary<SemesterSubjectScoreInfo, Dictionary<string, string>> subjectScore = new Dictionary<SemesterSubjectScoreInfo, Dictionary<string, string>>();
                decimal thisSemesterTotalCredit = 0;
                decimal thisSchoolYearTotalCredit = 0;
                decimal beforeSemesterTotalCredit = 0;

                Dictionary<int, decimal> resitStandard = var.Fields["補考標準"] as Dictionary<int, decimal>;

                foreach (SemesterSubjectScoreInfo info in var.SemesterSubjectScoreList)
                {
                    string invalidCredit = info.Detail.GetAttribute("不計學分");
                    string noScoreString = info.Detail.GetAttribute("不需評分");
                    bool noScore = (noScoreString != "是");

                    if (invalidCredit == "是")
                        continue;

                    if (info.SchoolYear == schoolyear && info.Semester == semester)
                    {
                        if (!subjectScore.ContainsKey(info))
                            subjectScore.Add(info, new Dictionary<string, string>());

                        subjectScore[info].Add("科目", info.Subject);
                        subjectScore[info].Add("級別", (string.IsNullOrEmpty(info.Level) ? "" : GetNumber(int.Parse(info.Level))));
                        subjectScore[info].Add("學分", info.CreditDec().ToString());
                        subjectScore[info].Add("分數", noScore ? info.Score.ToString() : "");
                        subjectScore[info].Add("必修", ((info.Require) ? "必" : "選"));

                        //判斷補考或重修 
                        if (!info.Pass)
                        {
                            if (info.Score >= resitStandard[info.GradeYear])
                                subjectScore[info].Add("補考", "是");
                            else
                                subjectScore[info].Add("補考", "否");
                        }
                    }

                    if (info.Pass)
                    {
                        if (info.SchoolYear == schoolyear && info.Semester == semester)
                            thisSemesterTotalCredit += info.CreditDec();

                        if (info.SchoolYear < schoolyear)
                            beforeSemesterTotalCredit += info.CreditDec();
                        else if (info.SchoolYear == schoolyear && info.Semester <= semester)
                            beforeSemesterTotalCredit += info.CreditDec();

                        if (info.SchoolYear == schoolyear)
                            thisSchoolYearTotalCredit += info.CreditDec();
                    }
                }

                //if (schoolyearscore)
                //{
                //    foreach (SchoolYearSubjectScoreInfo info in var.SchoolYearSubjectScoreList)
                //    {
                //        if (info.SchoolYear == schoolyear)
                //        {
                //            string subject = info.Subject;
                //            foreach (SemesterSubjectScoreInfo key in subjectScore.Keys)
                //            {
                //                if (subjectScore[key]["科目"] == subject && !subjectScore[key].ContainsKey("學年成績"))
                //                    subjectScore[key].Add("學年成績", info.Score.ToString());

                //            }
                //        }
                //    }
                //}

                mergeKeyValue.Add("科目成績起始位置", new object[] { subjectScore, resitSign, repeatSign,var });
                //mergeKeyValue.Add("取得學分數", "學期" + (schoolyearscore ? "/學年" : "") + "取得學分數");
                //mergeKeyValue.Add("名次", "");
                //mergeKeyValue.Add("學分數", thisSemesterTotalCredit.ToString());
                //mergeKeyValue.Add("累計學分數", beforeSemesterTotalCredit.ToString());

                #endregion

                #region 分項成績

                Dictionary<string, Dictionary<string, string>> entryScore = new Dictionary<string, Dictionary<string, string>>();

                foreach (SemesterEntryScoreInfo info in var.SemesterEntryScoreList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester)
                    {
                        string entry = info.Entry;
                        if (!entryScore.ContainsKey(entry))
                            entryScore.Add(entry, new Dictionary<string, string>());
                        entryScore[entry].Add("分數", info.Score.ToString());
                    }
                }

                //如果是下學期，就多列印學年德行成績。
                if (semester == 2)
                {
                    foreach (SchoolYearEntryScoreInfo info in var.SchoolYearEntryScoreList)
                    {
                        if (info.SchoolYear == schoolyear)
                        {
                            string entry = info.Entry;

                            if (entry == "德行")
                            {
                                entryScore.Add("學年德行成績", new Dictionary<string, string>());
                                entryScore["學年德行成績"].Add("分數", info.Score.ToString());
                            }
                        }
                    }
                }

                SemesterEntryRating rating = new SemesterEntryRating(var);

                Dictionary<string, string> totalCredit = new Dictionary<string, string>();
                totalCredit.Add("學業成績名次", rating.GetPlace(schoolyear, semester));
                totalCredit.Add("學期取得學分數", thisSemesterTotalCredit.ToString());
                totalCredit.Add("累計取得學分數", beforeSemesterTotalCredit.ToString());

                mergeKeyValue.Add("分項成績起始位置", new object[] { entryScore, totalCredit, over100 });

                #endregion

                #region 缺曠紀錄

                Dictionary<string, int> absenceInfo = new Dictionary<string, int>();

                foreach (string periodType in userType.Keys)
                {
                    foreach (string absence in userType[periodType])
                    {
                        if (!absenceInfo.ContainsKey(periodType + "_" + absence))
                            absenceInfo.Add(periodType + "_" + absence, 0);
                    }
                }

                foreach (AttendanceInfo info in var.AttendanceList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester)
                    {
                        if (PeriodTypeDic.ContainsKey(info.Period)) //2011/1/25 by dylan
                        {
                            if (absenceInfo.ContainsKey(PeriodTypeDic[info.Period] + "_" + info.Absence))
                                absenceInfo[PeriodTypeDic[info.Period] + "_" + info.Absence]++;
                        }
                    }
                }

                mergeKeyValue.Add("缺曠紀錄", new object[] { userType, absenceInfo });

                #endregion

                eachStudentDoc.MailMerge.MergeField += new Aspose.Words.Reporting.MergeFieldEventHandler(MailMerge_MergeField);
                eachStudentDoc.MailMerge.RemoveEmptyParagraphs = true;

                List<string> keys = new List<string>();
                List<object> values = new List<object>();

                foreach (string key in mergeKeyValue.Keys)
                {
                    keys.Add(key);
                    values.Add(mergeKeyValue[key]);
                }
                eachStudentDoc.MailMerge.Execute(keys.ToArray(), values.ToArray());

                doc.Sections.Add(doc.ImportNode(eachStudentDoc.Sections[0], true));

                //回報進度
                _BGWSemesterScoreReport.ReportProgress((int)(currentStudent++ * 100.0 / totalStudent));
            }

            #endregion

            e.Result = doc;
        }

        void MailMerge_MergeField(object sender, Aspose.Words.Reporting.MergeFieldEventArgs e)
        {
            #region 科目成績

            if (e.FieldName == "科目成績起始位置")
            {
                object[] objectValue = (object[])e.FieldValue;
                Dictionary<SemesterSubjectScoreInfo, Dictionary<string, string>> subjectScore = (Dictionary<SemesterSubjectScoreInfo, Dictionary<string, string>>)objectValue[0];
                string resitSign = (string)objectValue[1];
                string repeatSign = (string)objectValue[2];
                StudentRecord studRec = (StudentRecord)objectValue[3];

                DocumentBuilder builder = new DocumentBuilder(e.Document);
                builder.MoveToField(e.Field, false);

                Table SSTable = ((Row)((Cell)builder.CurrentParagraph.ParentNode).ParentRow).ParentTable;

                int SSRowNumber = SSTable.Rows.Count - 1;
                int SSTableRowIndex = 1;
                int SSTableColIndex = 0;
                int MaxSubjectCount = SSRowNumber * 2;

                try
                {                    
                    // 當科目數超過範本可存放數，不列入處理
                    if (subjectScore.Keys.Count > MaxSubjectCount)
                    {
                        if (_ErrorMessage.Length < 1)
                        {
                            _ErrorMessage.AppendLine("產生資料發生錯誤：學生成績科目數超過範本可顯示科目數:"+MaxSubjectCount+" ，請調整範本科目數後再列印");
                        }

                        string className = "";
                        if (studRec.RefClass != null)
                            className = studRec.RefClass.ClassName;
                        string ErrMsg = "學號：" + studRec.StudentNumber + ",班級：" + className + "座號：" + studRec.SeatNo + ",姓名：" + studRec.StudentName + ", 學生成績科目數：" + subjectScore.Keys.Count;
                        _ErrorMessage.AppendLine(ErrMsg);
                    }
                    else
                    {
                        List<SemesterSubjectScoreInfo> sortList = new List<SemesterSubjectScoreInfo>();
                        sortList.AddRange(subjectScore.Keys);
                        sortList.Sort(SortBySemesterSubjectScoreInfo);

                        foreach (SemesterSubjectScoreInfo info in sortList)
                        {
                            Runs runs = SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex].Paragraphs[0].Runs;
                            runs.Add(new Run(e.Document));
                            runs[runs.Count - 1].Text = subjectScore[info]["科目"] + ((string.IsNullOrEmpty(subjectScore[info]["級別"])) ? "" : (" (" + subjectScore[info]["級別"] + ")"));
                            runs[runs.Count - 1].Font.Size = 10;
                            runs[runs.Count - 1].Font.Name = "新細明體";

                            SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 1].Paragraphs[0].Runs.Add(new Run(e.Document, subjectScore[info]["必修"] + subjectScore[info]["學分"]));
                            SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 1].Paragraphs[0].Runs[0].Font.Size = 10;
                            SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 2].Paragraphs[0].Runs.Add(new Run(e.Document, subjectScore[info]["分數"]));
                            SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 2].Paragraphs[0].Runs[0].Font.Size = 10;

                            int colshift = 0;
                            string re = "";
                            if (subjectScore[info].ContainsKey("補考"))
                            {
                                if (subjectScore[info]["補考"] == "是")
                                    re = resitSign;
                                else if (subjectScore[info]["補考"] == "否")
                                    re = repeatSign;
                            }

                            SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 3 + colshift].Paragraphs[0].Runs.Add(new Run(e.Document, re));
                            SSTable.Rows[SSTableRowIndex].Cells[SSTableColIndex + 3 + colshift].Paragraphs[0].Runs[0].Font.Size = 10;

                            SSTableRowIndex++;
                            if (SSTableRowIndex > SSRowNumber)
                            {
                                SSTableRowIndex = 1;
                                SSTableColIndex += 4;
                            }
                        }                        
                    }
                    e.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    SmartSchool.ExceptionHandler.BugReporter.ReportException(ex,true);                
                }
            }

            #endregion

            #region 分項成績

            if (e.FieldName == "分項成績起始位置")
            {
                object[] objectValue = (object[])e.FieldValue;
                Dictionary<string, Dictionary<string, string>> entryScore = (Dictionary<string, Dictionary<string, string>>)objectValue[0];
                Dictionary<string, string> totalCredit = (Dictionary<string, string>)objectValue[1];
                bool over100 = (bool)objectValue[2];

                DocumentBuilder builder = new DocumentBuilder(e.Document);
                builder.MoveToField(e.Field, false);

                Table ESTable = ((Row)((Cell)builder.CurrentParagraph.ParentNode).ParentRow).ParentTable;

                int ESRowNumber = ESTable.Rows.Count - 1;
                int ESTableRowIndex = 1;
                int ESTableColIndex = 0;

                List<string> sortList = new List<string>();
                sortList.AddRange(entryScore.Keys);
                sortList.Sort(SortByEntryName);

                foreach (string entry in sortList)
                {
                    // 先過濾分項原始)
                    if (entry.Contains("(原始)"))
                        continue;

                    string semesterDegree = "";
                    if (entry == "德行" || entry == "學年德行成績")
                    {
                        decimal moralScore = decimal.Parse(entryScore[entry]["分數"]);
                        if (!over100 && moralScore > 100)
                            entryScore[entry]["分數"] = "100";
                        semesterDegree = " / " + ParseLevel(moralScore);
                    }

                    Runs runs = ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex].Paragraphs[0].Runs;

                    runs.Add(new Run(e.Document, ToDisplayName(entry)));
                    runs[runs.Count - 1].Font.Size = 10;
                    ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex + 1].Paragraphs[0].Runs.Add(new Run(e.Document, entryScore[entry]["分數"] + semesterDegree));
                    ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex + 1].Paragraphs[0].Runs[0].Font.Size = 10;

                    ESTableRowIndex++;
                    if (ESTableRowIndex > ESRowNumber)
                    {
                        ESTableRowIndex = 1;
                        ESTableColIndex += 2;
                    }
                }

                foreach (string key in totalCredit.Keys)
                {
                    Runs runs = ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex].Paragraphs[0].Runs;

                    runs.Add(new Run(e.Document, key));
                    runs[runs.Count - 1].Font.Size = 10;
                    ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex + 1].Paragraphs[0].Runs.Add(new Run(e.Document, totalCredit[key]));
                    ESTable.Rows[ESTableRowIndex].Cells[ESTableColIndex + 1].Paragraphs[0].Runs[0].Font.Size = 10;

                    ESTableRowIndex++;
                    if (ESTableRowIndex > ESRowNumber)
                    {
                        ESTableRowIndex = 1;
                        ESTableColIndex += 2;
                    }
                }

                e.Text = string.Empty;
            }

            #endregion

            #region 缺曠紀錄

            if (e.FieldName == "缺曠紀錄")
            {
                object[] objectValue = (object[])e.FieldValue;

                if ((Dictionary<string, List<string>>)objectValue[0] == null || ((Dictionary<string, List<string>>)objectValue[0]).Count == 0)
                {
                    e.Text = string.Empty;
                    return;
                }

                Dictionary<string, List<string>> userType = (Dictionary<string, List<string>>)objectValue[0];
                Dictionary<string, int> absenceInfo = (Dictionary<string, int>)objectValue[1];

                #region 產生缺曠紀錄表格

                DocumentBuilder builder = new DocumentBuilder(e.Document);
                builder.MoveToField(e.Field, false);

                int ARowNumber = 3;
                double AWidth = 0;
                double AHeight = 0;
                double ARowHeight = 0;

                int AColumn = 0;
                double AMicroColumn = 0;

                foreach (string periodType in userType.Keys)
                {
                    AColumn += userType[periodType].Count;
                }

                Cell ACell = (Cell)builder.CurrentParagraph.ParentNode;

                AWidth = ACell.CellFormat.Width;
                AHeight = (ACell.ParentNode as Row).RowFormat.Height;
                ARowHeight = (AHeight) / (ARowNumber);
                AMicroColumn = AWidth / (double)AColumn;

                builder.StartTable();
                builder.CellFormat.ClearFormatting();
                builder.RowFormat.HeightRule = HeightRule.Exactly;
                builder.RowFormat.Height = ARowHeight;
                builder.RowFormat.Alignment = RowAlignment.Center;
                builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
                builder.CellFormat.LeftPadding = 0.0;
                builder.CellFormat.RightPadding = 0.0;
                builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
                builder.ParagraphFormat.LineSpacing = 10;
                builder.Font.Size = 10;

                foreach (string periodType in userType.Keys)
                {
                    builder.InsertCell().CellFormat.Width = AMicroColumn * userType[periodType].Count;
                    builder.Write(periodType);
                }

                builder.EndRow();

                foreach (string periodType in userType.Keys)
                {
                    foreach (string absence in userType[periodType])
                    {
                        builder.InsertCell().CellFormat.Width = AMicroColumn;
                        builder.Write(absence);
                    }
                }

                builder.EndRow();

                foreach (string periodType in userType.Keys)
                {
                    foreach (string absence in userType[periodType])
                    {
                        builder.InsertCell().CellFormat.Width = AMicroColumn;
                        builder.Write(absenceInfo[periodType + "_" + absence].ToString());
                    }
                }

                builder.EndRow();

                Table ATable = builder.EndTable();

                //去除表格四邊的線
                foreach (Cell cell in ATable.FirstRow.Cells)
                    cell.CellFormat.Borders.Top.LineStyle = LineStyle.None;

                foreach (Cell cell in ATable.LastRow.Cells)
                    cell.CellFormat.Borders.Bottom.LineStyle = LineStyle.None;

                foreach (Row row in ATable.Rows)
                {
                    row.FirstCell.CellFormat.Borders.Left.LineStyle = LineStyle.None;
                    row.LastCell.CellFormat.Borders.Right.LineStyle = LineStyle.None;
                }

                #endregion

                #region 填入缺曠紀錄資料
                #endregion

                e.Text = string.Empty;
            }

            #endregion
        }

        private static string ToDisplayName(string entry)
        {
            switch (entry)
            {
                case "學業":
                    return "學期學業成績";
                case "德行":
                    return "學期德行成績";
                default:
                    return entry;
            }
        }

        /// <summary>
        /// 缺曠節次類型對照清單(By dylan)
        /// </summary>
        private Dictionary<string, string> PeriodTypeDic = new Dictionary<string, string>();

        /// <summary>
        /// 取得節次名稱對照節次類型(by dylan)
        /// </summary>
        /// <returns></returns>
        private void GetPeriodType()
        {
            PeriodTypeDic.Clear();
            foreach (SHPeriodMappingInfo period in SHSchool.Data.SHPeriodMapping.SelectAll())
            {
                if (!PeriodTypeDic.ContainsKey(period.Name))
                {
                    PeriodTypeDic.Add(period.Name, period.Type);
                }

            }
        }
    }

    /// <summary>
    /// 只處理學業成績的排名。
    /// </summary>
    class SemesterEntryRating
    {
        private XmlElement _sems_ratings = null;

        public SemesterEntryRating(StudentRecord student)
        {
            if (student.Fields.ContainsKey("SemesterEntryClassRating"))
                _sems_ratings = student.Fields["SemesterEntryClassRating"] as XmlElement;
        }

        public string GetPlace(int schoolYear, int semester)
        {
            if (_sems_ratings == null) return string.Empty;

            string path = string.Format("SemesterEntryScore[SchoolYear='{0}' and Semester='{1}']/ClassRating/Rating/Item[@分項='學業']/@排名", schoolYear, semester);
            XmlNode result = _sems_ratings.SelectSingleNode(path);

            if (result != null)
                return result.InnerText;
            else
                return string.Empty;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using SmartSchool.Customization.Data;
using Aspose.Cells;
using SmartSchool.Customization.Data.StudentExtension;
using System.IO;
using SmartSchool.Common;
using FISCA.Presentation;

namespace SmartSchool.Evaluation.Reports
{
    class ResitListBySubject
    {
        BackgroundWorker _BGWResitList;

        public ResitListBySubject()
        {
            int schoolyear = 0;
            int semester = 0;

            SelectSemesterForm form = new SelectSemesterForm("補考名單-依科目");
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                schoolyear = form.SchoolYear;
                semester = form.Semester;
            }
            else
                return;

            _BGWResitList = new BackgroundWorker();
            _BGWResitList.WorkerReportsProgress = true;
            _BGWResitList.DoWork += new DoWorkEventHandler(_BGWResitList_DoWork);
            _BGWResitList.ProgressChanged += new ProgressChangedEventHandler(_BGWResitList_ProgressChanged);
            _BGWResitList.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BGWResitList_RunWorkerCompleted);
            _BGWResitList.RunWorkerAsync(new object[] { schoolyear, semester });
        }

        private int SortBySemesterSubjectScore(string a, string b)
        {
            string[] aSplits = a.Split('_');
            string[] bSplits = b.Split('_');
            string aSubject = string.IsNullOrEmpty(aSplits[0]) ? "" : aSplits[0];
            string aLevel = string.IsNullOrEmpty(aSplits[1]) ? "" : aSplits[1];
            string aCredit = string.IsNullOrEmpty(aSplits[2]) ? "" : aSplits[2];
            string bSubject = string.IsNullOrEmpty(bSplits[0]) ? "" : bSplits[0];
            string bLevel = string.IsNullOrEmpty(bSplits[1]) ? "" : bSplits[1];
            string bCredit = string.IsNullOrEmpty(bSplits[2]) ? "" : bSplits[2];

            if (aSubject == bSubject)
            {
                if (aLevel == bLevel)
                {
                    return SortBySubjectName(aCredit, bCredit);
                }
                return SortBySubjectName(aLevel, bLevel);
            }
            return SortBySubjectName(aSubject, bSubject);
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
            int x = 0;
            switch (a1)
            {
                case "國":
                    x = 1;
                    break;
                case "英":
                    x = 2;
                    break;
                case "數":
                    x = 3;
                    break;
                case "物":
                    x = 4;
                    break;
                case "化":
                    x = 5;
                    break;
                case "生":
                    x = 6;
                    break;
                case "基":
                    x = 7;
                    break;
                case "歷":
                    x = 8;
                    break;
                case "地":
                    x = 9;
                    break;
                case "公":
                    x = 10;
                    break;
                case "文":
                    x = 11;
                    break;
                case "礎":
                    x = 12;
                    break;
                case "Ⅰ":
                    x = 51; break;
                case "Ⅱ":
                    x = 52; break;
                case "Ⅲ":
                    x = 53; break;
                case "Ⅳ":
                    x = 54; break;
                case "Ⅴ":
                    x = 55; break;
                case "Ⅵ":
                    x = 56; break;
                case "Ⅶ":
                    x = 57; break;
                case "Ⅷ":
                    x = 58; break;
                case "Ⅸ":
                    x = 59; break;
                case "Ⅹ":
                    x = 60; break;
                case "1":
                    x = 61; break;
                case "2":
                    x = 62; break;
                case "3":
                    x = 63; break;
                case "4":
                    x = 64; break;
                case "5":
                    x = 65; break;
                case "6":
                    x = 66; break;
                case "7":
                    x = 67; break;
                case "8":
                    x = 68; break;
                case "9":
                    x = 69; break;
                default:
                    x = 100;
                    break;
            }
            return x;
        }

        private string GetNumber(int p)
        {
            string levelNumber;
            switch (p)
            {
                #region 對應levelNumber
                case 0:
                    levelNumber = "";
                    break;
                case 1:
                    levelNumber = "Ⅰ";
                    break;
                case 2:
                    levelNumber = "Ⅱ";
                    break;
                case 3:
                    levelNumber = "Ⅲ";
                    break;
                case 4:
                    levelNumber = "Ⅳ";
                    break;
                case 5:
                    levelNumber = "Ⅴ";
                    break;
                case 6:
                    levelNumber = "Ⅵ";
                    break;
                case 7:
                    levelNumber = "Ⅶ";
                    break;
                case 8:
                    levelNumber = "Ⅷ";
                    break;
                case 9:
                    levelNumber = "Ⅸ";
                    break;
                case 10:
                    levelNumber = "Ⅹ";
                    break;
                default:
                    levelNumber = "" + (p);
                    break;
                #endregion
            }
            return levelNumber;
        }

        private void Completed(string inputReportName, Workbook inputWorkbook)
        {
            string reportName = inputReportName;

            string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".xls");

            Workbook wb = inputWorkbook;

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
                wb.Save(path, FileFormatType.Excel2003);
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                System.Windows.Forms.SaveFileDialog sd = new System.Windows.Forms.SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".xls";
                sd.Filter = "Excel檔案 (*.xls)|*.xls|所有檔案 (*.*)|*.*";
                if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        wb.Save(sd.FileName, FileFormatType.Excel2003);

                    }
                    catch
                    {
                        MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
            }
        }

        void _BGWResitList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MotherForm.SetStatusBarMessage("補考名單產生完成");
            Completed("補考名單", (Workbook)e.Result);
        }

        void _BGWResitList_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MotherForm.SetStatusBarMessage("補考名單產生中", e.ProgressPercentage);
            //Console.WriteLine(e.ProgressPercentage.ToString());
        }

        void _BGWResitList_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] objectValue = (object[])e.Argument;
            int schoolyear = (int)objectValue[0];
            int semester = (int)objectValue[1];

            _BGWResitList.ReportProgress(0);

            #region 取得所有學生以及補考資訊

            AccessHelper helper = new AccessHelper();
            List<StudentRecord> allStudents = new List<StudentRecord>();
            List<ClassRecord> allClasses = helper.ClassHelper.GetAllClass();
            WearyDogComputer computer = new WearyDogComputer();

            double currentClass = 1;
            double totalClasses = allClasses.Count;

            foreach (ClassRecord aClass in allClasses)
            {
                List<StudentRecord> classStudents = aClass.Students;
                computer.FillSemesterSubjectScoreInfoWithResit(helper, true, classStudents);
                allStudents.AddRange(classStudents);

                _BGWResitList.ReportProgress((int)(currentClass++ * 90.0 / totalClasses));
            }

            double currentStudent = 1;
            double totalStudents = allStudents.Count;

            #endregion

            #region 產生表格並填入資料

            Workbook template = new Workbook();
            template.Open(new MemoryStream(Properties.Resources.補考名單_依科目), FileFormatType.Excel2003);
            Workbook wb = new Workbook();
            wb.Copy(template);
            Worksheet ws = wb.Worksheets[0];

            Range eachSubject = template.Worksheets[0].Cells.CreateRange(0, 4, false);
            Range eachRow = template.Worksheets[0].Cells.CreateRange(4, 1, false);

            int index = 0;

            Dictionary<string, Dictionary<string, string>> subjectInfo = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, List<Dictionary<string, string>>> subjectStudentList = new Dictionary<string, List<Dictionary<string, string>>>();

            foreach (StudentRecord aStudent in allStudents)
            {
                string className = aStudent.RefClass.ClassName;
                string seatNo = aStudent.SeatNo;
                string studentName = aStudent.StudentName;
                string studentNumber = aStudent.StudentNumber;

                //aStudent.SemesterSubjectScoreList.Sort(SortBySemesterSubjectScore);

                foreach (SemesterSubjectScoreInfo info in aStudent.SemesterSubjectScoreList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester && !info.Pass)
                    {
                        if (info.Detail.GetAttribute("達補考標準") == "是")
                        {
                            string sl = info.Subject + "_" + info.Level + "_" + info.CreditDec();

                            if (!subjectInfo.ContainsKey(sl))
                            {
                                subjectInfo.Add(sl, new Dictionary<string, string>());
                                subjectInfo[sl].Add("科目", info.Subject);
                                subjectInfo[sl].Add("級別", info.Level);
                                subjectInfo[sl].Add("學分", info.CreditDec().ToString());
                            }

                            if (!subjectStudentList.ContainsKey(sl))
                                subjectStudentList.Add(sl, new List<Dictionary<string, string>>());

                            Dictionary<string, string> data = new Dictionary<string, string>();
                            data.Add("班級", className);
                            data.Add("座號", seatNo);
                            data.Add("姓名", studentName);
                            data.Add("學號", studentNumber);
                            data.Add("必選修", info.Require ? "必修" : "選修");
                            data.Add("校部訂", info.Detail.HasAttribute("修課校部訂") ? info.Detail.GetAttribute("修課校部訂") : "");
                            data.Add("補考標準", info.Detail.HasAttribute("補考標準") ? info.Detail.GetAttribute("補考標準") : "");
                            data.Add("原始成績", info.Detail.HasAttribute("原始成績") ? info.Detail.GetAttribute("原始成績") : "");

                            subjectStudentList[sl].Add(data);
                        }
                    }
                }

                _BGWResitList.ReportProgress(90 + (int)(currentStudent++ * 10.0 / totalStudents));
            }
            List<string> sortedList = new List<string>();
            sortedList.AddRange(subjectStudentList.Keys);
            sortedList.Sort(SortBySemesterSubjectScore);

            foreach (string key in sortedList)
            {
                int level;
                string levelString = "";
                if (!string.IsNullOrEmpty(subjectInfo[key]["級別"]) && int.TryParse(subjectInfo[key]["級別"], out level))
                    levelString = GetNumber(level);
                string sl = subjectInfo[key]["科目"] + levelString;

                ws.Cells.CreateRange(index, 4, false).Copy(eachSubject);
                ws.Cells[index, 0].PutValue(SystemInformation.SchoolChineseName + " " + schoolyear + " 學年度 第 " + semester + " 學期 學生補考名單");
                ws.Cells[index + 1, 2].PutValue(sl);
                ws.Cells[index + 1, 7].PutValue(subjectInfo[key]["學分"]);
                index += 4;

                foreach (Dictionary<string, string> dict in subjectStudentList[key])
                {
                    ws.Cells.CreateRange(index, 1, false).Copy(eachRow);
                    ws.Cells[index, 0].PutValue(dict["班級"]);
                    ws.Cells[index, 1].PutValue(dict["座號"]);
                    ws.Cells[index, 2].PutValue(dict["姓名"]);
                    ws.Cells[index, 3].PutValue(dict["學號"]);
                    ws.Cells[index, 4].PutValue(dict["必選修"]);
                    ws.Cells[index, 5].PutValue(dict["校部訂"]);
                    ws.Cells[index, 6].PutValue(dict["補考標準"]);
                    ws.Cells[index, 7].PutValue(dict["原始成績"]);

                    index++;
                }
                index++;
            }

            #endregion

            e.Result = wb;
        }
    }
}

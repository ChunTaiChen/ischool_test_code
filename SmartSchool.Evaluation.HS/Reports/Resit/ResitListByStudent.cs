using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using Aspose.Cells;
using System.IO;
using SmartSchool.Common;
using FISCA.Presentation;

namespace SmartSchool.Evaluation.Reports
{
    class ResitListByStudent
    {
        BackgroundWorker _BGWResitList;

        public ResitListByStudent()
        {
            int schoolyear = 0;
            int semester = 0;

            SelectSemesterForm form = new SelectSemesterForm("補考名單-依學生");
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

        private int SortBySemesterSubjectScore(SemesterSubjectScoreInfo a, SemesterSubjectScoreInfo b)
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
                default:
                    x = 12;
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
            Console.WriteLine(e.ProgressPercentage.ToString());
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
            template.Open(new MemoryStream(Properties.Resources.補考名單_依學生), FileFormatType.Excel2003);
            Workbook wb = new Workbook();
            wb.Copy(template);
            Worksheet ws = wb.Worksheets[0];

            Range eachRow = template.Worksheets[0].Cells.CreateRange(2, 1, false);

            ws.Cells[0, 0].PutValue(SystemInformation.SchoolChineseName + " " + schoolyear + " 學年度 第 " + semester + " 學期 學生補考名單");
            int index = 2;

            foreach (StudentRecord aStudent in allStudents)
            {
                string className = aStudent.RefClass.ClassName;
                string seatNo = aStudent.SeatNo;
                string studentNumber = aStudent.StudentNumber;
                string studentName = aStudent.StudentName;

                aStudent.SemesterSubjectScoreList.Sort(SortBySemesterSubjectScore);

                foreach (SemesterSubjectScoreInfo info in aStudent.SemesterSubjectScoreList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester && !info.Pass)
                    {
                        if (info.Detail.GetAttribute("達補考標準") == "是")
                        {
                            string subject = info.Subject;
                            string levelString = "";
                            int level;
                            if(int.TryParse(info.Level, out level))
                                levelString = GetNumber(level);
                            string credit = info.CreditDec().ToString();
                            string score = info.Detail.GetAttribute("原始成績");
                            string limit = info.Detail.GetAttribute("補考標準");

                            ws.Cells.CreateRange(index, 1, false).Copy(eachRow);
                            ws.Cells[index, 0].PutValue(className);
                            ws.Cells[index, 1].PutValue(seatNo);
                            ws.Cells[index, 2].PutValue(studentNumber);
                            ws.Cells[index, 3].PutValue(studentName);
                            ws.Cells[index, 4].PutValue(subject + (string.IsNullOrEmpty(levelString) ? "" : " " + levelString));
                            ws.Cells[index, 5].PutValue(credit);
                            ws.Cells[index, 6].PutValue(score);
                            ws.Cells[index, 7].PutValue(limit);

                            index++;
                        }
                    }
                }

                _BGWResitList.ReportProgress(90 + (int)(currentStudent++ * 10.0 / totalStudents));
            }

            #endregion

            e.Result = wb;
        }
    }
}

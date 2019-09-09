using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Aspose.Cells;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using System.IO;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.Common;
using FISCA.Presentation;

namespace SmartSchool.Evaluation.Reports
{
    class ResitScoreImport
    {
        private BackgroundWorker _BWResitScoreImport;
        private ResitSubjectMachine machine = new ResitSubjectMachine();

        public ResitScoreImport()
        {
            SelectSemesterForm form = new SelectSemesterForm("補考成績匯入表");
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _BWResitScoreImport = new BackgroundWorker();
                _BWResitScoreImport.DoWork += new DoWorkEventHandler(_BWResitScoreImport_DoWork);
                _BWResitScoreImport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BWResitScoreImport_RunWorkerCompleted);
                _BWResitScoreImport.ProgressChanged += new ProgressChangedEventHandler(_BWResitScoreImport_ProgressChanged);
                _BWResitScoreImport.WorkerReportsProgress = true;
                _BWResitScoreImport.RunWorkerAsync(new object[] { form.SchoolYear, form.Semester });
            }
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

        void _BWResitScoreImport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MotherForm.SetStatusBarMessage("補考成績匯入表產生中", e.ProgressPercentage);
        }

        void _BWResitScoreImport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MotherForm.SetStatusBarMessage("補考成績匯入表產生完成");
            Completed("補考成績匯入表", (Workbook)e.Result);
        }

        void _BWResitScoreImport_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] objectValue = (object[])e.Argument;
            int schoolyear = (int)objectValue[0];
            int semester = (int)objectValue[1];

            _BWResitScoreImport.ReportProgress(0);

            #region 取得所有學生以及補考資訊

            AccessHelper helper = new AccessHelper();
            List<StudentRecord> allStudents = new List<StudentRecord>();
            List<ClassRecord> allClasses = helper.ClassHelper.GetAllClass();
            WearyDogComputer computer = new WearyDogComputer();

            double currentClass = 1;
            double totalClasses = allClasses.Count;

            List<StudentRecord> loadCourseStudents = new List<StudentRecord>();

            foreach (ClassRecord aClass in allClasses)
            {
                List<StudentRecord> classStudents = aClass.Students;
                computer.FillSemesterSubjectScoreInfoWithResit(helper, true, classStudents);
                helper.StudentHelper.FillField("及格標準", classStudents);

                allStudents.AddRange(classStudents);
                foreach (StudentRecord aStudent in classStudents)
                {
                    foreach (SemesterSubjectScoreInfo info in aStudent.SemesterSubjectScoreList)
                    {
                        if (info.SchoolYear == schoolyear && info.Semester == semester)
                        {
                            if (!info.Pass && info.Detail.HasAttribute("達補考標準") && info.Detail.GetAttribute("達補考標準") == "是")
                            {
                                if (!loadCourseStudents.Contains(aStudent))
                                    loadCourseStudents.Add(aStudent);
                            }
                        }
                    }
                }
                _BWResitScoreImport.ReportProgress((int)(currentClass++ * 90.0 / totalClasses));
            }

            MultiThreadWorker<StudentRecord> multiThreadWorker = new MultiThreadWorker<StudentRecord>();
            multiThreadWorker.MaxThreads = 3;
            multiThreadWorker.PackageSize = 80;
            multiThreadWorker.PackageWorker += new EventHandler<PackageWorkEventArgs<StudentRecord>>(multiThreadWorker_PackageWorker);
            multiThreadWorker.Run(loadCourseStudents, new object[] { schoolyear, semester, helper });

            double currentStudent = 1;
            double totalStudents = allStudents.Count;

            #endregion

            #region 產生表格並填入資料

            Workbook template = new Workbook();
            template.Open(new MemoryStream(Properties.Resources.補考成績匯入), FileFormatType.Excel2003);
            Workbook wb = new Workbook();
            wb.Copy(template);
            Worksheet ws = wb.Worksheets[0];

            Dictionary<string, int> columnIndexTable = new Dictionary<string, int>();

            int headerIndex = 0;
            string headerString = template.Worksheets[0].Cells[0, headerIndex].StringValue;
            while (!string.IsNullOrEmpty(headerString))
            {
                columnIndexTable.Add(headerString, headerIndex);
                headerString = template.Worksheets[0].Cells[0, ++headerIndex].StringValue;
            }

            foreach (StudentRecord aStudent in loadCourseStudents)
            {
                foreach (SemesterSubjectScoreInfo info in aStudent.SemesterSubjectScoreList)
                {
                    if (info.SchoolYear == schoolyear && info.Semester == semester)
                    {
                        if (!info.Pass && info.Detail.HasAttribute("達補考標準") && info.Detail.GetAttribute("達補考標準") == "是")
                        {
                            #region 搜尋授課教師
                            foreach (StudentAttendCourseRecord attendCourse in aStudent.AttendCourseList)
                            {
                                if (attendCourse.SchoolYear == schoolyear && attendCourse.Semester == semester && attendCourse.Subject == info.Subject && attendCourse.SubjectLevel == info.Level)
                                {
                                    List<TeacherRecord> teachers = helper.TeacherHelper.GetLectureTeacher(attendCourse);
                                    if (teachers.Count > 0)
                                        info.Detail.SetAttribute("授課教師", teachers[0].TeacherName);
                                }
                            }
                            #endregion
                            machine.AddSubject(aStudent, info);
                        }
                    }
                }

                _BWResitScoreImport.ReportProgress(90 + (int)(currentStudent++ * 10.0 / totalStudents));
            }

            machine.Sort();

            int index = 1;
            foreach (Dictionary<string, string> info in machine.GetSubjects())
            {
                foreach (string key in info.Keys)
                {
                    ws.Cells[index, columnIndexTable[key]].PutValue(info[key]);
                }
                index++;
            }

            #endregion

            e.Result = wb;
        }

        void multiThreadWorker_PackageWorker(object sender, PackageWorkEventArgs<StudentRecord> e)
        {
            int schoolyear = (int)((object[])e.Argument)[0];
            int semester = (int)((object[])e.Argument)[1];
            AccessHelper helper = (AccessHelper)((object[])e.Argument)[2];
            helper.StudentHelper.FillAttendCourse(schoolyear, semester, e.List);
        }
    }

    /// <summary>
    /// 學生資料與補考科目
    /// </summary>
    class ResitSubjectMachine
    {
        List<Dictionary<string, string>> subjectInfo = new List<Dictionary<string, string>>();

        public ResitSubjectMachine()
        {
        }

        private int SortBySemesterSubject(Dictionary<string, string> a, Dictionary<string, string> b)
        {
            if (a["科目"] == b["科目"])
            {
                if (a["科目級別"] == b["科目級別"])
                {
                    return SortBySubjectName(a["學分數"], b["學分數"]);
                }
                return SortBySubjectName(a["科目級別"], b["科目級別"]);
            }
            return SortBySubjectName(a["科目"], b["科目"]);
            //return SortBySubjectName(a["科目"] + "_" + a["科目級別"] + "_" + a["學分數"], b["科目"] + "_" + b["科目級別"] + "_" + b["學分數"]);
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
                case "概":
                    x = 13;
                    break;
                case "學":
                    x = 14;
                    break;
                case "邏":
                    x = 15;
                    break;
                case "電":
                    x = 16;
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
                    x = int.MaxValue;
                    break;
            }
            return x;
        }

        public void AddSubject(StudentRecord student, SemesterSubjectScoreInfo info)
        {
            Dictionary<string, string> aInfo = new Dictionary<string, string>();
            aInfo.Add("學號", student.StudentNumber);
            aInfo.Add("班級", (student.RefClass != null) ? student.RefClass.ClassName : "");
            aInfo.Add("座號", student.SeatNo);
            aInfo.Add("科別", student.Department);
            aInfo.Add("姓名", student.StudentName);
            aInfo.Add("科目", info.Subject);
            aInfo.Add("科目級別", info.Level);
            aInfo.Add("學年度", info.SchoolYear.ToString());
            aInfo.Add("學期", info.Semester.ToString());
            aInfo.Add("學分數", info.CreditDec().ToString());
            aInfo.Add("成績年級", info.GradeYear.ToString());
            aInfo.Add("必選修", info.Require ? "必修" : "選修");
            aInfo.Add("校部訂", info.Detail.HasAttribute("修課校部訂") ? info.Detail.GetAttribute("修課校部訂") : "");
            aInfo.Add("原始成績", info.Detail.HasAttribute("原始成績") ? info.Detail.GetAttribute("原始成績") : "");
            aInfo.Add("補考標準", info.Detail.HasAttribute("補考標準") ? info.Detail.GetAttribute("補考標準") : "");
            aInfo.Add("補考成績", info.Detail.HasAttribute("補考成績") ? info.Detail.GetAttribute("補考成績") : "");
            
            Dictionary<int, decimal> std = student.Fields["及格標準"] as Dictionary<int, decimal>;
            aInfo.Add("及格標準", std[info.GradeYear].ToString());
            aInfo.Add("授課教師", info.Detail.HasAttribute("授課教師") ? info.Detail.GetAttribute("授課教師") : "");
            aInfo.Add("取得學分", "");

            subjectInfo.Add(aInfo);
        }

        public List<Dictionary<string, string>> GetSubjects()
        {
            return subjectInfo;
        }

        public void Sort()
        {
            subjectInfo.Sort(SortBySemesterSubject);
        }
    }
}

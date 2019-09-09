using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.Customization.Data;
using Aspose.Cells;
using System.IO;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.Common;
using SmartSchool.Evaluation.Reports.Retake;
using FISCA.Presentation;

namespace SmartSchool.Evaluation.Reports
{
    class RetakeScoreImport
    {
        RepeatSubjectMachine machine = new RepeatSubjectMachine();
        BackgroundWorker _BWRepeatScoreImport = new BackgroundWorker();

        public RetakeScoreImport()
        {
            RetakeSelectSemesterForm form = new RetakeSelectSemesterForm("重修成績匯入表");
            if (form.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            _BWRepeatScoreImport.DoWork += new DoWorkEventHandler(_BWRepeatScoreImport_DoWork);
            _BWRepeatScoreImport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BWRepeatScoreImport_RunWorkerCompleted);
            _BWRepeatScoreImport.ProgressChanged += new ProgressChangedEventHandler(_BWRepeatScoreImport_ProgressChanged);
            _BWRepeatScoreImport.WorkerReportsProgress = true;
            _BWRepeatScoreImport.RunWorkerAsync(new object[] { form.SchoolYear, form.Semester, form.IsPrintAllSemester });
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

        private void _BWRepeatScoreImport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MotherForm.SetStatusBarMessage("重修成績匯入表產生中", e.ProgressPercentage);
        }

        private void _BWRepeatScoreImport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MotherForm.SetStatusBarMessage("重修成績匯入表產生完成");
            Completed("重修成績匯入表", (Workbook)e.Result);
        }

        private void _BWRepeatScoreImport_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] obj = (object[])e.Argument;
            int schoolyear = (int)obj[0];
            int semester = (int)obj[1];
            bool printAll = (bool)obj[2];

            _BWRepeatScoreImport.ReportProgress(0);

            #region 取得所有學生以及補考資訊

            AccessHelper helper = new AccessHelper();
            List<SmartSchool.Customization.Data.StudentRecord> allStudents = new List<SmartSchool.Customization.Data.StudentRecord>();
            List<ClassRecord> allClasses = helper.ClassHelper.GetAllClass();

            double currentClass = 1;
            double totalClasses = allClasses.Count;

            foreach (ClassRecord aClass in allClasses)
            {
                List<SmartSchool.Customization.Data.StudentRecord> classStudents = aClass.Students;

                helper.StudentHelper.FillSemesterSubjectScore(true, classStudents);
                helper.StudentHelper.FillField("及格標準", classStudents);
                allStudents.AddRange(classStudents);

                _BWRepeatScoreImport.ReportProgress((int)(currentClass++ * 90.0 / totalClasses));
            }

            double currentStudent = 1;
            double totalStudents = allStudents.Count;

            #endregion

            #region 產生表格並填入資料

            Workbook template = new Workbook();
            template.Open(new MemoryStream(Properties.Resources.重修成績匯入), FileFormatType.Excel2003);
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

            foreach (SmartSchool.Customization.Data.StudentRecord aStudent in allStudents)
            {
                foreach (SemesterSubjectScoreInfo info in aStudent.SemesterSubjectScoreList)
                {
                    if ((info.SchoolYear == schoolyear && info.Semester == semester) || printAll)
                    {
                        if (info.Pass && machine.IsExistInList(aStudent.StudentNumber, aStudent.StudentName, info.Subject, info.Level))
                        {
                            machine.RemoveSubject(aStudent.StudentName, info.Subject, info.Level);
                        }
                        else if (!info.Pass)
                        {
                            machine.AddSubject(aStudent, info);
                        }
                    }
                }

                _BWRepeatScoreImport.ReportProgress(90 + (int)(currentStudent++ * 10.0 / totalStudents));
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

        /// <summary>
        /// 學生資料與補考科目
        /// </summary>
        class RepeatSubjectMachine
        {
            List<Dictionary<string, string>> subjectInfo = new List<Dictionary<string, string>>();

            List<string> subjectCheckList = new List<string>();

            public RepeatSubjectMachine()
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
                        x = Math.Abs(a1.GetHashCode()) - 842300000;
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

                //int level;
                //string levelString = "";
                //if (int.TryParse(info.Level, out level))
                //{
                //    levelString = GetNumber(level);
                //}

                //aInfo.Add("科目級別", levelString);
                aInfo.Add("科目級別", info.Level);
                aInfo.Add("學年度", info.SchoolYear.ToString());
                aInfo.Add("學期", info.Semester.ToString());
                aInfo.Add("學分數", info.CreditDec().ToString());
                aInfo.Add("成績年級", info.GradeYear.ToString());
                aInfo.Add("必選修", info.Require ? "必修" : "選修");
                aInfo.Add("校部訂", info.Detail.HasAttribute("修課校部訂") ? info.Detail.GetAttribute("修課校部訂") : "");
                aInfo.Add("原始成績", info.Detail.HasAttribute("原始成績") ? info.Detail.GetAttribute("原始成績") : "");
                //aInfo.Add("及格標準", info.Detail.HasAttribute("及格標準") ? info.Detail.GetAttribute("及格標準") : "");

                if (student.Fields.ContainsKey("及格標準") && (student.Fields["及格標準"] as Dictionary<int, decimal>).ContainsKey(info.GradeYear))
                    aInfo.Add("及格標準", (student.Fields["及格標準"] as Dictionary<int, decimal>)[info.GradeYear].ToString());
                else
                    aInfo.Add("及格標準", "");

                aInfo.Add("重修成績", info.Detail.HasAttribute("重修成績") ? info.Detail.GetAttribute("重修成績") : "");
                aInfo.Add("取得學分", "");

                subjectInfo.Add(aInfo);

                if (!subjectCheckList.Contains(student.StudentNumber + ":" + student.StudentName + ":" + info.Subject + ":" + info.Level))
                    subjectCheckList.Add(student.StudentNumber + ":" + student.StudentName + ":" + info.Subject + ":" + info.Level);
            }

            public void RemoveSubject(string name, string subject_name, string level)
            {
                Dictionary<string, string> die = null;
                foreach (Dictionary<string, string> sub in subjectInfo)
                {
                    if (sub["姓名"] == name && sub["科目"] == subject_name && sub["科目級別"] == level)
                    {
                        die = sub;
                        subjectCheckList.Remove(name + ":" + subject_name + ":" + level);
                        break;
                    }
                }
                if (die != null)
                    subjectInfo.Remove(die);
            }

            public bool IsExistInList(string student_number, string name, string subject_name, string level)
            {
                if (subjectCheckList.Contains(student_number + ":" + name + ":" + subject_name + ":" + level))
                    return true;
                return false;
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
}

using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Customization.PlugIn.Report;
using SmartSchool.Customization.PlugIn;
using System.ComponentModel;
using Aspose.Cells;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using System.Xml;
using System.IO;
using SmartSchool.Common;

namespace SmartSchool.Evaluation.Reports
{
    class SemesterMoralScoreTotal
    {
        private BackgroundWorker _BGWSemesterMoralScoresTotal;
        private ButtonAdapter button;

        public SemesterMoralScoreTotal()
        {
            button = new SecureButtonAdapter("Report0280");
            button.Text = "德行成績總表(舊制)";
            button.Path = "學務相關報表";
            button.OnClick += new EventHandler(button_OnClick);
            ClassReport.AddReport(button);
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

        void button_OnClick(object sender, EventArgs e)
        {
            int schoolyear = 0;
            int semester = 0;
            bool over100 = false;
            int sizeIndex = 0;
            Dictionary<string, List<string>> type = null;

            SemesterMoralScoreTotalForm form = new SemesterMoralScoreTotalForm();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                schoolyear = form.SchoolYear;
                semester = form.Semester;
                over100 = form.AllowMoralScoreOver100;
                sizeIndex = form.PaperSize;
                type = form.UserDefinedType;
            }
            else
                return;

            _BGWSemesterMoralScoresTotal = new BackgroundWorker();
            _BGWSemesterMoralScoresTotal.WorkerReportsProgress = true;
            _BGWSemesterMoralScoresTotal.DoWork += new DoWorkEventHandler(_BGWSemesterMoralScoresTotal_DoWork);
            _BGWSemesterMoralScoresTotal.ProgressChanged += new ProgressChangedEventHandler(_BGWSemesterMoralScoresTotal_ProgressChanged);
            _BGWSemesterMoralScoresTotal.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BGWSemesterMoralScoresTotal_RunWorkerCompleted);
            _BGWSemesterMoralScoresTotal.RunWorkerAsync(new object[] { schoolyear, semester, over100, sizeIndex, type });
        }

        void _BGWSemesterMoralScoresTotal_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button.SetBarMessage("德行成績總表產生完成");
            Completed("德行成績總表", (Workbook)e.Result);
        }

        void _BGWSemesterMoralScoresTotal_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            button.SetBarMessage("德行成績總表產生中...", e.ProgressPercentage);
        }

        void _BGWSemesterMoralScoresTotal_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;
            int schoolyear = (int)args[0];
            int semester = (int)args[1];
            bool over100 = (bool)args[2];
            int sizeIndex = (int)args[3];
            Dictionary<string, List<string>> userType = (Dictionary<string, List<string>>)args[4];

            _BGWSemesterMoralScoresTotal.ReportProgress(1);

            #region 取得資料

            AccessHelper dataSeed = new AccessHelper();
            List<ClassRecord> allClasses = dataSeed.ClassHelper.GetSelectedClass();
            List<StudentRecord> allStudents = new List<StudentRecord>();
            Dictionary<string, List<StudentRecord>> classStudents = new Dictionary<string, List<StudentRecord>>();
            AngelDemonComputer computer = new AngelDemonComputer();

            int maxStudents = 0;
            int totalStudent = 0;
            int currentStudent = 1;

            foreach (ClassRecord aClass in allClasses)
            {
                List<StudentRecord> studnetList = aClass.Students;
                if (studnetList.Count > maxStudents)
                    maxStudents = studnetList.Count;
                allStudents.AddRange(studnetList);

                //computer.FillDemonScore(dataSeed, schoolyear, semester, studnetList);

                classStudents.Add(aClass.ClassID, studnetList);
                totalStudent += studnetList.Count;
            }

            computer.FillDemonScore(dataSeed, schoolyear, semester, allStudents);
            dataSeed.StudentHelper.FillSemesterEntryScore(true, allStudents);
            if ( semester == 2 )
            {
                dataSeed.StudentHelper.FillSchoolYearEntryScore(true, allStudents);
                dataSeed.StudentHelper.FillSemesterHistory(allStudents);
            }

            SystemInformation.getField("DiffItem");
            SystemInformation.getField("Degree");

            Dictionary<string, decimal> degreeList = (Dictionary<string, decimal>)SystemInformation.Fields["Degree"];

            #endregion

            #region 產生表格
            Workbook template = new Workbook();
            Workbook prototype = new Workbook();

            //列印尺寸
            if (sizeIndex == 0)
                template.Open(new MemoryStream(Properties.Resources.德行成績總表A3), FileFormatType.Excel2003);
            else if (sizeIndex == 1)
                template.Open(new MemoryStream(Properties.Resources.德行成績總表A4), FileFormatType.Excel2003);
            else if (sizeIndex == 2)
                template.Open(new MemoryStream(Properties.Resources.德行成績總表B4), FileFormatType.Excel2003);
            prototype.Copy(template);

            Worksheet templateSheet = template.Worksheets[0];
            Worksheet prototypeSheet = prototype.Worksheets[0];

            Range tempInfoAndReward = templateSheet.Cells.CreateRange(0, 17, true);
            Range tempAbsence = templateSheet.Cells.CreateRange(17, 2, true);
            Range tempBeforeOtherDiff = templateSheet.Cells.CreateRange(19, 3, true);
            Range tempOtherDiff = templateSheet.Cells.CreateRange(22, 1, true);
            Range tempAfterOtherDiff = templateSheet.Cells.CreateRange(25, 2, true);

            Dictionary<string, int> columnIndexTable = new Dictionary<string, int>();

            Dictionary<string, List<string>> periodAbsence = new Dictionary<string, List<string>>();

            //紀錄獎懲的 Column Index
            columnIndexTable.Add("大功", 4);
            columnIndexTable.Add("小功", 6);
            columnIndexTable.Add("嘉獎", 8);
            columnIndexTable.Add("大過", 10);
            columnIndexTable.Add("小過", 12);
            columnIndexTable.Add("警告", 14);
            columnIndexTable.Add("獎懲小計", 16);

            //缺曠加減分
            int ptColIndex = 17;
            foreach ( SmartSchool.Evaluation.AngelDemonComputer.UsefulPeriodAbsence var in computer.UsefulPeriodAbsences )
            {
                if ( !periodAbsence.ContainsKey(var.Period) )
                    periodAbsence.Add(var.Period, new List<string>());
                if ( !periodAbsence[var.Period].Contains(var.Absence) )
                    periodAbsence[var.Period].Add(var.Absence);

                prototypeSheet.Cells.CreateRange(ptColIndex, 2, true).Copy(tempAbsence);
                ptColIndex += 2;
            }
            //foreach (string period in userType.Keys)
            //{
            //    if (!periodAbsence.ContainsKey(period))
            //    {
            //        periodAbsence.Add(period, new List<string>());
            //        foreach (string absence in userType[period])
            //        {
            //            if (!periodAbsence[period].Contains(absence))
            //            {
            //                periodAbsence[period].Add(absence);
            //                prototypeSheet.Cells.CreateRange(ptColIndex, 2, true).Copy(tempAbsence);
            //                ptColIndex += 2;
            //            }
            //        }
            //    }
            //}

            ptColIndex = 17;

            foreach (string period in periodAbsence.Keys)
            {
                prototypeSheet.Cells.CreateRange(2, ptColIndex, 1, periodAbsence[period].Count * 2).Merge();
                prototypeSheet.Cells[2, ptColIndex].PutValue(period);
                foreach (string absence in periodAbsence[period])
                {
                    prototypeSheet.Cells[3, ptColIndex].PutValue(absence);

                    columnIndexTable.Add(period + "_" + absence, ptColIndex);

                    ptColIndex += 2;
                }
            }
            prototypeSheet.Cells.CreateRange(1, 17, 1, ptColIndex - 15).Merge();
            prototypeSheet.Cells[1, 17].PutValue("缺曠加減分");

            columnIndexTable.Add("全勤", ptColIndex);
            columnIndexTable.Add("缺曠小計", ptColIndex + 1);

            prototypeSheet.Cells.CreateRange(ptColIndex, 3, true).Copy(tempBeforeOtherDiff);
            ptColIndex += 3;

            //導師加減分
            columnIndexTable.Add("導師加減分", ptColIndex - 1);

            //其他加減分項目
            foreach (string var in (List<string>)SystemInformation.Fields["DiffItem"])
            {
                prototypeSheet.Cells.CreateRange(ptColIndex, 1, true).Copy(tempOtherDiff);
                prototypeSheet.Cells[1, ptColIndex].PutValue(var);

                columnIndexTable.Add(var, ptColIndex);

                ptColIndex++;
            }

            if ( semester == 2 )
            {
                prototypeSheet.Cells.CreateRange(ptColIndex, 1, true).Copy(tempOtherDiff);
                prototypeSheet.Cells[1, ptColIndex].PutValue("上學期德行成績");
                columnIndexTable.Add("上學期成績", ptColIndex++);

                prototypeSheet.Cells.CreateRange(ptColIndex, 1, true).Copy(tempOtherDiff);
                prototypeSheet.Cells[1, ptColIndex].PutValue("學期德行成績");
                columnIndexTable.Add("學期成績", ptColIndex++);

                prototypeSheet.Cells.CreateRange(ptColIndex, 1, true).Copy(tempOtherDiff);
                prototypeSheet.Cells[1, ptColIndex].PutValue("等第");
                columnIndexTable.Add("等第", ptColIndex++);

                prototypeSheet.Cells.CreateRange(ptColIndex, 1, true).Copy(tempOtherDiff);
                prototypeSheet.Cells[1, ptColIndex].PutValue("學年德行成績");
                columnIndexTable.Add("學年成績", ptColIndex++);
            }
            else
            {

                prototypeSheet.Cells.CreateRange(ptColIndex, 1, true).Copy(tempOtherDiff);
                prototypeSheet.Cells[1, ptColIndex].PutValue("學期德行成績");
                columnIndexTable.Add("學期成績", ptColIndex++);

                prototypeSheet.Cells.CreateRange(ptColIndex, 1, true).Copy(tempOtherDiff);
                prototypeSheet.Cells[1, ptColIndex].PutValue("等第");
                columnIndexTable.Add("等第", ptColIndex++);
            }
            prototypeSheet.Cells.CreateRange(ptColIndex, 2, true).Copy(tempAfterOtherDiff);
            columnIndexTable.Add("評語", ptColIndex++);
            columnIndexTable.Add("更改等第", ptColIndex++);

            //加上底線
            prototypeSheet.Cells.CreateRange(maxStudents + 5, 0, 1, ptColIndex).SetOutlineBorder(BorderType.TopBorder, CellBorderType.Medium, System.Drawing.Color.Black);

            //填入製表日期
            prototypeSheet.Cells[0, 0].PutValue("製表日期：" + DateTime.Today.ToShortDateString());

            //填入標題
            prototypeSheet.Cells.CreateRange(0, 4, 1, ptColIndex - 4).Merge();
            prototypeSheet.Cells[0, 4].PutValue(SystemInformation.SchoolChineseName + " " + schoolyear + " 學年度 " + ((semester == 1) ? "上" : "下") + " 學期 德行成績總表　　　　　　　　");

            Range ptEachRow = prototypeSheet.Cells.CreateRange(5, 1, false);

            for (int i = 5; i < maxStudents + 5; i++)
            {
                prototypeSheet.Cells.CreateRange(i, 1, false).Copy(ptEachRow);
            }

            Range pt = prototypeSheet.Cells.CreateRange(0, maxStudents + 5, false);

            #endregion

            #region 填入表格
            Workbook wb = new Workbook();
            wb.Copy(prototype);
            Worksheet ws = wb.Worksheets[0];

            int index = 0;
            int dataIndex = 0;
            int classTotalRow = maxStudents + 5;

            foreach (ClassRecord aClass in allClasses)
            {
                //複製完成後的樣板
                ws.Cells.CreateRange(index, classTotalRow, false).Copy(pt);

                //填入班級名稱
                ws.Cells[index + 1, 0].PutValue(aClass.ClassName);

                Dictionary<string, int> degreeCount = new Dictionary<string, int>();
                foreach (string key in degreeList.Keys)
                {
                    degreeCount.Add(key, 0);
                }

                dataIndex = index + 5;

                foreach (StudentRecord aStudent in classStudents[aClass.ClassID])
                {
                    ws.Cells[dataIndex, 0].PutValue(aStudent.SeatNo);
                    ws.Cells[dataIndex, 1].PutValue(aStudent.StudentName);
                    ws.Cells[dataIndex, 2].PutValue(aStudent.StudentNumber);

                    decimal? score = null;
                    decimal rewardScore = 0;
                    decimal absenceScore = 0;
                    decimal diffScore = 0;
                    int gradeYear = -1;

                    XmlElement demonScore = (XmlElement)aStudent.Fields["DemonScore"];

                    //score = decimal.Parse(demonScore.GetAttribute("Score"));
                    foreach (SemesterEntryScoreInfo info in aStudent.SemesterEntryScoreList)
                    {
                        if (info.SchoolYear == schoolyear && info.Semester == semester && info.Entry == "德行")
                        {
                            score = (decimal?)info.Score;
                            gradeYear = info.GradeYear;
                            break;
                        }
                    }

                    foreach (XmlElement var in demonScore.SelectNodes("SubScore"))
                    {
                        if (var.GetAttribute("Type") == "基分")
                        {
                            ws.Cells[dataIndex, 3].PutValue(var.GetAttribute("Score"));
                        }
                        else if (var.GetAttribute("Type") == "獎懲")
                        {
                            int colIndex = columnIndexTable[var.GetAttribute("Name")];
                            if (decimal.Parse(var.GetAttribute("Count")) != 0)
                                ws.Cells[dataIndex, colIndex].PutValue(var.GetAttribute("Count"));
                            if (decimal.Parse(var.GetAttribute("Score")) != 0)
                                ws.Cells[dataIndex, colIndex + 1].PutValue(var.GetAttribute("Score"));
                            rewardScore += decimal.Parse(var.GetAttribute("Score"));
                        }
                        else if (var.GetAttribute("Type") == "缺曠")
                        {
                            string pa = var.GetAttribute("PeriodType") + "_" + var.GetAttribute("Absence");
                            if (columnIndexTable.ContainsKey(pa))
                            {
                                int colIndex = columnIndexTable[pa];
                                if (decimal.Parse(var.GetAttribute("Count")) != 0)
                                    ws.Cells[dataIndex, colIndex].PutValue(var.GetAttribute("Count"));
                                if (decimal.Parse(var.GetAttribute("Score")) != 0)
                                    ws.Cells[dataIndex, colIndex + 1].PutValue(var.GetAttribute("Score"));
                            }
                            absenceScore += decimal.Parse(var.GetAttribute("Score"));
                        }
                        else if (var.GetAttribute("Type") == "加減分")
                        {
                            int colIndex = columnIndexTable[var.GetAttribute("DiffItem")];
                            if (decimal.Parse(var.GetAttribute("Score")) != 0)
                                ws.Cells[dataIndex, colIndex].PutValue(var.GetAttribute("Score"));
                            diffScore += decimal.Parse(var.GetAttribute("Score"));
                        }
                        else if (var.GetAttribute("Type") == "全勤")
                        {
                            int colIndex = columnIndexTable["全勤"];
                            if (decimal.Parse(var.GetAttribute("Score")) != 0)
                                ws.Cells[dataIndex, colIndex].PutValue(var.GetAttribute("Score"));
                            absenceScore += decimal.Parse(var.GetAttribute("Score"));
                        }
                    }

                    //填入獎懲小計
                    if (rewardScore != 0)
                        ws.Cells[dataIndex, columnIndexTable["獎懲小計"]].PutValue(rewardScore.ToString());
                    //填入缺曠小計
                    if (absenceScore != 0)
                        ws.Cells[dataIndex, columnIndexTable["缺曠小計"]].PutValue(absenceScore.ToString());

                    //填入學業成績試算
                    if (!over100 && score > 100)
                        score = 100;
                    ws.Cells[dataIndex, columnIndexTable["學期成績"]].PutValue(score.ToString());

                    //填入上學期&學年成績
                    if ( semester == 2 )
                    {
                        //沒有學期歷程就用當下的學期
                        if ( gradeYear == -1 && schoolyear == SystemInformation.SchoolYear )
                        {
                            int.TryParse(aStudent.RefClass.GradeYear, out gradeYear);
                        }
                        //填入上學期成績
                        foreach ( SemesterEntryScoreInfo semesterEntryScore in aStudent.SemesterEntryScoreList )
                        {
                            if ( semesterEntryScore.Entry == "德行" && semesterEntryScore.GradeYear == gradeYear && semesterEntryScore.Semester == 1 )
                            {
                                if ( !over100 && semesterEntryScore.Score > 100 )
                                    ws.Cells[dataIndex, columnIndexTable["上學期成績"]].PutValue("100");
                                else
                                    ws.Cells[dataIndex, columnIndexTable["上學期成績"]].PutValue(semesterEntryScore.Score.ToString());
                                break;
                            }
                        }

                        //填入學年成績
                        foreach ( SchoolYearEntryScoreInfo schoolyearEntryScore in aStudent.SchoolYearEntryScoreList )
                        {
                            if ( schoolyearEntryScore.Entry == "德行" && schoolyearEntryScore.GradeYear == gradeYear )
                            {
                                if ( !over100 && schoolyearEntryScore.Score > 100 )
                                    ws.Cells[dataIndex, columnIndexTable["學年成績"]].PutValue("100");
                                else
                                    ws.Cells[dataIndex, columnIndexTable["學年成績"]].PutValue(schoolyearEntryScore.Score.ToString());
                                break;
                            }
                        }
                    }

                    //填入等第
                    string degree =score==null?"": computer.ParseLevel((decimal)score);
                    ws.Cells[dataIndex, columnIndexTable["等第"]].PutValue(degree);

                    //計算等第出現次數
                    if (degreeCount.ContainsKey(degree))
                        degreeCount[degree]++;

                    //評語
                    if (demonScore.SelectSingleNode("Others/@Comment") != null)
                        ws.Cells[dataIndex, columnIndexTable["評語"]].PutValue(demonScore.SelectSingleNode("Others/@Comment").InnerText);

                    dataIndex++;

                    //回報進度
                    _BGWSemesterMoralScoresTotal.ReportProgress((int)(currentStudent++ * 100.0 / totalStudent));
                }

                ws.Cells.CreateRange(index + classTotalRow + 1, 0, 1, ptColIndex).Merge();
                StringBuilder degreeSumString = new StringBuilder("");
                degreeSumString.Append("德行等第統計    ");
                foreach (string key in degreeCount.Keys)
                {
                    degreeSumString.Append(key + "等： " + degreeCount[key].ToString() + "    ");
                }
                ws.Cells[index + classTotalRow + 1, 0].Style.Font.Size = 12;
                ws.Cells.CreateRange(index + classTotalRow + 1, 1, false).RowHeight = 20;
                ws.Cells[index + classTotalRow + 1, 0].PutValue(degreeSumString.ToString());
                ws.Cells[index + classTotalRow + 1, 0].Style.HorizontalAlignment = TextAlignmentType.Left;

                index += classTotalRow + 3;
                ws.HPageBreaks.Add(index, ptColIndex);
            }

            #endregion

            e.Result = wb;
        }
    }
}

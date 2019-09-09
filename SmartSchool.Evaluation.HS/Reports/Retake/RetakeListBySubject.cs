using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.StudentRelated;
using Aspose.Cells;
using System.ComponentModel;
using System.Threading;
using FISCA.DSAUtil;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using SmartSchool.Common;
using SmartSchool.Evaluation.Reports.Retake;
using FISCA.Presentation;

namespace SmartSchool.Evaluation.Reports
{
    class RetakeListBySubject
    {
        private const int _PackageSize = 200;

        public RetakeListBySubject()
        {
            RetakeSelectSemesterForm form = new RetakeSelectSemesterForm("重修名單-依科目");
            if (form.ShowDialog() != DialogResult.OK)
                return;

            List<List<BriefStudentData>> splitList = new List<List<BriefStudentData>>();
            Dictionary<List<BriefStudentData>, ManualResetEvent> handle = new Dictionary<List<BriefStudentData>, ManualResetEvent>();
            Dictionary<List<BriefStudentData>, DSResponse> response = new Dictionary<List<BriefStudentData>, DSResponse>();

            //把全部在校生以_PackageSize人分一包
            #region 把全部在校生以_PackageSize人分一包
            int count = 0;
            List<BriefStudentData> package = new List<BriefStudentData>();
            foreach (BriefStudentData student in SmartSchool.StudentRelated.Student.Instance.Items)
            {
                if (student.IsNormal)
                {
                    if (count == 0)
                    {
                        count = _PackageSize;
                        package = new List<BriefStudentData>(_PackageSize);
                        splitList.Add(package);
                    }
                    package.Add(student);
                    count--;
                }
            }
            #endregion
            //每一包一個ManualResetEvent一個DSResponse
            #region 每一包一個ManualResetEvent(預設為不可通過)一個DSResponse
            foreach (List<BriefStudentData> p in splitList)
            {
                handle.Add(p, new ManualResetEvent(false));
                response.Add(p, new DSResponse());
            }
            #endregion
            //在背景執行取得資料
            BackgroundWorker bkwDataLoader = new BackgroundWorker();
            bkwDataLoader.DoWork += new DoWorkEventHandler(bkwDataLoader_DoWork);
            bkwDataLoader.RunWorkerAsync(new object[] { handle, response });
            //在背景計算不及格名單
            BackgroundWorker bkwNotPassComputer = new BackgroundWorker();
            bkwNotPassComputer.WorkerReportsProgress = true;
            bkwNotPassComputer.DoWork += new DoWorkEventHandler(bkwNotPassComputer_DoWork);
            bkwNotPassComputer.ProgressChanged += new ProgressChangedEventHandler(bkwNotPassComputer_ProgressChanged);
            bkwNotPassComputer.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bkwNotPassComputer_RunWorkerCompleted);
            bkwNotPassComputer.RunWorkerAsync(new object[] { handle, response, form.SchoolYear, form.Semester, form.IsPrintAllSemester });
        }

        private void bkwNotPassComputer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //MotherForm.SetStatusBarMessage("科目不及格名單產生完成。");

            object[] results = (object[])e.Result;
            Workbook report = (Workbook)results[0];
            string reportName = (string)results[1];

            if (e.Error == null)
            {
                //儲存 Excel
                #region 儲存 Excel
                string path = Path.Combine(Application.StartupPath, "Reports");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = Path.Combine(path, reportName + ".xlt");

                if (File.Exists(path))
                {
                    bool needCount = true;
                    try
                    {
                        File.Delete(path);
                        needCount = false;
                    }
                    catch { }
                    int i = 1;
                    while (needCount)
                    {
                        string newPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + (i++) + Path.GetExtension(path);
                        if (!File.Exists(newPath))
                        {
                            path = newPath;
                            break;
                        }
                        else
                        {
                            try
                            {
                                File.Delete(newPath);
                                path = newPath;
                                break;
                            }
                            catch { }
                        }
                    }
                }
                try
                {
                    File.Create(path).Close();
                }
                catch
                {
                    SaveFileDialog sd = new SaveFileDialog();
                    sd.Title = "另存新檔";
                    sd.FileName = Path.GetFileNameWithoutExtension(path) + ".xls";
                    sd.Filter = "Excel檔案 (*.xls)|*.xls|所有檔案 (*.*)|*.*";
                    if (sd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            File.Create(sd.FileName);
                            path = sd.FileName;
                        }
                        catch
                        {
                            MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
                report.Save(path, FileFormatType.Excel2003);
                #endregion
                MotherForm.SetStatusBarMessage(reportName + "產生完成。");
                System.Diagnostics.Process.Start(path);
            }
            else
                MotherForm.SetStatusBarMessage(reportName + "產生發生未預期錯誤。");
        }

        private void bkwNotPassComputer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MotherForm.SetStatusBarMessage("" + e.UserState, e.ProgressPercentage);
        }

        private void bkwNotPassComputer_DoWork(object sender, DoWorkEventArgs e)
        {
            string reportName = "科目不及格名單";
            //科目不及格學生清單(keyFormat:;<subject 科目='' 科目級別='' 學分數='' />)
            Dictionary<string, Dictionary<BriefStudentData, XmlElement>> notPassList = new Dictionary<string, Dictionary<BriefStudentData, XmlElement>>();
            #region 整理資料
            Dictionary<List<BriefStudentData>, ManualResetEvent> handle = (Dictionary<List<BriefStudentData>, ManualResetEvent>)((object[])e.Argument)[0];
            Dictionary<List<BriefStudentData>, DSResponse> response = (Dictionary<List<BriefStudentData>, DSResponse>)((object[])e.Argument)[1];

            //過濾學年度學期
            int schoolyear = (int)((e.Argument as object[])[2]);
            int semester = (int)((e.Argument as object[])[3]);
            bool printAll = (bool)((e.Argument as object[])[4]);

            double totleProgress = 0.0;
            double currentProgress = 80.0 / handle.Count;
            ((BackgroundWorker)sender).ReportProgress(1, reportName + "資料整理中...");
            foreach (List<BriefStudentData> splitList in handle.Keys)
            {
                //等待這包的成績資料載下來
                handle[splitList].WaitOne();
                //載下來的資料
                DSResponse resp = response[splitList];
                double miniProgress = currentProgress / splitList.Count;
                double miniProgressCount = 0.0;
                //每一個學生
                foreach (BriefStudentData student in splitList)
                {
                    List<string> studentPassedList = new List<string>();
                    //每學期成績
                    foreach (XmlElement scoreElement in resp.GetContent().GetElements("SemesterSubjectScore[RefStudentId='" + student.ID + "']"))
                    {
                        DSXmlHelper helper = new DSXmlHelper(scoreElement);
                        //每一個科目成績
                        foreach (XmlElement subjectScoreElement in helper.GetElements("ScoreInfo/SemesterSubjectScoreInfo/Subject"))
                        {
                            if (!printAll &&
                                (scoreElement.SelectSingleNode("SchoolYear").InnerText != schoolyear + "" ||
                                scoreElement.SelectSingleNode("Semester").InnerText != semester + ""))
                                continue;

                            //Debug by Cloud 2014.02.12
                            string subjectName = XMLEncoding.Encoding(subjectScoreElement.GetAttribute("科目"));
                            string leavel = XMLEncoding.Encoding(subjectScoreElement.GetAttribute("科目級別"));
                            string credit = XMLEncoding.Encoding(subjectScoreElement.GetAttribute("開課學分數"));

                            string subject = "<subject 科目='" + subjectName + "' 科目級別='" + leavel + "' 學分數='" + credit + "' />";
                            if (subjectScoreElement.GetAttribute("是否取得學分") == "是" || studentPassedList.Contains(subject))//如果該科目有取得學分獲該科目在其他學期已取得學分
                            {
                                //加入已取得學分科目清單
                                if (!studentPassedList.Contains(subject))
                                    studentPassedList.Add(subject);
                                //從未取得學分科目清單中移除
                                if (notPassList.ContainsKey(subject) && notPassList[subject].ContainsKey(student))
                                {
                                    notPassList[subject].Remove(student);
                                }
                            }
                            else
                            {
                                //把學年度學期加上去
                                subjectScoreElement.SetAttribute("學年度", scoreElement.SelectSingleNode("SchoolYear").InnerText);
                                subjectScoreElement.SetAttribute("學期", scoreElement.SelectSingleNode("Semester").InnerText);
                                subjectScoreElement.SetAttribute("年級", scoreElement.SelectSingleNode("GradeYear").InnerText);
                                //加入至未取得學分科目清單
                                if (!notPassList.ContainsKey(subject))
                                    notPassList.Add(subject, new Dictionary<BriefStudentData, XmlElement>());
                                if (!notPassList[subject].ContainsKey(student))
                                    notPassList[subject].Add(student, subjectScoreElement);
                            }
                        }
                    }
                    miniProgressCount += miniProgress;
                    ((BackgroundWorker)sender).ReportProgress((int)(totleProgress + miniProgressCount), reportName + "資料整理中...");
                }
                totleProgress += currentProgress;
                ((BackgroundWorker)sender).ReportProgress((int)totleProgress, reportName + "資料整理中...");
            }
            #endregion

            #region 產生報表
            currentProgress = 20.0 / notPassList.Count;
            Workbook template = new Workbook();
            #region 建立樣板
            template.Open(new MemoryStream(Properties.Resources.科目重修學生清單), FileFormatType.Excel2003);
            template.Worksheets[0].Cells[0, 0].PutValue(SmartSchool.CurrentUser.Instance.SchoolChineseName + "  科目重修學生清單");
            #endregion

            Workbook report = new Workbook();
            report.Open(new MemoryStream(Properties.Resources.科目重修學生清單), FileFormatType.Excel2003);

            Workbook wb = new Workbook();
            int index = 0;
            foreach (string subjectKey in notPassList.Keys)
            {
                if (notPassList[subjectKey].Count > 0)
                {
                    report.Worksheets[0].Cells.CopyRow(template.Worksheets[0].Cells, 0, index);
                    report.Worksheets[0].Cells.CopyRow(template.Worksheets[0].Cells, 1, index + 1);
                    report.Worksheets[0].Cells.CopyRow(template.Worksheets[0].Cells, 2, index + 2);
                    report.Worksheets[0].Cells.CopyRow(template.Worksheets[0].Cells, 3, index + 3);
                    report.Worksheets[0].Cells.SetRowHeight(index, template.Worksheets[0].Cells.GetRowHeight(0));
                    report.Worksheets[0].Cells.SetRowHeight(index + 1, template.Worksheets[0].Cells.GetRowHeight(1));
                    report.Worksheets[0].Cells.SetRowHeight(index + 2, template.Worksheets[0].Cells.GetRowHeight(2));
                    report.Worksheets[0].Cells.SetRowHeight(index + 3, template.Worksheets[0].Cells.GetRowHeight(3));

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(subjectKey);
                    int level = 0;
                    int.TryParse(doc.DocumentElement.GetAttribute("科目級別"), out level);
                    report.Worksheets[0].Cells[index + 1, 2].PutValue(doc.DocumentElement.GetAttribute("科目") + (level == 0 ? "" : " " + GetNumber(level)));
                    report.Worksheets[0].Cells[index + 1, 6].PutValue(doc.DocumentElement.GetAttribute("學分數"));
                    index += 4;
                    foreach (BriefStudentData student in notPassList[subjectKey].Keys)
                    {
                        XmlElement subjectElement = notPassList[subjectKey][student];
                        report.Worksheets[0].Cells.CopyRow(template.Worksheets[0].Cells, 4, index);
                        report.Worksheets[0].Cells[index, 0].PutValue("");//編號
                        report.Worksheets[0].Cells[index, 1].PutValue(student.ClassName);//班級
                        report.Worksheets[0].Cells[index, 2].PutValue(student.SeatNo);//座號
                        report.Worksheets[0].Cells[index, 3].PutValue(student.Name);//姓名
                        report.Worksheets[0].Cells[index, 4].PutValue(student.StudentNumber);//學號
                        report.Worksheets[0].Cells[index, 5].PutValue(subjectElement.GetAttribute("修課必選修"));//必/選修
                        report.Worksheets[0].Cells[index, 6].PutValue(subjectElement.GetAttribute("修課校部訂"));//校/部訂
                        report.Worksheets[0].Cells[index, 7].PutValue(subjectElement.GetAttribute("學年度"));//學年度
                        report.Worksheets[0].Cells[index, 8].PutValue(subjectElement.GetAttribute("學期"));//學期

                        int gradeyear;
                        if (ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(student.ID) != null && int.TryParse(subjectElement.GetAttribute("年級"), out gradeyear))
                            report.Worksheets[0].Cells[index, 9].PutValue(ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(student.ID).GetStudentPassScore(student, gradeyear));//及格基分
                        else
                            report.Worksheets[0].Cells[index, 9].PutValue("--");//及格基分                        

                        #region 取得最高分數
                        decimal maxScore = 0;
                        decimal tryParseDecimal;
                        if (decimal.TryParse(subjectElement.GetAttribute("原始成績"), out tryParseDecimal))
                            maxScore = tryParseDecimal;
                        if (decimal.TryParse(subjectElement.GetAttribute("學年調整成績"), out tryParseDecimal) && maxScore < tryParseDecimal)
                            maxScore = tryParseDecimal;
                        if (decimal.TryParse(subjectElement.GetAttribute("擇優採計成績"), out tryParseDecimal) && maxScore < tryParseDecimal)
                            maxScore = tryParseDecimal;
                        if (decimal.TryParse(subjectElement.GetAttribute("補考成績"), out tryParseDecimal) && maxScore < tryParseDecimal)
                            maxScore = tryParseDecimal;
                        if (decimal.TryParse(subjectElement.GetAttribute("重修成績"), out tryParseDecimal) && maxScore < tryParseDecimal)
                            maxScore = tryParseDecimal;
                        #endregion

                        report.Worksheets[0].Cells[index, 10].PutValue("" + maxScore);//學期成績

                        index++;
                    }
                    //留一行空白
                    index++;
                    report.Worksheets[0].HPageBreaks.Add(index, 11);
                    totleProgress += currentProgress;
                    ((BackgroundWorker)sender).ReportProgress((int)totleProgress, reportName + "報表產生中...");
                }
            }
            object[] results = new object[] { report, reportName };
            e.Result = results;
            #endregion
        }

        private string GetNumber(int p)
        {
            string levelNumber;
            switch (p)
            {
                #region 對應levelNumber
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

        private void bkwDataLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            Dictionary<List<BriefStudentData>, ManualResetEvent> handle = (Dictionary<List<BriefStudentData>, ManualResetEvent>)((object[])e.Argument)[0];
            Dictionary<List<BriefStudentData>, DSResponse> response = (Dictionary<List<BriefStudentData>, DSResponse>)((object[])e.Argument)[1];
            foreach (List<BriefStudentData> splitList in handle.Keys)
            {
                List<string> idList = new List<string>();
                foreach (BriefStudentData var in splitList)
                {
                    idList.Add(var.ID);
                }
                DSResponse resp = SmartSchool.Feature.Score.QueryScore.GetSemesterSubjectScore(idList.ToArray());
                response[splitList] = resp;
                handle[splitList].Set();
            }
        }
    }
}

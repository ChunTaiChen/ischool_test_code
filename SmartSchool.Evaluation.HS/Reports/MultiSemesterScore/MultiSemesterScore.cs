using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Customization.PlugIn.Report;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using System.ComponentModel;
using SmartSchool.Evaluation.Reports.MultiSemesterScore.Forms;
using SmartSchool.Evaluation.Reports.MultiSemesterScore.DataModel;
using Aspose.Words;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using SmartSchool.Common;

namespace SmartSchool.Evaluation.Reports.MultiSemesterScore
{
    class MultiSemesterScore
    {
        private ButtonAdapter _class_button;

        private BackgroundWorker _bgworker;

        //private SemsIndex _sems_index;
        //public SemsIndex SemsIndex
        //{
        //    get { return _sems_index; }
        //}

        private ReportOptions _option;
        public ReportOptions Option
        {
            get { return _option; }
            set { _option = value; }
        }

        public MultiSemesterScore()
        {
            string report_name = "多學期成績單";
            string report_path = "成績相關報表";

            //_student_button = new ButtonAdapter();
            //_student_button.Text = report_name;
            //_student_button.Path = report_path;
            //_student_button.OnClick += new EventHandler(studentButton_OnClick);
            //StudentReport.AddReport(_student_button);

            _class_button = new SecureButtonAdapter("Report0170");
            _class_button.Text = report_name;
            _class_button.Path = report_path;
            _class_button.OnClick += new EventHandler(classButton_OnClick);
            ClassReport.AddReport(_class_button);

            ButtonAdapter _student_button = new SecureButtonAdapter("Report0060");
            _student_button.Text = report_name;
            _student_button.Path = report_path;
            _student_button.OnClick += new EventHandler(_student_button_OnClick);
            StudentReport.AddReport(_student_button);
        }

        void _student_button_OnClick(object sender, EventArgs e)
        {
            AccessHelper helper = new AccessHelper();
            BackgroundWorker bkw = new BackgroundWorker();
            bkw.DoWork += new DoWorkEventHandler(bkw_DoWork);
            bkw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bgworker_RunWorkerCompleted);
            bkw.ProgressChanged += new ProgressChangedEventHandler(_bgworker_ProgressChanged);
            bkw.WorkerReportsProgress = true;
            bkw.RunWorkerAsync(helper.StudentHelper.GetSelectedStudent());
        }

        void bkw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = (BackgroundWorker)sender;
            List<StudentRecord> students = (List<StudentRecord>)e.Argument;
            bkw.ReportProgress(1);
            MultiThreadWorker<StudentRecord> mWorker = new MultiThreadWorker<StudentRecord>();
            mWorker.MaxThreads = 3;
            mWorker.PackageSize = 150;
            mWorker.PackageWorker += new EventHandler<PackageWorkEventArgs<StudentRecord>>(mWorker_PackageWorker);
            List<PackageWorkEventArgs<StudentRecord>> resp = mWorker.Run(students);
            List<SmartSchool.Feature.Student.StudentSnapShop> list = new List<SmartSchool.Feature.Student.StudentSnapShop>();
            foreach (PackageWorkEventArgs<StudentRecord> r in resp)
            {
                List<SmartSchool.Feature.Student.StudentSnapShop> snap = (List<SmartSchool.Feature.Student.StudentSnapShop>)r.Result;

                if (snap == null)
                    Console.WriteLine("StudentSnapshop List 是 Null");
                else
                    list.AddRange((List<SmartSchool.Feature.Student.StudentSnapShop>)r.Result);
            }
            bkw.ReportProgress(80);
            Document doc = new Document();
            doc.Sections.Clear();
            foreach (SmartSchool.Feature.Student.StudentSnapShop var in list)
            {
                Document d = new Document(new MemoryStream(Convert.FromBase64String(var.PresentContent)));

                doc.Sections.Add(doc.ImportNode(d.Sections[0], true));
            }
            bkw.ReportProgress(100);
            e.Result = doc;
        }

        void mWorker_PackageWorker(object sender, PackageWorkEventArgs<StudentRecord> e)
        {
            List<string> list = new List<string>();
            foreach (StudentRecord var in e.List)
            {
                list.Add(var.StudentID);
            }
            Dictionary<string, SmartSchool.Feature.Student.StudentSnapShop> sv = new Dictionary<string, SmartSchool.Feature.Student.StudentSnapShop>();
            foreach (SmartSchool.Feature.Student.StudentSnapShop s in SmartSchool.Feature.Student.StudentSnapShop.GetSnapShop(list.ToArray()))
            {
                if (!sv.ContainsKey(s.RefStudentID))
                    sv.Add(s.RefStudentID, s);
                else
                {
                    if (sv[s.RefStudentID].Version < s.Version)
                        sv[s.RefStudentID] = s;
                }
            }
            List<SmartSchool.Feature.Student.StudentSnapShop> slist = new List<SmartSchool.Feature.Student.StudentSnapShop>();
            slist.AddRange(sv.Values);
            e.Result = slist;
        }


        void classButton_OnClick(object sender, EventArgs e)
        {
            AccessHelper helper = new AccessHelper();
            List<StudentRecord> selected_students = new List<StudentRecord>();
            FormStart(helper, helper.ClassHelper.GetSelectedClass());
        }

        void FormStart(AccessHelper helper, List<ClassRecord> selected_classes)
        {
            Option = new ReportOptions();

            MultiSemesterScoreForm form = new MultiSemesterScoreForm(Option);
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Option.Save();
                if (MsgBox.Show("將會重新產生成績單，學生的班排名有可能會改變。\n\n如需列印已產生過的成績單，請使用學生的\"多學期成績單\"功能。\n\n產生報表並列印?", "重新產生並覆蓋就有多學期成績單", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _bgworker = new BackgroundWorker();
                    _bgworker.DoWork += new DoWorkEventHandler(_bgworker_DoWork);
                    _bgworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bgworker_RunWorkerCompleted);
                    _bgworker.ProgressChanged += new ProgressChangedEventHandler(_bgworker_ProgressChanged);
                    _bgworker.WorkerReportsProgress = true;
                    _bgworker.RunWorkerAsync(new object[] { helper, selected_classes, Option });
                }
            }
            else
            {
                Option.Save();
                return;
            }

        }

        void _bgworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("多學期成績單產生中...", e.ProgressPercentage);
        }

        void _bgworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("多學期成績單產生完成");
            Document doc = (Document)e.Result;

            string reportName = "多學期成績單";
            string path = Path.Combine(Application.StartupPath, "Reports");
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
                doc.Save(path, SaveFormat.Doc);
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
                        doc.Save(sd.FileName, SaveFormat.AsposePdf);
                    }
                    catch
                    {
                        MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
        }

        void _bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] obj = (object[])e.Argument;
            AccessHelper helper = (AccessHelper)obj[0];
            List<ClassRecord> selected_classes = (List<ClassRecord>)obj[1];
            ReportOptions option = (ReportOptions)obj[2];


            _bgworker.ReportProgress(0);

            StudentCollection student_collection = new StudentCollection();
            //List<StudentRecord> failed = new List<StudentRecord>();
            double student_counter = 1;
            double student_total = 0;
            List<StudentRecord> selected_students = new List<StudentRecord>();
            foreach (ClassRecord classRec in selected_classes)
            {
                student_total += classRec.Students.Count;
                selected_students.AddRange(classRec.Students);
            }
            #region 資料取得與整理

            int package_size = 50;
            List<StudentRecord> packaged_students = new List<StudentRecord>();
            foreach (StudentRecord record in selected_students)
            {
                packaged_students.Add(record);
                if (packaged_students.Count >= package_size)
                {
                    helper.StudentHelper.FillSemesterSubjectScore(true, packaged_students);
                    helper.StudentHelper.FillSemesterEntryScore(true, packaged_students);
                    packaged_students.Clear();
                }
                _bgworker.ReportProgress((int)(student_counter++ * 40.0 / student_total));
            }
            if (packaged_students.Count > 0)
            {
                helper.StudentHelper.FillSemesterSubjectScore(true, packaged_students);
                helper.StudentHelper.FillSemesterEntryScore(true, packaged_students);
                packaged_students.Clear();
            }

            foreach (ClassRecord classRec in selected_classes)
            {
                List<Student> list = new List<Student>();
                foreach (StudentRecord each_stu in classRec.Students)
                {
                    Student student = new Student(each_stu, option.ScoreType, option.PrintSemester,option.PrintEntries);
                    list.Add(student);
                    if (!student_collection.ContainsKey(each_stu.StudentID))
                        student_collection.Add(each_stu.StudentID, student);
                }
                #region 科目班排名
                List<string> rankedSubject = new List<string>();
                foreach (Student stu in list)
                {
                    foreach (string subject in stu.SubjectCollection.Keys)
                    {
                        if (rankedSubject.Contains(subject))
                            continue;
                        List<decimal> scores = new List<decimal>();
                        Dictionary<decimal, List<SubjectInfo>> rank = new Dictionary<decimal, List<SubjectInfo>>();
                        foreach (Student student in list)
                        {
                            if (student.SubjectCollection.ContainsKey(subject))
                            {
                                decimal score = student.SubjectCollection[subject].GetAverange();
                                scores.Add(score);
                                if (!rank.ContainsKey(score))
                                    rank.Add(score, new List<SubjectInfo>());
                                rank[score].Add(student.SubjectCollection[subject]);
                            }
                        }
                        scores.Sort();
                        scores.Reverse();

                        decimal lastscore = decimal.MinValue;
                        int count = scores.Count;
                        foreach (decimal score in scores)
                        {
                            if (lastscore != score)
                            {
                                int place = scores.IndexOf(score) + 1;
                                lastscore = score;
                                foreach (SubjectInfo s in rank[lastscore])
                                {
                                    s.Place = place;
                                    s.Radix = count;
                                }
                            }
                        }

                        rankedSubject.Add(subject);
                    }
                }
                #endregion
                #region 分項班排名
                List<string> rankedEntrys = new List<string>();
                foreach (Student stu in list)
                {
                    foreach (string entry in stu.EntryCollection.Keys)
                    {
                        if (rankedEntrys.Contains(entry))
                            continue;
                        List<decimal> scores = new List<decimal>();
                        Dictionary<decimal, List<EntryInfo>> rank = new Dictionary<decimal, List<EntryInfo>>();
                        foreach (Student student in list)
                        {
                            if (student.EntryCollection.ContainsKey(entry))
                            {
                                decimal score = student.EntryCollection[entry].GetAverange();
                                scores.Add(score);
                                if (!rank.ContainsKey(score))
                                    rank.Add(score, new List<EntryInfo>());
                                rank[score].Add(student.EntryCollection[entry]);
                            }
                        }
                        scores.Sort();
                        scores.Reverse();

                        decimal lastscore = decimal.MinValue;
                        int count = scores.Count;
                        //把排名填回每一個成績資料中
                        foreach (decimal score in scores)
                        {
                            if (lastscore != score)
                            {
                                int place = scores.IndexOf(score) + 1;
                                lastscore = score;
                                foreach (EntryInfo s in rank[lastscore])
                                {
                                    s.Place = place;
                                    s.Radix = count;
                                    //處理德行不允許破百
                                    if ( s.Name == "德行"&&option.FixMoralScore )
                                    {
                                        s.FixToLimit(100);
                                    }
                                }
                            }
                        }

                        rankedEntrys.Add(entry);
                    }
                }
                #endregion
            }
            #endregion
            student_counter = 1;
            #region 產生報表及SnapShop
            MemoryStream template = GetTemplate(option);
            Document doc = new Document();
            doc.Sections.Clear();
            List<SmartSchool.Feature.Student.StudentSnapShop> snapShops = new List<SmartSchool.Feature.Student.StudentSnapShop>();
            MultiThreadWorker<SmartSchool.Feature.Student.StudentSnapShop> mWorker = new MultiThreadWorker<SmartSchool.Feature.Student.StudentSnapShop>();
            mWorker.MaxThreads = 3;
            mWorker.PackageSize = 50;
            mWorker.PackageWorker += new EventHandler<PackageWorkEventArgs<SmartSchool.Feature.Student.StudentSnapShop>>(mWorker_PackageWorker);

            foreach ( string stuid in student_collection.Keys )
            {
                Student each_stu = student_collection[stuid];
                #region 一個學生產生一個新的Document
                //一個學生產生一個新的Document
                Document each_page = new Document(template, "", LoadFormat.Doc, "");
                #region 建立此學生的成績單
                //合併基本資料
                List<string> merge_keys = new List<string>();
                List<object> merge_values = new List<object>();
                merge_keys.AddRange(new string[] { "學校名稱", "科別名稱", "班級名稱", "座號", "學號", "姓名", "排名", "各學期成績" });
                merge_values.AddRange(new object[] { 
                    SmartSchool.Customization.Data.SystemInformation.SchoolChineseName,
                    each_stu.Department ,
                    each_stu.ClassName,
                    each_stu.SeatNo,
                    each_stu.StudentNumber,
                    each_stu.StudentName,
                    option.RatingMethod.ToString(),
                    each_stu 
                });
                each_page.MailMerge.MergeField += new Aspose.Words.Reporting.MergeFieldEventHandler(MailMerge_MergeField);
                each_page.MailMerge.Execute(merge_keys.ToArray(), merge_values.ToArray());
                #endregion
                #endregion
                #region 加入新的SnapShop
                SmartSchool.Feature.Student.StudentSnapShop newSnapShop = new SmartSchool.Feature.Student.StudentSnapShop();
                newSnapShop.SnapshopName = "多學期成績單";
                newSnapShop.RefStudentID = stuid;
                newSnapShop.PresentType = ".Doc";
                MemoryStream stream = new MemoryStream();
                each_page.Save(stream, SaveFormat.Doc);
                newSnapShop.PresentContent = Convert.ToBase64String(stream.ToArray());
                stream.Close();
                snapShops.Add(newSnapShop);
                #endregion
                //每300人次就直接上傳一次snapShops
                #region 每300人次就直接上傳一次
                if ( snapShops.Count == 300 )
                {
                    mWorker.Run(snapShops);
                    snapShops.Clear();
                }
                #endregion
                //合併成績單至一個檔案
                doc.Sections.Add(doc.ImportNode(each_page.Sections[0], true));
                //回報進度
                _bgworker.ReportProgress((int)( student_counter++ * 60.0 / student_total ) + 40);
            }
            //將最後一段的SnapShop上傳
            if (snapShops.Count > 0)
            {
                mWorker.Run(snapShops);
                snapShops.Clear();
            }
            #endregion

            e.Result = doc;
        }

        void mWorker_PackageWorker(object sender, PackageWorkEventArgs<SmartSchool.Feature.Student.StudentSnapShop> e)
        {
            SmartSchool.Feature.Student.StudentSnapShop.AddSnapShop(e.List.ToArray());
        }

        void MailMerge_MergeField(object sender, Aspose.Words.Reporting.MergeFieldEventArgs e)
        {

            if ( e.FieldName == "各學期成績" )
            {
                e.Text = string.Empty;

                Student stu = (Student)e.FieldValue;
                DocumentBuilder builder = new DocumentBuilder(e.Document);
                //有成績之學期
                List<int> semesters = new List<int>();
                #region 總計此學生有學期成績之學期
                foreach ( SubjectInfo subject in stu.SubjectCollection.Values )
                {
                    foreach ( int sem in subject.SemsScores.Keys )
                    {
                        if ( !semesters.Contains(sem) )
                            semesters.Add(sem);
                    }
                }
                foreach ( EntryInfo entry in stu.EntryCollection.Values )
                {
                    foreach ( int sem in entry.SemsScores.Keys )
                    {
                        if ( !semesters.Contains(sem) )
                            semesters.Add(sem);
                    }
                }
                #endregion
                semesters.Sort();

                builder.MoveToField(e.Field, false);
                #region 取得外框寬度並計算欄寬
                Cell SCell = (Cell)builder.CurrentParagraph.ParentNode;
                double Swidth = SCell.CellFormat.Width;
                double microUnit = Swidth / ( semesters.Count + 6 ); //每學期給一份，總平均給一份，百分比給一份，班級排名及科目各給兩份
                #endregion
                Table table = builder.StartTable();

                builder.CellFormat.ClearFormatting();
                builder.CellFormat.Borders.LineWidth = 0.5;

                builder.RowFormat.HeightRule = HeightRule.Auto;
                builder.RowFormat.Height = builder.Font.Size * 1.2d;
                builder.RowFormat.Alignment = RowAlignment.Center;
                builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
                builder.CellFormat.LeftPadding = 3.0;
                builder.CellFormat.RightPadding = 3.0;
                builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
                builder.ParagraphFormat.LineSpacing = 10;
                //builder.Font.Size = 8;

                List<EntryInfo> entries = new List<EntryInfo>();
                entries.AddRange(stu.EntryCollection.Values);
                entries.Sort(new EntryComparer());
                if ( entries.Count > 0 )
                {
                    #region 填表頭
                    builder.InsertCell().CellFormat.Width = microUnit * 2;
                    builder.CellFormat.Borders.Right.LineWidth = 0.25;
                    builder.Write("分項總成績");
                    foreach ( int sem in semesters )
                    {
                        #region 每學期給一欄
                        builder.InsertCell().CellFormat.Width = microUnit;
                        builder.CellFormat.Borders.Right.LineWidth = 0.25;
                        switch ( sem )
                        {
                            case 1:
                                builder.Write("一上");
                                break;
                            case 2:
                                builder.Write("一下");
                                break;
                            case 3:
                                builder.Write("二上");
                                break;
                            case 4:
                                builder.Write("二下");
                                break;
                            case 5:
                                builder.Write("三上");
                                break;
                            case 6:
                                builder.Write("三下");
                                break;
                            case 7:
                                builder.Write("四上");
                                break;
                            case 8:
                                builder.Write("四下");
                                break;
                            default:
                                builder.Write("第" + sem + "學期");
                                break;
                        }
                        #endregion
                    }
                    builder.InsertCell().CellFormat.Width = microUnit * 1.5;
                    builder.CellFormat.Borders.Right.LineWidth = 0.25;
                    builder.Write("總平均");
                    builder.InsertCell().CellFormat.Width = microUnit * 2;
                    builder.CellFormat.Borders.Right.LineWidth = 0.25;
                    builder.Write("班級排名");
                    builder.InsertCell().CellFormat.Width = microUnit;
                    builder.Write("百分比");
                    builder.EndRow();
                    #endregion
                    //分項跟科目成績中間劃雙線
                    foreach ( Cell cell in table.LastRow.Cells )
                        cell.CellFormat.Shading.BackgroundPatternColor = Color.LightGray;
                }
                //分項跟科目成績中間劃雙線
                //foreach ( Cell cell in table.LastRow.Cells )
                //    cell.CellFormat.Borders.Bottom.LineStyle = LineStyle.Double;
                #region 填分項成績
                foreach ( EntryInfo entryInfo in entries )
                {
                    //科目名稱
                    builder.InsertCell().CellFormat.Width = microUnit * 2;
                    builder.CellFormat.Borders.Right.LineWidth = 0.25;
                    builder.Write(entryInfo.Name);
                    #region 每學期成績
                    foreach ( int sem in semesters )
                    {
                        builder.InsertCell().CellFormat.Width = microUnit;
                        builder.CellFormat.Borders.Right.LineWidth = 0.25;
                        if ( entryInfo.SemsScores.ContainsKey(sem) )
                        {
                            builder.Write("" + entryInfo.SemsScores[sem]);
                        }
                    }
                    #endregion
                    //總平均
                    builder.InsertCell().CellFormat.Width = microUnit * 1.5;
                    builder.CellFormat.Borders.Right.LineWidth = 0.25;
                    if ( entryInfo.Name == "德行" )
                    {
                        decimal moral_score = Math.Round(entryInfo.GetAverange());
                        builder.Write("" + moral_score + " (" + GetMoralLevel(moral_score) + "級分)");
                    }
                    else
                        builder.Write("" + entryInfo.GetAverange());
                    //班級排名
                    builder.InsertCell().CellFormat.Width = microUnit * 2;
                    builder.CellFormat.Borders.Right.LineWidth = 0.25;
                    builder.Write("" + entryInfo.Place + " / " + entryInfo.Radix);
                    //百分比
                    builder.InsertCell().CellFormat.Width = microUnit;
                    builder.Write("" + entryInfo.GetPercentage() + "%");
                    builder.EndRow();
                }
                #endregion

                List<SubjectInfo> subjects = new List<SubjectInfo>();
                subjects.AddRange(stu.SubjectCollection.Values);
                subjects.Sort(new SubjectComparer());
                if ( subjects.Count > 0 )
                {
                    #region 填表頭
                    builder.InsertCell().CellFormat.Width = microUnit * 2;
                    builder.CellFormat.Borders.Right.LineWidth = 0.25;
                    builder.Write("科目成績");
                    foreach ( int sem in semesters )
                    {
                        #region 每學期給一欄
                        builder.InsertCell().CellFormat.Width = microUnit;
                        builder.CellFormat.Borders.Right.LineWidth = 0.25;
                        switch ( sem )
                        {
                            case 1:
                                builder.Write("一上");
                                break;
                            case 2:
                                builder.Write("一下");
                                break;
                            case 3:
                                builder.Write("二上");
                                break;
                            case 4:
                                builder.Write("二下");
                                break;
                            case 5:
                                builder.Write("三上");
                                break;
                            case 6:
                                builder.Write("三下");
                                break;
                            case 7:
                                builder.Write("四上");
                                break;
                            case 8:
                                builder.Write("四下");
                                break;
                            default:
                                builder.Write("第" + sem + "學期");
                                break;
                        }
                        #endregion
                    }
                    builder.InsertCell().CellFormat.Width = microUnit * 1.5;
                    builder.CellFormat.Borders.Right.LineWidth = 0.25;
                    builder.Write("總平均");
                    builder.InsertCell().CellFormat.Width = microUnit * 2;
                    builder.CellFormat.Borders.Right.LineWidth = 0.25;
                    builder.Write("班級排名");
                    builder.InsertCell().CellFormat.Width = microUnit;
                    builder.Write("百分比");
                    builder.EndRow();
                    #endregion
                    //分項跟科目成績中間劃雙線
                    foreach ( Cell cell in table.LastRow.Cells )
                        cell.CellFormat.Shading.BackgroundPatternColor = Color.LightGray;
                    //分項跟科目成績中間劃雙線
                    //foreach ( Cell cell in table.LastRow.Cells )
                    //    cell.CellFormat.Borders.Bottom.LineStyle = LineStyle.Double;
                }
                #region 填科目成績

                foreach ( SubjectInfo subjectInfo in subjects )
                {
                    //科目名稱
                    builder.InsertCell().CellFormat.Width = microUnit * 2;
                    builder.CellFormat.Borders.Right.LineWidth = 0.25;
                    builder.Write(subjectInfo.Name);
                    #region 每學期成績
                    foreach ( int sem in semesters )
                    {
                        builder.InsertCell().CellFormat.Width = microUnit;
                        builder.CellFormat.Borders.Right.LineWidth = 0.25;
                        if ( subjectInfo.SemsScores.ContainsKey(sem) )
                        {
                            builder.Write("" + subjectInfo.SemsScores[sem]);
                        }
                    }
                    #endregion
                    //總平均
                    builder.InsertCell().CellFormat.Width = microUnit * 1.5;
                    builder.CellFormat.Borders.Right.LineWidth = 0.25;
                    builder.Write("" + subjectInfo.GetAverange());
                    //班級排名
                    builder.InsertCell().CellFormat.Width = microUnit * 2;
                    builder.CellFormat.Borders.Right.LineWidth = 0.25;
                    builder.Write("" + subjectInfo.Place + " / " + subjectInfo.Radix);
                    //百分比
                    builder.InsertCell().CellFormat.Width = microUnit;
                    builder.Write("" + subjectInfo.GetPercentage() + "%");
                    builder.EndRow();
                }
                #endregion
                #region 去除表格四邊的線
                if ( table.FirstRow != null )
                {
                    foreach ( Cell cell in table.FirstRow.Cells )
                        cell.CellFormat.Borders.Top.LineStyle = LineStyle.None;
                }

                if ( table.LastRow != null )
                {
                    foreach ( Cell cell in table.LastRow.Cells )
                        cell.CellFormat.Borders.Bottom.LineStyle = LineStyle.None;
                }

                foreach ( Row row in table.Rows )
                {
                    row.FirstCell.CellFormat.Borders.Left.LineStyle = LineStyle.None;
                    row.LastCell.CellFormat.Borders.Right.LineStyle = LineStyle.None;
                }
                #endregion
            }
            //int row_index = 1;

            ////排序科目
            //List<SubjectInfo> subject_list = new List<SubjectInfo>();
            //subject_list.AddRange(each_stu.SubjectCollection.Values);
            //subject_list.Sort(new SubjectComparer());

            ////排序分項
            //List<EntryInfo> entry_list = new List<EntryInfo>();
            //entry_list.AddRange(each_stu.EntryCollection.Values);
            //entry_list.Sort(new EntryComparer());

            ////填入科目
            //foreach ( SubjectInfo info in subject_list )
            //{
            //    run.Text = info.Name;

            //    subject_table.Rows[row_index].Cells[0].Paragraphs[0].Runs.Add(run.Clone(true));

            //    foreach ( int sems_index in info.SemsScores.Keys )
            //    {
            //        run.Text = info.SemsScores[sems_index].ToString();
            //        subject_table.Rows[row_index].Cells[sems_index].Paragraphs[0].Runs.Add(run.Clone(true));
            //    }
            //    //平均
            //    run.Text = info.GetAverange().ToString();
            //    subject_table.Rows[row_index].Cells[9].Paragraphs[0].Runs.Add(run.Clone(true));
            //    //排名
            //    run.Text = "" + info.Place + " / " + info.Radix;
            //    subject_table.Rows[row_index].Cells[10].Paragraphs[0].Runs.Add(run.Clone(true));
            //    //百分比
            //    run.Text = "" + info.GetPercentage() + "%";
            //    subject_table.Rows[row_index].Cells[11].Paragraphs[0].Runs.Add(run.Clone(true));

            //    row_index++;
            //}

            //row_index = 0;

            ////填入分項
            //foreach ( EntryInfo info in entry_list )
            //{
            //    run.Text = info.Name;

            //    entry_table.Rows[row_index].Cells[0].Paragraphs[0].Runs.Add(run.Clone(true));

            //    foreach ( int sems_index in info.SemsScores.Keys )
            //    {
            //        run.Text = info.SemsScores[sems_index].ToString();
            //        entry_table.Rows[row_index].Cells[sems_index].Paragraphs[0].Runs.Add(run.Clone(true));
            //    }
            //    //平均
            //    run.Text = info.GetAverange().ToString();
            //    entry_table.Rows[row_index].Cells[9].Paragraphs[0].Runs.Add(run.Clone(true));
            //    //排名
            //    run.Text = "" + info.Place + " / " + info.Radix;
            //    entry_table.Rows[row_index].Cells[10].Paragraphs[0].Runs.Add(run.Clone(true));
            //    //百分比
            //    run.Text = "" + info.GetPercentage() + "%";
            //    entry_table.Rows[row_index].Cells[11].Paragraphs[0].Runs.Add(run.Clone(true));

            //    row_index++;
            //}
        }

        MemoryStream GetTemplate(ReportOptions option)
        {
            if (option.IsDefaultTemplate)
                return new MemoryStream(option.DefaultTemplate);
            return new MemoryStream(option.CustomizeTemplate);
        }

        int GetMoralLevel(decimal moral_score)
        {
            //moral_score = Math.Round(moral_score);
            if (moral_score >= 90)
                return 4;
            else if (moral_score >= 80)
                return 3;
            else if (moral_score >= 70)
                return 2;
            else if (moral_score >= 60)
                return 1;
            else
                return 0;
        }
    }

    class EntryComparer : IComparer<EntryInfo>
    {
        #region IComparer<string> 成員

        public int Compare(EntryInfo x, EntryInfo y)
        {
            return SortByEntryName(x.Name, y.Name);
        }

        #endregion

        public int SortByEntryName(string a, string b)
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
            list.AddRange(new string[] { "學業", "實習科目", "體育", "國防通識", "健康與護理", "德行" });

            int x = list.IndexOf(a1);
            if (x < 0)
                return list.Count;
            else
                return x;
        }
    }

    class SubjectComparer : IComparer<SubjectInfo>
    {
        #region IComparer<SemesterSubjectScoreInfo> 成員

        public int Compare(SubjectInfo x, SubjectInfo y)
        {
            return SortBySubjectName(x.Name, y.Name);
        }

        #endregion

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
    }
}
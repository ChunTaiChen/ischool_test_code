using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using SmartSchool.Customization.Data;
using System.Xml;
using DevComponents.DotNetBar.Rendering;
using FISCA.DSAUtil;

namespace SmartSchool.Evaluation.Process.Wizards
{
    public partial class CalcSemesterSubjectScoreWizard : SmartSchool.Common.BaseForm
    {
        private const int _MaxPackageSize = 250;

        private ErrorViewer _ErrorViewer = new ErrorViewer();
        private WarnViewer _WarnViewer;
        
        private BackgroundWorker runningBackgroundWorker = new BackgroundWorker();

        private SelectType _Type;

        private List<string> warningList = new List<string>();

        private List<StudentRecord> resultList; // 儲存最後結果的List

        private bool hasWarning = false;
        private bool hasError = false;

        public CalcSemesterSubjectScoreWizard(SelectType type)
        {
            _Type = type;
            InitializeComponent();

            #region 設定Wizard會跟著Style跑
            //this.wizard1.FooterStyle.ApplyStyle(( GlobalManager.Renderer as Office2007Renderer ).ColorTable.GetClass(ElementStyleClassKeys.RibbonFileMenuBottomContainerKey));
            this.wizard1.HeaderStyle.ApplyStyle((GlobalManager.Renderer as Office2007Renderer).ColorTable.GetClass(ElementStyleClassKeys.RibbonFileMenuBottomContainerKey));
            this.wizard1.FooterStyle.BackColorGradientAngle = -90;
            this.wizard1.FooterStyle.BackColorGradientType = eGradientType.Linear;
            this.wizard1.FooterStyle.BackColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TopBackground.Start;
            this.wizard1.FooterStyle.BackColor2 = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TopBackground.End;
            this.wizard1.BackColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TopBackground.Start;
            this.wizard1.BackgroundImage = null;
            for (int i = 0; i < 5; i++)
            {
                (this.wizard1.Controls[1].Controls[i] as ButtonX).ColorTable = eButtonColor.OrangeWithBackground;
            }
            (this.wizard1.Controls[0].Controls[1] as System.Windows.Forms.Label).ForeColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.MouseOver.TitleText;
            (this.wizard1.Controls[0].Controls[2] as System.Windows.Forms.Label).ForeColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TitleText;
            #endregion

            switch (_Type)
            {
                default:
                case SelectType.Student:
                    this.numericUpDown1.Value = SmartSchool.Customization.Data.SystemInformation.SchoolYear;
                    this.numericUpDown2.Value = SmartSchool.Customization.Data.SystemInformation.Semester;
                    labelX1.Text = "選擇學年度學期";
                    labelX2.Text = "學年度";
                    labelX3.Text = "學期";
                    labelX2.Top += 10;
                    labelX3.Top += 10;
                    numericUpDown1.Top += 10;
                    numericUpDown2.Top += 10;
                    this.checkBox1.Checked = false;
                    this.checkBox1.Visible = false;
                    break;
                case SelectType.GradeYearStudent:
                    this.Text = "計算" + SmartSchool.Customization.Data.SystemInformation.SchoolYear + "學年度第" + SmartSchool.Customization.Data.SystemInformation.Semester + "學期科目成績";
                    this.numericUpDown1.Minimum = 1;
                    this.numericUpDown1.Maximum = 5;
                    this.numericUpDown1.Value = 1;
                    wizardPage1.PageTitle = labelX1.Text = "選擇年級";
                    labelX2.Text = "年級";
                    labelX3.Visible = false;
                    numericUpDown2.Visible = false;
                    break;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            switch (_Type)
            {
                default:
                case SelectType.Student:
                    if (this.numericUpDown1.Value == SmartSchool.Customization.Data.SystemInformation.SchoolYear && this.numericUpDown2.Value == SmartSchool.Customization.Data.SystemInformation.Semester)
                    {
                        checkBox1.Checked = true;
                        checkBox1.Enabled = true;
                    }
                    else
                    {
                        checkBox1.Checked = false;
                        checkBox1.Enabled = false;
                    }
                    break;
                case SelectType.GradeYearStudent:
                    checkBox1.Checked = true;
                    checkBox1.Enabled = true;
                    break;
            }
        }

        private void CloseForm(object sender, CancelEventArgs e)
        {
            this.Close();
            if (runningBackgroundWorker.IsBusy)
                runningBackgroundWorker.CancelAsync();
            this._ErrorViewer.Clear();
            this._ErrorViewer.Hide();
        }

        #region 計算學期科目成績
        private void wizardPage2_AfterPageDisplayed(object sender, WizardPageChangeEventArgs e)
        {
            AccessHelper helper = new AccessHelper();
            List<StudentRecord> selectedStudents;
            int schooyYear;
            int semester;
            switch (_Type)
            {
                default:
                case SelectType.Student:
                    selectedStudents = helper.StudentHelper.GetSelectedStudent();
                    schooyYear = (int)numericUpDown1.Value;
                    semester = (int)numericUpDown2.Value;
                    break;
                case SelectType.GradeYearStudent:
                    selectedStudents = new List<StudentRecord>();
                    foreach (ClassRecord classrecord in helper.ClassHelper.GetAllClass())
                    {
                        int tryParseGradeYear;
                        if (int.TryParse(classrecord.GradeYear, out tryParseGradeYear) && tryParseGradeYear == (int)numericUpDown1.Value)
                            selectedStudents.AddRange(classrecord.Students);
                    }
                    schooyYear = SmartSchool.Customization.Data.SystemInformation.SchoolYear;
                    semester = SmartSchool.Customization.Data.SystemInformation.Semester;
                    break;
            }
            linkLabel1.Visible = false;
            linkLabel2.Visible = false;
            labelX4.Text = "學期成績計算中...";
            runningBackgroundWorker = new BackgroundWorker();
            runningBackgroundWorker.WorkerSupportsCancellation = true;
            runningBackgroundWorker.WorkerReportsProgress = true;
            runningBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(bkw_ProgressChanged);
            runningBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bkw_RunWorkerCompleted);
            runningBackgroundWorker.DoWork += new DoWorkEventHandler(bkw_DoWork);
            runningBackgroundWorker.RunWorkerAsync(new object[] { schooyYear, semester, helper, selectedStudents, checkBox1.Checked });
        }

        void bkw_DoWork(object sender, DoWorkEventArgs e)
        {
            warningList.Clear();// 將警告名單清空
            BackgroundWorker bkw = ((BackgroundWorker)sender);
            int schoolyear = (int)((object[])e.Argument)[0];
            int semester = (int)((object[])e.Argument)[1];
            AccessHelper helper = (AccessHelper)((object[])e.Argument)[2];
            List<StudentRecord> selectedStudents = (List<StudentRecord>)((object[])e.Argument)[3];
            bool registerSemesterHistory = (bool)((object[])e.Argument)[4];
            bkw.ReportProgress(1, null);
            WearyDogComputer computer = new WearyDogComputer();
            int packageSize = 50;
            int packageCount = 0;
            List<StudentRecord> package = null;
            List<List<StudentRecord>> packages = new List<List<StudentRecord>>();
            foreach (StudentRecord s in selectedStudents)
            {
                if (packageCount == 0)
                {
                    package = new List<StudentRecord>(packageSize);
                    packages.Add(package);
                    packageCount = packageSize;
                    packageSize += 50;
                    if (packageSize > _MaxPackageSize)
                        packageSize = _MaxPackageSize;
                }
                package.Add(s);
                packageCount--;
            }
            double maxStudents = selectedStudents.Count;
            if (maxStudents == 0)
                maxStudents = 1;
            double computedStudents = 0;
            bool allPass = true;
            foreach (List<StudentRecord> var in packages)
            {
                #region 處理學期歷程
                if (registerSemesterHistory)
                {
                    if (var.Count > 0)
                    {
                        helper.StudentHelper.FillField("SemesterHistory", var);
                        List<StudentRecord> editList = new List<StudentRecord>();
                        #region 檢查並編及每個選取學生的學期歷程
                        foreach (StudentRecord stu in var)
                        {
                            int gyear;
                            if (stu.RefClass != null && int.TryParse(stu.RefClass.GradeYear, out gyear))
                            {
                                XmlElement semesterHistory = (XmlElement)stu.Fields["SemesterHistory"];
                                XmlElement historyElement = null;
                                foreach (XmlElement history in new DSXmlHelper(semesterHistory).GetElements("History"))
                                {
                                    int year, sems, gradeyear;
                                    if (
                                        int.TryParse(history.GetAttribute("SchoolYear"), out year) &&
                                        int.TryParse(history.GetAttribute("Semester"), out sems) &&
                                        year == schoolyear && sems == semester
                                        )
                                    {
                                        historyElement = history;
                                    }
                                }

                                // 小郭, 2013/12/26
                                string className = string.Empty;
                                string deptName = string.Empty;
                                string seatNo = string.Empty;
                                string teacherName = string.Empty;

                                if (stu.RefClass != null)
                                {
                                    className = stu.RefClass.ClassName;
                                    deptName = stu.RefClass.Department;
                                    if (stu.RefClass.RefTeacher != null)
                                        teacherName = stu.RefClass.RefTeacher.TeacherName;
                                }
                                seatNo = stu.SeatNo;

                                if (historyElement == null)
                                {
                                    historyElement = semesterHistory.OwnerDocument.CreateElement("History");
                                    historyElement.SetAttribute("SchoolYear", "" + schoolyear);
                                    historyElement.SetAttribute("Semester", "" + semester);
                                    historyElement.SetAttribute("GradeYear", "" + gyear);
                                    // 小郭, 2013/12/26
                                    historyElement.SetAttribute("ClassName", className);
                                    historyElement.SetAttribute("DeptName", deptName);
                                    historyElement.SetAttribute("SeatNo", seatNo);
                                    historyElement.SetAttribute("Teacher", teacherName);

                                    semesterHistory.AppendChild(historyElement);
                                    editList.Add(stu);
                                }
                                else
                                {
                                    #region 判斷那些欄位需要更新
                                    bool isRevised = false; // 小郭, 2013/12/26
                                    if (historyElement.GetAttribute("GradeYear") != "" + gyear)
                                    {
                                        historyElement.SetAttribute("GradeYear", "" + gyear);
                                        // editList.Add(stu);
                                        isRevised = true;
                                    }

                                    // 小郭, 2013/12/26
                                    if (!string.IsNullOrEmpty(className) &&
                                            historyElement.GetAttribute("ClassName") != className)
                                    {
                                        historyElement.SetAttribute("ClassName", className);
                                        isRevised = true;
                                    }

                                    if (!string.IsNullOrEmpty(deptName) &&
                                            historyElement.GetAttribute("DeptName") != deptName)
                                    {
                                        historyElement.SetAttribute("DeptName", deptName);
                                        isRevised = true;
                                    }

                                    if (!string.IsNullOrEmpty(seatNo) &&
                                            historyElement.GetAttribute("SeatNo") != seatNo)
                                    {
                                        historyElement.SetAttribute("SeatNo", seatNo);
                                        isRevised = true;
                                    }

                                    if (!string.IsNullOrEmpty(teacherName) &&
                                            historyElement.GetAttribute("Teacher") != teacherName)
                                    {
                                        historyElement.SetAttribute("Teacher", teacherName);
                                        isRevised = true;
                                    }

                                    if (isRevised == true)
                                        editList.Add(stu);
                                    #endregion 判斷那些欄位需要更新
                                }
                            }
                        }
                        #endregion

                        string req = "<UpdateStudentList>";
                        foreach (StudentRecord stu in var)
                        {
                            int tryParseInt;
                            req += "<Student><Field><SemesterHistory>" + ((XmlElement)stu.Fields["SemesterHistory"]).InnerXml + "</SemesterHistory></Field><Condition><ID>" + stu.StudentID + "</ID></Condition></Student>";
                        }
                        req += "</UpdateStudentList>";
                        DSRequest dsreq = new DSRequest(req);
                        SmartSchool.Feature.EditStudent.Update(dsreq);
                    }
                }
                #endregion
                computedStudents += var.Count;
                Dictionary<StudentRecord, List<string>> errormessages = computer.FillSemesterSubjectCalcScore(schoolyear, semester, helper, var);
                if (errormessages.Count > 0)
                    allPass = false;
                if (bkw.CancellationPending)
                    break;
                else
                    bkw.ReportProgress((int)((computedStudents * 100.0) / maxStudents), errormessages);
            }

            //異常課程提示清單
            if (computer._WarningList != null && computer._WarningList.Count > 0)
            {
                hasWarning = true;
                foreach (string s in computer._WarningList) // 這邊先整理好有疑慮的課程警告名單，等全部都做完後一併處理
                {
                    warningList.Add(s);                
                }
            }

            if (allPass)
            {
                ISubjectCalcPostProcess obj = FISCA.InteractionService.DiscoverAPI<ISubjectCalcPostProcess>();
                if (obj != null)
                {
                    obj.PostProcess(schoolyear, semester, selectedStudents);
                }
                e.Result = selectedStudents;
            }
            else
                e.Result = null;
        }

        void bkw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!((BackgroundWorker)sender).CancellationPending)
            {
                if (e.Result == null)
                {
                    hasError = true; //有錯誤，使用者將無法繼續計算
                    _WarnViewer = new WarnViewer(hasError);
                }
                else
                {
                    hasError = false;
                    _WarnViewer = new WarnViewer(hasError);
                }

                foreach (string s in warningList)
                {                    
                    _WarnViewer.SetMessage("課程:" + s, "科目名稱或分項類別有錯誤, 故此課程不列入計算");
                }

                if (hasError && !hasWarning) // 有錯誤 沒警告
                {
                    linkLabel1.Visible = true;
                    labelX4.Text = "計算失敗，請檢查錯誤訊息。";
                }
                else if (hasError && hasWarning) // 有錯誤 有警告
                {
                    linkLabel1.Visible = true;
                    linkLabel2.Visible = true; // 不再主動跳出視窗，警告訊息放在這裡供使用者點選打開檢查
                    labelX4.Text = "計算失敗，請檢查警告訊息、錯誤訊息";

                }
                else if (!hasError && hasWarning) // 沒錯誤 有警告
                {
                    linkLabel2.Visible = true; // 不再主動跳出視窗，警告訊息放在這裡供使用者點選打開檢查
                    labelX4.Text = "計算成功，請檢查警告訊息後，上傳成績";
                    resultList = (List<StudentRecord>) e.Result;
                }
                else // 沒錯誤 沒警告
                {
                    wizard1.SelectedPage = wizardPage4;
                    upLoad((List<StudentRecord>)e.Result);
                }
                
            }
        }

        void bkw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!((BackgroundWorker)sender).CancellationPending)
            {
                if (e.UserState != null)
                {
                    Dictionary<StudentRecord, List<string>> errormessages = (Dictionary<StudentRecord, List<string>>)e.UserState;
                    if (errormessages.Count > 0)
                    {
                        foreach (StudentRecord stu in errormessages.Keys)
                        {
                            _ErrorViewer.SetMessage(stu, errormessages[stu]);
                        }
                        // 2018/5/24 穎驊完成項目調整 [H成績][02] 修正計算學期科目成績資料錯誤提醒，
                        // 統一將提醒錯誤視窗在bkw_RunWorkerCompleted 後才呈現，本來在背景執行序就不應該動到UI
                        //_ErrorViewer.Show(); 
                    }
                }
                this.progressBarX1.Value = e.ProgressPercentage;
            }
        }

        private void wizardPage2_BackButtonClick(object sender, CancelEventArgs e)
        {
            if (runningBackgroundWorker.IsBusy)
                runningBackgroundWorker.CancelAsync();
            this._ErrorViewer.Clear();
            this._WarnViewer.Clear(); //若按上一步，把之前的訊息刪掉。
            this._ErrorViewer.Hide();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this._ErrorViewer.ShowDialog();
        }

        private void CalcSemesterSubjectScoreWizard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (runningBackgroundWorker.IsBusy)
                runningBackgroundWorker.CancelAsync();
            this._ErrorViewer.Clear();
            this._ErrorViewer.Hide();
        }
        #endregion

        #region 上傳學期科目成績

        BackgroundWorker _uploadingWorker = new BackgroundWorker();
        private void wizardPage4_CancelButtonClick(object sender, CancelEventArgs e)
        {
            if (_uploadingWorker.IsBusy)
                _uploadingWorker.CancelAsync();
            this.Close();
        }

        private void upLoad(List<StudentRecord> list)
        {
            _uploadingWorker.WorkerReportsProgress = true;
            _uploadingWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_uploadingWorker_RunWorkerCompleted);
            _uploadingWorker.ProgressChanged += new ProgressChangedEventHandler(_uploadingWorker_ProgressChanged);
            _uploadingWorker.DoWork += new DoWorkEventHandler(_uploadingWorker_DoWork);
            _uploadingWorker.RunWorkerAsync(list);
        }

        void _uploadingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = ((BackgroundWorker)sender);
            List<StudentRecord> selectedStudents = (List<StudentRecord>)e.Argument;
            int packageSize = 50;
            int packageCount = 0;
            List<StudentRecord> package = null;
            List<List<StudentRecord>> packages = new List<List<StudentRecord>>();
            bkw.ReportProgress(1, null);
            foreach (StudentRecord s in selectedStudents)
            {
                if (packageCount == 0)
                {
                    package = new List<StudentRecord>(packageSize);
                    packages.Add(package);
                    packageCount = packageSize;
                    packageSize += 50;
                    if (packageSize > _MaxPackageSize)
                        packageSize = _MaxPackageSize;
                }
                package.Add(s);
                packageCount--;
            }
            double maxStudents = selectedStudents.Count;
            if (maxStudents == 0)
                maxStudents = 1;
            double computedStudents = 0;
            bool allPass = true;
            XmlDocument doc = new XmlDocument();
            foreach (List<StudentRecord> var in packages)
            {
                computedStudents += var.Count;
                List<SmartSchool.Feature.Score.EditScore.UpdateInfo> updateList = new List<SmartSchool.Feature.Score.EditScore.UpdateInfo>();
                List<SmartSchool.Feature.Score.AddScore.InsertInfo> insertList = new List<SmartSchool.Feature.Score.AddScore.InsertInfo>();
                foreach (StudentRecord stu in var)
                {
                    if (stu.Fields.ContainsKey("SemesterSubjectCalcScore"))
                    {
                        XmlElement SemesterSubjectCalcScore = (XmlElement)stu.Fields["SemesterSubjectCalcScore"];
                        #region 修改
                        foreach (XmlNode updateNode in SemesterSubjectCalcScore.SelectNodes("UpdateSemesterScore"))
                        {
                            XmlElement subjectScoreInfo = doc.CreateElement("SemesterSubjectScoreInfo");
                            string id = ((XmlElement)updateNode).GetAttribute("ID");
                            string gradeyear = ((XmlElement)updateNode).GetAttribute("GradeYear");
                            List<XmlElement> list = new List<XmlElement>();
                            foreach (XmlElement subjectNode in updateNode.SelectNodes("Subject"))
                            {
                                list.Add(subjectNode);
                            }
                            list.Sort(SortSubject);
                            foreach (XmlElement s in list)
                            {
                                subjectScoreInfo.AppendChild(doc.ImportNode(s, true));
                            }
                            updateList.Add(new SmartSchool.Feature.Score.EditScore.UpdateInfo(id, gradeyear, subjectScoreInfo));
                        }
                        #endregion
                        #region 新增
                        foreach (XmlNode updateNode in SemesterSubjectCalcScore.SelectNodes("InsertSemesterScore"))
                        {
                            XmlElement subjectScoreInfo = doc.CreateElement("SemesterSubjectScoreInfo");
                            string gradeYear = ((XmlElement)updateNode).GetAttribute("GradeYear");
                            string sy = ((XmlElement)updateNode).GetAttribute("SchoolYear");
                            string se = ((XmlElement)updateNode).GetAttribute("Semester");

                            List<XmlElement> list = new List<XmlElement>();
                            foreach (XmlElement subjectNode in updateNode.SelectNodes("Subject"))
                            {
                                list.Add(subjectNode);
                            }
                            list.Sort(SortSubject);
                            foreach (XmlElement s in list)
                            {
                                subjectScoreInfo.AppendChild(doc.ImportNode(s, true));
                            }
                            insertList.Add(new SmartSchool.Feature.Score.AddScore.InsertInfo(stu.StudentID, sy, se, gradeYear, "", subjectScoreInfo));
                        }
                        #endregion
                    }
                }
                if (updateList.Count > 0)
                    SmartSchool.Feature.Score.EditScore.UpdateSemesterSubjectScore(updateList.ToArray());
                if (insertList.Count > 0)
                    SmartSchool.Feature.Score.AddScore.InsertSemesterSubjectScore(insertList.ToArray());
                bkw.ReportProgress((int)((computedStudents * 100.0) / maxStudents));
            }
            e.Result = selectedStudents;
        }

        private static SmartSchool.Common.StringComparer sc1 = new SmartSchool.Common.StringComparer("學業", "體育", "國防通識", "健康與護理", "實習科目");
        private static SmartSchool.Common.StringComparer sc2 = new SmartSchool.Common.StringComparer("國文", "英文", "數學", "物理", "化學", "生物", "地理", "歷史", "公民");
        private static int SortSubject(XmlElement s1, XmlElement s2)
        {
            string e1 = s1.GetAttribute("開課分項類別");
            string e2 = s2.GetAttribute("開課分項類別");
            string n1 = s1.GetAttribute("科目");
            string n2 = s2.GetAttribute("科目");
            if (sc1.Compare(e1, e2) == 0)
                return sc2.Compare(n1, n2);
            else
                return sc1.Compare(e1, e2);
        }

        void _uploadingWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBarX2.Value = e.ProgressPercentage;
        }

        void _uploadingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.progressBarX2.Value = 100;
            List<StudentRecord> selectedStudents = (List<StudentRecord>)e.Result;
            List<string> idList = new List<string>();
            foreach (StudentRecord var in selectedStudents)
            {
                idList.Add(var.StudentID);
            }
            EventHub.Instance.InvokScoreChanged(idList.ToArray());
            labelX5.Text = "學期科目成績上傳完成";
            wizardPage4.FinishButtonEnabled = eWizardButtonState.True;

            #region Log

            LogUtility.WriteLog(_Type, selectedStudents, numericUpDown1.Value.ToString(), numericUpDown2.Value.ToString(), "科目");

            #endregion
        }
        #endregion

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //假若 _WarnViewer 警告視窗 經使用者確認過後資料後，
            //將會啟動上傳 學期科目成績機制(若有錯誤資訊，使用者將無法點選上傳按鈕)
            if (_WarnViewer.ShowDialog() == DialogResult.OK)
            {
                wizard1.SelectedPage = wizardPage4;
                upLoad(resultList);
            }
        }
    }
}
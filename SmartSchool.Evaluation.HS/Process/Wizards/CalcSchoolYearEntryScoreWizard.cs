using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Rendering;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.Evaluation.WearyDogComputerHelper;

namespace SmartSchool.Evaluation.Process.Wizards
{
    public partial class CalcSchoolYearEntryScoreWizard : SmartSchool.Common.BaseForm
    {
        private const int _MaxPackageSize = 450;

        private ErrorViewer _ErrorViewer = new ErrorViewer();

        private BackgroundWorker runningBackgroundWorker = new BackgroundWorker();

        private SelectType _Type;

        private SchoolYearScoreCalcType _CalcType;

        public CalcSchoolYearEntryScoreWizard(SelectType type):this(type, SchoolYearScoreCalcType.SemesterEntryScore)
        {

        }

        public CalcSchoolYearEntryScoreWizard(SelectType type,SchoolYearScoreCalcType calctype)
        {
            _Type = type;
            _CalcType = calctype;
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
                    labelX1.Text = "選擇學年度";
                    labelX2.Text = "學年度";
                    break;
                case SelectType.GradeYearStudent:
                    this.Text = "計算" + SmartSchool.Customization.Data.SystemInformation.SchoolYear + "學年分項成績";
                    this.numericUpDown1.Minimum = 1;
                    this.numericUpDown1.Maximum = 5;
                    this.numericUpDown1.Value = 1;
                    wizardPage1.PageTitle = labelX1.Text = "選擇年級";
                    labelX2.Text = "年級";
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

        #region 計算學年分項成績
        private void wizardPage2_AfterPageDisplayed(object sender, WizardPageChangeEventArgs e)
        {
            AccessHelper helper = new AccessHelper();
            List<StudentRecord> selectedStudents;
            int schooyYear;
            switch (_Type)
            {
                default:
                case SelectType.Student:
                    selectedStudents = helper.StudentHelper.GetSelectedStudent();
                    schooyYear = (int)numericUpDown1.Value;
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
                    break;
            }
            linkLabel1.Visible = false;
            labelX4.Text = "學年分項成績計算中...";
            runningBackgroundWorker = new BackgroundWorker();
            runningBackgroundWorker.WorkerSupportsCancellation = true;
            runningBackgroundWorker.WorkerReportsProgress = true;
            runningBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(bkw_ProgressChanged);
            runningBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bkw_RunWorkerCompleted);
            runningBackgroundWorker.DoWork += new DoWorkEventHandler(bkw_DoWork);
            runningBackgroundWorker.RunWorkerAsync(new object[] { schooyYear, helper, selectedStudents });
        }

        void bkw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = ((BackgroundWorker)sender);
            int schoolyear = (int)((object[])e.Argument)[0];
            AccessHelper helper = (AccessHelper)((object[])e.Argument)[1];
            List<StudentRecord> selectedStudents = (List<StudentRecord>)((object[])e.Argument)[2];
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

            List<SmartSchool.Feature.Score.AddScore.InsertInfo> insertList = new List<SmartSchool.Feature.Score.AddScore.InsertInfo>();
            List<SmartSchool.Feature.Score.EditScore.UpdateInfo> updateList = new List<SmartSchool.Feature.Score.EditScore.UpdateInfo>();
            XmlDocument doc = new XmlDocument();
            foreach (List<StudentRecord> var in packages)
            {
                computedStudents += var.Count;
                Dictionary<StudentRecord, List<string>> errormessages = new Dictionary<StudentRecord, List<string>>();

                if (_CalcType == SchoolYearScoreCalcType.SemesterEntryScore)
                    computer.FillSchoolYearEntryCalcScore(schoolyear, helper, var);
                else if (_CalcType == SchoolYearScoreCalcType.SchoolYearSubject)
                    computer.FillSchoolYearEntryCalcScoreBySchoolYearSubjectScore(schoolyear, helper, var);

                helper.StudentHelper.FillSchoolYearEntryScore(false, var);

                #region 每個學生去整理新增跟修改的資料
                foreach (StudentRecord stu in var)
                {
                    if (stu.Fields.ContainsKey("CalcSchoolYearEntryScores"))
                    {
                        Dictionary<string, decimal> entryScore = (Dictionary<string, decimal>)stu.Fields["CalcSchoolYearEntryScores"];
                        //有分項成績
                        if (entryScore.Count > 0)
                        {
                            List<string> enableEntryLists = new List<string>(new string[] { "體育", "國防通識", "健康與護理", "實習科目", "學業" });

                            int? gradeyear = null;

                            if (_CalcType == SchoolYearScoreCalcType.SemesterEntryScore)
                                gradeyear = stu.SemesterEntryScoreList.GetGradeYear(schoolyear);
                            else if (_CalcType == SchoolYearScoreCalcType.SchoolYearSubject)
                                gradeyear = stu.SchoolYearSubjectScoreList.GetGradeYear(schoolyear);

                            //年級沒有問題
                            if (gradeyear != null)
                            {

                                string updateid = "";
                                #region 找到ID，將計算分項與現有成績的分向做聯集
                                Dictionary<int, Dictionary<string, string>> scoreID = (Dictionary<int, Dictionary<string, string>>)stu.Fields["SchoolYearEntryScoreID"];
                                //只處理以下幾個分項類別(學習類)
                                foreach (SchoolYearEntryScoreInfo sc in stu.SchoolYearEntryScoreList)
                                {
                                    if (enableEntryLists.Contains(sc.Entry) && sc.SchoolYear == schoolyear)
                                    {
                                        updateid = scoreID[sc.SchoolYear]["學習"];
                                        //如果計算的結果並不包含已存在成績的分項，將該分項及成績加入至計算的結果
                                        if (!entryScore.ContainsKey(sc.Entry))
                                        {
                                            entryScore.Add(sc.Entry, sc.Score);
                                        }
                                    }
                                }
                                #endregion
                                if (updateid != "")
                                {
                                    XmlElement entryScoreInfo = doc.CreateElement("SchoolYearEntryScore");
                                    foreach (string entry in entryScore.Keys)
                                    {
                                        XmlElement entryElement = doc.CreateElement("Entry");
                                        entryElement.SetAttribute("分項", entry);
                                        entryElement.SetAttribute("成績", "" + entryScore[entry]);
                                        entryScoreInfo.AppendChild(entryElement);
                                    }
                                    updateList.Add(new SmartSchool.Feature.Score.EditScore.UpdateInfo(updateid, "" + gradeyear, entryScoreInfo));
                                }
                                else
                                {
                                    XmlElement entryScoreInfo = doc.CreateElement("SchoolYearEntryScore");
                                    foreach (string entry in entryScore.Keys)
                                    {
                                        XmlElement entryElement = doc.CreateElement("Entry");
                                        entryElement.SetAttribute("分項", entry);
                                        entryElement.SetAttribute("成績", "" + entryScore[entry]);
                                        entryScoreInfo.AppendChild(entryElement);
                                    }
                                    insertList.Add(new SmartSchool.Feature.Score.AddScore.InsertInfo(stu.StudentID, "" + schoolyear, "", "" + gradeyear, "學習", entryScoreInfo));
                                }
                            }
                        }
                    }
                }
                #endregion


                if (errormessages.Count > 0)
                    allPass = false;
                if (bkw.CancellationPending)
                    break;
                else
                    bkw.ReportProgress((int)((computedStudents * 100.0) / maxStudents), errormessages);
            }
            if (allPass)
                e.Result = new object[] { insertList, updateList, selectedStudents };
            else
                e.Result = null;
        }

        void bkw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!((BackgroundWorker)sender).CancellationPending)
            {
                if (e.Result == null)
                {
                    linkLabel1.Visible = true;
                    labelX4.Text = "計算失敗，請檢查錯誤訊息。";
                }
                else
                {
                    wizard1.SelectedPage = wizardPage4;
                    upLoad(e.Result);
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

        #region 上傳學年分項成績

        BackgroundWorker _uploadingWorker = new BackgroundWorker();
        private void wizardPage4_CancelButtonClick(object sender, CancelEventArgs e)
        {
            if (_uploadingWorker.IsBusy)
                _uploadingWorker.CancelAsync();
            this.Close();
        }

        private void upLoad(object list)
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
            List<SmartSchool.Feature.Score.AddScore.InsertInfo> insertList = (List<SmartSchool.Feature.Score.AddScore.InsertInfo>)((object[])e.Argument)[0];
            List<SmartSchool.Feature.Score.EditScore.UpdateInfo> updateList = (List<SmartSchool.Feature.Score.EditScore.UpdateInfo>)((object[])e.Argument)[1];
            List<StudentRecord> selectedStudents = (List<StudentRecord>)((object[])e.Argument)[2];
            double maxItems = insertList.Count + updateList.Count;
            if (maxItems == 0) maxItems = 1;
            double uploadedItems = 0;

            int packageSize = 50;
            int packageCount = 0;
            List<SmartSchool.Feature.Score.AddScore.InsertInfo> insertpackage = null;
            List<List<SmartSchool.Feature.Score.AddScore.InsertInfo>> insertPackages = new List<List<SmartSchool.Feature.Score.AddScore.InsertInfo>>();
            bkw.ReportProgress(1, null);
            foreach (SmartSchool.Feature.Score.AddScore.InsertInfo info in insertList)
            {
                if (packageCount == 0)
                {
                    insertpackage = new List<SmartSchool.Feature.Score.AddScore.InsertInfo>(packageSize);
                    insertPackages.Add(insertpackage);
                    packageCount = packageSize;
                    packageSize += 50;
                    if (packageSize > _MaxPackageSize)
                        packageSize = _MaxPackageSize;
                }
                insertpackage.Add(info);
                packageCount--;
            }

            packageSize = 50;
            packageCount = 0;
            List<SmartSchool.Feature.Score.EditScore.UpdateInfo> updatepackage = null;
            List<List<SmartSchool.Feature.Score.EditScore.UpdateInfo>> updatePackages = new List<List<SmartSchool.Feature.Score.EditScore.UpdateInfo>>();
            foreach (SmartSchool.Feature.Score.EditScore.UpdateInfo info in updateList)
            {
                if (packageCount == 0)
                {
                    updatepackage = new List<SmartSchool.Feature.Score.EditScore.UpdateInfo>(packageSize);
                    updatePackages.Add(updatepackage);
                    packageCount = packageSize;
                    packageSize += 50;
                    if (packageSize > _MaxPackageSize)
                        packageSize = _MaxPackageSize;
                }
                updatepackage.Add(info);
                packageCount--;
            }
            foreach (List<SmartSchool.Feature.Score.AddScore.InsertInfo> package in insertPackages)
            {
                if (package.Count > 0)
                    SmartSchool.Feature.Score.AddScore.InsertSchoolYearEntryScore(package.ToArray());
                uploadedItems += package.Count;
                bkw.ReportProgress((int)((uploadedItems * 100.0) / maxItems));
            }
            foreach (List<SmartSchool.Feature.Score.EditScore.UpdateInfo> package in updatePackages)
            {
                if (package.Count > 0)
                    SmartSchool.Feature.Score.EditScore.UpdateSchoolYearEntryScore(package.ToArray());
                uploadedItems += package.Count;
                bkw.ReportProgress((int)((uploadedItems * 100.0) / maxItems));
            }
            e.Result = selectedStudents;
        }

        void _uploadingWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBarX2.Value = e.ProgressPercentage;
        }

        void _uploadingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<StudentRecord> selectedStudents = (List<StudentRecord>)e.Result;
            List<string> idList = new List<string>();
            foreach (StudentRecord var in selectedStudents)
            {
                idList.Add(var.StudentID);
            }
            EventHub.Instance.InvokScoreChanged(idList.ToArray());
            this.progressBarX2.Value = 100;
            labelX5.Text = "學年分項成績上傳完成";
            wizardPage4.FinishButtonEnabled = eWizardButtonState.True;

            #region Log

            LogUtility.WriteLog(_Type, selectedStudents, numericUpDown1.Value.ToString(), "分項");

            #endregion
        }
        #endregion
    }
}
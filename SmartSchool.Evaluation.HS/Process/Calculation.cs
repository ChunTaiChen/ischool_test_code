using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//using SmartSchool.StudentRelated;
using SmartSchool.Evaluation.Process.Wizards;
using SmartSchool.Customization.Data;
using FISCA.DSAUtil;
using System.Xml;
using SmartSchool.Common;
using SmartSchool.StudentRelated;

namespace SmartSchool.Evaluation.Process
{
    public partial class Calculation : SmartSchool.Evaluation.Process.RibbonBarBase
    {
        public Calculation()
        {
            //InitializeComponent();

            ////SmartSchool.StudentRelated.Student.Instance.SelectionChanged += new EventHandler(Instance_SelectionChanged);
            //SmartSchool.Broadcaster.Events.Items["學生/選取變更"].Handler += delegate
            //{
            //    buttonItem103.Enabled = buttonItem9.Enabled = new AccessHelper().StudentHelper.GetSelectedStudent().Count > 0;

            //    if ( !CurrentUser.Acl["Button0040"].Executable )
            //        buttonItem103.Enabled = false;
            //    if ( !CurrentUser.Acl["Button0085"].Executable )
            //        buttonItem9.Enabled = false;
            //};
            //this.Level = 8.5;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Calculation));
            var buttonItem103 = K12.Presentation.NLDPanels.Student.RibbonBarItems["教務"]["成績作業"];
            //buttonItem103.Image = ( (System.Drawing.Image)( resources.GetObject("buttonItem103.Image") ) );

            //var buttonItem1 = buttonItem103["計算學期成績"];
            buttonItem103.Image = Properties.Resources.calc_save_64;
            buttonItem103.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            buttonItem103["計算學期科目成績"].Click += new System.EventHandler(this.buttonItem4_Click);
            buttonItem103["計算學期分項成績"].Click += new System.EventHandler(this.buttonItem5_Click);

            //var buttonItem7 = buttonItem103["計算學年成績"];
            //buttonItem103.BeginGroup = true;
            buttonItem103["計算學年科目成績"].BeginGroup = true;
            buttonItem103["計算學年科目成績"].Click += new System.EventHandler(this.buttonItem2_Click);
            buttonItem103["計算學年調整成績"].Click += new System.EventHandler(this.buttonItem2_1_Click);
            buttonItem103["計算學年分項成績(依學期分項成績，日校適用)"].Click += (sender, e) => new CalcSchoolYearEntryScoreWizard(SelectType.Student).ShowDialog();
            buttonItem103["計算學年分項成績(依學年科目成績，進校適用)"].Click += (sender, e) => new CalcSchoolYearEntryScoreWizard(SelectType.Student, SchoolYearScoreCalcType.SchoolYearSubject).ShowDialog();

            //new System.EventHandler(this.buttonItem3_Click);

            //private void buttonItem3_Click(object sender, EventArgs e)
            //{
            //    new CalcSchoolYearEntryScoreWizard(SelectType.Student).ShowDialog();
            //}

            buttonItem103["計算畢業成績"].BeginGroup = true;
            buttonItem103["計算畢業成績"].Click += new System.EventHandler(this.buttonItem6_Click_1);
            //buttonItem103["計算畢業成績"].BeginGroup = true;
            buttonItem103["檢查畢業資格"].Click += new System.EventHandler(this.buttonItem8_Click_1);
            buttonItem103["取得學程判斷"].Click += new System.EventHandler(this.buttonItem10_Click);
            //buttonItem103["取得學程判斷"].BeginGroup = true;

            buttonItem103.Enable = CurrentUser.Acl["Button0040"].Executable && K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0;

            var buttonItem9 = K12.Presentation.NLDPanels.Student.RibbonBarItems["學務"]["德行成績(舊制)"];
            buttonItem9.Image = ( (System.Drawing.Image)( resources.GetObject("buttonItem9.Image") ) );
            buttonItem9["計算德行學期成績(舊制)"].Click += new System.EventHandler(this.buttonItem6_Click);
            buttonItem9["計算德行學年成績(舊制)"].Click += new System.EventHandler(this.buttonItem8_Click);

            buttonItem9.Enable = CurrentUser.Acl["Button0085"].Executable && K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0;
            K12.Presentation.NLDPanels.Student.SelectedSourceChanged += delegate
            {
                buttonItem103.Enable = CurrentUser.Acl["Button0040"].Executable && K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0;
                buttonItem9.Enable = CurrentUser.Acl["Button0085"].Executable && K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0;
            };
        }

        //private void Instance_SelectionChanged(object sender, EventArgs e)
        //{
        //    buttonItem103.Enabled = SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
        //    buttonItem9.Enabled = SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;

        //    if (!CurrentUser.Acl["Button0040"].Executable)
        //        buttonItem103.Enabled = false;
        //    if (!CurrentUser.Acl["Button0085"].Executable)
        //        buttonItem9.Enabled = false;
        //}

        public override string ProcessTabName
        {
            get
            {
                return "學生";
            }
        }
        /// <summary>
        /// 計算學期分項成績
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonItem5_Click(object sender, EventArgs e)
        {
            new CalcSemesterEntryScoreWizard(SelectType.Student).ShowDialog();

        }
        /// <summary>
        /// 計算學年科目成績
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonItem4_Click(object sender, EventArgs e)
        {
            ISubjectCalcPostProcess obj = FISCA.InteractionService.DiscoverAPI<ISubjectCalcPostProcess>();
            if (obj != null)
            {
                obj.ShowConfigForm();
            }

            new CalcSemesterSubjectScoreWizard(SelectType.Student).ShowDialog();
        }

        private void buttonItem6_Click(object sender, EventArgs e)
        {
            new CalcSemesterMoralScoreWizard(SelectType.Student).ShowDialog();
        }

        private void buttonItem8_Click(object sender, EventArgs e)
        {
            new CalcSchoolYearMoralScoreWizard(SelectType.Student).ShowDialog();
        }

        private void buttonItem2_Click(object sender, EventArgs e)
        {
            new CalcSchoolYearSubjectScoreWizard(SelectType.Student).ShowDialog();
        }

        private void buttonItem2_1_Click(object sender, EventArgs e)
        {
            new CalcSchoolYearApplyScoreWizard(SelectType.Student).ShowDialog();
        }

        private const int _MaxPackageSize = 250;

        private ErrorViewer _ErrorViewer = new ErrorViewer();

        private void buttonItem6_Click_1(object sender, EventArgs e)
        {
            AccessHelper helper = new AccessHelper();
            List<StudentRecord> selectedStudents = helper.StudentHelper.GetSelectedStudent();

            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("學生畢業成績計算中...");
            BackgroundWorker runningBackgroundWorker = new BackgroundWorker();
            runningBackgroundWorker.WorkerSupportsCancellation = true;
            runningBackgroundWorker.WorkerReportsProgress = true;
            runningBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(runningBackgroundWorker_ProgressChanged);
            runningBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(runningBackgroundWorker_RunWorkerCompleted);
            runningBackgroundWorker.DoWork += new DoWorkEventHandler(runningBackgroundWorker_DoWork);
            runningBackgroundWorker.RunWorkerAsync(new object[] { helper, selectedStudents });
        }

        #region 計算畢業成績

        void runningBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = ( (BackgroundWorker)sender );
            AccessHelper helper = (AccessHelper)( (object[])e.Argument )[0];
            List<StudentRecord> selectedStudents = (List<StudentRecord>)( (object[])e.Argument )[1];
            WearyDogComputer computer = new WearyDogComputer();
            int packageSize = 50;
            int packageCount = 0;
            List<StudentRecord> package = null;
            List<List<StudentRecord>> packages = new List<List<StudentRecord>>();
            bkw.ReportProgress(1, null);
            #region 切package
            foreach ( StudentRecord s in selectedStudents )
            {
                if ( packageCount == 0 )
                {
                    package = new List<StudentRecord>(packageSize);
                    packages.Add(package);
                    packageCount = packageSize;
                    packageSize += 50;
                    if ( packageSize > _MaxPackageSize )
                        packageSize = _MaxPackageSize;
                }
                package.Add(s);
                packageCount--;
            }
            #endregion

            double maxStudents = selectedStudents.Count;
            if ( maxStudents == 0 )
                maxStudents = 1;
            double computedStudents = 0;
            bool allPass = true;

            List<DSRequest> requests = new List<DSRequest>();
            foreach ( List<StudentRecord> var in packages )
            {
                if ( var.Count == 0 ) continue;
                Dictionary<StudentRecord, List<string>> errormessages = computer.FillStudentGradCalcScore(helper, var);
                DSXmlHelper requesthelper = new DSXmlHelper("UpdateStudentList");
                foreach ( StudentRecord stu in var )
                {
                    requesthelper.AddElement("Student");
                    requesthelper.AddElement("Student", "Field");
                    requesthelper.AddElement("Student/Field", "GradScore");
                    requesthelper.AddElement("Student/Field/GradScore", ( (XmlElement)stu.Fields["GradCalcScore"] ));
                    requesthelper.AddElement("Student", "Condition");
                    requesthelper.AddElement("Student/Condition", "ID", stu.StudentID);
                }
                requests.Add(new DSRequest(requesthelper));
                computedStudents += var.Count;
                if ( errormessages.Count > 0 )
                    allPass = false;
                if ( bkw.CancellationPending )
                    break;
                else
                    bkw.ReportProgress((int)( ( computedStudents * 100.0 ) / maxStudents ), errormessages);
            }
            if ( allPass )
                e.Result = new object[] { requests, selectedStudents };
            else
                e.Result = null;
        }

        void runningBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ( !( (BackgroundWorker)sender ).CancellationPending )
            {
                if ( e.Result == null )
                {
                    SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("畢業成績計算失敗，請檢查錯誤訊息。");
                }
                else
                {
                    SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("畢業成績計算完成。", 100);
                    upLoad(e.Result);
                }
            }
        }

        void runningBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if ( !( (BackgroundWorker)sender ).CancellationPending )
            {
                if ( e.UserState != null )
                {
                    Dictionary<StudentRecord, List<string>> errormessages = (Dictionary<StudentRecord, List<string>>)e.UserState;
                    if ( errormessages.Count > 0 )
                    {
                        foreach ( StudentRecord stu in errormessages.Keys )
                        {
                            _ErrorViewer.SetMessage(stu, errormessages[stu]);
                        }
                        _ErrorViewer.Show();
                    }
                }
                SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("畢業成績計算中...", e.ProgressPercentage);
            }
        }

        private void LogError(StudentRecord var, Dictionary<StudentRecord, List<string>> _ErrorList, string p)
        {
            if ( !_ErrorList.ContainsKey(var) )
                _ErrorList.Add(var, new List<string>());
            _ErrorList[var].Add(p);
        }

        private void upLoad(object list)
        {
            BackgroundWorker _uploadingWorker = new BackgroundWorker();
            _uploadingWorker.WorkerReportsProgress = true;
            _uploadingWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_uploadingWorker_RunWorkerCompleted);
            _uploadingWorker.ProgressChanged += new ProgressChangedEventHandler(_uploadingWorker_ProgressChanged);
            _uploadingWorker.DoWork += new DoWorkEventHandler(_uploadingWorker_DoWork);
            _uploadingWorker.RunWorkerAsync(list);
        }

        void _uploadingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = ( (BackgroundWorker)sender );
            List<DSRequest> requests = (List<DSRequest>)( (object[])e.Argument )[0];
            List<StudentRecord> selectedStudents = (List<StudentRecord>)( (object[])e.Argument )[1];

            double maxPackage = requests.Count;
            if ( maxPackage == 0 ) maxPackage = 1;
            double processedPackage = 0;
            bkw.ReportProgress(1, null);

            foreach ( DSRequest req in requests )
            {
                SmartSchool.Feature.EditStudent.Update(req);
                processedPackage++;
                bkw.ReportProgress((int)( ( processedPackage * 100.0 ) / maxPackage ));
            }
            e.Result = selectedStudents;
        }

        void _uploadingWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("畢業成績上傳中...", e.ProgressPercentage);
        }

        void _uploadingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<StudentRecord> selectedStudents = (List<StudentRecord>)e.Result;
            List<string> idList = new List<string>();
            foreach ( StudentRecord var in selectedStudents )
            {
                idList.Add(var.StudentID);
            }
            EventHub.Instance.InvokScoreChanged(idList.ToArray());
            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("畢業成績上傳完成。", 100);
        }
        #endregion

        private void buttonItem8_Click_1(object sender, EventArgs e)
        {
            AccessHelper helper = new AccessHelper();
            List<StudentRecord> selectedStudents = helper.StudentHelper.GetSelectedStudent();

            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("學生畢業成績計算中...");
            BackgroundWorker runningBackgroundWorker2 = new BackgroundWorker();
            runningBackgroundWorker2.WorkerSupportsCancellation = true;
            runningBackgroundWorker2.WorkerReportsProgress = true;
            runningBackgroundWorker2.ProgressChanged += new ProgressChangedEventHandler(runningBackgroundWorker2_ProgressChanged);
            runningBackgroundWorker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(runningBackgroundWorker2_RunWorkerCompleted);
            runningBackgroundWorker2.DoWork += new DoWorkEventHandler(runningBackgroundWorker2_DoWork);
            runningBackgroundWorker2.RunWorkerAsync(new object[] { helper, selectedStudents });
        }

        #region 審查畢業資格
        void runningBackgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = ( (BackgroundWorker)sender );
            AccessHelper helper = (AccessHelper)( (object[])e.Argument )[0];
            List<StudentRecord> selectedStudents = (List<StudentRecord>)( (object[])e.Argument )[1];
            WearyDogComputer computer = new WearyDogComputer();
            int packageSize = 50;
            int packageCount = 0;
            List<StudentRecord> package = null;
            List<List<StudentRecord>> packages = new List<List<StudentRecord>>();
            bkw.ReportProgress(1, null);
            #region 切package
            foreach ( StudentRecord s in selectedStudents )
            {
                if ( packageCount == 0 )
                {
                    package = new List<StudentRecord>(packageSize);
                    packages.Add(package);
                    packageCount = packageSize;
                    packageSize += 50;
                    if ( packageSize > _MaxPackageSize )
                        packageSize = _MaxPackageSize;
                }
                package.Add(s);
                packageCount--;
            }
            #endregion

            Dictionary<int, List<int>> insertTags = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> removeTags = new Dictionary<int, List<int>>();
            Dictionary<string, int> usefulTags = new Dictionary<string, int>();
            int unPassStudentCount = 0;
            #region 抓現有 未達畢業標準 類的類別
            foreach ( XmlElement tagElement in SmartSchool.Feature.Tag.QueryTag.GetDetailList(SmartSchool.Feature.Tag.TagCategory.Student).SelectNodes("Tag") )
            {
                int id = int.Parse(tagElement.GetAttribute("ID"));
                string name = "";
                string prefix = "";
                if ( tagElement.SelectSingleNode("Prefix") != null )
                    prefix = tagElement.SelectSingleNode("Prefix").InnerText;
                if ( tagElement.SelectSingleNode("Name") != null )
                    name = tagElement.SelectSingleNode("Name").InnerText;
                if ( prefix == "未達畢業標準" )
                {
                    if ( !usefulTags.ContainsKey(name) )
                        usefulTags.Add(name, id);
                }
            }
            #endregion


            double maxStudents = selectedStudents.Count;
            if ( maxStudents == 0 )
                maxStudents = 1;
            double computedStudents = 0;
            bool allPass = true;
            foreach ( List<StudentRecord> var in packages )
            {
                if ( var.Count == 0 ) continue;
                Dictionary<StudentRecord, List<string>> errormessages = computer.FillStudentGradCheck(helper, var);
                #region 把學生"未達畢業標準"類的標籤都加入移除清單
                List<int> idList = new List<int>();
                foreach ( StudentRecord stu in var )
                {
                    idList.Add(int.Parse(stu.StudentID));
                }
                XmlElement studentTags = SmartSchool.Feature.Tag.QueryTag.GetDetailListByStudent(idList);
                foreach ( XmlElement tag in studentTags.SelectNodes("Tag") )
                {
                    int id = int.Parse(tag.GetAttribute("ID"));
                    string prefix = "";
                    int refStudentID = 0;
                    if ( tag.SelectSingleNode("Prefix") != null )
                        prefix = tag.SelectSingleNode("Prefix").InnerText;
                    if ( tag.SelectSingleNode("StudentID") != null )
                        refStudentID = int.Parse(tag.SelectSingleNode("StudentID").InnerText);
                    if ( prefix == "未達畢業標準" )
                    {
                        if ( !removeTags.ContainsKey(id) )
                            removeTags.Add(id, new List<int>());
                        removeTags[id].Add(refStudentID);
                    }
                }
                #endregion

                // 在畢業及離校資訊的離校類別加入檢查可已畢業                
                List<string> studIDList = new List<string>();
                
                foreach ( StudentRecord student in var )
                {
                    #region 整理每個學生未達畢業標準原因
                    XmlElement gradCheckElement = (XmlElement)student.Fields["GradCheck"];
                    int studentID = int.Parse(student.StudentID);
                    //累計未達標準人數
                    if (gradCheckElement.SelectNodes("UnPassReson").Count > 0)
                        unPassStudentCount++;
                    else
                        studIDList.Add(student.StudentID); // 可畢業
                    
                    foreach ( XmlElement unPassElement in gradCheckElement.SelectNodes("UnPassReson") )
                    {
                        string reson = unPassElement.InnerText;
                        int tagID;
                        if ( !usefulTags.ContainsKey(reson) )
                        {
                            //新加入的未達標準原因
                            tagID = SmartSchool.Feature.Tag.EditTag.Insert("未達畢業標準", reson, Color.Tomato.ToArgb(), SmartSchool.Feature.Tag.TagCategory.Student);
                            usefulTags.Add(reson, tagID);
                        }
                        else
                            tagID = usefulTags[reson];
                        //此學生本來有這個TAG就不刪除
                        if ( removeTags.ContainsKey(tagID) && removeTags[tagID].Contains(studentID) )
                            removeTags[tagID].Remove(studentID);
                        else
                        {
                            //學生原本沒有這個TAG就加入新增清單
                            if ( !insertTags.ContainsKey(tagID) )
                                insertTags.Add(tagID, new List<int>());
                            insertTags[tagID].Add(studentID);
                        }
                    }
                    #endregion
                }
                computedStudents += var.Count;

                // 修改畢業及離校資訊的離校類別
                if (studIDList.Count > 0 && errormessages.Count==0)
                {
                    List<K12.Data.LeaveInfoRecord> LeaveInfoRecordList = K12.Data.LeaveInfo.SelectByStudentIDs(studIDList);

                    foreach (K12.Data.LeaveInfoRecord rec in LeaveInfoRecordList)
                        rec.Reason = "畢業";

                    // 更新
                    K12.Data.LeaveInfo.Update(LeaveInfoRecordList);
                    // 同步資料
                    Student.Instance.SyncAllBackground();
                }

                if ( errormessages.Count > 0 )
                    allPass = false;
                if ( bkw.CancellationPending )
                    break;
                else
                    bkw.ReportProgress((int)( ( computedStudents * 100.0 ) / maxStudents ), errormessages);
            }
            if ( allPass )
                e.Result = new object[] { insertTags, removeTags, selectedStudents, unPassStudentCount };
            else
                e.Result = null;
        }

        void runningBackgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if ( !( (BackgroundWorker)sender ).CancellationPending )
            {
                if ( e.UserState != null )
                {
                    Dictionary<StudentRecord, List<string>> errormessages = (Dictionary<StudentRecord, List<string>>)e.UserState;
                    if ( errormessages.Count > 0 )
                    {
                        foreach ( StudentRecord stu in errormessages.Keys )
                        {
                            _ErrorViewer.SetMessage(stu, errormessages[stu]);
                        }
                        _ErrorViewer.Show();
                    }
                }
                SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("畢業資格檢查中...", e.ProgressPercentage);
            }
        }

        void runningBackgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ( !( (BackgroundWorker)sender ).CancellationPending )
            {
                if ( e.Result == null )
                {
                    SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("畢業資格檢查失敗，請檢查錯誤訊息。");
                }
                else
                {
                    SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("畢業資格檢查完成。", 100);
                    upLoad2(e.Result);
                }
            }
        }

        private void upLoad2(object list)
        {
            BackgroundWorker _uploadingWorker2 = new BackgroundWorker();
            _uploadingWorker2.WorkerReportsProgress = true;
            _uploadingWorker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_uploadingWorker2_RunWorkerCompleted);
            _uploadingWorker2.ProgressChanged += new ProgressChangedEventHandler(_uploadingWorker2_ProgressChanged);
            _uploadingWorker2.DoWork += new DoWorkEventHandler(_uploadingWorker2_DoWork);
            _uploadingWorker2.RunWorkerAsync(list);
        }

        void _uploadingWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = ( (BackgroundWorker)sender );
            Dictionary<int, List<int>> insertTags = (Dictionary<int, List<int>>)( (object[])e.Argument )[0];
            Dictionary<int, List<int>> removeTags = (Dictionary<int, List<int>>)( (object[])e.Argument )[1];
            List<StudentRecord> selectedStudents = (List<StudentRecord>)( (object[])e.Argument )[2];
            int unPassStudentCount = (int)( (object[])e.Argument )[3];

            double maxPackage = insertTags.Count + removeTags.Count;
            if ( maxPackage == 0 ) maxPackage = 1;
            double processedPackage = 0;
            bkw.ReportProgress(1, null);

            List<string> updatedList = new List<string>();

            foreach ( int tagid in removeTags.Keys )
            {
                if ( removeTags[tagid].Count == 0 )
                    continue;
                foreach ( int id in removeTags[tagid] )
                {
                    if ( !updatedList.Contains("" + id) )
                        updatedList.Add("" + id);
                }

                SmartSchool.Feature.Tag.EditStudentTag.Remove(removeTags[tagid], tagid);
                processedPackage++;
                bkw.ReportProgress((int)( ( processedPackage * 100.0 ) / maxPackage ));
            }
            foreach ( int tagid in insertTags.Keys )
            {
                if ( insertTags[tagid].Count == 0 )
                    continue;
                foreach ( int id in insertTags[tagid] )
                {
                    if ( !updatedList.Contains("" + id) )
                        updatedList.Add("" + id);
                }

                SmartSchool.Feature.Tag.EditStudentTag.Add(insertTags[tagid], tagid);
                processedPackage++;
                bkw.ReportProgress((int)( ( processedPackage * 100.0 ) / maxPackage ));
            }


            e.Result = new object[] { updatedList, unPassStudentCount };
        }

        void _uploadingWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<string> idList = (List<string>)( (object[])e.Result )[0];
            int unPassCount = (int)( (object[])e.Result )[1];

            SmartSchool.StudentRelated.Student.Instance.TagManager.Refresh();
            //SmartSchool.StudentRelated.Student.Instance.InvokBriefDataChanged(idList.ToArray());
            SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Invoke(idList.ToArray());
            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("檢查結果標註完成。", 100);

            if ( unPassCount > 0 )
                MsgBox.Show("檢查結果標註完成，\n發現" + unPassCount + "名學生未達標準，\n\n這些學生已被標上\"未達畢業標準\"類別，\n\n您可切換至\"依學生分類檢視\"模式檢視這些學生");
            else
                MsgBox.Show("檢查結果標註完成，選取的學生皆達畢業標準。");
        }

        void _uploadingWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("檢查結果標註中...", e.ProgressPercentage);
        }
        #endregion

        private void buttonItem10_Click(object sender, EventArgs e)
        {
            AccessHelper helper = new AccessHelper();
            List<StudentRecord> selectedStudents = helper.StudentHelper.GetSelectedStudent();

            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("學生取得學程判斷中...");
            BackgroundWorker runningBackgroundWorker3 = new BackgroundWorker();
            runningBackgroundWorker3.WorkerSupportsCancellation = true;
            runningBackgroundWorker3.WorkerReportsProgress = true;
            runningBackgroundWorker3.ProgressChanged += new ProgressChangedEventHandler(runningBackgroundWorker3_ProgressChanged);
            runningBackgroundWorker3.RunWorkerCompleted += new RunWorkerCompletedEventHandler(runningBackgroundWorker3_RunWorkerCompleted);
            runningBackgroundWorker3.DoWork += new DoWorkEventHandler(runningBackgroundWorker3_DoWork);
            runningBackgroundWorker3.RunWorkerAsync(new object[] { helper, selectedStudents });
        }

        void runningBackgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = ( (BackgroundWorker)sender );
            AccessHelper helper = (AccessHelper)( (object[])e.Argument )[0];
            List<StudentRecord> selectedStudents = (List<StudentRecord>)( (object[])e.Argument )[1];
            WearyDogComputer computer = new WearyDogComputer();
            int packageSize = 50;
            int packageCount = 0;
            List<StudentRecord> package = null;
            List<List<StudentRecord>> packages = new List<List<StudentRecord>>();
            bkw.ReportProgress(1, null);
            #region 切package
            foreach ( StudentRecord s in selectedStudents )
            {
                if ( packageCount == 0 )
                {
                    package = new List<StudentRecord>(packageSize);
                    packages.Add(package);
                    packageCount = packageSize;
                    packageSize += 50;
                    if ( packageSize > _MaxPackageSize )
                        packageSize = _MaxPackageSize;
                }
                package.Add(s);
                packageCount--;
            }
            #endregion

            Dictionary<int, List<int>> insertTags = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> removeTags = new Dictionary<int, List<int>>();
            Dictionary<string, int> usefulTags = new Dictionary<string, int>();

            List<DSRequest> updateList = new List<DSRequest>();
            #region 抓現有 "取得多學程"或"未取得預設學程"的類別
            foreach ( XmlElement tagElement in SmartSchool.Feature.Tag.QueryTag.GetDetailList(SmartSchool.Feature.Tag.TagCategory.Student).SelectNodes("Tag") )
            {
                int id = int.Parse(tagElement.GetAttribute("ID"));
                string name = "";
                string prefix = "";
                if ( tagElement.SelectSingleNode("Prefix") != null )
                    prefix = tagElement.SelectSingleNode("Prefix").InnerText;
                if ( tagElement.SelectSingleNode("Name") != null )
                    name = tagElement.SelectSingleNode("Name").InnerText;
                if ( prefix == "" && ( name == "取得多學程" || name == "未取得預設學程" ) )
                {
                    if ( !usefulTags.ContainsKey(name) )
                        usefulTags.Add(name, id);
                }
            }
            #endregion


            double maxStudents = selectedStudents.Count;
            if ( maxStudents == 0 )
                maxStudents = 1;
            double computedStudents = 0;
            bool allPass = true;
            foreach ( List<StudentRecord> var in packages )
            {
                if ( var.Count == 0 ) continue;
                //判斷取得學程
                Dictionary<StudentRecord, List<string>> errormessages = computer.FillStudentFulfilledProgram(helper, var);
                //取得學生畢業資訊
                helper.StudentHelper.FillField("DiplomaNumber", var);

                #region 把學生"取得多學程"或"未取得預設學程"的標籤都加入移除清單
                List<int> idList = new List<int>();
                foreach ( StudentRecord stu in var )
                {
                    idList.Add(int.Parse(stu.StudentID));
                }
                XmlElement studentTags = SmartSchool.Feature.Tag.QueryTag.GetDetailListByStudent(idList);
                foreach ( XmlElement tag in studentTags.SelectNodes("Tag") )
                {
                    int id = int.Parse(tag.GetAttribute("ID"));
                    string name = "";
                    string prefix = "";
                    int refStudentID = 0;
                    if ( tag.SelectSingleNode("Prefix") != null )
                        prefix = tag.SelectSingleNode("Prefix").InnerText;
                    if ( tag.SelectSingleNode("StudentID") != null )
                        refStudentID = int.Parse(tag.SelectSingleNode("StudentID").InnerText);
                    if ( tag.SelectSingleNode("Name") != null )
                        name = tag.SelectSingleNode("Name").InnerText;
                    if ( prefix == "" && ( name == "取得多學程" || name == "未取得預設學程" ) )
                    {
                        if ( !removeTags.ContainsKey(id) )
                            removeTags.Add(id, new List<int>());
                        removeTags[id].Add(refStudentID);
                    }
                }
                #endregion

                foreach ( StudentRecord student in var )
                {
                    int studentID = int.Parse(student.StudentID);
                    bool diplomaChanged = false;
                    XmlElement diplomaElement;
                    List<string> programList = new List<string>();
                    #region 整理每個學生取得學程資訊
                    if ( student.Fields.ContainsKey("DiplomaNumber") && student.Fields["DiplomaNumber"] != null )
                    {
                        diplomaElement = student.Fields["DiplomaNumber"] as XmlElement;
                        foreach ( XmlElement program in diplomaElement.SelectNodes("Message[@Type='取得學程']") )
                            if ( !programList.Contains(program.GetAttribute("Value")) )
                                programList.Add(program.GetAttribute("Value"));
                    }
                    else
                    {
                        diplomaElement = new XmlDocument().CreateElement("DiplomaNumber");
                    }

                    XmlElement fulfilledProgramElement = (XmlElement)student.Fields["FulfilledProgram"];

                    foreach ( XmlElement programElement in fulfilledProgramElement.SelectNodes("Program") )
                    {
                        if ( !programList.Contains(programElement.InnerText) )//取得新學程
                        {
                            diplomaChanged = true;
                            XmlElement msg = (XmlElement)diplomaElement.AppendChild(diplomaElement.OwnerDocument.CreateElement("Message"));
                            msg.SetAttribute("Type", "取得學程");
                            msg.SetAttribute("Value", programElement.InnerText);
                            programList.Add(programElement.InnerText);
                        }
                    }
                    #endregion
                    if ( diplomaChanged )
                    {
                        #region 有取得新學程的就加入更新清單
                        DSXmlHelper helper2 = new DSXmlHelper("UpdateStudentList");
                        helper2.AddElement("Student");
                        helper2.AddElement("Student", "Field");
                        helper2.AddElement("Student/Field", diplomaElement);
                        helper2.AddElement("Student", "Condition");
                        helper2.AddElement("Student/Condition", "ID", student.StudentID);
                        updateList.Add(new DSRequest(helper2));
                        #endregion
                    }
                    #region 整理學生取得類別
                    List<string> getTags = new List<string>();
                    if ( programList.Count > 1 )
                        getTags.Add("取得多學程");
                    if ( student.Fields.ContainsKey("SubDepartment") &&//科別有子項
                        SubjectTable.Items["學程科目表"].Contains("" + student.Fields["SubDepartment"]) &&//子項是一個學程名稱
                        !programList.Contains("" + student.Fields["SubDepartment"])//取得的學程中沒有子項的這個學程
                        )
                        getTags.Add("未取得預設學程");

                    foreach ( string getTag in getTags )
                    {
                        int tagID;
                        if ( !usefulTags.ContainsKey(getTag) )
                        {
                            //新加入的未達標準原因
                            tagID = SmartSchool.Feature.Tag.EditTag.Insert("", getTag, Color.CornflowerBlue.ToArgb(), SmartSchool.Feature.Tag.TagCategory.Student);
                            usefulTags.Add(getTag, tagID);
                        }
                        else
                            tagID = usefulTags[getTag];
                        //此學生本來有這個TAG就不刪除
                        if ( removeTags.ContainsKey(tagID) && removeTags[tagID].Contains(studentID) )
                            removeTags[tagID].Remove(studentID);
                        else
                        {
                            //學生原本沒有這個TAG就加入新增清單
                            if ( !insertTags.ContainsKey(tagID) )
                                insertTags.Add(tagID, new List<int>());
                            insertTags[tagID].Add(studentID);
                        }
                    }
                    #endregion
                }
                computedStudents += var.Count;
                if ( errormessages.Count > 0 )
                    allPass = false;
                if ( bkw.CancellationPending )
                    break;
                else
                    bkw.ReportProgress((int)( ( computedStudents * 100.0 ) / maxStudents ), errormessages);
            }
            if ( allPass )
                e.Result = new object[] { insertTags, removeTags, updateList, selectedStudents };
            else
                e.Result = null;
        }

        void runningBackgroundWorker3_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if ( !( (BackgroundWorker)sender ).CancellationPending )
            {
                if ( e.UserState != null )
                {
                    Dictionary<StudentRecord, List<string>> errormessages = (Dictionary<StudentRecord, List<string>>)e.UserState;
                    if ( errormessages.Count > 0 )
                    {
                        foreach ( StudentRecord stu in errormessages.Keys )
                        {
                            _ErrorViewer.SetMessage(stu, errormessages[stu]);
                        }
                        _ErrorViewer.Show();
                    }
                }
                SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("學生取得學程判斷中...", e.ProgressPercentage);
            }
        }

        void runningBackgroundWorker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ( !( (BackgroundWorker)sender ).CancellationPending )
            {
                if ( e.Result == null )
                {
                    SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("取得學程判斷失敗，請檢查錯誤訊息。");
                }
                else
                {
                    SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("取得學程判斷完成。", 100);
                    upLoad3(e.Result);
                }
            }
        }

        private void upLoad3(object list)
        {
            BackgroundWorker _uploadingWorker4 = new BackgroundWorker();
            _uploadingWorker4.WorkerReportsProgress = true;
            _uploadingWorker4.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_uploadingWorker4_RunWorkerCompleted);
            _uploadingWorker4.ProgressChanged += new ProgressChangedEventHandler(_uploadingWorker4_ProgressChanged);
            _uploadingWorker4.DoWork += new DoWorkEventHandler(_uploadingWorker4_DoWork);
            _uploadingWorker4.RunWorkerAsync(list);
        }

        void _uploadingWorker4_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = ( (BackgroundWorker)sender );
            Dictionary<int, List<int>> insertTags = (Dictionary<int, List<int>>)( (object[])e.Argument )[0];
            Dictionary<int, List<int>> removeTags = (Dictionary<int, List<int>>)( (object[])e.Argument )[1];
            List<DSRequest> updateRequests = (List<DSRequest>)( (object[])e.Argument )[2];
            List<StudentRecord> selectedStudents = (List<StudentRecord>)( (object[])e.Argument )[3];

            double maxPackage = insertTags.Count + removeTags.Count;
            if ( maxPackage == 0 ) maxPackage = 1;
            double processedPackage = 0;
            bkw.ReportProgress(1, null);

            List<string> updatedList = new List<string>();

            foreach ( int tagid in removeTags.Keys )
            {
                if ( removeTags[tagid].Count == 0 )
                    continue;
                foreach ( int id in removeTags[tagid] )
                {
                    if ( !updatedList.Contains("" + id) )
                        updatedList.Add("" + id);
                }

                SmartSchool.Feature.Tag.EditStudentTag.Remove(removeTags[tagid], tagid);
                processedPackage++;
                bkw.ReportProgress((int)( ( processedPackage * 100.0 ) / maxPackage ));
            }

            MultiThreadWorker<DSRequest> multiThreadUpdater = new MultiThreadWorker<DSRequest>();
            multiThreadUpdater.MaxThreads = 2;
            multiThreadUpdater.PackageSize = 150;
            multiThreadUpdater.PackageWorker += new EventHandler<PackageWorkEventArgs<DSRequest>>(multiThreadUpdater_PackageWorker);
            multiThreadUpdater.Run(updateRequests);

            foreach ( int tagid in insertTags.Keys )
            {
                if ( insertTags[tagid].Count == 0 )
                    continue;
                foreach ( int id in insertTags[tagid] )
                {
                    if ( !updatedList.Contains("" + id) )
                        updatedList.Add("" + id);
                }

                SmartSchool.Feature.Tag.EditStudentTag.Add(insertTags[tagid], tagid);
                processedPackage++;
                bkw.ReportProgress((int)( ( processedPackage * 100.0 ) / maxPackage ));
            }

            e.Result = new object[] { updatedList };
        }

        void multiThreadUpdater_PackageWorker(object sender, PackageWorkEventArgs<DSRequest> e)
        {
            DSXmlHelper helper = new DSXmlHelper("UpdateStudentList");
            foreach ( DSRequest request in e.List )
            {
                foreach ( XmlElement ele in request.GetContent().GetElements("Student") )
                {
                    helper.AddElement(".", ele);
                }
            }
            SmartSchool.Feature.EditStudent.Update(new DSRequest(helper));
        }

        void _uploadingWorker4_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("學生取得學程標註中...", e.ProgressPercentage);
        }

        void _uploadingWorker4_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<string> idList = (List<string>)( (object[])e.Result )[0];

            SmartSchool.StudentRelated.Student.Instance.TagManager.Refresh();
            //SmartSchool.StudentRelated.Student.Instance.InvokBriefDataChanged(idList.ToArray());
            SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Invoke(idList.ToArray());
            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("學生取得學程標註完成。", 100);
        }
    }
}


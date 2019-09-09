using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.Common;
using SmartSchool.Customization.Data;
using SmartSchool.Evaluation.GraduationPlan;
using SmartSchool.ExceptionHandler;
using SmartSchool.Security;
using SystemInformation = SmartSchool.Customization.Data.SystemInformation;
using SHSchool.Data;

namespace SmartSchool.Evaluation.Process
{
    public partial class CreateCourceForClass : UserControl
    {
        FeatureAccessControl createCourseButtonCtl;

        public CreateCourceForClass()
        {
            //班級開課權限
            createCourseButtonCtl = new FeatureAccessControl("Button0365");
            var buttonItem1 = K12.Presentation.NLDPanels.Class.RibbonBarItems["教務"]["班級開課"];
            buttonItem1.Enable = K12.Presentation.NLDPanels.Class.SelectedSource.Count > 0 && createCourseButtonCtl.Executable();
            //buttonItem1.Image = new Bitmap(20, 20);
            buttonItem1.Image = Properties.Resources.subject_64;
            K12.Presentation.NLDPanels.Class.SelectedSourceChanged += delegate
            {
                buttonItem1.Enable = K12.Presentation.NLDPanels.Class.SelectedSource.Count > 0 && createCourseButtonCtl.Executable();
            };
            var buttonItem2 = buttonItem1["直接開課"];
            buttonItem2.Click += new System.EventHandler(this.buttonItem2_Click);
            var buttonItem3 = buttonItem1["依課程規劃表開課"];
            buttonItem3.Click += new System.EventHandler(this.buttonItem3_Click);
        }


        #region 課程規劃表開課
        private void buttonItem3_Click(object sender, EventArgs e)
        {
            SelectSemesterForm form = new SelectSemesterForm();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            if (MsgBox.Show("將會以目前的班級年級資訊，開設" + form.SchoolYear + "學年度" + form.Semester + "學期的課程。\n請確認班級目前的年級以及班級名稱資料無誤，以免開課內容錯誤。", "新建課程", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            {
                BackgroundWorker bkw = new BackgroundWorker();
                bkw.DoWork += new DoWorkEventHandler(bkw_DoWork);
                bkw.ProgressChanged += new ProgressChangedEventHandler(bkw_ProgressChanged);
                bkw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bkw_RunWorkerCompleted);
                bkw.WorkerReportsProgress = true;
                bkw.RunWorkerAsync(form);
            }
        }

        void bkw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = (BackgroundWorker)sender;
            SelectSemesterForm form = e.Argument as SelectSemesterForm;
            
            AccessHelper accessHelper = new AccessHelper();
            bkw.ReportProgress(1);
            double totleClass = accessHelper.ClassHelper.GetSelectedClass().Count;
            if (totleClass <= 0)
                totleClass = 0;
            double processedClass = 0;
            foreach (ClassRecord classRec in accessHelper.ClassHelper.GetSelectedClass())
            {
                processedClass += 1;
                #region 班級開課
                int gradeYear = 0;
                if (!int.TryParse(classRec.GradeYear, out gradeYear)) continue;
                //班級內每個學生的課程規劃表
                Dictionary<GraduationPlanInfo, List<StudentRecord>> graduations = new Dictionary<GraduationPlanInfo, List<StudentRecord>>();
                #region 整理班級內每個學生的課程規劃表
                foreach (StudentRecord studentRec in classRec.Students)
                {
                    //取得學生的課程規劃表
                    GraduationPlanInfo info = GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(studentRec.StudentID);
                    if (info != null)
                    {
                        if (!graduations.ContainsKey(info))
                            graduations.Add(info, new List<StudentRecord>());
                        graduations[info].Add(studentRec);
                    }
                }
                #endregion
                //所有課程規劃表中要開的課程
                Dictionary<string, GraduationPlanSubject> courseList = new Dictionary<string, GraduationPlanSubject>();
                //課程的科目
                Dictionary<string, string> subjectList = new Dictionary<string, string>();
                //課程的級別
                Dictionary<string, string> levelList = new Dictionary<string, string>();
                //有此課程的課程規劃表
                Dictionary<string, List<GraduationPlanInfo>> graduationList = new Dictionary<string, List<GraduationPlanInfo>>();
                #region 整裡所有要開的課程
                foreach (GraduationPlanInfo gplan in graduations.Keys)
                {
                    foreach (GraduationPlanSubject gplanSubject in gplan.SemesterSubjects(gradeYear, form.Semester))
                    {
                        // 如果開選課程沒有勾起，只開必修課程。
                        if(!form.isCreateAll)
                        {
                            if (gplanSubject.Required == "選修")
                                continue;
                        }

                        string key = gplanSubject.SubjectName.Trim() + "^_^" + gplanSubject.Level;
                        if (!courseList.ContainsKey(key))
                        {
                            //新增一個要開的課程
                            courseList.Add(key, gplanSubject);
                            subjectList.Add(key, gplanSubject.SubjectName.Trim());
                            levelList.Add(key, gplanSubject.Level);
                            graduationList.Add(key, new List<GraduationPlanInfo>());
                        }
                        graduationList[key].Add(gplan);
                    }
                }
                #endregion
                //本學期已開的課程
                Dictionary<string, CourseRecord> existSubject = new Dictionary<string, CourseRecord>();
                #region 整裡本學期已開的課程
                foreach (CourseRecord courseRec in accessHelper.CourseHelper.GetClassCourse(form.SchoolYear, form.Semester, classRec))
                {
                    string key = courseRec.Subject + "^_^" + courseRec.SubjectLevel;
                    if (!existSubject.ContainsKey(key))
                        existSubject.Add(key, courseRec);
                }
                #endregion
                #region 開課
                List<SmartSchool.Feature.Course.AddCourse.InsertCourse> newCourses = new List<SmartSchool.Feature.Course.AddCourse.InsertCourse>();
                foreach (string key in courseList.Keys)
                {
                    //是原來沒有的課程
                    if (!existSubject.ContainsKey(key))
                    {
                        GraduationPlanSubject cinfo = courseList[key];
                        newCourses.Add(new SmartSchool.Feature.Course.AddCourse.InsertCourse(
                            classRec.ClassName + " " + cinfo.FullName,
                                cinfo.SubjectName.Trim(),
                                cinfo.Level,
                                classRec.ClassID,
                                form.SchoolYear.ToString(),
                                form.Semester.ToString(),
                                cinfo.Credit,
                                (cinfo.NotIncludedInCredit) ? "是" : "否",
                                (cinfo.NotIncludedInCalc) ? "是" : "否",
                                cinfo.Entry,
                                cinfo.Required == "必修" ? "必" : "選",
                                cinfo.RequiredBy
                            ));
                    }
                }
                if (newCourses.Count > 0)
                {
                    SmartSchool.Feature.Course.AddCourse.Insert(newCourses);
                    SmartSchool.Broadcaster.Events.Items["課程/新增"].Invoke();
                }
                #endregion
                #region 重新整理已開的課程
                existSubject.Clear();
                foreach (CourseRecord courseRec in accessHelper.CourseHelper.GetClassCourse(form.SchoolYear, form.Semester, classRec))
                {
                    string key = courseRec.Subject + "^_^" + courseRec.SubjectLevel;
                    if (!existSubject.ContainsKey(key))
                        existSubject.Add(key, courseRec);
                }
                //填入修課學生
                accessHelper.CourseHelper.FillStudentAttend(existSubject.Values);
                #endregion
                #region 加入學生修課
                DSXmlHelper insertSCAttendHelper = new DSXmlHelper("InsertSCAttend");
                bool addAttend = false;
                foreach (StudentRecord studentRec in classRec.Students)
                {
                    if (GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(studentRec.StudentID) != null)
                    {
                        foreach (GraduationPlanSubject subject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(studentRec.StudentID).SemesterSubjects(gradeYear, form.Semester))
                        {
                            string key = subject.SubjectName.Trim() + "^_^" + subject.Level;
                            bool found = false;
                            if (existSubject.ContainsKey(key))
                            {
                                foreach (StudentAttendCourseRecord attend in existSubject[key].StudentAttendList)
                                {
                                    if (attend.StudentID == studentRec.StudentID)
                                        found = true;
                                }


                                if (!found)
                                {
                                    XmlElement attend = insertSCAttendHelper.AddElement("Attend");
                                    DSXmlHelper.AppendChild(attend, "<RefStudentID>" + studentRec.StudentID + "</RefStudentID>");
                                    DSXmlHelper.AppendChild(attend, "<RefCourseID>" + existSubject[key].CourseID + "</RefCourseID>");

                                    //insertSCAttendHelper.AddElement(".", attend);
                                    addAttend = true;
                                }
                            }
                        }
                    }
                }
                if (addAttend)
                    SmartSchool.Feature.Course.AddCourse.AttendCourse(insertSCAttendHelper);
                #endregion
                #endregion
                //回報進度
                bkw.ReportProgress((int)(processedClass * 100d / totleClass));
            }
        }

        void bkw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("依課程規劃表開課中...", e.ProgressPercentage);
        }

        void bkw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SmartSchool.Broadcaster.Events.Items["課程/新增"].Invoke();
                //有些時後上列事件引發的機制似乎會無效，並且課程的重新整理功能會失效。
                FISCA.Features.Invoke("CourseSyncAllBackground");
                SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("班級開課完成。");
            }
            else
            {
                SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("班級開課發生未預期的錯誤，開課或學生修課動作可能僅部分完成。");
                //BugReporter.ReportException(e.Error, false);
            }
        }
        #endregion

        private void buttonItem2_Click(object sender, EventArgs e)
        {
            new SmartSchool.Evaluation.Process.Wizards.CreateClassCourse().ShowDialog();
        }
    }
}

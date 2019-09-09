using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using SmartSchool.Customization.Data;
using SystemInformation = SmartSchool.Customization.Data.SystemInformation;
using FISCA.DSAUtil;
using DevComponents.DotNetBar;

namespace SmartSchool.Evaluation.Process.Wizards
{
    public partial class CreateClassCourse : BaseForm
    {
        private EnhancedErrorProvider _errors;

        public CreateClassCourse()
        {
            InitializeComponent();
            _errors = new EnhancedErrorProvider();
            _errors.BlinkRate = 0;
            this.cboEntry.Items.Clear();
            this.cboEntry.Items.AddRange(new string[] { "學業", "體育", "國防通識", "健康與護理", "實習科目" });
            this.cboRequired.Items.Clear();
            this.cboRequired.Items.AddRange(new string[] { "選修", "必修" });
            this.cboRequiredBy.Items.Clear();
            this.cboRequiredBy.Items.AddRange(new string[] { "校訂", "部訂" });
            this.cboEntry.SelectedIndex = this.cboRequired.SelectedIndex = this.cboRequiredBy.SelectedIndex = 0;

            try
            {
                for (int i = -2; i <= 2; i++)
                {
                    cboSchoolYear.Items.Add((CurrentUser.Instance.SchoolYear + i) + "");
                }
                cboSchoolYear.Text = CurrentUser.Instance.SchoolYear.ToString();
                cboSemester.Text = CurrentUser.Instance.Semester.ToString();
            }
            catch (Exception ex)
            {
                CurrentUser.ReportError(ex);
            }
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkInItem(object sender, EventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (!cbox.Items.Contains(cbox.Text))
                _errors.SetError(cbox, "輸入值不被允許。");
            else
                _errors.SetError(cbox, "");
            buttonX1.Enabled = !_errors.HasError;
        }

        private void checkIsInteger(object sender, EventArgs e)
        {
            int input;
            TextBox txtbox = (TextBox)sender;
            if (txtbox.Text != "" && (!int.TryParse(txtbox.Text, out input) || input < 0))
                _errors.SetError(txtbox, "必需輸入正整數。");
            else
                _errors.SetError(txtbox, "");
            buttonX1.Enabled = !_errors.HasError;
        }

        private void checkIsdecimal(object sender, EventArgs e)
        {
            decimal input;
            TextBox txtbox = (TextBox)sender;
            if (txtbox.Text != "" && (!decimal.TryParse(txtbox.Text, out input) || input < 0))
                _errors.SetError(txtbox, "必需輸入學分數。");
            else
                _errors.SetError(txtbox, "");
            buttonX1.Enabled = !_errors.HasError;
        }

        private void txtSubject_TextChanged(object sender, EventArgs e)
        {
            if (txtSubject.Text == "")
                _errors.SetError(txtSubject, "必需輸入科目。");
            else
                _errors.SetError(txtSubject, "");
            buttonX1.Enabled = !_errors.HasError;
        }

        private void buttonX1_Click(object sender, EventArgs MessageBox)
        {
            if (MsgBox.Show("目前學期為\"" + SystemInformation.SchoolYear + "\"學年度第\"" + SystemInformation.Semester + "\"學期。\n新建課程將屬於此學期課程，\n如欲新建之課程為下學期課程，\n需先修改系統之學年度學期設定。\n\n確定為選取班級開課？", "新建課程", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                BackgroundWorker bkw = new BackgroundWorker();
                bkw.DoWork += new DoWorkEventHandler(bkw_DoWork);
                bkw.ProgressChanged += new ProgressChangedEventHandler(bkw_ProgressChanged);
                bkw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bkw_RunWorkerCompleted);
                bkw.WorkerReportsProgress = true;
                bkw.RunWorkerAsync(new object[] { txtSubject.Text, txtLevel.Text, txtCredit.Text, cboEntry.Text, cboRequired.Text, cboRequiredBy.Text, int.Parse(cboSchoolYear.Text), int.Parse(cboSemester.Text) });
            }
            this.Close();
        }

        void bkw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = (BackgroundWorker)sender;
            List<string> errors = new List<string>();
            e.Result = errors;
            object[] args = (object[])e.Argument;
            string subject = "" + args[0];
            string level = "" + args[1];
            string credit = "" + args[2];
            string entry = "" + args[3];
            string required = "" + args[4];
            string requiredby = "" + args[5];
            //int schoolYear = SystemInformation.SchoolYear;
            //int semester = SystemInformation.Semester;
            int schoolYear = (int)args[6];
            int semester = (int)args[7];
            int intlevel;
            if (!int.TryParse(level, out intlevel)) intlevel = int.MinValue;

            AccessHelper accessHelper = new AccessHelper();
            Dictionary<ClassRecord, string> newCourseID = new Dictionary<ClassRecord, string>();
            List<ClassRecord> selectedClassList = accessHelper.ClassHelper.GetSelectedClass();
            double totle = selectedClassList.Count;
            double counter = 0;
            if (totle == 0)
                totle = 1;
            else
                totle *= 3;
            bkw.ReportProgress(1, errors);
            #region 檢查重複開課
            foreach (ClassRecord classRecord in selectedClassList)
            {
                foreach (CourseRecord courseRec in accessHelper.CourseHelper.GetClassCourse(schoolYear, semester, classRecord))
                {
                    if (courseRec.Subject == subject)
                    {
                        errors.Add(classRecord.ClassName + "：已有相同科目(" + subject + (intlevel > 0 ? " " + GetNumber(intlevel) : "") + ")的課程。");
                    }
                }
            }
            #endregion
            if (errors.Count == 0)
            {
                #region 開課
                foreach (ClassRecord classRecord in selectedClassList)
                {
                    string courseID = SmartSchool.Feature.Course.AddCourse.Insert(classRecord.ClassName + " " + subject + (intlevel > 0 ? " " + GetNumber(intlevel) : ""), "" + schoolYear, "" + semester);
                    newCourseID.Add(classRecord, courseID);
                    counter++;
                    bkw.ReportProgress((int)(counter * 100d / totle));
                }
                #endregion
                #region 修改課程資料
                DSXmlHelper updateCourseReq = new DSXmlHelper("UpdateRequest");
                foreach (ClassRecord classRecord in selectedClassList)
                {
                    updateCourseReq.AddElement("Course");
                    updateCourseReq.AddElement("Course", "Field");
                    updateCourseReq.AddElement("Course/Field", "Subject", subject);
                    updateCourseReq.AddElement("Course/Field", "SubjectLevel", level);
                    updateCourseReq.AddElement("Course/Field", "RefClassID", classRecord.ClassID);
                    updateCourseReq.AddElement("Course/Field", "Credit", credit);
                    updateCourseReq.AddElement("Course/Field", "IsRequired", (required == "必修" ? "必" : "選"));
                    updateCourseReq.AddElement("Course/Field", "RequiredBy", requiredby);
                    updateCourseReq.AddElement("Course/Field", "ScoreType", entry);
                    updateCourseReq.AddElement("Course", "Condition", "<ID>" + newCourseID[classRecord] + "</ID>", true);
                    counter++;
                    bkw.ReportProgress((int)(counter * 100d / totle));
                }
                SmartSchool.Feature.Course.EditCourse.UpdateCourse(new DSRequest(updateCourseReq));
                #endregion
                #region 加入學生修課
                DSXmlHelper InsertAttendReq = new DSXmlHelper("InsertSCAttend");
                bool hasAttend = false;
                foreach (ClassRecord classRecord in selectedClassList)
                {
                    foreach (StudentRecord studentRec in classRecord.Students)
                    {
                        InsertAttendReq.AddElement("Attend");
                        InsertAttendReq.AddElement("Attend", "RefCourseID", newCourseID[classRecord]);
                        InsertAttendReq.AddElement("Attend", "RefStudentID", studentRec.StudentID);
                        hasAttend = true;
                    }
                    counter++;
                    bkw.ReportProgress((int)(counter * 100d / totle));
                }
                if (hasAttend)
                    SmartSchool.Feature.Course.AddCourse.AttendCourse(InsertAttendReq);
                #endregion
                SmartSchool.Broadcaster.Events.Items["課程/新增"].Invoke();
            }
        }

        void bkw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("班級開課中...", e.ProgressPercentage);
        }

        void bkw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<string> errors = (List<string>)e.Result;
            if (errors.Count == 0)
                SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("班級開課完成。");
            else
            {
                #region 建立錯誤顯示視窗
                System.Windows.Forms.RichTextBox richTextBox1 = new System.Windows.Forms.RichTextBox();
                richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                            | System.Windows.Forms.AnchorStyles.Left)
                            | System.Windows.Forms.AnchorStyles.Right)));
                richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                richTextBox1.Location = new System.Drawing.Point(6, 9);
                richTextBox1.Name = "richTextBox1";
                richTextBox1.Size = new System.Drawing.Size(440, 340);
                richTextBox1.TabIndex = 0;
                richTextBox1.Text = "";
                BaseForm errorForm = new BaseForm();
                errorForm.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
                errorForm.ClientSize = new System.Drawing.Size(453, 358);
                errorForm.Controls.Add(richTextBox1);
                errorForm.Icon = this.Icon;
                errorForm.MaximizeBox = true;
                errorForm.MinimizeBox = true;
                errorForm.Name = "Errors";
                errorForm.ShowIcon = true;
                errorForm.ShowInTaskbar = true;
                errorForm.Text = "資料有誤，無法開課";
                errorForm.ClientSize = new System.Drawing.Size(320, 280);
                #endregion
                foreach (string error in errors)
                {
                    richTextBox1.Text += error + "\n";
                }
                errorForm.Text += "共 " + errors.Count + " 筆錯誤";
                errorForm.Show();
                SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("資料有誤，班級開課作業已取消。");
            }
        }

        private string GetNumber(int p)
        {
            string levelNumber;
            switch (p)
            {
                #region 對應levelNumber
                case 1:
                    levelNumber = "I";
                    break;
                case 2:
                    levelNumber = "II";
                    break;
                case 3:
                    levelNumber = "III";
                    break;
                case 4:
                    levelNumber = "IV";
                    break;
                case 5:
                    levelNumber = "V";
                    break;
                case 6:
                    levelNumber = "VI";
                    break;
                case 7:
                    levelNumber = "VII";
                    break;
                case 8:
                    levelNumber = "VIII";
                    break;
                case 9:
                    levelNumber = "IX";
                    break;
                case 10:
                    levelNumber = "X";
                    break;
                default:
                    levelNumber = "" + (p);
                    break;
                #endregion
            }
            return levelNumber;
        }

        private void checkSchoolYear(object sender, EventArgs e)
        {
            int i;
            if (!int.TryParse(cboSchoolYear.Text, out i))
                _errors.SetError(cboSchoolYear, "學年度必須為數字");
            else if (int.TryParse(cboSchoolYear.Text, out i) && i <= 0)
                _errors.SetError(cboSchoolYear, "學年度必須為正整數");
            else
                _errors.SetError(cboSchoolYear, "");
            buttonX1.Enabled = !_errors.HasError;
        }

        private void checkSemester(object sender, EventArgs e)
        {
            int i;
            if (!int.TryParse(cboSemester.Text, out i))
                _errors.SetError(cboSemester, "學期必須為 1 或 2");
            else if (int.TryParse(cboSemester.Text, out i) && (i < 1 || i > 2))
                _errors.SetError(cboSemester, "學期必須為 1 或 2");
            else
                _errors.SetError(cboSemester, "");
            buttonX1.Enabled = !_errors.HasError;
        }
    }
}
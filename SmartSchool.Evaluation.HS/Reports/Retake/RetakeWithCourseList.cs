using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Customization.PlugIn;
using System.ComponentModel;
using Aspose.Cells;
using System.IO;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.Common;

namespace SmartSchool.Evaluation.Reports.Retake
{
    class RetakeWithCourseList:ButtonAdapter
    {
        public RetakeWithCourseList()
        {
            this.Path = "重修資料";
            this.Text = "隨堂重修課程表";
            this.OnClick += delegate
            {
                SelectSemesterForm form = new SelectSemesterForm();
                if ( form.ShowDialog() == System.Windows.Forms.DialogResult.OK )
                {
                    int schoolYear = form.SchoolYear, semester = form.Semester;
                    AccessHelper helper = new AccessHelper();
                    List<StudentRecord> studentList = helper.StudentHelper.GetSelectedStudent();
                    #region 下載資料
                    MultiThreadBackgroundWorker<StudentRecord> dataLoader = new MultiThreadBackgroundWorker<StudentRecord>();
                    //dataLoader.Loading = SmartSchool.Common.MultiThreadLoading.Heavy;
                    dataLoader.PackageSize = 125;
                    dataLoader.DoWork += delegate(object sender, PackageDoWorkEventArgs<StudentRecord> e)
                    {
                        helper.StudentHelper.FillSemesterSubjectScore(true, e.Items);
                        helper.StudentHelper.FillAttendCourse(schoolYear, semester, e.Items);
                    };
                    dataLoader.ProgressChanged += delegate(object sender, ProgressChangedEventArgs e) { SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("隨堂重修課程表產生中...", e.ProgressPercentage * 70 / 100); };
                    dataLoader.RunWorkerCompleted += delegate(object se, RunWorkerCompletedEventArgs ex)
                    {
                        if ( ex.Error != null )
                            throw ex.Error;
                        #region 資料抓完就生張報表出來
                        Workbook template = new Workbook();
                        Workbook workBook = new Workbook();

                        template.Open(new MemoryStream(Properties.Resources.隨堂重修清單), FileFormatType.Excel2003);
                        workBook.Open(new MemoryStream(Properties.Resources.隨堂重修清單), FileFormatType.Excel2003);

                        BackgroundWorker worker = new BackgroundWorker();
                        worker.WorkerReportsProgress = true;
                        worker.RunWorkerCompleted += delegate { SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("隨堂重修課程表產生完成"); Common.Excel.Save("隨堂重修課程表", workBook); };
                        worker.ProgressChanged += delegate(object sender, ProgressChangedEventArgs e) { SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("隨堂重修課程表產生中...", e.ProgressPercentage); };
                        worker.DoWork += delegate
                        {
                            #region 產生報表
                            #region 填入報表
                            int rowIndex = 1;
                            int count = 0, sum = studentList.Count;
                            foreach ( StudentRecord studentRec in studentList )
                            {
                                foreach ( StudentAttendCourseRecord attendRecord in studentRec.AttendCourseList )
                                {
                                    foreach ( SemesterSubjectScoreInfo subjectScore in studentRec.SemesterSubjectScoreList )
                                    {
                                        if ( ( subjectScore.SchoolYear * 10 + subjectScore.Semester ) < ( schoolYear * 10 + semester ) && subjectScore.Subject == attendRecord.Subject && subjectScore.Level == attendRecord.SubjectLevel )
                                        {
                                            #region 如果需要換頁就填入下一頁的樣版
                                            if ( rowIndex % 45 == 0 )
                                            {
                                                for ( int i = 0 ; i < 45 ; i++ )
                                                {
                                                    workBook.Worksheets[0].Cells.CopyRow(template.Worksheets[0].Cells, i, rowIndex + i);
                                                    workBook.Worksheets[0].Cells.SetRowHeight(rowIndex + i, template.Worksheets[0].Cells.GetRowHeight(i));
                                                }
                                                workBook.Worksheets[0].HPageBreaks.Add(rowIndex, 7);
                                                rowIndex++;
                                            }
                                            #endregion
                                            workBook.Worksheets[0].Cells[rowIndex, 0].PutValue(studentRec.StudentNumber);
                                            workBook.Worksheets[0].Cells[rowIndex, 1].PutValue(studentRec.RefClass == null ? "" : studentRec.RefClass.ClassName);
                                            workBook.Worksheets[0].Cells[rowIndex, 2].PutValue(studentRec.SeatNo);
                                            workBook.Worksheets[0].Cells[rowIndex, 3].PutValue(studentRec.StudentName);
                                            workBook.Worksheets[0].Cells[rowIndex, 4].PutValue(attendRecord.CourseName);
                                            workBook.Worksheets[0].Cells[rowIndex, 5].PutValue(subjectScore.SchoolYear);
                                            workBook.Worksheets[0].Cells[rowIndex, 6].PutValue(subjectScore.Semester);
                                            rowIndex++;
                                        }
                                    }
                                }
                                worker.ReportProgress(70 + count * 30 / sum);
                            }
                            #endregion
                            #endregion
                        };
                        worker.RunWorkerAsync(); 
                        #endregion
                    };
                    dataLoader.RunWorkerAsync(studentList);
                    #endregion
                }
            };
        }
    }
}

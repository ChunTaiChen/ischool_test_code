using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Customization.PlugIn.Report;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using Aspose.Cells;
using System.Collections;
using System.Windows.Forms;
using SmartSchool.Common;

namespace SmartSchool.Evaluation.Reports.CreditStatistic
{
     public class CreditStatistic
     {
          public CreditStatistic()
          {
               ButtonAdapter button = new SecureButtonAdapter("Report0180");
               button.Text = "學分統計表";
               button.Path = "成績相關報表";
               button.OnClick += new EventHandler(button_OnClick);

               ClassReport.AddReport(button);
          }

          private void button_OnClick(object sender, EventArgs e)
          {
               Workbook wb = new Workbook();
               wb.Open(new MemoryStream(Properties.Resources.學分統計表), FileFormatType.Excel2003);

               Worksheet templateSheet = wb.Worksheets[0];
               Worksheet resultSheet = wb.Worksheets[wb.Worksheets.Add()];

               resultSheet.Copy(templateSheet);

               Range tempalteHeader = templateSheet.Cells.CreateRange(0, 3, false);
               Range tempalteRow = templateSheet.Cells.CreateRange(3, 1, false);

               AccessHelper dataSource = new AccessHelper();
               List<ClassRecord> classes = dataSource.ClassHelper.GetSelectedClass();
               List<StudentRecord> students = new List<StudentRecord>();

               foreach (ClassRecord eachClass in classes)
                    dataSource.StudentHelper.FillSemesterSubjectScore(true, eachClass.Students);

               int rowIndex = 0;
               foreach (ClassRecord eachClass in classes)
               {
                    Range currentHeader = resultSheet.Cells.CreateRange(rowIndex, 3, false);
                    currentHeader.Copy(tempalteHeader);
                    rowIndex += 3;

                    currentHeader[1, 7].PutValue(eachClass.ClassName);

                    //計算每一個學生的學分統計。
                    foreach (StudentRecord eachStudent in eachClass.Students)
                    {
                         Range currentRow = resultSheet.Cells.CreateRange(rowIndex, 1, false);
                         currentRow.Copy(tempalteRow);

                         CreditCalcluator calcluator = new CreditCalcluator(eachStudent);

                         //填入結果。
                         currentRow[0, 0].PutValue(eachStudent.StudentName);
                         currentRow[0, 1].PutValue(eachStudent.SeatNo);
                         currentRow[0, 2].PutValue(calcluator.TotalCredit);
                         currentRow[0, 3].PutValue(calcluator.TotalPassedCredit);
                         currentRow[0, 4].PutValue(calcluator.PassedRequiredCredit);
                         currentRow[0, 5].PutValue(calcluator.PassedSelectCredit);
                         currentRow[0, 6].PutValue(calcluator.RequiredRestCredit);

                         rowIndex++;
                    }
                    resultSheet.HPageBreaks.Add(rowIndex, 0);
               }
               resultSheet.Name = "學分統計表";

               foreach (Worksheet sheet in new ArrayList(wb.Worksheets))
               {
                    if (sheet != resultSheet)
                         wb.Worksheets.RemoveAt(sheet.Index);
               }

               Completed(wb);
               //try
               //{
               //    string path = Path.Combine(Application.StartupPath, "Reports");
               //    if (!Directory.Exists(path))
               //        Directory.CreateDirectory(path);
               //    path = Path.Combine(path, "學分統計表" + ".xls");

               //    wb.Save(path);
               //    Process.Start(path);
               //}
               //catch (Exception ex)
               //{
               //    MsgBox.Show(ex.Message);
               //}
          }

          void Completed(Workbook wb)
          {
               SmartSchool.Customization.PlugIn.Global.SetStatusBarMessage("學分統計表產生完成");
               //Workbook wb = (Workbook)e.Result;

               string reportName = "學分統計表";
               string path = Path.Combine(Application.StartupPath, "Reports");
               if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
               path = Path.Combine(path, reportName + ".xls");

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
                    wb.Save(path);
                    System.Diagnostics.Process.Start(path);
               }
               catch
               {
                    SaveFileDialog sd = new SaveFileDialog();
                    sd.Title = "另存新檔";
                    sd.FileName = reportName + ".xls";
                    sd.Filter = "Excel檔案 (*.xls)|*.xls|所有檔案 (*.*)|*.*";
                    if (sd.ShowDialog() == DialogResult.OK)
                    {
                         try
                         {
                              wb.Save(sd.FileName, FileFormatType.Excel2003);
                         }
                         catch
                         {
                              MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                              return;
                         }
                    }
               }
          }
     }
}

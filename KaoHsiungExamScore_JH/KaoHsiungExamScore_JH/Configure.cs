using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KaoHsiungExamScore_JH
{
     [FISCA.UDT.TableName(Global._UDTTableName)]
    public class Configure : FISCA.UDT.ActiveRecord
    {
         public Configure()
         {
             PrintSubjectList = new List<string>();
         }

         /// <summary>
         /// 設定檔名稱
         /// </summary>
         [FISCA.UDT.Field]
         public string Name { get; set; }
         /// <summary>
         /// 學年度
         /// </summary>
         [FISCA.UDT.Field]
         public string SchoolYear { get; set; }
         /// <summary>
         /// 學期
         /// </summary>
         [FISCA.UDT.Field]
         public string Semester { get; set; }



         /// <summary>
         /// 列印樣板
         /// </summary>
         [FISCA.UDT.Field]
         private string TemplateStream { get; set; }
         public Aspose.Words.Document Template { get; set; }

         /// <summary>
         /// 樣板中支援列印科目的最大數
         /// </summary>
         [FISCA.UDT.Field]
         public int SubjectLimit { get; set; }
         /// <summary>
         /// 列印試別
         /// </summary>
         [FISCA.UDT.Field]
         private string ExamRecordID { get; set; }
         public K12.Data.ExamRecord ExamRecord { get; set; }

         /// <summary>
         /// 列印科別
         /// </summary>
         [FISCA.UDT.Field]
         private string PrintSubjectListString { get; set; }
         public List<string> PrintSubjectList { get;  set; }

         /// <summary>
         /// 列印缺曠類別
         /// </summary>
         [FISCA.UDT.Field]
         private string PrintAttendanceListString { get; set; }
         public List<string> PrintAttendanceList { get; set; }


         /// <summary>
         /// 開始日期
         /// </summary>
         [FISCA.UDT.Field]
         public string BeginDate { get; set; }

         /// <summary>
         /// 結束日期
         /// </summary>
         [FISCA.UDT.Field]
         public string EndDate { get; set; }

         /// <summary>
         /// 成績校正日期
         /// </summary>
         [FISCA.UDT.Field]
         public string ScoreEditDate { get; set; }

         /// <summary>
         /// 列印時選樣板設定檔
         /// </summary>
         [FISCA.UDT.Field]
         public string SelSetConfigName { get; set; }

         /// <summary>
         /// 不排名學生類別
         /// </summary>
         [FISCA.UDT.Field]
         public string NotRankedTagNameFilter { get; set; }


         /// <summary>
         /// 在儲存前，把資料填入儲存欄位中
         /// </summary>
         public void Encode()
         {
             this.ExamRecordID = (this.ExamRecord == null ? "" : this.ExamRecord.ID);
             // 科目
             this.PrintSubjectListString = "";
             if (this.PrintSubjectList == null)
                 this.PrintSubjectList = new List<string>();

             foreach (var item in this.PrintSubjectList)
             {
                 this.PrintSubjectListString += (this.PrintSubjectListString == "" ? "" : "^^^") + item;
             }

             // 缺曠類別
             this.PrintAttendanceListString = "";
             if (this.PrintAttendanceList == null)
                 this.PrintAttendanceList = new List<string>();

             foreach (string item in this.PrintAttendanceList)
             { 
                 this.PrintAttendanceListString+=(this.PrintAttendanceListString=="" ?"":"^^^")+item;
             }

             System.IO.MemoryStream stream = new System.IO.MemoryStream();
             this.Template.Save(stream, Aspose.Words.SaveFormat.Doc);
             this.TemplateStream = Convert.ToBase64String(stream.ToArray());
         }
         /// <summary>
         /// 在資料取出後，把資料從儲存欄位轉換至資料欄位
         /// </summary>
         public void Decode()
         {
             this.ExamRecord = K12.Data.Exam.SelectByID(this.ExamRecordID);
             // 科目
             this.PrintSubjectList = new List<string>(this.PrintSubjectListString.Split(new string[] { "^^^" }, StringSplitOptions.RemoveEmptyEntries));

             // 缺曠選項
             this.PrintAttendanceList = new List<string>(this.PrintAttendanceListString.Split(new string[] { "^^^" }, StringSplitOptions.RemoveEmptyEntries));

             this.Template = new Aspose.Words.Document(new MemoryStream(Convert.FromBase64String(this.TemplateStream)));
         }
    }
}

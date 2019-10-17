using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using FISCA.Data;
using System.Data;
using Aspose.Words;

namespace KaoHsiungExamScore_JH
{
    public class Global
    {

        #region 設定檔記錄用

        /// <summary>
        /// UDT TableName
        /// </summary>
        public const string _UDTTableName = "ischool.高雄國中評量成績通知單.configure";

        public static string _ProjectName = "國中高雄評量成績單";

        public static string _DefaultConfTypeName = "預設設定檔";

        public static string _UserConfTypeName = "使用者選擇設定檔";

        public static int _SelSchoolYear;
        public static int _SelSemester;
        public static string _SelExamID = "";

        public static List<string> _SelStudentIDList = new List<string>();

        // [ischoolkingdom] Vicky新增，個人評量成績單建議能增加可列印數量(高雄國中)，移除原本六張預設樣板，新增 領域成績單(預設)、科目成績單(預設)。
        /// <summary>
        /// 設定檔預設名稱
        /// </summary>
        /// <returns></returns>
        public static List<string> DefaultConfigNameList()
        {
            List<string> retVal = new List<string>();
            retVal.Add("領域成績單(預設)");
            retVal.Add("科目成績單(預設)");
            //retVal.Add("領域成績單");
            //retVal.Add("領域成績單(含平時成績)");
            //retVal.Add("科目成績單");
            //retVal.Add("科目成績單(含平時成績)");
            //retVal.Add("科目及領域成績單");
            //retVal.Add("科目及領域成績單(含平時成績)");
            return retVal;
        }

        #endregion

        /// <summary>
        /// 設定領域名稱
        /// </summary>
        public static void SetDomainList()
        {
            DomainNameList.Clear();

            // 從學生修課動態取得科目領域名稱
            if (_SelStudentIDList.Count > 0 && _SelSchoolYear > 0 && _SelSemester > 0 && _SelExamID != "")
            {
                QueryHelper qh = new QueryHelper();
                string strSQL = "SELECT DISTINCT " +
                    "domain " +
                    "FROM " +
                    "sc_attend " +
                    "INNER JOIN " +
                    "course " +
                    "ON sc_attend.ref_course_id=course.id " +
                    "INNER JOIN te_include " +
                    "ON course.ref_exam_template_id = te_include.ref_exam_template_id " +
                    "WHERE sc_attend.ref_student_id IN(" + string.Join(",", _SelStudentIDList.ToArray()) + ") " +
                    "AND course.school_year=" + _SelSchoolYear + " " +
                    "AND course.semester=" + _SelSemester + " " +
                    "AND te_include.ref_exam_id = " + _SelExamID + " AND domain <>'';";
                DataTable dt = qh.Select(strSQL);

                foreach(DataRow dr in dt.Rows)
                {
                    string domain = dr["domain"].ToString();
                    if (!DomainNameList.Contains(domain))
                        DomainNameList.Add(domain);
                }
            }
            else
            {
                // 預設
                DomainNameList.Add("國語文");
                DomainNameList.Add("英語");
                DomainNameList.Add("數學");
                DomainNameList.Add("社會");
                DomainNameList.Add("自然與生活科技");
                DomainNameList.Add("自然科學");
                DomainNameList.Add("藝術");
                DomainNameList.Add("健康與體育");
                DomainNameList.Add("藝術與人文");
                DomainNameList.Add("綜合活動");
                DomainNameList.Add("彈性課程");
                DomainNameList.Add("科技");
                DomainNameList.Add("特殊需求");
            }

                if (!DomainNameList.Contains("彈性課程"))
                DomainNameList.Add("彈性課程");
        }


        /// <summary>
        /// 固定領域名稱
        /// </summary>
        /// <returns></returns>
        public static List<string> DomainNameList = new List<string>();
      

        /// <summary>
        /// 取得獎懲名稱
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDisciplineNameList()
        {
            return new string[] { "大功", "小功", "嘉獎", "大過", "小過", "警告"}.ToList();
        }


        /// <summary>
        /// Data Table 內需要加入合併欄位
        /// </summary>
        /// <returns></returns>
        public static List<string> DTColumnsList()
        {
            List<string> retVal = new List<string>();
            // 固定欄位
            retVal.Add("StudentID");
            retVal.Add("學校名稱");
            retVal.Add("學年度");
            retVal.Add("學期");
            retVal.Add("試別名稱");
            retVal.Add("班級");
            retVal.Add("學號");
            retVal.Add("座號");
            retVal.Add("姓名");
            retVal.Add("監護人姓名");
            retVal.Add("父親姓名");
            retVal.Add("母親姓名");
            retVal.Add("戶籍地址");
            retVal.Add("聯絡地址");
            retVal.Add("其他地址");
            retVal.Add("領域成績加權平均");
            retVal.Add("科目評量分數加權平均");
            retVal.Add("科目平時成績加權平均");            
            // 獎懲名稱
            foreach (string str in GetDisciplineNameList())
                retVal.Add(str + "區間統計");

            retVal.Add("缺曠紀錄");
            retVal.Add("服務學習時數");
            retVal.Add("校長");
            retVal.Add("教務主任");
            retVal.Add("班導師");
            retVal.Add("區間開始日期");
            retVal.Add("區間結束日期");
            retVal.Add("成績校正日期");
            return retVal;
        }       

        /// <summary>
        /// 匯出合併欄位總表Word
        /// </summary>
        public static void ExportMappingFieldWord()
        {
            #region 儲存檔案
            string inputReportName = "高雄評量成績單合併欄位總表";
            string reportName = inputReportName;

            string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "Reports");
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

            Document tempDoc = new Document(new MemoryStream(Properties.Resources.高雄評量成績合併欄位總表_多樣版));
            try
            {

                #region 動態產生合併欄位
                // 讀取總表檔案並動態加入合併欄位

                DocumentBuilder builder = new DocumentBuilder(tempDoc);

                builder.StartTable();
                builder.Write("=== 高雄評量成績單合併欄位總表 ===");
                builder.CellFormat.Borders.LineStyle = LineStyle.None;

                builder.InsertCell();
                builder.Write("學校名稱");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 學校名稱" + " \\* MERGEFORMAT ", "«學校名稱" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("學年度");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 學年度" + " \\* MERGEFORMAT ", "«學年度" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("學期");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 學期" + " \\* MERGEFORMAT ", "«學期" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("試別名稱");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 試別名稱" + " \\* MERGEFORMAT ", "«試別名稱" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("班級");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 班級" + " \\* MERGEFORMAT ", "«班級" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("學號");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 學號" + " \\* MERGEFORMAT ", "«學號" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("座號");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 座號" + " \\* MERGEFORMAT ", "«座號" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("姓名");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 姓名" + " \\* MERGEFORMAT ", "«姓名" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("領域成績加權平均");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均" + " \\* MERGEFORMAT ", "«DA" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("科目評量分數加權平均");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目評量分數加權平均" + " \\* MERGEFORMAT ", "«SS" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("科目平時成績加權平均");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目平時成績加權平均" + " \\* MERGEFORMAT ", "«SA" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("缺曠紀錄");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 缺曠紀錄" + " \\* MERGEFORMAT ", "«缺曠紀錄" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("服務學習時數");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 服務學習時數" + " \\* MERGEFORMAT ", "«SL" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("校長");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 校長" + " \\* MERGEFORMAT ", "«校長" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("教務主任");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 教務主任" + " \\* MERGEFORMAT ", "«教務主任" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("班導師");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 班導師" + " \\* MERGEFORMAT ", "«班導師" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("成績校正日期");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 成績校正日期" + " \\* MERGEFORMAT ", "«成績校正日期" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("區間開始日期");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 區間開始日期" + " \\* MERGEFORMAT ", "«區間開始日期" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("區間結束日期");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 區間結束日期" + " \\* MERGEFORMAT ", "«區間結束日期" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("大功區間統計");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 大功區間統計" + " \\* MERGEFORMAT ", "«D1A" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("小功區間統計");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 小功區間統計" + " \\* MERGEFORMAT ", "«D1B" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("嘉獎區間統計");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 嘉獎區間統計" + " \\* MERGEFORMAT ", "«D1C" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("大過區間統計");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 大過區間統計" + " \\* MERGEFORMAT ", "«D2A" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("小過區間統計");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 小過區間統計" + " \\* MERGEFORMAT ", "«D2B" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("警告區間統計");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 警告區間統計" + " \\* MERGEFORMAT ", "«D2C" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("監護人姓名");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 監護人姓名" + " \\* MERGEFORMAT ", "«監護人姓名" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("父親姓名");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 父親姓名" + " \\* MERGEFORMAT ", "«父親姓名" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("母親姓名");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 母親姓名" + " \\* MERGEFORMAT ", "«母親姓名" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("戶籍地址");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 戶籍地址" + " \\* MERGEFORMAT ", "«戶籍地址" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("聯絡地址");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 聯絡地址" + " \\* MERGEFORMAT ", "«聯絡地址" + "»");
                builder.EndRow();
                builder.InsertCell();
                builder.Write("其他地址");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 其他地址" + " \\* MERGEFORMAT ", "«其他地址" + "»");
                builder.EndRow();
                builder.EndTable();

                builder.Writeln();
                builder.Writeln();
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;
                // 領域成績
                builder.Writeln("領域成績");
                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("加權平均");
                builder.InsertCell();
                builder.Write("平時加權平均");
                builder.InsertCell();
                builder.Write("權數");
                builder.InsertCell();
                builder.Write("努力程度");
                builder.EndRow();

                foreach(string domainName in DomainNameList)
                {
                    builder.InsertCell();
                    builder.Write(domainName);
                    string mName1 = domainName + "_領域加權平均";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD "+mName1 + " \\* MERGEFORMAT ", "«DS" + "»");
                    mName1 = domainName + "領域平時加權平均";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DA" + "»");
                    mName1 = domainName + "領域權數";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DC" + "»");
                    mName1 = domainName + "領域努力程度";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DE" + "»");
                    builder.EndRow();
                }
                builder.EndTable();
                builder.Writeln();
                builder.Writeln();

              
                // 領域科目成績
                foreach (string domainName in DomainNameList)
                {
                    builder.StartTable();
                    builder.CellFormat.Borders.LineStyle = LineStyle.None;

                    builder.Writeln(domainName + "領域");
                    builder.InsertCell();
                    builder.Write("科目名稱");
                    builder.InsertCell();
                    builder.Write("科目權數");
                    builder.InsertCell();
                    builder.Write("科目努力程度");
                    builder.InsertCell();
                    builder.Write("科目評量分數");
                    builder.InsertCell();
                    builder.Write("科目平時成績");
                    builder.InsertCell();
                    builder.Write("科目文字描述");
                    builder.EndRow();
                    // 科目數
                    for (int sj = 1; sj <=10; sj ++)
                    {
                        string sName1 = domainName + "_科目名稱"+ sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«SN" + "»");
                        sName1 = domainName + "_科目權數" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«SC" + "»");
                        sName1 = domainName + "_科目努力程度" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«SE" + "»");
                        sName1 = domainName + "_科目評量分數" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«SS" + "»");
                        sName1 = domainName + "_科目平時成績" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«SA" + "»");
                        sName1 = domainName + "_科目文字描述" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«ST" + "»");
                        builder.EndRow();
                    }
                    builder.EndTable();
                    builder.Writeln();
                    builder.Writeln();
                }   
                tempDoc.Save(path, SaveFormat.Doc);
                #endregion

                //System.IO.FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);

                //stream.Write(Properties.Resources.高雄評量成績合併欄位總表, 0, Properties.Resources.高雄評量成績合併欄位總表.Length);
                //stream.Flush();
                //stream.Close();
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                System.Windows.Forms.SaveFileDialog sd = new System.Windows.Forms.SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".doc";
                sd.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
                if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        tempDoc.Save(sd.FileName, SaveFormat.Doc);
                        //System.IO.FileStream stream = new FileStream(sd.FileName, FileMode.Create, FileAccess.Write);
                        //stream.Write(Properties.Resources.高雄評量成績合併欄位總表, 0, Properties.Resources.高雄評量成績合併欄位總表.Length);
                        //stream.Flush();
                        //stream.Close();

                    }
                    catch
                    {
                        FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            #endregion
        }
    }
}

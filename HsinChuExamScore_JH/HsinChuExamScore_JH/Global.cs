using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using FISCA.Data;
using System.Data;
using Aspose.Words;

namespace HsinChuExamScore_JH
{
    public class Global
    {
        #region 設定檔記錄用

        /// <summary>
        /// UDT TableName
        /// </summary>
        public const string _UDTTableName = "ischool.新竹國中評量成績通知單.configure";

        public static string _ProjectName = "國中新竹評量成績單";

        public static string _DefaultConfTypeName = "預設設定檔";

        public static string _UserConfTypeName = "使用者選擇設定檔";

        public static int _SelSchoolYear;
        public static int _SelSemester;
        public static string _SelExamID = "";

        public static List<string> _SelStudentIDList = new List<string>();

        /// <summary>
        /// 設定檔預設名稱
        /// </summary>
        /// <returns></returns>
        public static List<string> DefaultConfigNameList()
        {
            List<string> retVal = new List<string>();
            retVal.Add("領域成績單");
            retVal.Add("科目成績單");
            retVal.Add("科目及領域成績單_領域組距");
            retVal.Add("科目及領域成績單_科目組距");          
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

                foreach (DataRow dr in dt.Rows)
                {
                    string domain = dr["domain"].ToString();
                    if (!DomainNameList.Contains(domain))
                        DomainNameList.Add(domain);
                }
            }
            else
            {
                // 預設
                DomainNameList.Add("語文");
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
            retVal.Add("系統編號");
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
            retVal.Add("科目定期評量加權平均");
            retVal.Add("科目平時評量加權平均");
            retVal.Add("科目總成績加權平均");
            retVal.Add("領域成績加權平均(不含彈性)");
            retVal.Add("科目定期評量加權平均(不含彈性)");
            retVal.Add("科目平時評量加權平均(不含彈性)");
            retVal.Add("科目總成績加權平均(不含彈性)");
            retVal.Add("領域成績加權總分");
            retVal.Add("科目定期評量加權總分");
            retVal.Add("科目平時評量加權總分");
            retVal.Add("科目總成績加權總分");
            retVal.Add("領域成績加權總分(不含彈性)");
            retVal.Add("科目定期評量加權總分(不含彈性)");
            retVal.Add("科目平時評量加權總分(不含彈性)");
            retVal.Add("科目總成績加權總分(不含彈性)");
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
            string inputReportName = "新竹評量成績單合併欄位總表";
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

            Document tempDoc = new Document(new MemoryStream(Properties.Resources.新竹_評量成績合併欄位總表_動態版));

            try
            {
                #region 動態產生合併欄位
                // 讀取總表檔案並動態加入合併欄位

                DocumentBuilder builder = new DocumentBuilder(tempDoc);
                builder.Write("=== 新竹評量成績單合併欄位總表 ===");
                builder.StartTable();              
                builder.CellFormat.Borders.LineStyle = LineStyle.None;

                builder.InsertCell();
                builder.Write("學生電子報表識別");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 學生電子報表識別" + " \\* MERGEFORMAT ", "«學生電子報表識別" + "»");
                builder.EndRow();

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
                builder.Write("領域成績加權平均班排名名次");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均班排名名次" + " \\* MERGEFORMAT ", "«DACR" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均班排名PR值");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均班排名PR值" + " \\* MERGEFORMAT ", "«DACPR" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均班排名百分比");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均班排名百分比" + " \\* MERGEFORMAT ", "«DACP" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均班排名母體頂標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均班排名母體頂標" + " \\* MERGEFORMAT ", "«DAC25T" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均班排名母體前標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均班排名母體前標" + " \\* MERGEFORMAT ", "«DAC50T" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均班排名母體平均");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均班排名母體平均" + " \\* MERGEFORMAT ", "«DACA" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均班排名母體後標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均班排名母體後標" + " \\* MERGEFORMAT ", "«DAC50B " + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均班排名母體底標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均班排名母體底標" + " \\* MERGEFORMAT ", "«DAC25B" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均年排名名次");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均年排名名次" + " \\* MERGEFORMAT ", "«DAYR" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均年排名PR值");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均年排名PR值" + " \\* MERGEFORMAT ", "«DAYPR" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均年排名百分比");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均年排名百分比" + " \\* MERGEFORMAT ", "«DAYP" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均年排名母體頂標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均年排名母體頂標" + " \\* MERGEFORMAT ", "«DAY25T" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均年排名母體前標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均年排名母體前標" + " \\* MERGEFORMAT ", "«DAY50T" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均年排名母體平均");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均年排名母體平均" + " \\* MERGEFORMAT ", "«DAYA" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均年排名母體後標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均年排名母體後標" + " \\* MERGEFORMAT ", "«DAY50B" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均年排名母體底標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均年排名母體底標" + " \\* MERGEFORMAT ", "«DAY25B" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域類別1排名名次");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域類別1排名名次" + " \\* MERGEFORMAT ", "«D1TR " + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域類別1排名PR值");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域類別1排名PR值" + " \\* MERGEFORMAT ", "«D1TPR" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域類別1排名百分比");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域類別1排名百分比" + " \\* MERGEFORMAT ", "«D1TP" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均類別1排名母體頂標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均類別1排名母體頂標" + " \\* MERGEFORMAT ", "«DA1T25T" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均類別1排名母體前標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均類別1排名母體前標" + " \\* MERGEFORMAT ", "«DA1T50T" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均類別1排名母體平均");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均類別1排名母體平均" + " \\* MERGEFORMAT ", "«DA1TA" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均類別1排名母體後標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均類別1排名母體後標" + " \\* MERGEFORMAT ", "«DA1T50B " + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均類別1排名母體底標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均類別1排名母體底標" + " \\* MERGEFORMAT ", "«DA1T25B" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域類別2排名名次");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域類別2排名名次" + " \\* MERGEFORMAT ", "«D2TR " + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域類別2排名PR值");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域類別2排名PR值" + " \\* MERGEFORMAT ", "«D2TPR" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域類別2排名百分比");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域類別2排名百分比" + " \\* MERGEFORMAT ", "«D2TP" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均類別2排名母體頂標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均類別2排名母體頂標" + " \\* MERGEFORMAT ", "«DA2T25T" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均類別2排名母體前標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均類別2排名母體前標" + " \\* MERGEFORMAT ", "«DA2T50T" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均類別2排名母體平均");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均類別2排名母體平均" + " \\* MERGEFORMAT ", "«DA2TA" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均類別2排名母體後標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均類別2排名母體後標" + " \\* MERGEFORMAT ", "«DA2T50B" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均類別2排名母體底標");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均類別2排名母體底標" + " \\* MERGEFORMAT ", "«DA2T25B" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("科目定期評量加權平均");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目定期評量加權平均" + " \\* MERGEFORMAT ", "«SF" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("科目平時評量加權平均");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目平時評量加權平均" + " \\* MERGEFORMAT ", "«SA" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("科目總成績加權平均");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目總成績加權平均" + " \\* MERGEFORMAT ", "«ST" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權平均(不含彈性課程)");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權平均(不含彈性課程)" + " \\* MERGEFORMAT ", "«DAN" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("科目定期評量加權平均(不含彈性課程)");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目定期評量加權平均(不含彈性課程)" + " \\* MERGEFORMAT ", "«SFN" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("科目平時評量加權平均(不含彈性課程)");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目平時評量加權平均(不含彈性課程)" + " \\* MERGEFORMAT ", "«SAN" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("科目總成績加權平均(不含彈性課程)");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目總成績加權平均(不含彈性課程)" + " \\* MERGEFORMAT ", "«STN" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權總分");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權總分" + " \\* MERGEFORMAT ", "«DA" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("科目定期評量加權總分");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目定期評量加權總分" + " \\* MERGEFORMAT ", "«SF" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("科目平時評量加權總分");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目平時評量加權總分" + " \\* MERGEFORMAT ", "«SA" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("科目總成績加權總分");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目總成績加權總分" + " \\* MERGEFORMAT ", "«ST" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("領域成績加權總分(不含彈性課程)");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 領域成績加權總分(不含彈性課程)" + " \\* MERGEFORMAT ", "«DAN" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("科目定期評量加權總分(不含彈性課程)");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目定期評量加權總分(不含彈性課程)" + " \\* MERGEFORMAT ", "«SFN" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("科目平時評量加權總分(不含彈性課程)");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目平時評量加權總分(不含彈性課程)" + " \\* MERGEFORMAT ", "«SAN" + "»");
                builder.EndRow();

                builder.InsertCell();
                builder.Write("科目總成績加權總分(不含彈性課程)");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD 科目總成績加權總分(不含彈性課程)" + " \\* MERGEFORMAT ", "«STN" + "»");
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

                // 處理領域成績
                builder.Write("領域成績");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;
                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("領域加權平均");
                builder.InsertCell();
                builder.Write("領域定期加權平均");
                builder.InsertCell();
                builder.Write("領域平時加權平均");
                builder.InsertCell();
                builder.Write("領域權數");
                builder.InsertCell();
                builder.Write("領域班排名名次");
                builder.InsertCell();
                builder.Write("領域班排名PR值");
                builder.InsertCell();
                builder.Write("領域班排名百分比");
                builder.InsertCell();
                builder.Write("領域班排名母體平均");
                builder.InsertCell();
                builder.Write("領域年排名名次");
                builder.InsertCell();
                builder.Write("領域年排名PR值");
                builder.InsertCell();
                builder.Write("領域年排名百分比");
                builder.EndRow();

                foreach (string domainName in DomainNameList)
                {
                    builder.InsertCell();
                    builder.Write(domainName);
                    string mName1 = "";
                    mName1 = domainName + "_領域加權平均";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DS" + "»");
                    mName1 = domainName + "_領域定期加權平均";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DF" + "»");
                    mName1 = domainName + "_領域平時加權平均";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DA" + "»");
                    mName1 = domainName + "_領域權數";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DC" + "»");
                    mName1 = domainName + "_領域班排名名次";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DCR" + "»");
                    mName1 = domainName + "_領域班排名PR值";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DCPR" + "»");
                    mName1 = domainName + "_領域班排名百分比";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DCP" + "»");
                    mName1 = domainName + "_領域班排名母體平均";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DCA" + "»");
                    mName1 = domainName + "_領域年排名名次";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DYR" + "»");
                    mName1 = domainName + "_領域年排名PR值";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DYPR" + "»");
                    mName1 = domainName + "_領域年排名百分比";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DYP" + "»");

                    builder.EndRow();
                }
                builder.EndTable(); // 領域成績

                builder.Writeln();
                builder.Writeln();

                // 領域成績排名1
                builder.Write("領域成績排名");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;
                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("領域年排名名次");
                builder.InsertCell();
                builder.Write("領域年排名PR值");
                builder.InsertCell();
                builder.Write("領域年排名百分比");
                builder.InsertCell();
                builder.Write("領域班排名名次");
                builder.InsertCell();
                builder.Write("領域班排名PR值");
                builder.InsertCell();
                builder.Write("領域班排名百分比");
                builder.EndRow();

                foreach (string domainName in DomainNameList)
                {
                    builder.InsertCell();
                    builder.Write(domainName);
                    string mName1 = "";
                    mName1 = domainName + "_領域年排名名次";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DYR" + "»");
                    mName1 = domainName + "_領域年排名PR值";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DYPR" + "»");
                    mName1 = domainName + "_領域年排名百分比";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DYP" + "»");
                    mName1 = domainName + "_領域班排名名次";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DCR" + "»");
                    mName1 = domainName + "_領域班排名PR值";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DCPR" + "»");
                    mName1 = domainName + "_領域班排名百分比";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DCP" + "»");
                    builder.EndRow();
                }

                builder.EndTable(); // 領域成績排名1

                builder.Writeln();
                builder.Writeln();

                // 領域成績排名2
                builder.Write("領域成績排名");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;
                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("領域類別1排名名次");
                builder.InsertCell();
                builder.Write("領域類別1排名PR值");
                builder.InsertCell();
                builder.Write("領域類別1排名百分比");
                builder.InsertCell();
                builder.Write("領域類別2排名名次");
                builder.InsertCell();
                builder.Write("領域類別2排名PR值");
                builder.InsertCell();
                builder.Write("領域類別2排名百分比");
                builder.EndRow();

                foreach (string domainName in DomainNameList)
                {
                    builder.InsertCell();
                    builder.Write(domainName);
                    string mName1 = "";
                    mName1 = domainName + "_領域類別1排名名次";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D1TR" + "»");
                    mName1 = domainName + "_領域類別1排名PR值";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D1TPR" + "»");
                    mName1 = domainName + "_領域類別1排名百分比";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D1TP" + "»");
                    mName1 = domainName + "_領域類別2排名名次";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D2TR" + "»");
                    mName1 = domainName + "_領域類別2排名PR值";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D2TPR" + "»");
                    mName1 = domainName + "_領域類別2排名百分比";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D2TP" + "»");
                    builder.EndRow();

                }
                builder.EndTable(); // 領域成績排名2

                builder.Writeln();
                builder.Writeln();

                // 領域成績五標(年排名及班排名)
                builder.Write("領域成績五標(年排名及班排名)");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;
                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("領域年排名母體頂標");
                builder.InsertCell();
                builder.Write("領域年排名母體前標");
                builder.InsertCell();
                builder.Write("領域年排名母體平均");
                builder.InsertCell();
                builder.Write("領域年排名母體後標");
                builder.InsertCell();
                builder.Write("領域年排名母體底標");
                builder.InsertCell();
                builder.Write("領域班排名母體頂標");
                builder.InsertCell();
                builder.Write("領域班排名母體前標");
                builder.InsertCell();
                builder.Write("領域班排名母體平均");
                builder.InsertCell();
                builder.Write("領域班排名母體後標");
                builder.InsertCell();
                builder.Write("領域班排名母體底標");
                builder.EndRow();

                foreach (string domainName in DomainNameList)
                {
                    builder.InsertCell();
                    builder.Write(domainName);
                    string mName1 = "";
                    mName1 = domainName + "_領域年排名母體頂標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DY25T" + "»");
                    mName1 = domainName + "_領域年排名母體前標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DY50T" + "»");
                    mName1 = domainName + "_領域年排名母體平均";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DYA" + "»");
                    mName1 = domainName + "_領域年排名母體後標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DY50B" + "»");
                    mName1 = domainName + "_領域年排名母體底標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DY25B" + "»");
                    mName1 = domainName + "_領域班排名母體頂標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DC25T" + "»");
                    mName1 = domainName + "_領域班排名母體前標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DC50T" + "»");
                    mName1 = domainName + "_領域班排名母體平均";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DCA" + "»");
                    mName1 = domainName + "_領域班排名母體後標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DC50B" + "»");
                    mName1 = domainName + "_領域班排名母體底標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DC25B" + "»");
                    builder.EndRow();
                }

                builder.EndTable(); // 領域成績五標(年排名及班排名)

                builder.Writeln();
                builder.Writeln();

                // 領域成績五標(類別1排名及類別2排名)
                builder.Write("領域成績五標(類別1排名及類別2排名)");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;
                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("領域類別1排名母體頂標");
                builder.InsertCell();
                builder.Write("領域類別1排名母體前標");
                builder.InsertCell();
                builder.Write("領域類別1排名母體平均");
                builder.InsertCell();
                builder.Write("領域類別1排名母體後標");
                builder.InsertCell();
                builder.Write("領域類別1排名母體底標");
                builder.InsertCell();
                builder.Write("領域類別2排名母體頂標");
                builder.InsertCell();
                builder.Write("領域類別2排名母體前標");
                builder.InsertCell();
                builder.Write("領域類別2排名母體平均");
                builder.InsertCell();
                builder.Write("領域類別2排名母體後標");
                builder.InsertCell();
                builder.Write("領域類別2排名母體底標");
                builder.EndRow();

                foreach (string domainName in DomainNameList)
                {
                    builder.InsertCell();
                    builder.Write(domainName);
                    string mName1 = "";
                    mName1 = domainName + "_領域類別1排名母體頂標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D1T25T" + "»");
                    mName1 = domainName + "_領域類別1排名母體前標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D1T50T" + "»");
                    mName1 = domainName + "_領域類別1排名母體平均";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D1TA" + "»");
                    mName1 = domainName + "_領域類別1排名母體後標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D1T50B" + "»");
                    mName1 = domainName + "_領域類別1排名母體底標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D1T25B" + "»");
                    mName1 = domainName + "_領域類別2排名母體頂標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D2T25T" + "»");
                    mName1 = domainName + "_領域類別2排名母體前標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D2T50T" + "»");
                    mName1 = domainName + "_領域類別2排名母體平均";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D2TA" + "»");
                    mName1 = domainName + "_領域類別2排名母體後標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D2T50B" + "»");
                    mName1 = domainName + "_領域類別2排名母體底標";
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«D2T25B" + "»");
                    builder.EndRow();
                }

                builder.EndTable(); // 領域成績五標(類別1排名及類別2排名)

                builder.Writeln();
                builder.Writeln();

                builder.Writeln("序列化科目資料(年排名及班排名)");

                foreach (string domainName in DomainNameList)
                {
                    builder.Write(domainName+"領域");
                    string mName1 = "";

                    builder.StartTable();
                    builder.CellFormat.Borders.LineStyle = LineStyle.None;
                    builder.InsertCell();
                    builder.Write("科目名稱");
                    builder.InsertCell();
                    builder.Write("科目班排名名次");
                    builder.InsertCell();
                    builder.Write("科目班排名PR值");
                    builder.InsertCell();
                    builder.Write("科目班排名百分比");
                    builder.InsertCell();
                    builder.Write("科目年排名名次");
                    builder.InsertCell();
                    builder.Write("科目年排名PR值");
                    builder.InsertCell();
                    builder.Write("科目年排名百分比");
                    builder.EndRow();

                    for (int sj = 1 ;sj <= 12;sj++)
                    {
                        mName1 = domainName + "_科目名稱" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SN" + "»");
                        mName1 = domainName + "_科目班排名名次" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SCR" + "»");
                        mName1 = domainName + "_科目班排名PR值" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SCPR" + "»");
                        mName1 = domainName + "_科目班排名百分比" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SCP" + "»");
                        mName1 = domainName + "_科目年排名名次" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SYR" + "»");
                        mName1 = domainName + "_科目年排名PR值" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SYPR" + "»");
                        mName1 = domainName + "_科目年排名百分比" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SYP" + "»");
                        builder.EndRow();
                    }

                    builder.EndTable();
                    builder.Writeln();
                    builder.Writeln();
                }
                
                builder.Writeln("序列化科目資料(類別1排名及類別2排名)");
                foreach (string domainName in DomainNameList)
                {
                    builder.Write(domainName + "領域");
                    string mName1 = "";

                    builder.StartTable();
                    builder.CellFormat.Borders.LineStyle = LineStyle.None;
                    builder.InsertCell();
                    builder.Write("科目名稱");
                    builder.InsertCell();
                    builder.Write("科目類別1排名名次");
                    builder.InsertCell();
                    builder.Write("科目類別1排名PR值");
                    builder.InsertCell();
                    builder.Write("科目類別1排名百分比");
                    builder.InsertCell();
                    builder.Write("科目類別2排名名次");
                    builder.InsertCell();
                    builder.Write("科目類別2排名PR值");
                    builder.InsertCell();
                    builder.Write("科目類別2排名百分比");
                    builder.EndRow();

                    for (int sj = 1; sj <= 12; sj++)
                    {
                        mName1 = domainName + "_科目名稱" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SN" + "»");
                        mName1 = domainName + "_科目類別1排名名次" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S1TR" + "»");
                        mName1 = domainName + "_科目類別1排名PR值" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S1TPR" + "»");
                        mName1 = domainName + "_科目類別1排名百分比" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S1TP" + "»");
                        mName1 = domainName + "_科目類別2排名名次" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S2TR" + "»");
                        mName1 = domainName + "_科目類別2排名PR值" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S2TPR" + "»");
                        mName1 = domainName + "_科目類別2排名百分比" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S2TP" + "»");
                        builder.EndRow();
                    }

                    builder.EndTable();
                    builder.Writeln();
                    builder.Writeln();
                }

                builder.Writeln("序列化科目資料五標(年排名及班排名)");
                foreach (string domainName in DomainNameList)
                {
                    builder.Write(domainName + "領域");
                    string mName1 = "";

                    builder.StartTable();
                    builder.CellFormat.Borders.LineStyle = LineStyle.None;
                    builder.InsertCell();
                    builder.Write("科目名稱");
                    builder.InsertCell();
                    builder.Write("科目年排名母體頂標");
                    builder.InsertCell();
                    builder.Write("科目年排名母體前標");
                    builder.InsertCell();
                    builder.Write("科目年排名母體平均");
                    builder.InsertCell();
                    builder.Write("科目年排名母體後標");
                    builder.InsertCell();
                    builder.Write("科目年排名母體底標");
                    builder.InsertCell();
                    builder.Write("科目班排名母體頂標");
                    builder.InsertCell();
                    builder.Write("科目班排名母體前標");
                    builder.InsertCell();
                    builder.Write("科目班排名母體平均");
                    builder.InsertCell();
                    builder.Write("科目班排名母體後標");
                    builder.InsertCell();
                    builder.Write("科目班排名母體底標");
                    builder.EndRow();

                    for (int sj = 1; sj <= 12; sj++)
                    {
                        mName1 = domainName + "_科目名稱" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SN" + "»");
                        mName1 = domainName + "_科目年排名母體頂標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SY25T" + "»");
                        mName1 = domainName + "_科目年排名母體前標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SY50T" + "»");
                        mName1 = domainName + "_科目年排名母體平均" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SYA" + "»");
                        mName1 = domainName + "_科目年排名母體後標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SY50B" + "»");
                        mName1 = domainName + "_科目年排名母體底標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SY25B" + "»");
                        mName1 = domainName + "_科目班排名母體頂標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SC25T" + "»");
                        mName1 = domainName + "_科目班排名母體前標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SC50T" + "»");
                        mName1 = domainName + "_科目班排名母體平均" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SCA" + "»");
                        mName1 = domainName + "_科目班排名母體後標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SC50B" + "»");
                        mName1 = domainName + "_科目班排名母體底標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SC25B" + "»");
                        builder.EndRow();
                    }

                    builder.EndTable();
                    builder.Writeln();
                    builder.Writeln();
                }



                builder.Writeln("序列化科目資料五標(類別1排名及類別2排名)");
                foreach (string domainName in DomainNameList)
                {
                    builder.Write(domainName + "領域");
                    string mName1 = "";

                    builder.StartTable();
                    builder.CellFormat.Borders.LineStyle = LineStyle.None;
                    builder.InsertCell();
                    builder.Write("科目名稱");
                    builder.InsertCell();
                    builder.Write("科目類別1排名母體頂標");
                    builder.InsertCell();
                    builder.Write("科目類別1排名母體前標");
                    builder.InsertCell();
                    builder.Write("科目類別1排名母體平均");
                    builder.InsertCell();
                    builder.Write("科目類別1排名母體後標");
                    builder.InsertCell();
                    builder.Write("科目類別1排名母體底標");
                    builder.InsertCell();
                    builder.Write("科目類別2排名母體頂標");
                    builder.InsertCell();
                    builder.Write("科目類別2排名母體前標");
                    builder.InsertCell();
                    builder.Write("科目類別2排名母體平均");
                    builder.InsertCell();
                    builder.Write("科目類別2排名母體後標");
                    builder.InsertCell();
                    builder.Write("科目類別2排名母體底標");
                    builder.EndRow();

                    for (int sj = 1; sj <= 12; sj++)
                    {
                        mName1 = domainName + "_科目名稱" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SN" + "»");
                        mName1 = domainName + "_科目類別1排名母體頂標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S1T25T" + "»");
                        mName1 = domainName + "_科目類別1排名母體前標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S1T50T" + "»");
                        mName1 = domainName + "_科目類別1排名母體平均" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S1TA" + "»");
                        mName1 = domainName + "_科目類別1排名母體後標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S1T50B" + "»");
                        mName1 = domainName + "_科目類別1排名母體底標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S1T25B" + "»");
                        mName1 = domainName + "_科目類別2排名母體頂標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S2T25T" + "»");
                        mName1 = domainName + "_科目類別2排名母體前標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S2T50T" + "»");
                        mName1 = domainName + "_科目類別2排名母體平均" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S2TA" + "»");
                        mName1 = domainName + "_科目類別2排名母體後標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S2T50B" + "»");
                        mName1 = domainName + "_科目類別2排名母體底標" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«S2T25B" + "»");

                        builder.EndRow();
                    }

                    builder.EndTable();
                    builder.Writeln();
                    builder.Writeln();
                }

                List<string> tmpRNameList = new List<string>();
                tmpRNameList.Add("_R0_9");
                tmpRNameList.Add("_R10_19");
                tmpRNameList.Add("_R20_29");
                tmpRNameList.Add("_R30_39");
                tmpRNameList.Add("_R40_49");
                tmpRNameList.Add("_R50_59");
                tmpRNameList.Add("_R60_69");
                tmpRNameList.Add("_R70_79");
                tmpRNameList.Add("_R80_89");
                tmpRNameList.Add("_R90_99");
                tmpRNameList.Add("_R100_u");

                builder.Writeln("領域成績組距(總成績)");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;
                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("0-9");
                builder.InsertCell();
                builder.Write("10-19");
                builder.InsertCell();
                builder.Write("20-29");
                builder.InsertCell();
                builder.Write("30-39");
                builder.InsertCell();
                builder.Write("40-49");
                builder.InsertCell();
                builder.Write("50-59");
                builder.InsertCell();
                builder.Write("60-69");
                builder.InsertCell();
                builder.Write("70-79");
                builder.InsertCell();
                builder.Write("80-89");
                builder.InsertCell();
                builder.Write("90-99");
                builder.InsertCell();
                builder.Write("100以上");
                builder.EndRow();

                foreach (string domainName in DomainNameList)
                {
                    builder.InsertCell();
                    builder.Write("班級_"+ domainName);
                  
                    string mName1 = "";

                    foreach(string itName in tmpRNameList)
                    {
                        mName1 = "班級_"+domainName + itName;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DR" + "»");
                    }
                    builder.EndRow();
                }

                foreach (string domainName in DomainNameList)
                {
                    builder.InsertCell();
                    builder.Write("年級" + domainName);

                    string mName1 = "";

                    foreach (string itName in tmpRNameList)
                    {
                        mName1 = "年級_" + domainName + itName;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DR" + "»");
                    }
                    builder.EndRow();
                }

                builder.EndTable();
                builder.Writeln();
                builder.Writeln();


                builder.Writeln("領域成績組距(定期)");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;
                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("0-9");
                builder.InsertCell();
                builder.Write("10-19");
                builder.InsertCell();
                builder.Write("20-29");
                builder.InsertCell();
                builder.Write("30-39");
                builder.InsertCell();
                builder.Write("40-49");
                builder.InsertCell();
                builder.Write("50-59");
                builder.InsertCell();
                builder.Write("60-69");
                builder.InsertCell();
                builder.Write("70-79");
                builder.InsertCell();
                builder.Write("80-89");
                builder.InsertCell();
                builder.Write("90-99");
                builder.InsertCell();
                builder.Write("100以上");
                builder.EndRow();

                foreach (string domainName in DomainNameList)
                {
                    builder.InsertCell();
                    builder.Write("班級_" + domainName);

                    string mName1 = "";

                    foreach (string itName in tmpRNameList)
                    {
                        mName1 = "班級_" + domainName + itName+"F";
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DR" + "»");
                    }
                    builder.EndRow();
                }

                foreach (string domainName in DomainNameList)
                {
                    builder.InsertCell();
                    builder.Write("年級" + domainName);

                    string mName1 = "";

                    foreach (string itName in tmpRNameList)
                    {
                        mName1 = "年級_" + domainName + itName + "F";
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DR" + "»");
                    }
                    builder.EndRow();
                }

                builder.EndTable();
                builder.Writeln();
                builder.Writeln();

                builder.Writeln("領域成績組距(平時)");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;
                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("0-9");
                builder.InsertCell();
                builder.Write("10-19");
                builder.InsertCell();
                builder.Write("20-29");
                builder.InsertCell();
                builder.Write("30-39");
                builder.InsertCell();
                builder.Write("40-49");
                builder.InsertCell();
                builder.Write("50-59");
                builder.InsertCell();
                builder.Write("60-69");
                builder.InsertCell();
                builder.Write("70-79");
                builder.InsertCell();
                builder.Write("80-89");
                builder.InsertCell();
                builder.Write("90-99");
                builder.InsertCell();
                builder.Write("100以上");
                builder.EndRow();

                foreach (string domainName in DomainNameList)
                {
                    builder.InsertCell();
                    builder.Write("班級_" + domainName);

                    string mName1 = "";

                    foreach (string itName in tmpRNameList)
                    {
                        mName1 = "班級_" + domainName + itName + "A";
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DR" + "»");
                    }
                    builder.EndRow();
                }

                foreach (string domainName in DomainNameList)
                {
                    builder.InsertCell();
                    builder.Write("年級" + domainName);

                    string mName1 = "";

                    foreach (string itName in tmpRNameList)
                    {
                        mName1 = "年級_" + domainName + itName + "A";
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«DR" + "»");
                    }
                    builder.EndRow();
                }

                builder.EndTable();
                builder.Writeln();
                builder.Writeln();


                foreach (string domainName in DomainNameList)
                {

                    builder.Writeln(domainName + "領域");
                    builder.StartTable();
                    builder.CellFormat.Borders.LineStyle = LineStyle.None;
                    builder.InsertCell();
                    builder.Write("科目名稱");
                    builder.InsertCell();
                    builder.Write("科目權數");
                    builder.InsertCell();
                    builder.Write("科目定期評量");
                    builder.InsertCell();
                    builder.Write("科目平時評量");
                    builder.InsertCell();
                    builder.Write("科目總成績");
                    builder.InsertCell();
                    builder.Write("科目文字評量");
                    builder.InsertCell();
                    builder.Write("科目班排名名次");
                    builder.InsertCell();
                    builder.Write("科目班排名PR值");
                    builder.InsertCell();
                    builder.Write("科目班排名百分比");
                    builder.InsertCell();
                    builder.Write("科目班排名母體平均");
                    builder.InsertCell();
                    builder.Write("科目年排名名次");
                    builder.InsertCell();
                    builder.Write("科目年排名PR值");
                    builder.InsertCell();
                    builder.Write("科目年排名百分比");
                    builder.InsertCell();
                    builder.Write("科目年排名母體平均");
                    builder.EndRow();

                    for (int sj = 1; sj <= 12; sj++)
                    {
                        string mName1 = "";
                        mName1 = domainName + "_科目名稱" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SN" + "»");
                        mName1 = domainName + "_科目權數" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SC" + "»");
                        mName1 = domainName + "_科目定期評量" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SF" + "»");
                        mName1 = domainName + "_科目平時評量" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SA" + "»");
                        mName1 = domainName + "_科目總成績" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SST" + "»");
                        mName1 = domainName + "_科目文字評量" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«ST" + "»");
                        mName1 = domainName + "_科目班排名名次" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SCR" + "»");
                        mName1 = domainName + "_科目班排名PR值" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SCPR" + "»");
                        mName1 = domainName + "_科目班排名百分比" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SCP" + "»");
                        mName1 = domainName + "_科目班排名母體平均" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SCA" + "»");
                        mName1 = domainName + "_科目年排名名次" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SYR" + "»");
                        mName1 = domainName + "_科目年排名PR值" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SYPR" + "»");
                        mName1 = domainName + "_科目年排名百分比" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SYP" + "»");
                        mName1 = domainName + "_科目年排名母體平均" + sj;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + mName1 + " \\* MERGEFORMAT ", "«SYA" + "»");
                        builder.EndRow();
                    }

                    builder.EndTable();
                    builder.Writeln();
                    builder.Writeln();
                }

                builder.Writeln("科目班級組距(總成績)");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;

                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("0-9");
                builder.InsertCell();
                builder.Write("10-19");
                builder.InsertCell();
                builder.Write("20-29");
                builder.InsertCell();
                builder.Write("30-39");
                builder.InsertCell();
                builder.Write("40-49");
                builder.InsertCell();
                builder.Write("50-59");
                builder.InsertCell();
                builder.Write("60-69");
                builder.InsertCell();
                builder.Write("70-79");
                builder.InsertCell();
                builder.Write("80-89");
                builder.InsertCell();
                builder.Write("90-99");
                builder.InsertCell();
                builder.Write("100以上");
                builder.EndRow();


                for (int ss = 1; ss <= 30; ss ++)
                {
                    string sName1 = "";
                    sName1 = "s班級_科目名稱" + ss ;
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«N" + "»");

                    foreach (string itName in tmpRNameList)
                    {
                        
                        sName1 = "s班級_科目" + ss + itName;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«SR" + "»");
                    }
                    builder.EndRow();
                }
                builder.EndTable();
                builder.Writeln();
                builder.Writeln();

                builder.Writeln("科目班級組距(定期)");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;

                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("0-9");
                builder.InsertCell();
                builder.Write("10-19");
                builder.InsertCell();
                builder.Write("20-29");
                builder.InsertCell();
                builder.Write("30-39");
                builder.InsertCell();
                builder.Write("40-49");
                builder.InsertCell();
                builder.Write("50-59");
                builder.InsertCell();
                builder.Write("60-69");
                builder.InsertCell();
                builder.Write("70-79");
                builder.InsertCell();
                builder.Write("80-89");
                builder.InsertCell();
                builder.Write("90-99");
                builder.InsertCell();
                builder.Write("100以上");
                builder.EndRow();


                for (int ss = 1; ss <= 30; ss++)
                {
                    string sName1 = "";
                    sName1 = "sf班級_科目名稱" + ss;
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«N" + "»");

                    foreach (string itName in tmpRNameList)
                    {

                        sName1 = "sf班級_科目" + ss + itName;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«SR" + "»");
                    }
                    builder.EndRow();
                }
                builder.EndTable();
                builder.Writeln();
                builder.Writeln();


                builder.Writeln("科目班級組距(平時)");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;

                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("0-9");
                builder.InsertCell();
                builder.Write("10-19");
                builder.InsertCell();
                builder.Write("20-29");
                builder.InsertCell();
                builder.Write("30-39");
                builder.InsertCell();
                builder.Write("40-49");
                builder.InsertCell();
                builder.Write("50-59");
                builder.InsertCell();
                builder.Write("60-69");
                builder.InsertCell();
                builder.Write("70-79");
                builder.InsertCell();
                builder.Write("80-89");
                builder.InsertCell();
                builder.Write("90-99");
                builder.InsertCell();
                builder.Write("100以上");
                builder.EndRow();


                for (int ss = 1; ss <= 30; ss++)
                {
                    string sName1 = "";
                    sName1 = "sa班級_科目名稱" + ss;
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«N" + "»");

                    foreach (string itName in tmpRNameList)
                    {

                        sName1 = "sa班級_科目" + ss + itName;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«SR" + "»");
                    }
                    builder.EndRow();
                }
                builder.EndTable();
                builder.Writeln();
                builder.Writeln();


                builder.Writeln("科目年級組距(總成績)");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;

                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("0-9");
                builder.InsertCell();
                builder.Write("10-19");
                builder.InsertCell();
                builder.Write("20-29");
                builder.InsertCell();
                builder.Write("30-39");
                builder.InsertCell();
                builder.Write("40-49");
                builder.InsertCell();
                builder.Write("50-59");
                builder.InsertCell();
                builder.Write("60-69");
                builder.InsertCell();
                builder.Write("70-79");
                builder.InsertCell();
                builder.Write("80-89");
                builder.InsertCell();
                builder.Write("90-99");
                builder.InsertCell();
                builder.Write("100以上");
                builder.EndRow();


                for (int ss = 1; ss <= 30; ss++)
                {
                    string sName1 = "";
                    sName1 = "s年級_科目名稱" + ss;
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«N" + "»");

                    foreach (string itName in tmpRNameList)
                    {

                        sName1 = "s年級_科目" + ss + itName;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«SR" + "»");
                    }
                    builder.EndRow();
                }
                builder.EndTable();
                builder.Writeln();
                builder.Writeln();

                builder.Writeln("科目年級組距(定期)");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;

                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("0-9");
                builder.InsertCell();
                builder.Write("10-19");
                builder.InsertCell();
                builder.Write("20-29");
                builder.InsertCell();
                builder.Write("30-39");
                builder.InsertCell();
                builder.Write("40-49");
                builder.InsertCell();
                builder.Write("50-59");
                builder.InsertCell();
                builder.Write("60-69");
                builder.InsertCell();
                builder.Write("70-79");
                builder.InsertCell();
                builder.Write("80-89");
                builder.InsertCell();
                builder.Write("90-99");
                builder.InsertCell();
                builder.Write("100以上");
                builder.EndRow();


                for (int ss = 1; ss <= 30; ss++)
                {
                    string sName1 = "";
                    sName1 = "sf年級_科目名稱" + ss;
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«N" + "»");

                    foreach (string itName in tmpRNameList)
                    {

                        sName1 = "sf年級_科目" + ss + itName;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«SR" + "»");
                    }
                    builder.EndRow();
                }
                builder.EndTable();
                builder.Writeln();
                builder.Writeln();


                builder.Writeln("科目年級組距(平時)");
                builder.StartTable();
                builder.CellFormat.Borders.LineStyle = LineStyle.None;

                builder.InsertCell();
                builder.Write("領域名稱");
                builder.InsertCell();
                builder.Write("0-9");
                builder.InsertCell();
                builder.Write("10-19");
                builder.InsertCell();
                builder.Write("20-29");
                builder.InsertCell();
                builder.Write("30-39");
                builder.InsertCell();
                builder.Write("40-49");
                builder.InsertCell();
                builder.Write("50-59");
                builder.InsertCell();
                builder.Write("60-69");
                builder.InsertCell();
                builder.Write("70-79");
                builder.InsertCell();
                builder.Write("80-89");
                builder.InsertCell();
                builder.Write("90-99");
                builder.InsertCell();
                builder.Write("100以上");
                builder.EndRow();


                for (int ss = 1; ss <= 30; ss++)
                {
                    string sName1 = "";
                    sName1 = "sa年級_科目名稱" + ss;
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«N" + "»");

                    foreach (string itName in tmpRNameList)
                    {

                        sName1 = "sa年級_科目" + ss + itName;
                        builder.InsertCell();
                        builder.InsertField("MERGEFIELD " + sName1 + " \\* MERGEFORMAT ", "«SR" + "»");
                    }
                    builder.EndRow();
                }
                builder.EndTable();
                builder.Writeln();
                builder.Writeln();

                #endregion
                tempDoc.Save(path, SaveFormat.Doc);
             


                //System.IO.FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);

                //stream.Write(Properties.Resources.新竹評量成績合併欄位總表, 0, Properties.Resources.新竹評量成績合併欄位總表.Length);
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
                        //stream.Write(Properties.Resources.新竹評量成績合併欄位總表, 0, Properties.Resources.新竹評量成績合併欄位總表.Length);
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

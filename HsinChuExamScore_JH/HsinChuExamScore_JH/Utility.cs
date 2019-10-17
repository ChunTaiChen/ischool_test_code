using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FISCA.Data;
using K12.Data;
using System.IO;

namespace HsinChuExamScore_JH
{
    public class Utility
    {

        /// <summary>
        /// 透過學生編號、開始與結束日期，取得學習服務統計值
        /// </summary>
        /// <param name="StudentIDList"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static Dictionary<string, decimal> GetServiceLearningDetailByDate(List<string> StudentIDList, DateTime beginDate, DateTime endDate)
        {
            Dictionary<string, decimal> retVal = new Dictionary<string, decimal>();
            if (StudentIDList.Count > 0)
            {
                QueryHelper qh = new QueryHelper();
                string query = "select ref_student_id,occur_date,reason,hours from $k12.service.learning.record where ref_student_id in('" + string.Join("','", StudentIDList.ToArray()) + "') and occur_date >='" + beginDate.ToShortDateString() + "' and occur_date <='" + endDate.ToShortDateString() + "'order by ref_student_id,occur_date;";
                DataTable dt = qh.Select(query);
                foreach (DataRow dr in dt.Rows)
                {
                    decimal hr;

                    string sid = dr[0].ToString();
                    if (!retVal.ContainsKey(sid))
                        retVal.Add(sid, 0);

                    if (decimal.TryParse(dr["hours"].ToString(), out hr))
                        retVal[sid] += hr;
                }
            }
            return retVal;
        }


        /// <summary>
        /// 透過日期區間取得獎懲資料,傳入學生ID,開始日期,結束日期,回傳：學生ID,獎懲統計名稱,統計值
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, int>> GetDisciplineCountByDate(List<string> StudentIDList, DateTime beginDate, DateTime endDate)
        {
            Dictionary<string, Dictionary<string, int>> retVal = new Dictionary<string, Dictionary<string, int>>();

            List<string> nameList = new string[] { "大功", "小功", "嘉獎", "大過", "小過", "警告", "留校" }.ToList();

            // 取得獎懲資料
            List<DisciplineRecord> dataList = Discipline.SelectByStudentIDs(StudentIDList);

            foreach (DisciplineRecord data in dataList)
            {
                if (data.OccurDate >= beginDate && data.OccurDate <= endDate)
                {
                    // 初始化
                    if (!retVal.ContainsKey(data.RefStudentID))
                    {
                        retVal.Add(data.RefStudentID, new Dictionary<string, int>());
                        foreach (string str in nameList)
                            retVal[data.RefStudentID].Add(str, 0);
                    }

                    // 獎勵
                    if (data.MeritFlag == "1")
                    {
                        if (data.MeritA.HasValue)
                            retVal[data.RefStudentID]["大功"] += data.MeritA.Value;

                        if (data.MeritB.HasValue)
                            retVal[data.RefStudentID]["小功"] += data.MeritB.Value;

                        if (data.MeritC.HasValue)
                            retVal[data.RefStudentID]["嘉獎"] += data.MeritC.Value;
                    }
                    else if (data.MeritFlag == "0")
                    { // 懲戒
                        if (data.Cleared != "是")
                        {
                            if (data.DemeritA.HasValue)
                                retVal[data.RefStudentID]["大過"] += data.DemeritA.Value;

                            if (data.DemeritB.HasValue)
                                retVal[data.RefStudentID]["小過"] += data.DemeritB.Value;

                            if (data.DemeritC.HasValue)
                                retVal[data.RefStudentID]["警告"] += data.DemeritC.Value;
                        }
                    }
                    else if (data.MeritFlag == "2")
                    {
                        // 留校察看
                        retVal[data.RefStudentID]["留校"]++;
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// 透過日期區間取得學生缺曠統計(傳入學生系統編號、開始日期、結束日期；回傳：學生系統編號、獎懲名稱,統計值
        /// </summary>
        /// <param name="StudIDList"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, int>> GetAttendanceCountByDate(List<StudentRecord> StudRecordList, DateTime beginDate, DateTime endDate)
        {
            Dictionary<string, Dictionary<string, int>> retVal = new Dictionary<string, Dictionary<string, int>>();

            List<PeriodMappingInfo> PeriodMappingList = PeriodMapping.SelectAll();
            // 節次>類別
            Dictionary<string, string> PeriodMappingDict = new Dictionary<string, string>();
            foreach (PeriodMappingInfo rec in PeriodMappingList)
            {
                if (!PeriodMappingDict.ContainsKey(rec.Name))
                    PeriodMappingDict.Add(rec.Name, rec.Type);
            }

            List<AttendanceRecord> attendList = K12.Data.Attendance.SelectByDate(StudRecordList, beginDate, endDate);

            // 計算統計資料
            foreach (AttendanceRecord rec in attendList)
            {
                if (!retVal.ContainsKey(rec.RefStudentID))
                    retVal.Add(rec.RefStudentID, new Dictionary<string, int>());

                foreach (AttendancePeriod per in rec.PeriodDetail)
                {
                    if (!PeriodMappingDict.ContainsKey(per.Period))
                        continue;

                    // ex.一般:曠課
                    //string key = "區間" + PeriodMappingDict[per.Period] + "_" + per.AbsenceType;

                    string key = PeriodMappingDict[per.Period] + per.AbsenceType;
                    if (!retVal[rec.RefStudentID].ContainsKey(key))
                        retVal[rec.RefStudentID].Add(key, 0);

                    retVal[rec.RefStudentID][key]++;
                }
            }

            return retVal;
        }

        /// <summary>
        /// 透過 ClassID 取得學生為一般，依照年級、班級名稱、座號排序
        /// </summary>
        /// <param name="ClassIDList"></param>
        /// <returns></returns>
        public static List<string> GetClassStudentIDList1ByClassID(List<string> ClassIDList)
        {
            List<string> retVal = new List<string>();
            QueryHelper qh = new QueryHelper();
            string query = "select student.id from student inner join class on student.ref_class_id = class.id where student.status=1 and class.id in(" + string.Join(",", ClassIDList.ToArray()) + ") order by class.grade_year,class.class_name,student.seat_no";
            DataTable dt = new DataTable();
            dt = qh.Select(query);

            foreach (DataRow dr in dt.Rows)
                retVal.Add(dr[0].ToString());

            dt.Clear();
            return retVal;
        }


        /// <summary>
        /// 取得系統內學生類別,群組用[]表示,沒有群組直接名稱
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, List<string>> GetStudentTagRefDict()
        {
            // 學生類別,StudentID
            Dictionary<string, List<string>> retVal = new Dictionary<string, List<string>>();
            QueryHelper qh = new QueryHelper();
            string query = "select tag.prefix,tag.name,ref_student_id from tag left join tag_student on tag.id = tag_student.ref_tag_id order by tag.prefix,tag.name";
            DataTable dt = new DataTable();
            dt = qh.Select(query);

            foreach (DataRow dr in dt.Rows)
            {
                string strP = "", key = "", StudID = "";

                if (dr["prefix"] != null)
                    strP = dr["prefix"].ToString();

                if (string.IsNullOrEmpty(strP))
                    key = dr["name"].ToString();
                else
                    key = "[" + strP + "]";

                if (dr["ref_student_id"] != null)
                    StudID = dr["ref_student_id"].ToString();

                if (!retVal.ContainsKey(key))
                    retVal.Add(key, new List<string>());

                if (!string.IsNullOrEmpty(StudID))
                    retVal[key].Add(StudID);

            }
            return retVal;
        }

        public static Dictionary<string, Dictionary<string, DAO.SubjectDomainName>> GetStudentSCAttendCourse(List<string> StudentIDList, List<string> CourseIDList, string examID)
        {
            Dictionary<string, Dictionary<string, DAO.SubjectDomainName>> retVal = new Dictionary<string, Dictionary<string, DAO.SubjectDomainName>>();
            QueryHelper qh = new QueryHelper();
            string query = "select ref_student_id,course.domain,course.subject,course.credit from sc_attend inner join course on sc_attend.ref_course_id=course.id inner join te_include on course.ref_exam_template_id = te_include.ref_exam_template_id where sc_attend.ref_student_id in(" + string.Join(",", StudentIDList.ToArray()) + ") and course.id in(" + string.Join(",", CourseIDList.ToArray()) + ") and te_include.ref_exam_id=" + examID;
            DataTable dt = qh.Select(query);
            foreach (DataRow dr in dt.Rows)
            {
                string id = dr[0].ToString();

                if (!retVal.ContainsKey(id))
                    retVal.Add(id, new Dictionary<string, DAO.SubjectDomainName>());

                string domainName = dr["domain"].ToString();
                string subjectName = dr["subject"].ToString();

                if (string.IsNullOrEmpty(domainName))
                    domainName = "彈性課程";



                if (!retVal[id].ContainsKey(subjectName))
                {
                    DAO.SubjectDomainName sdn = new DAO.SubjectDomainName();
                    sdn.SubjectName = subjectName;
                    sdn.DomainName = domainName;
                    decimal credit;
                    if (decimal.TryParse(dr["credit"].ToString(), out credit))
                    {
                        sdn.Credit = credit;
                    }

                    retVal[id].Add(subjectName, sdn);
                }
            }

            return retVal;
        }

        /// <summary>
        /// 透過學生系統編號、學年度、學期，取得考試的科目名稱
        /// </summary>
        /// <param name="StudentIDList"></param>
        /// <param name="SchoolYear"></param>
        /// <param name="Semester"></param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> GetExamSubjecList(List<string> StudentIDList, int SchoolYear, int Semester)
        {
            Dictionary<string, List<string>> retVal = new Dictionary<string, List<string>>();
            if (StudentIDList.Count > 0)
            {
                QueryHelper qh = new QueryHelper();
                string query = "select distinct ref_exam_id,course.subject from sc_attend inner join course on sc_attend.ref_course_id=course.id inner join te_include on course.ref_exam_template_id = te_include.ref_exam_template_id where sc_attend.ref_student_id in(" + string.Join(",", StudentIDList.ToArray()) + ") and course.school_year=" + SchoolYear + " and  course.semester=" + Semester + " and course.subject is not null order by ref_exam_id,subject";
                DataTable dt = qh.Select(query);
                foreach (DataRow dr in dt.Rows)
                {
                    string id = dr[0].ToString();

                    if (!retVal.ContainsKey(id))
                        retVal.Add(id, new List<string>());

                    string subjectName = dr["subject"].ToString();

                    if (!retVal[id].Contains(subjectName))
                        retVal[id].Add(subjectName);
                }
            }
            return retVal;
        }

        /// <summary>
        /// 取得評量比例設定
        /// </summary>
        public static Dictionary<string, decimal> GetScorePercentageHS()
        {
            Dictionary<string, decimal> returnData = new Dictionary<string, decimal>();
            FISCA.Data.QueryHelper qh1 = new FISCA.Data.QueryHelper();
            string query1 = @"select id,CAST(regexp_replace( xpath_string(exam_template.extension,'/Extension/ScorePercentage'), '^$', '0') as integer) as ScorePercentage  from exam_template";
            System.Data.DataTable dt1 = qh1.Select(query1);

            foreach (System.Data.DataRow dr in dt1.Rows)
            {
                string id = dr["id"].ToString();
                decimal sp = 50;
                if (decimal.TryParse(dr["ScorePercentage"].ToString(), out sp))
                    returnData.Add(id, sp);
                else
                    returnData.Add(id, 50);

            }
            return returnData;
        }

        // 建立word 大量功能變數
        public static void CreateFieldTemplate()
        {
            Aspose.Words.Document doc = new Aspose.Words.Document();
            Aspose.Words.DocumentBuilder builder = new Aspose.Words.DocumentBuilder(doc);
            
            builder.Write("變數");
            builder.Writeln();

            //領域(校排名及班排名)
            builder.Write("領域成績排名");
            builder.StartTable();
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

            foreach (string key in new string[]{
                    "語文",
                    "數學",
                    "社會",
                    "自然與生活科技",
                    "健康與體育",
                    "藝術與人文",
                    "綜合活動",
                    "彈性課程"
                })
            {

                builder.InsertCell();
                builder.Write(key);
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域年排名名次" + " \\* MERGEFORMAT ", "«DYR»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域年排名PR值" + " \\* MERGEFORMAT ", "«DYPR»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域年排名百分比" + " \\* MERGEFORMAT ", "«DYP»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域班排名名次" + " \\* MERGEFORMAT ", "«DCR»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域班排名PR值" + " \\* MERGEFORMAT ", "«DCPR»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域班排名百分比" + " \\* MERGEFORMAT ", "«DCP»");
                builder.EndRow();

            }

            builder.EndTable();

            builder.Writeln();

            //領域(類別一排名及類別二排名)
            builder.Write("領域成績排名");
            builder.StartTable();
            builder.InsertCell();
            builder.Write("領域名稱");
            builder.InsertCell();
            builder.Write("領域類一排名名次");
            builder.InsertCell();
            builder.Write("領域類一排名PR值");
            builder.InsertCell();
            builder.Write("領域類一排名百分比");
            builder.InsertCell();
            builder.Write("領域類二排名名次");
            builder.InsertCell();
            builder.Write("領域類二排名PR值");
            builder.InsertCell();
            builder.Write("領域類二排名百分比");
            builder.EndRow();

            foreach (string key in new string[]{
                    "語文",
                    "數學",
                    "社會",
                    "自然與生活科技",
                    "健康與體育",
                    "藝術與人文",
                    "綜合活動",
                    "彈性課程"
                })
            {

                builder.InsertCell();
                builder.Write(key);
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類一排名名次" + " \\* MERGEFORMAT ", "«D1TR»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類一排名PR值" + " \\* MERGEFORMAT ", "«D1TPR»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類一排名百分比" + " \\* MERGEFORMAT ", "«D1TP»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類二排名名次" + " \\* MERGEFORMAT ", "«D2TR»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類二排名PR值" + " \\* MERGEFORMAT ", "«D2TPR»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類二排名百分比" + " \\* MERGEFORMAT ", "«D2TP»");
                builder.EndRow();

            }

            builder.EndTable();

            builder.Writeln();

            //領域成績五標(年排名及班排名)
            builder.Write("領域成績五標(年排名及班排名)");
            builder.StartTable();
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

            foreach (string key in new string[]{
                    "語文",
                    "數學",
                    "社會",
                    "自然與生活科技",
                    "健康與體育",
                    "藝術與人文",
                    "綜合活動",
                    "彈性課程"
                })
            {
                builder.InsertCell();
                builder.Write(key);
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域年排名母體頂標" + " \\* MERGEFORMAT ", "«DY25T»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域年排名母體前標" + " \\* MERGEFORMAT ", "«DY50T»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域年排名母體平均" + " \\* MERGEFORMAT ", "«DYA»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域年排名母體後標" + " \\* MERGEFORMAT ", "«DY50B»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域年排名母體底標" + " \\* MERGEFORMAT ", "«DY25B»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域班排名母體頂標" + " \\* MERGEFORMAT ", "«DC25T»");
                builder.InsertCell();                           
                builder.InsertField("MERGEFIELD " + key + "_領域班排名母體前標" + " \\* MERGEFORMAT ", "«DC50T»");
                builder.InsertCell();                           
                builder.InsertField("MERGEFIELD " + key + "_領域班排名母體平均" + " \\* MERGEFORMAT ", "«DCA»");
                builder.InsertCell();                           
                builder.InsertField("MERGEFIELD " + key + "_領域班排名母體後標" + " \\* MERGEFORMAT ", "«DC50B»");
                builder.InsertCell();                           
                builder.InsertField("MERGEFIELD " + key + "_領域班排名母體底標" + " \\* MERGEFORMAT ", "«DC25B»");
                builder.EndRow();

            }

            builder.EndTable();

            builder.Writeln();

            //領域成績五標(類一排名及類二排名)
            builder.Write("領域成績五標(類一排名及類二排名)");
            builder.StartTable();
            builder.InsertCell();
            builder.Write("領域名稱");
            builder.InsertCell();
            builder.Write("領域類一排名母體頂標");
            builder.InsertCell();
            builder.Write("領域類一排名母體前標");
            builder.InsertCell();
            builder.Write("領域類一排名母體平均");
            builder.InsertCell();
            builder.Write("領域類一排名母體後標");
            builder.InsertCell();
            builder.Write("領域類一排名母體底標");
            builder.InsertCell();
            builder.Write("領域類二排名母體頂標");
            builder.InsertCell();
            builder.Write("領域類二排名母體前標");
            builder.InsertCell();
            builder.Write("領域類二排名母體平均");
            builder.InsertCell();
            builder.Write("領域類二排名母體後標");
            builder.InsertCell();
            builder.Write("領域類二排名母體底標");
            builder.EndRow();

            foreach (string key in new string[]{
                    "語文",
                    "數學",
                    "社會",
                    "自然與生活科技",
                    "健康與體育",
                    "藝術與人文",
                    "綜合活動",
                    "彈性課程"
                })
            {
                builder.InsertCell();
                builder.Write(key);
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類一排名母體頂標" + " \\* MERGEFORMAT ", "«D1T25T»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類一排名母體前標" + " \\* MERGEFORMAT ", "«D1T50T»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類一排名母體平均" + " \\* MERGEFORMAT ", "«D1TA»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類一排名母體後標" + " \\* MERGEFORMAT ", "«D1T50B»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類一排名母體底標" + " \\* MERGEFORMAT ", "«D1T25B»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類二排名母體頂標" + " \\* MERGEFORMAT ", "«D2T25T»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類二排名母體前標" + " \\* MERGEFORMAT ", "«D2T50T»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類二排名母體平均" + " \\* MERGEFORMAT ", "«D2TA»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類二排名母體後標" + " \\* MERGEFORMAT ", "«D2T50B»");
                builder.InsertCell();
                builder.InsertField("MERGEFIELD " + key + "_領域類二排名母體底標" + " \\* MERGEFORMAT ", "«D2T25B»");
                builder.EndRow();

            }

            builder.EndTable();

            builder.Writeln();

            //序列化科目資料(年排名及班排名)
            builder.Write("序列化科目資料(年排名及班排名)");
            foreach (string key in new string[]{
                    "語文",
                    "數學",
                    "社會",
                    "自然與生活科技",
                    "健康與體育",
                    "藝術與人文",
                    "綜合活動",
                    "彈性課程"
                })
            {
                builder.Write("領域:" +key);

                builder.StartTable();
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

                for (int i = 1; i <= 7; i++)
                {
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目名稱" + i + " \\* MERGEFORMAT ", "«SN»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目班排名名次" + i + " \\* MERGEFORMAT ", "«SCR»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目班排名PR值" + i + " \\* MERGEFORMAT ", "«SCPR»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目班排名百分比" + i + " \\* MERGEFORMAT ", "«SCP»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目年排名名次" + i + " \\* MERGEFORMAT ", "«SYR»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目年排名PR值" + i + " \\* MERGEFORMAT ", "«SYPR»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目年排名百分比" + i + " \\* MERGEFORMAT ", "«SYP»");
                    builder.EndRow();
                }
                builder.EndTable();

                builder.Writeln();

            }

            //序列化科目資料(類一排名及類二排名)
            builder.Write("序列化科目資料(類一排名及類二排名)");
            foreach (string key in new string[]{
                    "語文",
                    "數學",
                    "社會",
                    "自然與生活科技",
                    "健康與體育",
                    "藝術與人文",
                    "綜合活動",
                    "彈性課程"
                })
            {
                builder.Write("領域:" + key);

                builder.StartTable();
                builder.InsertCell();
                builder.Write("科目類一排名名次");
                builder.InsertCell();
                builder.Write("科目類一排名PR值");
                builder.InsertCell();
                builder.Write("科目類一排名百分比");
                builder.InsertCell();
                builder.Write("科目名稱");
                builder.InsertCell();
                builder.Write("科目類二排名名次");
                builder.InsertCell();
                builder.Write("科目類二排名PR值");
                builder.InsertCell();
                builder.Write("科目類二排名百分比");
                builder.EndRow();

                for (int i = 1; i <= 7; i++)
                {
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目名稱" + i + " \\* MERGEFORMAT ", "«SN»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類一排名名次" + i + " \\* MERGEFORMAT ", "«S1TR»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類一排名PR值" + i + " \\* MERGEFORMAT ", "«S1TPR»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類一排名百分比" + i + " \\* MERGEFORMAT ", "«S1TP»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類二排名名次" + i + " \\* MERGEFORMAT ", "«S2TR»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類二排名PR值" + i + " \\* MERGEFORMAT ", "«S2TPR»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類二排名百分比" + i + " \\* MERGEFORMAT ", "«S2TP»");
                    builder.EndRow();
                }
                builder.EndTable();

                builder.Writeln();

            }

            //序列化科目資料五標(年排名及班排名)
            builder.Write("序列化科目資料五標(年排名及班排名)");
            foreach (string key in new string[]{
                    "語文",
                    "數學",
                    "社會",
                    "自然與生活科技",
                    "健康與體育",
                    "藝術與人文",
                    "綜合活動",
                    "彈性課程"
                })
            {
                builder.Write("領域:" + key);

                builder.StartTable();
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

                for (int i = 1; i <= 7; i++)
                {
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目名稱" + i + " \\* MERGEFORMAT ", "«SN»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目年排名母體頂標" + i + " \\* MERGEFORMAT ", "«SY25T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目年排名母體前標" + i + " \\* MERGEFORMAT ", "«SY50T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目年排名母體平均" + i + " \\* MERGEFORMAT ", "«SYA»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目年排名母體後標" + i + " \\* MERGEFORMAT ", "«SY50B»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目年排名母體底標" + i + " \\* MERGEFORMAT ", "«SY25B»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目班排名母體頂標" + i + " \\* MERGEFORMAT ", "«SC25T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目班排名母體前標" + i + " \\* MERGEFORMAT ", "«SC50T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目班排名母體平均" + i + " \\* MERGEFORMAT ", "«SCA»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目班排名母體後標" + i + " \\* MERGEFORMAT ", "«SC50B»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目班排名母體底標" + i + " \\* MERGEFORMAT ", "«SC25B»");
                    builder.EndRow();
                }
                builder.EndTable();

                builder.Writeln();

            }

            //序列化科目資料五標(類一排名及類二排名)
            builder.Write("序列化科目資料五標(類一排名及類二排名)");
            foreach (string key in new string[]{
                    "語文",
                    "數學",
                    "社會",
                    "自然與生活科技",
                    "健康與體育",
                    "藝術與人文",
                    "綜合活動",
                    "彈性課程"
                })
            {
                builder.Write("領域:" + key);

                builder.StartTable();
                builder.InsertCell();
                builder.Write("科目名稱");
                builder.InsertCell();
                builder.Write("科目類一排名母體頂標");
                builder.InsertCell();
                builder.Write("科目類一排名母體前標");
                builder.InsertCell();
                builder.Write("科目類一排名母體平均");
                builder.InsertCell();
                builder.Write("科目類一排名母體後標");
                builder.InsertCell();
                builder.Write("科目類一排名母體底標");
                builder.InsertCell();
                builder.Write("科目類二排名母體頂標");
                builder.InsertCell();
                builder.Write("科目類二排名母體前標");
                builder.InsertCell();
                builder.Write("科目類二排名母體平均");
                builder.InsertCell();
                builder.Write("科目類二排名母體後標");
                builder.InsertCell();
                builder.Write("科目類二排名母體底標");
                builder.EndRow();

                for (int i = 1; i <= 7; i++)
                {
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目名稱" + i + " \\* MERGEFORMAT ", "«SN»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類一排名母體頂標" + i + " \\* MERGEFORMAT ", "«S1T25T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類一排名母體前標" + i + " \\* MERGEFORMAT ", "«S1T50T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類一排名母體平均" + i + " \\* MERGEFORMAT ", "«S1TA»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類一排名母體後標" + i + " \\* MERGEFORMAT ", "«S1T50B»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類一排名母體底標" + i + " \\* MERGEFORMAT ", "«S1T25B»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類二排名母體頂標" + i + " \\* MERGEFORMAT ", "«S2T25T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類二排名母體前標" + i + " \\* MERGEFORMAT ", "«S2T50T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類二排名母體平均" + i + " \\* MERGEFORMAT ", "«S2TA»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類二排名母體後標" + i + " \\* MERGEFORMAT ", "«S2T50B»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + "_科目類二排名母體底標" + i + " \\* MERGEFORMAT ", "«S2T25B»");
                    builder.EndRow();
                }
                builder.EndTable();

                builder.Writeln();

            }

            #region 儲存檔案
            string inputReportName = "合併欄位總表";
            string reportName = inputReportName;

            string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".doc");

            if (System.IO.File.Exists(path))
            {
                int i = 1;
                while (true)
                {
                    string newPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + (i++) + Path.GetExtension(path);
                    if (!System.IO.File.Exists(newPath))
                    {
                        path = newPath;
                        break;
                    }
                }
            }

            try
            {
                doc.Save(path, Aspose.Words.SaveFormat.Doc);
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                System.Windows.Forms.SaveFileDialog sd = new System.Windows.Forms.SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".doc";
                sd.Filter = "Excel檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
                if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        doc.Save(path, Aspose.Words.SaveFormat.Doc);
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

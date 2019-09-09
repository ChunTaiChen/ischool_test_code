using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.Data;
using System.Data;
using System.Xml.Linq;

namespace SmartSchool.Evaluation
{
    public class Utility
    {
        /// <summary>
        /// 不處理進位(word合併欄位中自行處理)
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public static decimal NoRound(decimal score)
        {
            return score;
        }

        /// <summary>
        /// 四捨五入到小數下二位
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public static decimal ParseD2(decimal score)
        {
            return Math.Round(score, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 取得排名百分比：名次減一除母數後取左邊第一個整數
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public static int ParseRankPercent(int rank, int total)
        {
            int retVal = 0;
            if (total > 0 && rank > 0)
            {
                return (int)(Math.Floor(((decimal)rank - 1) * 100m / (decimal)total) + 1);

                //decimal rr = (decimal)(rank - 1);
                //decimal tt = (decimal)total;


                //decimal xR = Math.Round(rr * 100 / total, 0);
                //decimal x = rr * 100 / total + 1;

                //if (xR == x)
                //    retVal = (int)xR;
                //else
                //    retVal = (int)x;

                //retVal = (int)(Math.Floor((rr * 100) / tt));
            }
            return retVal;
        }


        /// <summary>
        /// 透過學生編號取得學測報名序號
        /// </summary>
        /// <param name="StudentIDList"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetStudentSATSerNoByStudentIDList(List<string> StudentIDList)
        {
            Dictionary<string, string> retValue = new Dictionary<string, string>();
            if (StudentIDList.Count > 0)
            {
                try
                {
                    QueryHelper qh = new QueryHelper();
                    string query = "select ref_student_id as sid,sat_ser_no as sno from $sh.college.sat.student where ref_student_id in('" + string.Join("','", StudentIDList.ToArray()) + "')";
                    DataTable dt = qh.Select(query);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string sid = dr["sid"].ToString();
                        string sno = "";
                        if (dr["sno"] != null)
                            sno = dr["sno"].ToString();

                        if (!retValue.ContainsKey(sid))
                            retValue.Add(sid, sno);
                    }
                }
                catch (Exception ex)
                { }
            }
            return retValue;
        }

        public static Dictionary<string, List<StudSemsEntryRating>> GetStudSemsEntryRatingByStudentID(List<string> StudentIDList)
        {
            Dictionary<string, List<StudSemsEntryRating>> retValue = new Dictionary<string, List<StudSemsEntryRating>>();
            if (StudentIDList.Count > 0)
            {
                QueryHelper qh = new QueryHelper();
                string query = "select ref_student_id as sid,school_year,semester,grade_year,class_rating,dept_rating,year_rating,group_rating from sems_entry_score where ref_student_id in (" + string.Join(",", StudentIDList.ToArray()) + ") and entry_group=1  order by ref_student_id,grade_year,semester";

                DataTable dt = qh.Select(query);
                foreach (DataRow dr in dt.Rows)
                {
                    string sid = dr["sid"].ToString();

                    if (!retValue.ContainsKey(sid))
                        retValue.Add(sid, new List<StudSemsEntryRating>());

                    StudSemsEntryRating sser = new StudSemsEntryRating();
                    sser.StudentID = sid;
                    sser.SchoolYear = dr["school_year"].ToString();
                    sser.Semester = dr["semester"].ToString();
                    sser.GradeYear = dr["grade_year"].ToString();
                    sser.EntryName = "學業";
                    // Parse XML
                    try
                    {
                        string cStr = dr["class_rating"].ToString();
                        string dStr = dr["dept_rating"].ToString();
                        string yStr = dr["year_rating"].ToString();
                        string g1Str = dr["group_rating"].ToString();

                        // 班排
                        if (!string.IsNullOrEmpty(cStr))
                        {
                            XElement elmC = XElement.Parse(cStr);
                            foreach (XElement elm in elmC.Elements("Item"))
                            {
                                if (elm.Attribute("分項") != null && elm.Attribute("分項").Value == "學業")
                                {
                                    if (elm.Attribute("成績人數") != null && elm.Attribute("成績人數").Value != "")
                                    {
                                        int x;
                                        if (int.TryParse(elm.Attribute("成績人數").Value, out x))
                                            sser.ClassCount = x;

                                    }
                                    if (elm.Attribute("排名") != null && elm.Attribute("排名").Value != "")
                                    {
                                        int x;
                                        if (int.TryParse(elm.Attribute("排名").Value, out x))
                                            sser.ClassRank = x;
                                    }
                                }
                            }
                        }

                        // 科排
                        if (!string.IsNullOrEmpty(dStr))
                        {
                            XElement elmD = XElement.Parse(dStr);
                            foreach (XElement elm in elmD.Elements("Item"))
                            {
                                if (elm.Attribute("分項") != null && elm.Attribute("分項").Value == "學業")
                                {
                                    if (elm.Attribute("成績人數") != null && elm.Attribute("成績人數").Value != "")
                                    {
                                        int x;
                                        if (int.TryParse(elm.Attribute("成績人數").Value, out x))
                                            sser.DeptCount = x;
                                    }
                                    if (elm.Attribute("排名") != null && elm.Attribute("排名").Value != "")
                                    {
                                        int x;
                                        if (int.TryParse(elm.Attribute("排名").Value, out x))
                                            sser.DeptRank = x;
                                    }
                                }
                            }
                        }

                        // 校排
                        if (!string.IsNullOrEmpty(yStr))
                        {
                            XElement elmY = XElement.Parse(yStr);
                            foreach (XElement elm in elmY.Elements("Item"))
                            {
                                if (elm.Attribute("分項") != null && elm.Attribute("分項").Value == "學業")
                                {
                                    if (elm.Attribute("成績人數") != null && elm.Attribute("成績人數").Value != "")
                                    {
                                        int x;
                                        if (int.TryParse(elm.Attribute("成績人數").Value, out x))
                                            sser.YearCount = x;
                                    }

                                    if (elm.Attribute("排名") != null && elm.Attribute("排名").Value != "")
                                    {
                                        int x;
                                        if (int.TryParse(elm.Attribute("排名").Value, out x))
                                            sser.YearRank = x;
                                    }
                                }
                            }
                        }

                        // 類排  ，穎驊新增 類1 類2 、類別種類邏輯 
                        if (!string.IsNullOrEmpty(g1Str))
                        {
                            int groupCount = 1;

                            XElement elmG1 = XElement.Parse(g1Str);
                            foreach (XElement elmR in elmG1.Elements("Rating"))
                            {
                                //類別1
                                if (groupCount == 1)
                                {
                                    if (elmR.Attribute("類別") != null && elmR.Attribute("類別").Value != "")
                                    {
                                        sser.Group1 = elmR.Attribute("類別").Value;
                                    }

                                    foreach (XElement elm in elmR.Elements("Item"))
                                    {
                                        if (elm.Attribute("分項") != null && elm.Attribute("分項").Value == "學業")
                                        {
                                            if (elm.Attribute("成績人數") != null && elm.Attribute("成績人數").Value != "")
                                            {
                                                int x;
                                                if (int.TryParse(elm.Attribute("成績人數").Value, out x))
                                                    sser.Group1Count = x;
                                            }

                                            if (elm.Attribute("排名") != null && elm.Attribute("排名").Value != "")
                                            {
                                                int x;
                                                if (int.TryParse(elm.Attribute("排名").Value, out x))
                                                    sser.Group1Rank = x;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // 2018/8 穎驊註解，經過討論後， 先暫時將 ischool類別2 排名的概念拿掉，因為目前的結構 無法區隔類別1、類別2，待日後設計完整
                                    //if (elmR.Attribute("類別") != null && elmR.Attribute("類別").Value != "")
                                    //{
                                    //    sser.Group2 = elmR.Attribute("類別").Value;
                                    //}

                                    //foreach (XElement elm in elmR.Elements("Item"))
                                    //{
                                    //    if (elm.Attribute("分項") != null && elm.Attribute("分項").Value == "學業")
                                    //    {
                                    //        if (elm.Attribute("成績人數") != null && elm.Attribute("成績人數").Value != "")
                                    //        {
                                    //            int x;
                                    //            if (int.TryParse(elm.Attribute("成績人數").Value, out x))
                                    //                sser.Group2Count = x;
                                    //        }

                                    //        if (elm.Attribute("排名") != null && elm.Attribute("排名").Value != "")
                                    //        {
                                    //            int x;
                                    //            if (int.TryParse(elm.Attribute("排名").Value, out x))
                                    //                sser.Group2Rank = x;
                                    //        }

                                    //        if (elm.Attribute("成績") != null && elm.Attribute("成績").Value != "")
                                    //        {
                                    //           sser.Score = "" + elm.Attribute("成績").Value;
                                    //        }
                                    //    }
                                    //}

                                }

                                groupCount++;


                            }
                        }
                    }
                    catch (Exception ex) { }

                    retValue[sid].Add(sser);
                }
            }
            return retValue;
        }

        public static Dictionary<string, List<StudSemsSubjRating>> GetStudSemsSubjRatingByStudentID(List<string> StudentIDList)
        {
            //2018/3/31 穎驊註解，因應新民的客服#5822 無法轉出大學個人申請前五學期成績單，其學校一筆查詢太多，會造成資料庫當機的問題
            //在此改寫，倘若學生數太多，則分500人一批，來做查詢
            Dictionary<string, List<StudSemsSubjRating>> retValue = new Dictionary<string, List<StudSemsSubjRating>>();
            if (StudentIDList.Count > 0 && StudentIDList.Count < 500)
            {
                retValue = GetStudSemsSubjRatingByStudentID_fx(StudentIDList);
            }
            //假如學生數大於500 則分批作業
            if (StudentIDList.Count >= 500)
            {
                List<List<string>> listGroup = new List<List<string>>();
                int j = 500;
                for (int i = 0; i < StudentIDList.Count; i += 500)
                {
                    List<string> cList = new List<string>();
                    cList = StudentIDList.Take(j).Skip(i).ToList();
                    j += 500;
                    listGroup.Add(cList);
                }
                // 把分匹的查詢結果，再併成dict ，以利回傳結果
                foreach (List<string> list in listGroup)
                {
                    Dictionary<string, List<StudSemsSubjRating>> retValue_batch = new Dictionary<string, List<StudSemsSubjRating>>();

                    retValue_batch = GetStudSemsSubjRatingByStudentID_fx(list);

                    retValue = retValue.Concat(retValue_batch).ToDictionary(k => k.Key, v => v.Value);
                }


            }
            return retValue;
        }
        // 舊的方法，不變
        private static Dictionary<string, List<StudSemsSubjRating>> GetStudSemsSubjRatingByStudentID_fx(List<string> StudentIDList)
        {
            Dictionary<string, List<StudSemsSubjRating>> _retValue = new Dictionary<string, List<StudSemsSubjRating>>();


            QueryHelper qh = new QueryHelper();
            string query = "select ref_student_id as sid,school_year,semester,grade_year,class_rating,dept_rating,year_rating,group_rating from sems_subj_score where ref_student_id in (" + string.Join(",", StudentIDList.ToArray()) + ")  order by ref_student_id,grade_year,semester";

            DataTable dt = qh.Select(query);
            foreach (DataRow dr in dt.Rows)
            {
                string sid = dr["sid"].ToString();

                if (!_retValue.ContainsKey(sid))
                    _retValue.Add(sid, new List<StudSemsSubjRating>());

                StudSemsSubjRating sser = new StudSemsSubjRating();
                sser.StudentID = sid;
                sser.SchoolYear = dr["school_year"].ToString();
                sser.Semester = dr["semester"].ToString();
                sser.GradeYear = dr["grade_year"].ToString();
                // Parse XML
                try
                {
                    string cStr = dr["class_rating"].ToString();
                    string dStr = dr["dept_rating"].ToString();
                    string yStr = dr["year_rating"].ToString();
                    string g1Str = dr["group_rating"].ToString();

                    // 班排
                    if (!string.IsNullOrEmpty(cStr))
                    {
                        XElement elmC = XElement.Parse(cStr);
                        foreach (XElement elm in elmC.Elements("Item"))
                        {
                            if (elm.Attribute("科目") != null)
                            {
                                if (elm.Attribute("成績人數") != null)
                                    sser.AddClassCount(elm.Attribute("科目").Value, int.Parse(elm.Attribute("成績人數").Value));

                                if (elm.Attribute("排名") != null)
                                    sser.AddClassRank(elm.Attribute("科目").Value, int.Parse(elm.Attribute("排名").Value));
                            }
                        }
                    }

                    // 科排
                    if (!string.IsNullOrEmpty(dStr))
                    {
                        XElement elmC = XElement.Parse(dStr);
                        foreach (XElement elm in elmC.Elements("Item"))
                        {
                            if (elm.Attribute("科目") != null)
                            {
                                if (elm.Attribute("成績人數") != null)
                                    sser.AddDeptCount(elm.Attribute("科目").Value, int.Parse(elm.Attribute("成績人數").Value));

                                if (elm.Attribute("排名") != null)
                                    sser.AddDeptRank(elm.Attribute("科目").Value, int.Parse(elm.Attribute("排名").Value));
                            }
                        }
                    }
                    // 年排
                    if (!string.IsNullOrEmpty(yStr))
                    {
                        XElement elmC = XElement.Parse(yStr);
                        foreach (XElement elm in elmC.Elements("Item"))
                        {
                            if (elm.Attribute("科目") != null)
                            {
                                if (elm.Attribute("成績人數") != null)
                                    sser.AddYearCount(elm.Attribute("科目").Value, int.Parse(elm.Attribute("成績人數").Value));

                                if (elm.Attribute("排名") != null)
                                    sser.AddYearRank(elm.Attribute("科目").Value, int.Parse(elm.Attribute("排名").Value));
                            }
                        }
                    }

                    // 類1排
                    if (!string.IsNullOrEmpty(g1Str))
                    {
                        XElement elmC = XElement.Parse(g1Str);
                        foreach (XElement elmR in elmC.Elements("Rating"))
                        {

                            if (elmR.Attribute("類別") != null && elmR.Attribute("類別").Value != "")
                            {
                                sser.Group1 = elmR.Attribute("類別").Value;
                            }

                            foreach (XElement elm in elmR.Elements("Item"))
                            {
                                if (elm.Attribute("科目") != null)
                                {
                                    if (elm.Attribute("成績人數") != null)
                                        sser.AddGroup1Count(elm.Attribute("科目").Value, int.Parse(elm.Attribute("成績人數").Value));

                                    if (elm.Attribute("排名") != null)
                                        sser.AddGroup1Rank(elm.Attribute("科目").Value, int.Parse(elm.Attribute("排名").Value));

                                }
                            }
                            break;
                        }

                    }
                }
                catch (Exception ex) { }

                _retValue[sid].Add(sser);
            }
            return _retValue;
        }


        // 2018/12/04 穎驊 新增 此為專門抓取 學生排名資料 Rating Xml 的方法
        public static Dictionary<string, List<StudSemsSubjRatingXML>> GetStudSemsSubjRatingXMLByStudentID(List<string> StudentIDList)
        {
            Dictionary<string, List<StudSemsSubjRatingXML>> _retValue = new Dictionary<string, List<StudSemsSubjRatingXML>>();

            if (StudentIDList.Count > 0)
            {
                QueryHelper qh = new QueryHelper();
                string query = "select ref_student_id as sid,school_year,semester,grade_year,class_rating,dept_rating,year_rating,group_rating from sems_subj_score where ref_student_id in (" + string.Join(",", StudentIDList.ToArray()) + ")  order by ref_student_id,grade_year,semester";

                DataTable dt = qh.Select(query);
                foreach (DataRow dr in dt.Rows)
                {
                    string sid = dr["sid"].ToString();

                    if (!_retValue.ContainsKey(sid))
                        _retValue.Add(sid, new List<StudSemsSubjRatingXML>());

                    StudSemsSubjRatingXML sserx = new StudSemsSubjRatingXML();
                    sserx.StudentID = sid;
                    sserx.SchoolYear = dr["school_year"].ToString();
                    sserx.Semester = dr["semester"].ToString();
                    sserx.GradeYear = dr["grade_year"].ToString();

                    if (dr["class_rating"].ToString() != "")
                    {
                        sserx.ClassRankXML = XElement.Parse(dr["class_rating"].ToString());
                    }
                    if (dr["dept_rating"].ToString() != "")
                    {
                        sserx.DeptRankXML = XElement.Parse(dr["dept_rating"].ToString());
                    }
                    if (dr["year_rating"].ToString() != "")
                    {
                        sserx.YearRankXML = XElement.Parse(dr["year_rating"].ToString());
                    }
                    if (dr["group_rating"].ToString() != "")
                    {
                        sserx.GroupRankXML = XElement.Parse(dr["group_rating"].ToString());
                    }


                    _retValue[sid].Add(sserx);
                }
            }
         
            return _retValue;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace HsinChuExamScore_JH
{
    /// <summary>
    /// 使用自訂的字根來比較字串大小，可配合List<T>.Sort()使用
    /// </summary>
    public class StringComparer : IComparer<string>
    {
        private static string[] keys = new string[] { "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", 
            "忠", "孝", "仁", "愛", "信", "義", "和", "平",
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "一", "二", "三", "四", "五", "六", "七", "八", "九", "十"
        };
        /// <summary>
        /// 使用預設字根比較字串
        /// </summary>
        public static int Comparer(string s1, string s2)
        {
            return Comparer(s1, s2, keys);
        }
        /// <summary>
        /// 使用自訂字根比較字串
        /// </summary>
        /// <param name="keys">使用字根</param>
        public static int Comparer(string s1, string s2, params string[] keys)
        {
            if ( s1 == s2 ) return 0;
            if ( s1.Length == 0 ) return -1;
            if ( s2.Length == 0 ) return 1;
            int length = s1.Length > s2.Length ? s2.Length : s1.Length;
            //string ls1 = "", ls2 = "";
            for ( int i = 0 ; i < length ; i++ )
            {
                //先用兩個字串罪的開頭比關鍵字
                foreach ( string key in keys )
                {
                    bool b1 = false, b2 = false;
                    b1 = s1.StartsWith(key);
                    b2 = s2.StartsWith(key);
                    if ( b1 && !b2 )
                        return -1;
                    if ( b2 && !b1 )
                        return 1;
                }
                //如果兩個字串第一個字相同就砍掉第一個再比一次
                if ( s1.Substring(0, 1) == s2.Substring(0, 1) )
                {
                    s1 = s1.Substring(1);
                    s2 = s2.Substring(1);
                }
                else
                {
                    return s1.Substring(0, 1).CompareTo(s2.Substring(0, 1));
                }
            }
            if ( string.IsNullOrEmpty(s1) ) return -1;
            if ( string.IsNullOrEmpty(s2) ) return 1;
            return s1.CompareTo(s2);
        }

        private string[] _Keys;

        /// <summary>
        /// 建構子
        /// </summary>
        public StringComparer()
        {
            _Keys = keys;
        }

        /// <summary>
        /// 建構子，傳入自訂的字根
        /// </summary>
        /// <param name="keys">使用字根</param>
        public StringComparer(params string[] keys)
        {
            _Keys = keys;
        }

        #region IComparer<string> 成員
        /// <summary>
        /// 比較
        /// </summary>
        public int Compare(string x, string y)
        {
            return Comparer(x, y, _Keys);
        }

        #endregion
    }
}

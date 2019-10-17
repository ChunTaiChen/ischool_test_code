using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;

namespace KaoHsiungExamScore_JH.DAO
{
    public class UDTTransfer
    {
        /// <summary>
        /// 取得預測專案名稱
        /// </summary>
        /// <returns></returns>
        public static List<UDT_ScoreConfig> GetDefaultConfigNameListByTableName(string tableName)
        {
            List<UDT_ScoreConfig> retVal = new List<UDT_ScoreConfig>();
            AccessHelper _AccessHelper = new AccessHelper();           
            string query = "table_name='"+tableName+"'";
                retVal = _AccessHelper.Select<UDT_ScoreConfig>(query);

            return retVal;
        }

        /// <summary>
        /// 新增設定資料
        /// </summary>
        /// <param name="dataList"></param>
        public static void InsertConfigData(List<UDT_ScoreConfig> dataList)
        {
            AccessHelper accessHelper = new AccessHelper();
            accessHelper.InsertValues(dataList);
        }

        /// <summary>
        /// 更新設定資料
        /// </summary>
        /// <param name="dataList"></param>
        public static void UpdateConfigData(List<UDT_ScoreConfig> dataList)
        {
            AccessHelper accessHelper = new AccessHelper();
            accessHelper.UpdateValues(dataList);
        }

        /// <summary>
        /// 刪除設定資料
        /// </summary>
        /// <param name="dataList"></param>
        public static void DeleteConfigData(List<UDT_ScoreConfig> dataList)
        {
            foreach (UDT_ScoreConfig data in dataList)
                data.Deleted = true;

            AccessHelper accessHelper = new AccessHelper();
            accessHelper.DeletedValues(dataList);
        }

    }
}

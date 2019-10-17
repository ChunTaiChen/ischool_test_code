using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;

namespace HsinChuExamScore_JH.DAO
{
    [TableName("ischoolHsinChuScoreConfig")]
    public class UDT_ScoreConfig : ActiveRecord
    {
        
            /// <summary>
            /// 專案名稱
            /// </summary>
            [Field(Field = "project_name", Indexed = false)]
            public string ProjectName { get; set; }

            /// <summary>
            /// UDT資料表
            /// </summary>
            [Field(Field = "table_name", Indexed = false)]
            public string UDTTableName { get; set; }

            /// <summary>
            /// 名稱
            /// </summary>
            [Field(Field = "name", Indexed = false)]
            public string Name { get; set; }

            /// <summary>
            /// 類型
            /// </summary>
            [Field(Field = "type", Indexed = false)]
            public string Type { get; set; }


    }
}

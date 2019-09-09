using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.DSAClient;

namespace SmartSchool.Evaluation
{
    /// <summary>
    /// 資料更新。
    /// </summary>
    internal class UpdateHelper
    {
        private const string AdminContractName = "admin";

        #region Static
        private static Connection _default_connection = null;

        internal static Connection DefaultConnection
        {
            get
            {
                if (_default_connection == null)
                    return FISCA.Authentication.DSAServices.DefaultDataSource.AsContract(AdminContractName);
                else
                    return _default_connection;
            }
        }

        /// <summary>
        /// 設定 QueryHelper 預設的資料來源連線。
        /// </summary>
        /// <param name="datasource"></param>
        public static void SetDefaultDataSource(Connection datasource)
        {
            _default_connection = datasource.AsContract(AdminContractName); ;
        }
        #endregion

        private Connection Connection { get; set; }

        /// <summary>
        /// 使用系統預設的連線建立 QueryHelper 實體。
        /// </summary>
        public UpdateHelper()
        {
            Connection = DefaultConnection;
        }

        /// <summary>
        /// 使用指定的連線建立 QueryHelper 實體。
        /// </summary>
        /// <param name="datasource"></param>
        public UpdateHelper(Connection datasource)
        {
            Connection = datasource.AsContract(AdminContractName);
        }

        public int Execute(string cmd)
        {
            return Execute(new List<string>(new string[] { cmd }));
        }

        public int Execute(List<string> cmds)
        {
            XmlHelper req = new XmlHelper("<Request/>");

            foreach (string cmd in cmds)
                req.AddElement(".", "Command", cmd);

            //呼叫 Service。
            Envelope rsp = Connection.SendRequest("UDTService.DML.Command", new Envelope(req));

            return 0;
        }
    }
}

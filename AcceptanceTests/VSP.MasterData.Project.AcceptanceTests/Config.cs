using System;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace VSP.MasterData.Project.AcceptanceTests
{
    public class Config
    {
        public static string MySqlConnString = ConfigurationManager.ConnectionStrings["MySql.Connection"].ConnectionString;
        public static string MySqlDbName = "`" + MySqlConnString.Split(';')[1].Split('=')[1] + "`";

        public static string KafkaEndpoint = ConfigurationManager.AppSettings["KafkaEndpoint"];
        public static string CustomerMasterDataTopic = ConfigurationManager.AppSettings["CustomerMasterDataTopic"];
        public static string CustomerUserMasterDataTopic = ConfigurationManager.AppSettings["CustomerUserMasterDataTopic"];
    }
}

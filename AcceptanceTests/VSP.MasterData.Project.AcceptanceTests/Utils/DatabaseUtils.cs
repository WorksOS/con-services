using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace VSP.MasterData.Project.AcceptanceTests.Utils
{
    public class DatabaseUtils
    {
        public static string ExecuteMySqlQueryResult(string connectionString, string queryString)
        {

            string queryResult = null;
            using (MySqlConnection mySqlConnection = new MySqlConnection(connectionString))
            {
                mySqlConnection.Open();
                MySqlCommand mySqlCommand = new MySqlCommand(queryString, mySqlConnection);
                MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();

                while (mySqlDataReader.Read())
                {
                    queryResult = mySqlDataReader[0].ToString();
                }
            }
            return queryResult;
        }

        public static int GetTheHighestProjectId()
        {
            try
            {
                return Convert.ToInt32(ExecuteMySqlQueryResult(Config.MySqlConnString,
                    "SELECT max(ProjectID) FROM " + Config.MySqlDbName + ".Project"));
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
